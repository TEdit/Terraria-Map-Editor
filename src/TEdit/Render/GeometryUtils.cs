/* 
Copyright (c) 2011 BinaryConstruct
 
This source is subject to the Microsoft Public License.
See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL.
All other rights reserved.

THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED 
WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
 */

using Microsoft.Xna.Framework.Graphics;

namespace TEdit.Render
{
    public static class GeometryUtils
    {
        public static Microsoft.Xna.Framework.Rectangle Frame(
             this Texture2D tex,
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
}
