using System;
using System.ComponentModel.Composition;
using System.Windows;
using TEdit.Common;
using TEdit.Common.Structures;

namespace TEdit.Tools
{
    [Export]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class SelectionArea : ObservableObject
    {
        private Int32Rect _Rectangle;
        private Visibility _selectionVisibility;

        public SelectionArea()
        {
            Rectangle = new Int32Rect();
            Deactive();
        }

        public int SelectedArea
        {
            get { return Rectangle.Width*Rectangle.Height; }
        }

        public Visibility SelectionVisibility
        {
            get { return _selectionVisibility; }
            set
            {
                if (_selectionVisibility != value)
                {
                    _selectionVisibility = value;
                    RaisePropertyChanged("SelectionVisibility");
                }
            }
        }

        public Int32Rect Rectangle
        {
            get { return _Rectangle; }
            set
            {
                if (_Rectangle != value)
                {
                    _Rectangle = value;
                    RaisePropertyChanged("Rectangle");
                }
            }
        }

        public bool IsValid(PointInt32 point)
        {
            if (_selectionVisibility != Visibility.Visible)
                return true;

            return (Rectangle.Contains(point));
        }

        public void SetRectangle(PointInt32 p1, PointInt32 p2)
        {
            int x = p1.X < p2.X ? p1.X : p2.X;
            int y = p1.Y < p2.Y ? p1.Y : p2.Y;
            int width = Math.Abs(p2.X - p1.X) + 1;
            int height = Math.Abs(p2.Y - p1.Y) + 1;
            Rectangle = new Int32Rect(x, y, width, height);
            SelectionVisibility = Visibility.Visible;
        }

        public void Deactive()
        {
            SelectionVisibility = Visibility.Collapsed;
        }
    }
}