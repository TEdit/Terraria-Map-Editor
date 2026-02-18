using System.IO;
using TEdit.Terraria;

namespace TEdit5.Models
{
    public partial class WorldFile : ReactiveObject
    {
        /// <summary>
        /// World Name
        /// </summary>
        [Reactive] public string _name;

        /// <summary>
        /// FileInfo
        /// </summary>
        [Reactive] public FileInfo _fileInfo;

        /// <summary>
        /// World Headers
        /// </summary>
        [Reactive] public World _world;
    }
}
