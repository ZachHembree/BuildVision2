using RichHudFramework.Internal;
using Sandbox.Definitions;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;
using CollisionLayers = Sandbox.Engine.Physics.MyPhysics.CollisionLayers;

namespace DarkHelmet.BuildVision2
{
    [Flags]
    public enum TerminalPermissionStates : int
    {
        None = 0x0,
        Denied = 0x1,
        Granted = 0x2,

        GridUnowned = 0x4,
        GridUnfriendly = 0x8,
        GridFriendly = 0x10,
        
        BlockUnfriendly = 0x20,
        BlockFriendly = 0x40
    }

    public enum HudState : int
    {
        Hidden = 0,
        Full = 1,
        Minimal = 2
    }

    /// <summary>
    /// Wrapper for various local player related fields and methods.
    /// </summary>
    public static partial class LocalPlayer
    {
        /// <summary>
        /// Returns the local human player
        /// </summary>
        public static IMyPlayer Player { get { return MyAPIGateway.Session.LocalHumanPlayer; } }

        /// <summary>
        /// Returns the currently controlled object as an IMyCharacter
        /// </summary>
        public static IMyCharacter PlyEnt { get { return MyAPIGateway.Session.ControlledObject as IMyCharacter; } }

        /// <summary>
        /// Returns the object builder for PlyEnt, provided PlyEnt isn't null
        /// </summary>
        public static MyObjectBuilder_Character PlyBuilder { get { return PlyEnt?.GetObjectBuilder() as MyObjectBuilder_Character; } }

        /// <summary>
        /// Returns the object builder for the cube block currently selected by the player
        /// </summary>
        public static MyCubeBlockDefinition CurrentBuilderBlock { get { return MyCubeBuilder.Static?.CubeBuilderState?.CurrentBlockDefinition; } }

        /// <summary>
        /// Returns the matrix for the player's head
        /// </summary>
        public static MatrixD HeadTransform { get { return PlyEnt != null ? PlyEnt.GetHeadMatrix(true) : MatrixD.Zero; } }

        /// <summary>
        /// Returns players position in the world
        /// </summary>
        public static Vector3D Position { get { return PlyEnt != null ? PlyEnt.GetPosition() : Vector3D.Zero; } }

        /// <summary>
        /// Returns false if the player is holding a tool or a weapon
        /// </summary>
        public static bool HasEmptyHands { get { return PlyBuilder?.HandWeapon == null; } }

        /// <summary>
        /// Finds where position of a world coordinate lies on the screen's plane (with the Z-coord being distance from that plane).
        /// </summary>
        public static Vector3D GetWorldToScreenPos(Vector3D pos) =>
            MyAPIGateway.Session.Camera.WorldToScreen(ref pos);

        /// <summary>
        /// Determines whether or not the target block is within a 180 degree arc in front of the camera.
        /// </summary>
        public static bool IsLookingInBlockDir(IMyTerminalBlock block)
        {
            if (block != null)
            {
                Vector3D dir = (block.GetPosition() - MyAPIGateway.Session.Camera.Position),
                    forward = MyAPIGateway.Session.Camera.WorldMatrix.Forward;

                return Vector3D.Dot(dir, forward) > 0;
            }
            else
                return false;
        }

        /// <summary>
        /// Tries to retrieve targeted <see cref="IMyCubeBlock"/> on a grid within a given distance.
        /// </summary>
        public static bool TryGetTargetedBlock(double maxDist, out IMyCubeBlock fatBlock)
        {
            IMyCubeGrid grid;
            IMySlimBlock slimBlock;
            LineD line = new LineD(HeadTransform.Translation, HeadTransform.Translation + HeadTransform.Forward * maxDist);
            double dist;
            fatBlock = null;

            if (TryGetTargetedGrid(line, out grid))
            {
                grid.GetLineIntersectionExactAll(ref line, out dist, out slimBlock);
                fatBlock = slimBlock?.FatBlock;
            }

            return fatBlock != null;
        }

