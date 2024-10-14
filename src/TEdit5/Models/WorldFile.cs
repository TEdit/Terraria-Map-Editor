using ReactiveUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TEdit.Terraria;

namespace TEdit5.Models
{
    public class WorldFile : ReactiveObject
    {
        /// <summary>
        /// World Name
        /// </summary>
        [Reactive] public string Name { get; set; }

        /// <summary>
        /// FileInfo
        /// </summary>
        [Reactive] public FileInfo FileInfo { get; set; }

        /// <summary>
        /// World Headers
        /// </summary>
        [Reactive] public World world { get; set; }
    }
}
