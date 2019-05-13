using Sandbox.ModAPI;
using System;
using System.Xml.Serialization;
using System.Collections.Generic;
using DarkHelmet.Game;

namespace DarkHelmet.IO
{
    /// <summary>
    /// Generic base for serializable config types.
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

    /// <summary>
    /// Base class for config root. This is the only config type accepted by ConfigIO.
    /// </summary>
    public abstract class ConfigRootBase<TConfig> : ConfigBase<TConfig> where TConfig : ConfigRootBase<TConfig>, new()
    {
        [XmlAttribute]
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

        private readonly LocalFileIO cfgFile;
        private readonly TaskPool taskPool;

        private ConfigIO(string configFileName)
        {
            cfgFile = new LocalFileIO(configFileName);

            taskPool = new TaskPool(1, ErrorCallback);
            SaveInProgress = false;
        }

        public static void Init(string configFileName)
        {
            if (Instance == null)
                Instance = new ConfigIO<TConfig>(configFileName);
        }

        public void Close()
        {
            Instance = null;
        }

        private void ErrorCallback(List<IOException> known, AggregateException unknown)
        {
            if (known != null && known.Count > 0)
            {
                SaveInProgress = false;
                string exceptions = "";

                foreach (Exception e in known)
                {
                    ModBase.SendChatMessage(e.Message);
                    exceptions += e.ToString();
                }

                LogIO.Instance.TryWriteToLogStart(exceptions);
            }

            if (unknown != null)
            {
                LogIO.Instance.TryWriteToLogStart($"\nSave operation failed.\n{unknown.ToString()}");
                ModBase.SendChatMessage("Save operation failed.");
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
                if (!silent) ModBase.SendChatMessage("Loading configuration...");

                taskPool.EnqueueTask(() =>
                {
                    TConfig cfg;
                    IOException loadException, saveException;

                    loadException = TryLoad(out cfg);
                    cfg = ValidateConfig(cfg);
                    taskPool.EnqueueAction(() => UpdateConfig(cfg));
                    saveException = TrySave(cfg);

                    if (loadException != null)
                    {
                        loadException = TrySave(cfg);
                        taskPool.EnqueueAction(() => LoadFinish(false, silent));

                        if (saveException != null)
                        {
                            LogIO.Instance.TryWriteToLog(loadException.ToString() + "\n" + saveException.ToString());
                            taskPool.EnqueueAction(() => ModBase.SendChatMessage("Unable to load or create configuration file."));
                        }
                    }
                    else
                        taskPool.EnqueueAction(() => LoadFinish(true, silent));
                });
            }
            else
                ModBase.SendChatMessage("Save operation already in progress.");
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

                    taskPool.EnqueueAction(() => ModBase.SendChatMessage("Config version mismatch. Some settings may have " +
                        "been reset. A backup of the original config file will be made."));
                }

                return cfg;
            }
            else
            {
                taskPool.EnqueueAction(() => ModBase.SendChatMessage("Unable to load configuration. Loading default settings..."));

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
                        ModBase.SendChatMessage("Configuration loaded.");
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
                if (!silent) ModBase.SendChatMessage("Saving configuration...");
                SaveInProgress = true;

                taskPool.EnqueueTask(() =>
                {
                    cfg.Validate();
                    IOException exception = TrySave(cfg);

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
                ModBase.SendChatMessage("Save operation already in progress.");
        }

        private void SaveFinish(bool success, bool silent = false)
        {
            if (SaveInProgress)
            {
                if (!silent)
                {
                    if (success)
                        ModBase.SendChatMessage("Configuration saved.");
                    else
                        ModBase.SendChatMessage("Unable to save configuration.");
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
                IOException exception = TrySave(cfg);

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
                IOException exception = cfgFile.TryDuplicate($"old_" + cfgFile.file);

                if (exception != null)
                    throw exception;
            }
        }

        /// <summary>
        /// Attempts to load config file and creates a new one if it can't.
        /// </summary>
        private IOException TryLoad(out TConfig cfg)
        {
            string data;
            IOException exception = cfgFile.TryRead(out data);
            cfg = null;

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
        private IOException TrySave(TConfig cfg)
        {
            string xmlOut;
            IOException exception = TrySerializeToXml(cfg, out xmlOut);

            if (exception == null && xmlOut != null)
                exception = cfgFile.TryWrite(xmlOut);

            return exception;
        }

        /// <summary>
        /// Attempts to serialize an object to an Xml string.
        /// </summary>
        private static IOException TrySerializeToXml<T>(T obj, out string xmlOut)
        {
            IOException exception = null;
            xmlOut = null;

            try
            {
                xmlOut = MyAPIGateway.Utilities.SerializeToXML(obj);
            }
            catch (Exception e)
            {
                exception = new IOException("IO Error. Failed to generate XML.", e);
            }

            return exception;
        }

        /// <summary>
        /// Attempts to deserialize an Xml string to an object of a given type.
        /// </summary>
        private static IOException TryDeserializeXml<T>(string xmlIn, out T obj)
        {
            IOException exception = null;
            obj = default(T);

            try
            {
                obj = MyAPIGateway.Utilities.SerializeFromXML<T>(xmlIn);
            }
            catch (Exception e)
            {
                exception = new IOException("IO Error. Unable to interpret XML.", e);
            }

            return exception;
        }
    }    
}