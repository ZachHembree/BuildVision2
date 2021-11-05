using RichHudFramework;
using RichHudFramework.Internal;
using RichHudFramework.IO;
using RichHudFramework.UI;
using RichHudFramework.UI.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Sandbox.ModAPI;

namespace DarkHelmet.BuildVision2
{
    public sealed partial class BvMain
    {
        private void AddChatCommands()
        {
            CmdManager.GetOrCreateGroup("/bv2", new CmdGroupInitializer 
            {
                { "help", x => RichHudTerminal.OpenToPage(helpMain) },
                { "printBinds", x => ExceptionHandler.SendChatMessage(HelpText.GetPrintBindsMessage()) },
                { "bind", x => UpdateBind(x[0], x.GetSubarray(1)), 2 },
                { "resetBinds", x => BvBinds.Cfg = BindsConfig.Defaults },
                { "save", x => BvConfig.SaveStart() },
                { "load", x => BvConfig.LoadStart() },
                { "resetConfig", x => BvConfig.ResetConfig() },
                { "toggleAutoclose", x => Cfg.general.closeIfNotInView = !Cfg.general.closeIfNotInView },
                { "toggleOpenWhileHolding", x => Cfg.general.canOpenIfHolding = !Cfg.general.canOpenIfHolding },

                // Debug/Testing
                { "open", x => PropertiesMenu.TryOpenMenu() },
                { "close", x => PropertiesMenu.HideMenu() },
                { "reload", x => ExceptionHandler.ReloadClients() },
                { "crash", x => Crash() },
                { "printControlsToLog", x => LogIO.WriteToLogStart($"Control List:\n{HelpText.controlList}") },
                { "export", x => ExportBlockData() },
                { "import", x => TryImportBlockData() },
                { "checkType", x => ExceptionHandler.SendChatMessage($"Block Type: {(PropertiesMenu.Target?.SubtypeId.ToString() ?? "No Target")}") },
                { "toggleBoundingBox", x => PropertiesMenu.DrawBoundingBox = !PropertiesMenu.DrawBoundingBox },
                { "targetBench", TargetBench, 1 },
                { "getTarget", x => GetTarget() },
                { "echo", x => ExceptionHandler.SendChatMessage($"echo: {x[0]}") },
            });
        }

        private static void UpdateBind(string bindName, string[] controls)
        {
            IBind bind = BvBinds.OpenGroup.GetBind(bindName);

            if (bind == null)
                bind = BvBinds.MainGroup.GetBind(bindName);

            if (bind == null)
                ExceptionHandler.SendChatMessage("Error: The bind specified could not be found.");
            else
                bind.TrySetCombo(controls, true, false);
        }

        private void TryImportBlockData()
        {
            LocalFileIO blockIO = new LocalFileIO($"{PropertiesMenu.Target?.TypeID}.bin");
            byte[] byteData;

            if (blockIO.FileExists && blockIO.TryRead(out byteData) == null)
            {
                BlockData data;

                if (Utils.ProtoBuf.TryDeserialize(byteData, out data) == null)
                    PropertiesMenu.Target.ImportSettings(data);
            }
        }

        private void ExportBlockData()
        {
            LocalFileIO blockIO = new LocalFileIO($"{PropertiesMenu.Target?.TypeID}.bin");
            byte[] byteData;

            if (Utils.ProtoBuf.TrySerialize(PropertiesMenu.Target?.ExportSettings(), out byteData) == null)
                blockIO.TryWrite(byteData);
        }

        private void TargetBench(string[] args)
        {
            IMyTerminalBlock tblock;

            if (PropertiesMenu.TryGetTargetedBlock(100d, out tblock))
            {
                int iterations;
                bool getProperties = false;

                int.TryParse(args[0], out iterations);

                if (args.Length > 1)
                    bool.TryParse(args[1], out getProperties);

                Stopwatch timer = new Stopwatch();
                timer.Start();

                var grid = new TerminalGrid();
                var pBlock = new PropertyBlock();

                grid.SetGrid(tblock.CubeGrid);

                for (int n = 0; n < iterations; n++)
                {
                    IMyTerminalBlock temp;
                    PropertiesMenu.TryGetTargetedBlock(100d, out temp);
                    pBlock.SetBlock(grid, tblock);

                    if (getProperties)
                        pBlock.GetEnabledElementCount();
                }

                timer.Stop();
                ExceptionHandler.SendChatMessage
                (
                    $"Target Bench:\n" +
                    $"\tGetProperties: {getProperties}\n" +
                    $"\tTime: {(timer.ElapsedTicks / (double)TimeSpan.TicksPerMillisecond):G6} ms\n" +
                    $"\tIsHighResolution: {Stopwatch.IsHighResolution}\n" +
                    $"\tIterations: {iterations}"
                );
            }
            else
                ExceptionHandler.SendChatMessage($"Cant start target bench. No target found.");
        }

        private void GetTarget()
        {
            IMyTerminalBlock tblock;
            
            if (PropertiesMenu.TryGetTargetedBlock(100d, out tblock))
            {
                ExceptionHandler.SendChatMessage($"Target: {tblock.GetType()}\nAccess: {LocalPlayer.GetBlockAccessPermissions(tblock)}");
            }
            else
                ExceptionHandler.SendChatMessage($"Error: No target found.");
        }

        private static void Crash()
        {
            throw new Exception($"Crash chat command was called.");
        }
    }
}