using Sandbox.ModAPI;
using System;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace DarkHelmet.IO
{
    /// <summary>
    /// Generic base for serializable config types. ConfigBase.Defaults must be hidden with a new implimentation
    /// for each config type.
    /// </summary>
    public abstract class ConfigBase<TConfig> where TConfig : ConfigBase<TConfig>, new()
    {
        [XmlIgnore]
        public static TConfig Defaults
        {
            get
            {
                if (defaults == null)
                    defaults = new TConfig().GetDefaults();

                return defaults;
            }
        }

        private static TConfig defaults;

        public abstract TConfig GetDefaults();

        public abstract void Validate();
    }

    public abstract class ConfigRootBase<TConfig> : ConfigBase<TConfig> where TConfig : ConfigRootBase<TConfig>, new()
    {
        public virtual int VersionID { get; set; }
    }

    /// <summary>
    /// Handles loading/saving configuration data; singleton
    /// </summary>
    internal sealed class ConfigIO<TConfig> where TConfig : ConfigRootBase<TConfig>, new()
    {
        public static ConfigIO<TConfig> Instance { get; private set; }
        public delegate void ConfigDataCallback(TConfig cfg);
        public bool SaveInProgress { get; private set; }

        private readonly Action<string> SendMessage;
        private readonly LogIO log;
        private readonly LocalFileIO cfgFile;
        private readonly TaskPool taskPool;

        private ConfigIO(string configFileName, LogIO log, Action<string> SendMessage)
        {
            cfgFile = new LocalFileIO(configFileName);
            this.log = log;
            this.SendMessage = SendMessage;
            taskPool = new TaskPool(1, ErrorCallback);
            SaveInProgress = false;
        }

        public static void Init(string configFileName, LogIO log, Action<string> SendMessage)
        {
            if (Instance == null)
                Instance = new ConfigIO<TConfig>(configFileName, log, SendMessage);
        }

        public void Close()
        {
            Instance = null;
        }

        /// <summary>
        /// Updates internal task queue. Parallel methods will not work properly if this isn't being
        /// updated regularly.
        /// </summary>
        public void Update() =>
            taskPool.Update();

        private void ErrorCallback(List<DhException> known, AggregateException unknown)
        {
            if (known != null && known.Count > 0)
            {
                SaveInProgress = false;
                string exceptions = "";

                foreach (Exception e in known)
                {
                    SendMessage(e.Message);
                    exceptions += e.ToString();
                }

                log.TryWriteToLogStart(exceptions);
            }

            if (unknown != null)
            {
                log.TryWriteToLogStart($"\nSave operation failed.\n{unknown.ToString()}");
                SendMessage("Save operation failed.");
                SaveInProgress = false;

                throw unknown;
            }
        }

        /// <summary>
        /// Loads the current configuration in parallel.
        /// </summary>
        public void LoadStart(ConfigDataCallback UpdateConfig, bool silent = false)
        {
            if (!SaveInProgress)
            {
                SaveInProgress = true;
                if (!silent) SendMessage("Loading configuration...");

                taskPool.EnqueueTask(() =>
                {
                    TConfig cfg;
                    DhException loadException, saveException;

                    loadException = TryLoad(out cfg);
                    cfg = ValidateConfig(cfg);
                    taskPool.EnqueueAction(() => UpdateConfig(cfg));
                    saveException = TrySave(cfg);

                    if (loadException != null)
                    {
                        loadException = TrySave(cfg);
                        taskPool.EnqueueAction(
                            () => LoadFinish(false, silent));

                        if (saveException != null)
                        {
                            log.TryWriteToLog(loadException.ToString() + "\n" + saveException.ToString());
                            taskPool.EnqueueAction(() => 
                                SendMessage("Unable to load or create configuration file."));
                        }
                    }
                    else
                        taskPool.EnqueueAction(() => LoadFinish(true, silent));
                });
            }
            else
                SendMessage("Save operation already in progress.");
        }

        private TConfig ValidateConfig(TConfig cfg)
        {
            if (cfg != null)
            {
                if (cfg.VersionID == ConfigRootBase<TConfig>.Defaults.VersionID)
                    cfg.Validate();
                else
                {
                    Backup();
                    cfg.Validate();

                    taskPool.EnqueueAction(() => 
                        SendMessage("Config version mismatch. Some settings may have " +
                        "been reset. A backup of the original config file will be made."));
                }

                return cfg;
            }
            else
            {
                taskPool.EnqueueAction(() =>
                    SendMessage("Unable to load configuration. Loading default settings..."));

                return ConfigRootBase<TConfig>.Defaults;
            }
        }

        private void LoadFinish(bool success, bool silent = false)
        {
            if (SaveInProgress)
            {
                if (!silent)
                {
                    if (success)
                        SendMessage("Configuration loaded.");
                }

                SaveInProgress = false;
            }
        }

        /// <summary>
        /// Saves a given configuration to the save file in parallel.
        /// </summary>
        public void SaveStart(TConfig cfg, bool silent = false)
        {
            if (!SaveInProgress)
            {
                if (!silent) SendMessage("Saving configuration...");
                SaveInProgress = true;

                taskPool.EnqueueTask(() =>
                {
                    cfg.Validate();
                    DhException exception = TrySave(cfg);

                    if (exception != null)
                    {
                        taskPool.EnqueueAction(() => SaveFinish(false, silent));
                        throw exception;
                    }
                    else
                        taskPool.EnqueueAction(() => SaveFinish(true, silent));
                });
            }
            else
                SendMessage("Save operation already in progress.");
        }

        private void SaveFinish(bool success, bool silent = false)
        {
            if (SaveInProgress)
            {
                if (!silent)
                {
                    if (success)
                        SendMessage("Configuration saved.");
                    else
                        SendMessage("Unable to save configuration.");
                }

                SaveInProgress = false;
            }
        }

        /// <summary>
        /// Saves the current configuration synchronously.
        /// </summary>
        public void Save(TConfig cfg)
        {
            if (!SaveInProgress)
            {
                cfg.Validate();
                DhException exception = TrySave(cfg);

                if (exception != null)
                    throw exception;
            }
        }

        /// <summary>
        /// Creates a duplicate of the config file starting with a new file name starting with "old_"
        /// if one exists.
        /// </summary>
        private void Backup()
        {
            if (MyAPIGateway.Utilities.FileExistsInLocalStorage(cfgFile.file, typeof(TConfig)))
            {
                DhException exception = cfgFile.TryDuplicate($"old_" + cfgFile.file);

                if (exception != null)
                    throw exception;
            }
        }

        /// <summary>
        /// Attempts to load config file and creates a new one if it can't.
        /// </summary>
        private DhException TryLoad(out TConfig cfg)
        {
            string data;
            DhException exception = cfgFile.TryRead(out data);
            cfg = default(TConfig);

            if (exception != null || data == null)
                return exception;
            else
                exception = TryDeserializeXml(data, out cfg);

            if (exception != null)
            {
                Backup();
                TrySave(ConfigRootBase<TConfig>.Defaults);
            }

            return exception;
        }

        /// <summary>
        /// Attempts to save current configuration to a file.
        /// </summary>
        private DhException TrySave(TConfig cfg)
        {
            string xmlOut = null;
            DhException exception = TrySerializeToXml(cfg, out xmlOut);

            if (exception == null && xmlOut != null)
                exception = cfgFile.TryWrite(xmlOut);

            return exception;
        }

        /// <summary>
        /// Attempts to serialize an object to an Xml string.
        /// </summary>
        private static DhException TrySerializeToXml<T>(T obj, out string xmlOut)
        {
            DhException exception = null;
            xmlOut = null;

            try
            {
                xmlOut = MyAPIGateway.Utilities.SerializeToXML(obj);
            }
            catch (Exception e)
            {
                exception = new DhException("IO Error. Failed to generate Xml.", e);
            }

            return exception;
        }

        /// <summary>
        /// Attempts to deserialize an Xml string to an object of a given type.
        /// </summary>
        private static DhException TryDeserializeXml<T>(string xmlIn, out T obj)
        {
            DhException exception = null;
            obj = default(T);

            try
            {
                obj = MyAPIGateway.Utilities.SerializeFromXML<T>(xmlIn);
            }
            catch (Exception e)
            {
                exception = new DhException("IO Error. Unable to interpret XML.", e);
            }

            return exception;
        }
    }    
}