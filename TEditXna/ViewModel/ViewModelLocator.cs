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
                }
                return _worldViewModel;
            }

        }
    }
}