using Sandbox.ModAPI;
using System;
using System.Xml.Serialization;
using System.Collections.Generic;
using DarkHelmet.UI;
using DarkHelmet.IO;

namespace DarkHelmet.BuildVision2
{
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