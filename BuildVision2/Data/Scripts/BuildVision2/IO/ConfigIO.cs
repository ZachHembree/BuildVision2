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
        public readonly static ConfigData defaults = new ConfigData
        {
            general = GeneralConfig.defaults,
            menu = PropMenuConfig.defaults,
            propertyBlock = PropBlockConfig.defaults,
            binds = BindsConfig.Defaults
        };

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
                general = general,
                menu = menu,
                propertyBlock = propertyBlock,
                binds = binds
            };
        }

        public void Validate()
        {
            menu.Validate();
            binds.Validate();
            propertyBlock.Validate();
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

        private readonly BvMain main;
        private readonly LogIO log;
        private readonly LocalFileIO cfgFile;
        private readonly TaskPool taskPool;

        private ConfigIO(BvMain main, LogIO log, string configFileName)
        {
            this.main = main;
            this.log = log;
            cfgFile = new LocalFileIO(configFileName);
            taskPool = new TaskPool(1, ErrorCallback);
            SaveInProgress = false;
        }

        public static ConfigIO GetInstance(BvMain main, LogIO log, string configFileName)
        {
            if (Instance == null)
                Instance = new ConfigIO(main, log, configFileName);

            return Instance;
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
                    main.SendChatMessage(e.Message);
                    exceptions += e.ToString();
                }

                log.TryWriteToLogStart(exceptions);
            }

            if (unknown != null)
            {
                log.TryWriteToLogStart($"\nSave operation failed.\n{unknown.ToString()}");
                main.SendChatMessage("Save operation failed.");
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
        public void LoadConfigStart(ConfigDataCallback UpdateConfig, bool silent = false)
        {
            if (!SaveInProgress)
            {
                SaveInProgress = true;
                if (!silent) main.SendChatMessage("Loading configuration...");

                taskPool.EnqueueTask(() =>
                {
                    ConfigData cfg;
                    BvException exception = TryLoadConfig(out cfg);

                    if (exception != null)
                    {
                        taskPool.EnqueueAction(() => LoadConfigFinish(false, silent));
                        taskPool.EnqueueAction(() => UpdateConfig(null));
                        throw exception;
                    }
                    else
                    {
                        taskPool.EnqueueAction(() => LoadConfigFinish(true, silent));
                        taskPool.EnqueueAction(() => UpdateConfig(cfg));
                    }
                });
            }
            else
                main.SendChatMessage("Save operation already in progress.");
        }

        private void LoadConfigFinish(bool success, bool silent = false)
        {
            if (SaveInProgress)
            {
                if (!silent)
                {
                    if (success)
                        main.SendChatMessage("Configuration loaded.");
                    else
                        main.SendChatMessage("Unable to load configuration.");
                }

                SaveInProgress = false;
            }
        }

        /// <summary>
        /// Saves a given configuration to the save file in parallel.
        /// </summary>
        public void SaveConfigStart(ConfigData cfg, bool silent = false)
        {
            if (!SaveInProgress)
            {
                if (!silent) main.SendChatMessage("Saving configuration...");
                SaveInProgress = true;

                taskPool.EnqueueTask(() =>
                {
                    BvException exception = TrySaveConfig(cfg);

                    if (exception != null)
                    {
                        taskPool.EnqueueAction(() => SaveConfigFinish(false, silent));
                        throw exception;
                    }
                    else
                        taskPool.EnqueueAction(() => SaveConfigFinish(true, silent));
                });
            }
            else
                main.SendChatMessage("Save operation already in progress.");
        }

        private void SaveConfigFinish(bool success, bool silent = false)
        {
            if (SaveInProgress)
            {
                if (!silent)
                {
                    if (success)
                        main.SendChatMessage("Configuration saved.");
                    else
                        main.SendChatMessage("Unable to save configuration.");
                }

                SaveInProgress = false;
            }
        }

        /// <summary>
        /// Saves the current configuration synchronously.
        /// </summary>
        public void SaveConfig(ConfigData cfg)
        {
            BvException exception = TrySaveConfig(cfg);

            if (exception != null)
                throw exception;
        }

        /// <summary>
        /// Creates a duplicate of the config file starting with a new file name starting with "old_"
        /// if one exists. Runs in parallel.
        /// </summary>
        public void BackupConfig()
        {
            if (MyAPIGateway.Utilities.FileExistsInLocalStorage(cfgFile.file, typeof(ConfigIO)))
            {
                BvException exception =
                    cfgFile.TryDuplicate($"old_" + cfgFile.file);

                if (exception != null)
                    throw exception;
            }
        }

        /// <summary>
        /// Attempts to load config file and creates a new one if it can't.
        /// </summary>
        private BvException TryLoadConfig(out ConfigData cfg)
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
                BackupConfig();
                TrySaveConfig(ConfigData.defaults);
            }

            return exception;
        }

        /// <summary>
        /// Attempts to save current configuration to a file.
        /// </summary>
        private BvException TrySaveConfig(ConfigData cfg)
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