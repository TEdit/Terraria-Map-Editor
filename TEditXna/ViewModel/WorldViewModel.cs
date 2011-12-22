using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using BCCL.MvvmLight;
using BCCL.MvvmLight.Command;
using Microsoft.Win32;
using Microsoft.Xna.Framework;
using TEditXNA.Terraria;

namespace TEditXna.ViewModel
{
    public class WorldViewModel : ViewModelBase
    {
        public WorldViewModel()
        {
            World.ProgressChanged += OnProgressChanged;
        }

        private void OnProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            BCCL.MvvmLight.Threading.DispatcherHelper.CheckBeginInvokeOnUI(() => Progress = e);
        }

        private ProgressChangedEventArgs _progress;
        private World _currentWorld;


        public World CurrentWorld
        {
            get { return _currentWorld; }
            set { Set("CurrentWorld", ref _currentWorld, value); }
        }

        public ProgressChangedEventArgs Progress
        {
            get { return _progress; }
            set { Set("Progress", ref _progress, value); }
        }

        private ICommand _commandOpenWorld;


        public ICommand CommandOpenWorld
        {
            get { return _commandOpenWorld ?? (_commandOpenWorld = new RelayCommand(OpenWorld, CanOpenWorld)); }
        }

        private bool CanOpenWorld()
        {
            return true;
        }

        private void OpenWorld()
        {
            var ofd = new OpenFileDialog();
            ofd.Filter = "Terrarial World File|*.wld|Terraria World Backup|*.bak|TEdit Backup File|*.TEdit";
            ofd.DefaultExt = "Terrarial World File|*.wld";
            ofd.Title = "Load Terraria World File";
            ofd.InitialDirectory = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"My Games\Terraria\Worlds");
            ofd.Multiselect = false;
            if ((bool)ofd.ShowDialog())
            {
                LoadWorld(ofd.FileName);
            }
        }

        private void LoadWorld(string filename)
        {
            Task.Factory.StartNew(() => World.LoadWorld(filename))
                .ContinueWith(t => this.CurrentWorld = t.Result, BCCL.MvvmLight.Threading.TaskFactoryHelper.UiTaskScheduler)
                .ContinueWith(t => RenderEntireWorld())
                .ContinueWith(t => this.PixelMap = t.Result, BCCL.MvvmLight.Threading.TaskFactoryHelper.UiTaskScheduler);
        }

        private Color[] _pixelMap;


        public Color[] PixelMap
        {
            get { return _pixelMap; }
            set { Set("PixelMap", ref _pixelMap, value); }
        }
        private Color[] RenderEntireWorld()
        {
            Color[] pixels = new Color[0];
            if (CurrentWorld != null)
            {
                pixels = new Color[CurrentWorld.TilesWide * CurrentWorld.TilesHigh];

                for (int y = 0; y < CurrentWorld.TilesHigh; y++)
                {
                    OnProgressChanged(this, new ProgressChangedEventArgs(BCCL.Utility.Calc.ProgressPercentage(y, CurrentWorld.TilesHigh), "Calculating Colors..."));
                    for (int x = 0; x < CurrentWorld.TilesWide; x++)
                    {
                        pixels[x + y * CurrentWorld.TilesWide] = Render.PixelMap.GetTileColor(CurrentWorld.Tiles[x, y]);
                    }
                } 
            }
            return pixels;
        }

    }
}