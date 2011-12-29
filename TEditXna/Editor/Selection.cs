using System;
using BCCL.Geometry.Primitives;
using BCCL.MvvmLight;
using Microsoft.Xna.Framework;

namespace TEditXna.Editor
{
    public class Selection : ObservableObject
    {
        private Rectangle _selectionArea = new Rectangle(0, 0, 0, 0);
        private bool _isActive;

        public bool IsActive
        {
            get { return _isActive; }
            set { Set("IsActive", ref _isActive, value); }
        }

        public Rectangle SelectionArea
        {
            get { return _selectionArea; }
            set { Set("SelectionArea", ref _selectionArea, value); }
        }

        public void SetRectangle(Vector2Int32 p1, Vector2Int32 p2)
        {
            int x1 = p1.X < p2.X ? p1.X : p2.X;
            int y1 = p1.Y < p2.Y ? p1.Y : p2.Y;
            int width = Math.Abs(p2.X - p1.X) + 1;
            int height = Math.Abs(p2.Y - p1.Y) + 1;

            SelectionArea = new Rectangle(x1, y1, width, height);
            IsActive = true;
        }
    }
}