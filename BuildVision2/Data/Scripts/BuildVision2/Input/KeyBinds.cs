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
        public static BindsConfig Cfg
        {
            get { return new BindsConfig { bindData = BindManager.GetBindData() }; }
            set
            {
                if(BindManager.TryUpdateBinds(value.bindData))
                    UpdateBindReferences();
            }
        }

        static KeyBinds()
        {
            BvConfig.OnConfigLoad += () => { Cfg = BvConfig.Current.binds; };
        }

        public static BindManager BindManager { get { return BindManager.Instance; } }
        public static IKeyBind Open { get; private set; }
        public static IKeyBind Hide { get; private set; }
        public static IKeyBind Select { get; private set; }
        public static IKeyBind ScrollUp { get; private set; }
        public static IKeyBind ScrollDown { get; private set; }
        public static IKeyBind MultX { get; private set; }
        public static IKeyBind MultY { get; private set; }
        public static IKeyBind MultZ { get; private set; }

        private static void UpdateBindReferences()
        {
            Open = BindManager["open"];
            Hide = BindManager["close"];
            Select = BindManager["select"];
            ScrollUp = BindManager["scrollup"];
            ScrollDown = BindManager["scrolldown"];
            MultX = BindManager["multx"];
            MultY = BindManager["multy"];
            MultZ = BindManager["multz"];
        }
    }
}