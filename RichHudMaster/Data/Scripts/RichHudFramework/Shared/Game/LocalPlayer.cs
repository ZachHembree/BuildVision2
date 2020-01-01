using Sandbox.Definitions;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.ModAPI;
using VRageMath;
using System;
using CollisionLayers = Sandbox.Engine.Physics.MyPhysics.CollisionLayers;

namespace RichHudFramework.Game
{
    /// <summary>
    /// Wrapper for various local player related fields and methods.
    /// </summary>
    public static partial class LocalPlayer
    {
        public static IMyCharacter PlyEnt { get { return MyAPIGateway.Session.ControlledObject as IMyCharacter; } }
        public static MyObjectBuilder_Character CharEnt { get { return PlyEnt?.GetObjectBuilder() as MyObjectBuilder_Character; } }
        public static MyCubeBlockDefinition CurrentBuilderBlock { get { return MyCubeBuilder.Static?.CubeBuilderState?.CurrentBlockDefinition; } }
        public static MatrixD HeadTransform { get { return PlyEnt != null ? PlyEnt.GetHeadMatrix(true) : MatrixD.Zero; } }
        public static Vector3D Position { get { return PlyEnt != null ? PlyEnt.GetPosition() : Vector3D.Zero; } }
        public static bool HasEmptyHands { get { return CharEnt?.HandWeapon == null; } }

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
        public static bool TryGetTargetedBlock(double maxDist, out IMyCubeBlock fatBlock, int rayLimit = 1)
        {
            IMyCubeGrid grid;
            Vector3D hitPos, headPos = HeadTransform.Translation, forward = HeadTransform.Forward;
            LineD line = new LineD(HeadTransform.Translation, headPos + forward * maxDist);

            double dist;
            int count = 0;
            fatBlock = null;

            while (TryGetTargetedGrid(line, out grid, out hitPos) && count < rayLimit)
            {
                IMySlimBlock slimBlock;
                grid.GetLineIntersectionExactAll(ref line, out dist, out slimBlock);

                if (slimBlock?.FatBlock != null)
                {
                    if (fatBlock == null)
                        fatBlock = slimBlock.FatBlock;

                    BoundingBoxD bb;
                    slimBlock.GetWorldBoundingBox(out bb);

                    if (bb.Distance(hitPos) <= 0d)
                    {
                        if (slimBlock?.FatBlock != null)
                            fatBlock = slimBlock.FatBlock;

                        break;
                    }
                }

                line.From = hitPos;
                count++;
            }

            return fatBlock != null;
        }

        /// <summary>
        /// Tries to find a targeted grid within a given distance.
        /// </summary>
        public static bool TryGetTargetedGrid(LineD line, out IMyCubeGrid grid, out Vector3D hitPos)
        {
            IHitInfo rayInfo;
            grid = null;
            hitPos = new Vector3D();

            if (PlyEnt != null)
            {
                MyAPIGateway.Physics.CastRay(line.From, line.To, out rayInfo);

                if (rayInfo != null)
                {
                    grid = rayInfo.HitEntity as IMyCubeGrid;
                    hitPos = rayInfo.Position;
                }
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

                MyAPIGateway.Physics.CastRay(start, end, out rayInfo);
                grid = rayInfo?.HitEntity as IMyCubeGrid;
                hitPos = rayInfo.Position;
            }
            else
            {
                grid = null;
                hitPos = new Vector3D();
            }

            return grid != null;
        }
    }
}
