using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TerrariaWorld.Game
{
    public class Chest
    {
        public static int MAXITEMS = 20;

        public Chest()
        {
            this._Items = new Item[Chest.MAXITEMS];
        }

        public Common.Point Location { get; set; }

        private Item[] _Items;
        public Item[] Items
        {
            get { return this._Items; }
        }

        public override string ToString()
        {
            return String.Format("[Chest: {0}]", this.Location);
        }
    }
}
