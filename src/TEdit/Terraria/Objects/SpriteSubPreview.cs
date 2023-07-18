using System.Windows.Media;
using System.Windows.Media.Imaging;
using TEdit.Common.Reactive;
using TEdit.ViewModel;
using TEdit.Editor;
using System.Collections.Generic;
using TEdit.Geometry;
using TEdit.Common;
using TEdit.Render;

namespace TEdit.Terraria.Objects
{
    public class SpriteSubPreview : SpriteSub
    {
        public WriteableBitmap Preview { get; set; } //TODO: move rendering separate from config
    }
}
