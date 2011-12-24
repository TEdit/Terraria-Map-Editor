using System.Collections.Generic;
using System.Collections.ObjectModel;
using TEditXna.Editor.Tools;

namespace TEditXna.ViewModel
{
    public static class ViewModelLocator
    {
        private static WorldViewModel _worldViewModel;
        public static WorldViewModel WorldViewModel
        {
            get
            {
                if (_worldViewModel == null)
                {
                    _worldViewModel = new WorldViewModel();
                    _worldViewModel.Tools.Add(new ArrowTool());
                }
                return _worldViewModel;
            }
        }
    }
}