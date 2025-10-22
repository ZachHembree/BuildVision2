using RichHudFramework;
using RichHudFramework.UI.Rendering;
using VRage.ModAPI;
using VRageMath;

namespace DarkHelmet.BuildVision2
{
    /// <summary>
    /// Rectangular prisim made of billboards used to draw the bounding box of <see cref="IMyEntity"/> objects.
    /// </summary>
    public class BoundingBoard : BlockBoard
    {
        public BoundingBoard()
        {
            Front.Color = Color.Blue.SetAlphaPct(0.7f);
            Back.Color = Color.LightBlue.SetAlphaPct(0.7f);
            Top.Color = Color.Red.SetAlphaPct(0.7f);
            Bottom.Color = Color.Orange.SetAlphaPct(0.7f);
            Left.Color = Color.Green.SetAlphaPct(0.7f);
            Right.Color = Color.DarkOliveGreen.SetAlphaPct(0.7f);
        }

        public void Draw(IMyEntity targetEntity)
        {
            if (targetEntity != null)
            {
                MatrixD matrix = targetEntity.WorldMatrix;
                matrix.Translation = targetEntity.WorldAABB.Center;

                Vector3 dim = Vector3.Max(targetEntity.LocalAABB.Size, new Vector3(0.25f));
                Size = dim * 1.05f;

                Draw(ref matrix);
            }
        }
    }
}