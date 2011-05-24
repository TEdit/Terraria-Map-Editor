using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TerrariaWorld.Game
{
    public class Item
    {
        public Item()
        {
            this.Stack = 0;
            this.Name = "[empty]";
        }

        public int Stack { get; set; }

        public string Name { get; set; }

        public override string ToString()
        {
            if (this.Stack > 0)
                return string.Format("{0}: {1}", this.Name, this.Stack);

            return this.Name;
        }
    }
}
