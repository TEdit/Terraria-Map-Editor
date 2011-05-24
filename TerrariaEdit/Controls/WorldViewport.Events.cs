using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.ComponentModel;

namespace TerrariaMapEditor.Controls
{
    partial class WorldViewport
    {

        public event EventHandler ZoomChanged;
        protected virtual void OnZoomChanged(object sender, EventArgs e)
        {
            if (ZoomChanged != null)
                ZoomChanged(sender, e);
        }

        public event PaintEventHandler DrawToolOverlay;
        protected virtual void OnDrawToolOverlay(object sender, PaintEventArgs e)
        {
            if (DrawToolOverlay != null)
                DrawToolOverlay(sender, e);
        }


        public event MouseEventHandler MouseDownTile;
        protected virtual void OnMouseDownTile(object sender, MouseEventArgs e)
        {
            if (MouseDownTile != null)
                MouseDownTile(sender, e);

        }

        public event MouseEventHandler MouseMoveTile;
        protected virtual void OnMouseMoveTile(object sender, MouseEventArgs e)
        {
            if (MouseMoveTile != null)
                MouseMoveTile(sender, e);

        }

        public event MouseEventHandler MouseUpTile;
        protected virtual void OnMouseUpTile(object sender, MouseEventArgs e)
        {
            if (MouseUpTile != null)
                MouseUpTile(sender, e);

        }

        #region Property Change Methods and Events

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            var handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected void RaisePropertyChanged(String propertyName)
        {
            VerifyPropertyName(propertyName);
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Warns the developer if this Object does not have a public property with
        /// the specified name. This method does not exist in a Release build.
        /// </summary>
        [Conditional("DEBUG")]
        [DebuggerStepThrough]
        public void VerifyPropertyName(String propertyName)
        {
            // verify that the property name matches a real,  
            // public, instance property on this Object.
            if (TypeDescriptor.GetProperties(this)[propertyName] == null)
            {
                Debug.Fail("Invalid property name: " + propertyName);
            }
        }

        #endregion
    }
}
