using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using TEdit.Common;
using TEdit.RenderWorld;

namespace TEdit.Tools
{
    [Export]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class SpritePicker : ObservableObject
    {
        private readonly ObservableCollection<FrameProperty> _sprites = new ObservableCollection<FrameProperty>();
        private FrameProperty _selectedSprite;

        public SpritePicker()
        {
            for (int i = 0; i < byte.MaxValue; i++)
            {
                if (WorldSettings.Tiles[i].IsFramed)
                {
                    //_property.Add(WorldSettings.Tiles[i]); // default
                    foreach (var frame in WorldSettings.Tiles[i].Frames)
                    {
                        _sprites.Add(frame);
                    }
                }
            }
        }

        public ObservableCollection<FrameProperty> Sprites
        {
            get { return _sprites; }
        }

        public FrameProperty SelectedSprite
        {
            get { return _selectedSprite; }
            set { SetProperty(ref _selectedSprite, ref value, "SelectedSprite"); }
        }
    }
}