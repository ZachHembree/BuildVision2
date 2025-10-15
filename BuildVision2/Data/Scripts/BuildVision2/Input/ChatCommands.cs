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
                { "bind", x => UpdateBind(x[0], x.GetSubarray(1)), 2 },
                { "resetBinds", x => BvBinds.Cfg = BindsConfig.Defaults },
                { "save", x => BvConfig.SaveStart() },
                { "load", x => BvConfig.LoadStart() },
                { "resetConfig", x => BvConfig.ResetConfig() },

                // Debug/Testing
                { "open", x => QuickActionHudSpace.TryOpenMenu() },
                { "close", x => QuickActionHudSpace.CloseMenu() },
                { "reload", x => ExceptionHandler.ReloadClients() },
                { "crash", x => Crash() },
                { "printControlsToLog", x => LogIO.WriteToLogStart($"Control List:\n{HelpText.controlList}") },
                { "export", x => ExportBlockData() },
                { "import", x => TryImportBlockData() },
                { "checkType", x => ExceptionHandler.SendChatMessage($"Block Type: {(QuickActionHudSpace.Target?.SubtypeId.ToString() ?? "No Target")}") },
                { "toggleDebug", x => { QuickActionMenu.DrawDebug = !QuickActionMenu.DrawDebug; ExceptionHandler.DebugLogging = QuickActionMenu.DrawDebug; } },
                { "toggleVisDbg", x => PropertyBlock.DebugVisibility = !PropertyBlock.DebugVisibility },
                { "toggleTargetVis", x => QuickActionHudSpace.EnableTargetDebugVis = !QuickActionHudSpace.EnableTargetDebugVis },
                { "targetBench", TargetBench, 1 },
                { "getTarget", x => GetTarget() },
            });
        }

        private static void UpdateBind(string bindName, string[] controls)
        {
            IBind bind = BvBinds.ModifierGroup.GetBind(bindName);

            if (bind == null)
                bind = BvBinds.MainGroup.GetBind(bindName);

            if (bind == null)
                ExceptionHandler.SendChatMessage("Error: The bind specified could not be found.");
            else
                bind.TrySetCombo(controls, 0, true, false);
        }

        private void TryImportBlockData()
        {
            LocalFileIO blockIO = new LocalFileIO($"{QuickActionHudSpace.Target?.TypeID}.bin");
            byte[] byteData;

            if (blockIO.FileExists && blockIO.TryRead(out byteData) == null)
            {
                BlockData data;

                if (Utils.ProtoBuf.TryDeserialize(byteData, out data) == null)
                    QuickActionHudSpace.Target.ImportSettings(data);
            }
        }

        private void ExportBlockData()
        {
            BlockData blockData = default(BlockData);
            blockData.propertyList = new List<PropertyData>();
            QuickActionHudSpace.Target?.ExportSettings(ref blockData);

            LocalFileIO blockIO = new LocalFileIO($"{QuickActionHudSpace.Target?.TypeID}.bin");
            byte[] byteData;

            if (Utils.ProtoBuf.TrySerialize(blockData, out byteData) == null)
                blockIO.TryWrite(byteData);
        }

        private void TargetBench(string[] args)
        {
            IMyTerminalBlock tblock;

            if (QuickActionHudSpace.TryGetTargetedBlock(100d, out tblock))
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
                    QuickActionHudSpace.TryGetTargetedBlock(100d, out temp);
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
            
            if (QuickActionHudSpace.TryGetTargetedBlock(100d, out tblock))
            {
                Type[] interfaces = MyAPIGateway.Reflection.GetInterfaces(tblock.GetType());
                StringBuilder sb = new StringBuilder();
                
                sb.Append(
                    $"Target: {tblock.CustomName} ({tblock.GetType()})\n" +
                    $"Access: {tblock.GetAccessPermissions()}\n" +
                    $"Interfaces ({interfaces.Length}):\n");

                foreach (Type subtype in interfaces)
                    sb.AppendLine(subtype.Name);

                sb.AppendLine("Components Types:");
                
                foreach (var component in tblock.Components)
                    sb.AppendLine($"{component.GetType().Name}");

                sb.AppendLine("Upgrades:");

                foreach (var pair in tblock.UpgradeValues)
                    sb.AppendLine($"{pair.Key}: {pair.Value}");

                ExceptionHandler.SendChatMessage(sb.ToString());
                ExceptionHandler.WriteToLogAndConsole(sb.ToString());
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