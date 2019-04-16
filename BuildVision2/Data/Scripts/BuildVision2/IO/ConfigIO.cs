using Sandbox.ModAPI;
using System;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace DarkHelmet.BuildVision2
{
    /// <summary>
    /// Stores all information needed for reading/writing from the config file.
    /// </summary>
    [XmlRoot, XmlType(TypeName = "BuildVisionSettings")]
    public class ConfigData
    {
        [XmlIgnore]
        public static ConfigData Defaults
        {
            get
            {
                return new ConfigData
                {
                    versionID = BvMain.configVersionID,
                    general = GeneralConfig.Defaults,
                    menu = PropMenuConfig.Defaults,
                    propertyBlock = PropBlockConfig.Defaults,
                    binds = BindsConfig.Defaults
                };
            }
        }

        [XmlAttribute(AttributeName = "ConfigVersionID")]
        public int versionID;

        [XmlElement(ElementName = "GeneralSettings")]
        public GeneralConfig general;

        [XmlElement(ElementName = "GuiSettings")]
        public PropMenuConfig menu;

        [XmlElement(ElementName = "BlockPropertySettings")]
        public PropBlockConfig propertyBlock;

        [XmlElement(ElementName = "InputSettings")]
        public BindsConfig binds;

        public ConfigData() { }

        public ConfigData GetCopy()
        {
            return new ConfigData
            {
                versionID = versionID,
                general = general,
                menu = menu,
                propertyBlock = propertyBlock,
                binds = binds
            };
        }

        public void Validate()
        {
            if (versionID != BvMain.configVersionID)
                versionID = BvMain.configVersionID;

            if (menu != null)
                menu.Validate();
            else
                menu = PropMenuConfig.Defaults;

            if (binds != null)
                binds.Validate();
            else
                binds = BindsConfig.Defaults;

            if (propertyBlock != null)
                propertyBlock.Validate();
            else
                propertyBlock = PropBlockConfig.Defaults;
        }
    }

    /// <summary>
    /// Handles loading/saving configuration data; singleton
    /// </summary>
    internal sealed class ConfigIO
    {
        public delegate void ConfigDataCallback(ConfigData cfg);
        public static ConfigIO Instance { get; private set; }
        public bool SaveInProgress { get; private set; }

        private static BvMain Main { get { return BvMain.Instance; } }
        private static LogIO Log { get { return LogIO.Instance; } }
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
                Instance = new ConfigIO(configFileName);
        }

        /// <summary>
        /// Updates internal task queue. Parallel methods will not work properly if this isn't being
        /// updated regularly.
        /// </summary>
        public void Update() =>
            taskPool.Update();

        private void ErrorCallback(List<BvException> known, BvAggregateException unknown)
        {
            if (known != null && known.Count > 0)
            {
                SaveInProgress = false;
                string exceptions = "";

                foreach (Exception e in known)
                {
                    Main.SendChatMessage(e.Message);
                    exceptions += e.ToString();
                }

                Log.TryWriteToLogStart(exceptions);
            }

            if (unknown != null)
            {
                Log.TryWriteToLogStart($"\nSave operation failed.\n{unknown.ToString()}");
                Main.SendChatMessage("Save operation failed.");
                SaveInProgress = false;

                throw unknown;
            }
        }

        public void Close()
        {
            Instance = null;
        }

        /// <summary>
        /// Loads the current configuration in parallel.
        /// </summary>
        public void LoadStart(ConfigDataCallback UpdateConfig, bool silent = false)
        {
            if (!SaveInProgress)
            {
                SaveInProgress = true;
                if (!silent) Main.SendChatMessage("Loading configuration...");

                taskPool.EnqueueTask(() =>
                {
                    ConfigData cfg;
                    BvException loadException, saveException;

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
                            BvMain.Log.TryWriteToLog(loadException.ToString() + "\n" + saveException.ToString());
                            taskPool.EnqueueAction(() => 
                                Main.SendChatMessage("Unable to load or create configuration file."));
                        }
                    }
                    else
                        taskPool.EnqueueAction(() => LoadFinish(true, silent));
                });
            }
            else
                Main.SendChatMessage("Save operation already in progress.");
        }

        private ConfigData ValidateConfig(ConfigData cfg)
        {
            if (cfg != null)
            {
                if (cfg.versionID == BvMain.configVersionID)
                    cfg.Validate();
                else
                {
                    Backup();

                    if (cfg.versionID < 4)
                        cfg.menu.apiHudConfig = ApiHudConfig.Defaults;

                    cfg.Validate();

                    taskPool.EnqueueAction(() => 
                        Main.SendChatMessage("Config version mismatch. Some settings may have " +
                        "been reset. A backup of the original config file will be made."));
                }

                return cfg;
            }
            else
            {
                taskPool.EnqueueAction(() => 
                    Main.SendChatMessage("Unable to load configuration. Loading default settings..."));

                return ConfigData.Defaults;
            }
        }

        private void LoadFinish(bool success, bool silent = false)
        {
            if (SaveInProgress)
            {
                if (!silent)
                {
                    if (success)
                        Main.SendChatMessage("Configuration loaded.");
                }

                SaveInProgress = false;
            }
        }

        /// <summary>
        /// Saves a given configuration to the save file in parallel.
        /// </summary>
        public void SaveStart(ConfigData cfg, bool silent = false)
        {
            if (!SaveInProgress)
            {
                if (!silent) Main.SendChatMessage("Saving configuration...");
                SaveInProgress = true;

                taskPool.EnqueueTask(() =>
                {
                    cfg.Validate();
                    BvException exception = TrySave(cfg);

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
                Main.SendChatMessage("Save operation already in progress.");
        }

        private void SaveFinish(bool success, bool silent = false)
        {
            if (SaveInProgress)
            {
                if (!silent)
                {
                    if (success)
                        Main.SendChatMessage("Configuration saved.");
                    else
                        Main.SendChatMessage("Unable to save configuration.");
                }

                SaveInProgress = false;
            }
        }

        /// <summary>
        /// Saves the current configuration synchronously.
        /// </summary>
        public void Save(ConfigData cfg)
        {
            if (!SaveInProgress)
            {
                cfg.Validate();
                BvException exception = TrySave(cfg);

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
            if (MyAPIGateway.Utilities.FileExistsInLocalStorage(cfgFile.file, typeof(ConfigIO)))
            {
                BvException exception = cfgFile.TryDuplicate($"old_" + cfgFile.file);

                if (exception != null)
                    throw exception;
            }
        }

        /// <summary>
        /// Attempts to load config file and creates a new one if it can't.
        /// </summary>
        private BvException TryLoad(out ConfigData cfg)
        {
            string data;
            BvException exception = cfgFile.TryRead(out data);
            cfg = null;

            if (exception != null || data == null)
                return exception;
            else
                exception = TryDeserializeXml(data, out cfg);

            if (exception != null)
            {
                Backup();
                TrySave(ConfigData.Defaults);
            }

            return exception;
        }

        /// <summary>
        /// Attempts to save current configuration to a file.
        /// </summary>
        private BvException TrySave(ConfigData cfg)
        {
            string xmlOut = null;
            BvException exception = TrySerializeToXml(cfg, out xmlOut);

            if (exception == null && xmlOut != null)
                exception = cfgFile.TryWrite(xmlOut);

            return exception;
        }

        /// <summary>
        /// Attempts to serialize an object to an Xml string.
        /// </summary>
        private static BvException TrySerializeToXml<T>(T obj, out string xmlOut)
        {
            BvException exception = null;
            xmlOut = null;

            try
            {
                xmlOut = MyAPIGateway.Utilities.SerializeToXML(obj);
            }
            catch (Exception e)
            {
                exception = new BvException("IO Error. Failed to generate Xml.", e);
            }

            return exception;
        }

        /// <summary>
        /// Attempts to deserialize an Xml string to an object of a given type.
        /// </summary>
        private static BvException TryDeserializeXml<T>(string xmlIn, out T obj)
        {
            BvException exception = null;
            obj = default(T);

            try
            {
                obj = MyAPIGateway.Utilities.SerializeFromXML<T>(xmlIn);
            }
            catch (Exception e)
            {
                exception = new BvException("IO Error. Unable to interpret XML.", e);
            }

            return exception;
        }
    }    
}