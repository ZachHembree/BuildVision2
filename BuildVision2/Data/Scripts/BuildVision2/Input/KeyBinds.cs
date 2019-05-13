using Sandbox.ModAPI;
using System;
using System.Xml.Serialization;
using System.Collections.Generic;
using DarkHelmet.UI;
using DarkHelmet.Game;

namespace DarkHelmet.BuildVision2
{
    internal static class KeyBinds
    {
        public static BindsConfig Cfg { get { return cfg; }
            set
            {
                cfg = value;

                if(BindManager.TryUpdateBinds(cfg.bindData))
                    UpdateBindReferences();
            }
        }
        public static IKeyBind Open { get; private set; }
        public static IKeyBind Hide { get; private set; }
        public static IKeyBind Select { get; private set; }
        public static IKeyBind ScrollUp { get; private set; }
        public static IKeyBind ScrollDown { get; private set; }
        public static IKeyBind MultX { get; private set; }
        public static IKeyBind MultY { get; private set; }
        public static IKeyBind MultZ { get; private set; }
        public static BindManager BindManager { get { return BindManager.Instance; } }

        private static BindsConfig cfg = BindsConfig.Defaults;

        static KeyBinds()
        {
            BindManager.AddBinds(new string[] { "open", "close", "select", "scrollup", "scrolldown", "multx", "multy", "multz" });

            if (BindManager.TryUpdateBinds(Cfg.bindData))
                UpdateBindReferences();
            else if (BindManager.TryUpdateBinds(BindsConfig.DefaultBinds))
                UpdateBindReferences();
        }

        private static void UpdateBindReferences()
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