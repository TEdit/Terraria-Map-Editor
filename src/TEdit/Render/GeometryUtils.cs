namespace TEdit.Render;

public static class GeometryUtils
{
    public static Microsoft.Xna.Framework.Rectangle Frame(
         this Microsoft.Xna.Framework.Graphics.Texture2D tex,
         int horizontalFrames = 1,
         int verticalFrames = 1,
         int frameX = 0,
         int frameY = 0,
         int sizeOffsetX = 0,
         int sizeOffsetY = 0)
    {
        int num1 = tex.Width / horizontalFrames;
        int num2 = tex.Height / verticalFrames;
        return new Microsoft.Xna.Framework.Rectangle(num1 * frameX, num2 * frameY, num1 + sizeOffsetX, num2 + sizeOffsetY);
    }
}