        /// <summary>
        /// Tries to find a targeted grid within a given distance.
        /// </summary>
        public static bool TryGetTargetedGrid(LineD line, out IMyCubeGrid grid)
        {
            IHitInfo rayInfo;

            if (PlyEnt != null)
            {
                MyAPIGateway.Physics.CastRay(line.From, line.To, out rayInfo, CollisionLayers.CollisionLayerWithoutCharacter);
                grid = rayInfo?.HitEntity.GetTopMostParent() as IMyCubeGrid;
            }
            else
            {
                grid = null;
            }

            return grid != null;
        }

        /// <summary>
        /// Tries to find a targeted grid within a given distance.
        /// </summary>
        public static bool TryGetTargetedGrid(LineD line, out IMyCubeGrid grid, out IHitInfo rayInfo)
        {
            grid = null;
            rayInfo = null;

            if (PlyEnt != null)
            {
                MyAPIGateway.Physics.CastRay(line.From, line.To, out rayInfo, CollisionLayers.CollisionLayerWithoutCharacter);
                grid = rayInfo?.HitEntity.GetTopMostParent() as IMyCubeGrid;
            }

            return grid != null;
        }

        /// <summary>
        /// Tries to find a targeted grid within a given distance in front of the player's camera.
        /// </summary>
        public static bool TryGetTargetedGrid(double maxDist, out IMyCubeGrid grid, out Vector3D hitPos)
        {
            Vector3D start, end;
            IHitInfo rayInfo;

            if (PlyEnt != null)
            {
                start = HeadTransform.Translation;
                end = start + HeadTransform.Forward * maxDist;

                MyAPIGateway.Physics.CastRay(start, end, out rayInfo, CollisionLayers.CollisionLayerWithoutCharacter);
                grid = rayInfo?.HitEntity.GetTopMostParent() as IMyCubeGrid;
                hitPos = rayInfo.Position;
            }
            else
            {
                grid = null;
                hitPos = new Vector3D();
            }

            return grid != null;
        }

        /// <summary>
        /// Returns true if the local player can access the given terminal block. Blocks without ownership
        /// permissions require the player to have at least neutral relations with the grid owner's faction.
        /// </summary>
        public static TerminalPermissionStates GetBlockAccessPermissions(IMyTerminalBlock block)
        {
            long plyID = Player.IdentityId;
            IMyCubeGrid grid = block.CubeGrid;
            var accessState = TerminalPermissionStates.None;

            // Ensure owners are up to date
            grid.UpdateOwnership(0, false);

            List<long> bigOwners = grid.BigOwners;
            var def = MyDefinitionManager.Static.GetDefinition(block.BlockDefinition) as MyCubeBlockDefinition;

            // Terminal blocks with computers are ownable. If there are no bigOwners, the grid is unowned.
            bool blockOwnable = def?.Components.Any(x => x.Definition.Id.SubtypeName == "Computer") ?? false,
                gridUnowned = bigOwners.Count == 0;

            if (blockOwnable)
            {
                if (block.HasPlayerAccess(plyID))
                    accessState |= TerminalPermissionStates.Granted | TerminalPermissionStates.BlockFriendly;
                else
                    accessState |= TerminalPermissionStates.Denied | TerminalPermissionStates.BlockUnfriendly;
            }
            else if (gridUnowned)
            {
                accessState |= TerminalPermissionStates.Granted | TerminalPermissionStates.GridUnowned;
            }
            else
            {
                bool gridFriendly = bigOwners.Contains(plyID);

                if (!gridFriendly)
                {
                    foreach (long owner in bigOwners)
                    {
                        IMyFaction ownerFaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(owner);

                        if (ownerFaction != null && (ownerFaction.IsFriendly(plyID) || ownerFaction.IsMember(plyID)))
                        {
                            gridFriendly = true;
                            break;
                        }
                    }
                }

                if (gridFriendly)
                    accessState |= TerminalPermissionStates.Granted | TerminalPermissionStates.GridFriendly;
                else
                    accessState |= TerminalPermissionStates.Denied | TerminalPermissionStates.GridUnfriendly;
            }

            return accessState;
        }

        /// <summary>
        /// Returns the current HUD state as configured (hidden/min/full).
        /// </summary>
        public static HudState GetHudState()
        {
            int hudState = MyAPIGateway.Session.Config.HudState;
            return (HudState)hudState;
        }
    }
}
