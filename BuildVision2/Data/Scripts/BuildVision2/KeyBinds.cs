using Sandbox.ModAPI;
using System;
using System.Xml.Serialization;
using System.Collections.Generic;
using DarkHelmet.Input;
using DarkHelmet.IO;

namespace DarkHelmet.BuildVision2
{
    /// <summary>
    /// Stores data for serializing the configuration of the Binds class.
    /// </summary>
    public class BindsConfig : ConfigBase<BindsConfig>
    {
        [XmlIgnore]
        public new static BindsConfig Defaults { get { return new BindsConfig { bindData = DefaultBinds }; } }

        [XmlIgnore]
        public static KeyBindData[] DefaultBinds
        {
            get
            {
                KeyBindData[] copy = new KeyBindData[defaultBinds.Length];

                for (int n = 0; n < defaultBinds.Length; n++)
                    copy[n] = defaultBinds[n];

                return copy;
            }
        }

        private static readonly KeyBindData[] defaultBinds = new KeyBindData[]
        {
            new KeyBindData("open", new string[] { "control", "middlebutton" }),
            new KeyBindData("close", new string[] { "shift", "middlebutton" }),
            new KeyBindData("select", new string[] { "middlebutton" }),
            new KeyBindData("scrollup", new string[] { "mousewheelup" }),
            new KeyBindData("scrolldown", new string[] { "mousewheeldown" }),
            new KeyBindData("multx", new string[] { "control" }),
            new KeyBindData("multy", new string[] { "shift" }),
            new KeyBindData("multz", new string[] { "control", "shift" })
        };

        [XmlArray("KeyBinds")]
        public KeyBindData[] bindData;

        /// <summary>
        /// Checks any if fields have invalid values and resets them to the default if necessary.
        /// </summary>
        public override void Validate()
        {
            if (bindData == null || bindData.Length != defaultBinds.Length)
                bindData = DefaultBinds;
        }
    }

    internal sealed class KeyBinds
    {
        public static KeyBinds Instance { get; private set; }
        public BindsConfig Cfg { get { return new BindsConfig { bindData = BindManager.GetBindData() }; }
            set
            {
                if(BindManager.TryUpdateBinds(value.bindData))
                    UpdateBindReferences();
            }
        }
        public IKeyBind Open { get; private set; }
        public IKeyBind Hide { get; private set; }
        public IKeyBind Select { get; private set; }
        public IKeyBind ScrollUp { get; private set; }
        public IKeyBind ScrollDown { get; private set; }
        public IKeyBind MultX { get; private set; }
        public IKeyBind MultY { get; private set; }
        public IKeyBind MultZ { get; private set; }
        public static BindManager BindManager { get { return BindManager.Instance; } }

        private KeyBinds(Action<string> SendMessage, BindsConfig cfg)
        {
            BindManager.Init(SendMessage);
            BindManager.AddBinds(new string[] { "open", "close", "select", "scrollup", "scrolldown", "multx", "multy", "multz" });

            if (BindManager.TryUpdateBinds(cfg.bindData))
            {
                UpdateBindReferences();
            }
            else if (BindManager.TryUpdateBinds(BindsConfig.DefaultBinds))
            {
                UpdateBindReferences();
            }
        }

        public static void Init(Action<string> SendMessage, BindsConfig cfg)
        {
            if (Instance == null)
                Instance = new KeyBinds(SendMessage, cfg);
        }

        public void Update() =>
            BindManager.Update();

        public void Close()
        {
            Instance = null;
        }

        private void UpdateBindReferences()
        {
            Open = BindManager.GetBindByName("open");
            Hide = BindManager.GetBindByName("close");
            Select = BindManager.GetBindByName("select");
            ScrollUp = BindManager.GetBindByName("scrollup");
            ScrollDown = BindManager.GetBindByName("scrolldown");
            MultX = BindManager.GetBindByName("multx");
            MultY = BindManager.GetBindByName("multy");
            MultZ = BindManager.GetBindByName("multz");
        }
    }
}