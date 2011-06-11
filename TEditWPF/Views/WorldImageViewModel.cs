using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TEditWPF.Common;
using System.ComponentModel.Composition;

namespace TEditWPF.Views
{
    [Export]
    public class WorldImageViewModel : ObservableObject
    {
        public WorldImageViewModel()
        {
            //this._bmp = new WriteableBitmap
        }

        WriteableBitmap _bmp;
    }
}
