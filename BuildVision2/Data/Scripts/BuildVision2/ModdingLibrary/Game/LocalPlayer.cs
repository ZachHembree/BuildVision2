using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.ModAPI;
using VRageMath;

namespace DarkHelmet.Game
{
    /// <summary>
    /// Wrapper for various local player related fields and methods.
    /// </summary>
    public static class LocalPlayer
    {
        public static IMyCharacter LocalPly { get { return MyAPIGateway.Session.ControlledObject as IMyCharacter; } }
        public static MyObjectBuilder_Character CharEnt { get { return LocalPly?.GetObjectBuilder() as MyObjectBuilder_Character; } }
        public static MatrixD HeadTransform { get { return LocalPly != null ? LocalPly.GetHeadMatrix(true) : MatrixD.Zero; } }
        public static Vector3D Position { get { return LocalPly != null ? LocalPly.GetPosition() : Vector3D.Zero; } }
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
        /// Tries to retrieve targeted fat block on a grid within a given distance.
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

            if (LocalPly != null)
            {
                MyAPIGateway.Physics.CastRay(line.From, line.To, out rayInfo);
                grid = rayInfo?.HitEntity as IMyCubeGrid;
            }
            else
            {
                rayInfo = null;
                grid = null;
            }

            return grid != null;
        }

        /// <summary>
        /// Tries to find a targeted grid within a given distance.
        /// </summary>
        public static bool TryGetTargetedGrid(double maxDist, out IMyCubeGrid grid)
        {
            Vector3D start, end;
            IHitInfo rayInfo;

            if (LocalPly != null)
            {
                start = HeadTransform.Translation;
                end = start + HeadTransform.Forward * maxDist;

                MyAPIGateway.Physics.CastRay(start, end, out rayInfo);
                grid = rayInfo?.HitEntity as IMyCubeGrid;
            }
            else
            {
                rayInfo = null;
                grid = null;
            }

            return grid != null;
        }
    }
}
