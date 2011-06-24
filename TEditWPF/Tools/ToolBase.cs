// -----------------------------------------------------------------------
// <copyright file="ToolBase.cs" company="Microsoft">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using System.Windows.Media.Imaging;

namespace TEditWPF.Tools
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.ComponentModel;
    using TEditWPF.Common;
    using System.Diagnostics;
    using System.Linq.Expressions;
    using TEditWPF.TerrariaWorld.Structures;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public abstract class ToolBase : ITool, INotifyPropertyChanged
    {
        #region INotifyPropertyChanged

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

        protected void RaisePropertyChanged<T>(Expression<Func<T>> propertyExpresssion)
        {
            var propertyName = PropertySupport.ExtractPropertyName(propertyExpresssion);
            this.RaisePropertyChanged(propertyName);
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

        public abstract string Name
        {
            get;
        }


        public abstract System.Windows.Media.Imaging.BitmapImage Image
        {
            get;
        }


        public abstract ToolType Type
        {
            get;
        }

        public abstract bool IsActive { get; set; }

        public abstract bool PressTool(TileMouseEventArgs e);
        public abstract bool MoveTool(TileMouseEventArgs e);
        public abstract bool ReleaseTool(TileMouseEventArgs e);
        public abstract WriteableBitmap PreviewTool();
    }
}
