﻿using Sandbox.ModAPI;
using System;
using System.Xml.Serialization;
using System.Collections.Generic;
using DarkHelmet.UI;
using DarkHelmet.Game;

namespace DarkHelmet.BuildVision2
{
    /// <summary>
    /// Wrapper used to provide easy access to Build Vision key binds.
    /// </summary>
    internal sealed class KeyBinds : ModBase.ComponentBase
    {
        public static BindsConfig Cfg
        {
            get { return new BindsConfig { bindData = BindManager.GetBindData() }; }
            set
            {
                if(BindManager.TryUpdateBinds(value.bindData))
                    Instance.UpdateBindReferences();
            }
        }

        public static IKeyBind Open { get { return Instance.bvBinds[0]; } }
        public static IKeyBind Hide { get { return Instance.bvBinds[1]; } }
        public static IKeyBind Select { get { return Instance.bvBinds[2]; } }
        public static IKeyBind ScrollUp { get { return Instance.bvBinds[3]; } }
        public static IKeyBind ScrollDown { get { return Instance.bvBinds[4]; } }
        public static IKeyBind MultX { get { return Instance.bvBinds[5]; } }
        public static IKeyBind MultY { get { return Instance.bvBinds[6]; } }
        public static IKeyBind MultZ { get { return Instance.bvBinds[7]; } }

        private static KeyBinds Instance
        {
            get { Init(); return instance; }
            set { instance = value; }
        }
        private static KeyBinds instance;

        private static readonly string[] bindNames = new string[] { "open", "close", "select", "scrollup", "scrolldown", "multx", "multy", "multz" };
        private IKeyBind[] bvBinds;

        private KeyBinds() { }

        private static void Init()
        {
            if (instance == null)
            {
                instance = new KeyBinds();

                BindManager.RegisterBinds(bindNames);
                Cfg = BvConfig.Current.binds;

                BvConfig.OnConfigLoad += instance.UpdateConfig;
            }
        }

        public override void Close()
        {
            Instance = null;
            BvConfig.OnConfigLoad -= UpdateConfig;
        }

        private void UpdateConfig()
        {
            Cfg = BvConfig.Current.binds;
            ModBase.SendChatMessage("Updating Bind Cfg...");
        }

        private void UpdateBindReferences()
        {
            bvBinds = new IKeyBind[bindNames.Length];

            for (int n = 0; n < bvBinds.Length; n++)
                bvBinds[n] = BindManager.GetBindByName(bindNames[n]);
        }
    }
}