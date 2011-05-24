using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TerrariaWorld.Game
{
    public class Sign
    {
        public string Text { get; set; }

        public Common.Point Location { get; set; }

        public override string ToString()
        {
            return String.Format("[Sign: {0}, {1}]", this.Text, this.Location);
        }
    }
}
