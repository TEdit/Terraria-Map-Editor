using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TEdit.Terraria;

namespace TEdit.Desktop.Models
{
    public class WorldFile : ObservableObject
    {
        private FileInfo _fileInfo;
        private string _name;
        private World _world;

        /// <summary>
        /// World Name
        /// </summary>
        public string Name
        {
            get => _name;
            set => this.SetProperty(ref _name, value);
        }

        /// <summary>
        /// FileInfo
        /// </summary>
        public FileInfo FileInfo
        {
            get => _fileInfo;
            set => this.SetProperty(ref _fileInfo, value);
        }

        /// <summary>
        /// World Headers
        /// </summary>
        public World world
        {
            get => _world;
            set => this.SetProperty(ref _world, value);
        }
    }
}
