using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TEditXna.ViewModel
{
    public class Vector2EventArgs : EventArgs
    {
        private readonly Vector2 _v;

        public Vector2EventArgs(Vector2 v)
        {
            _v = v;
        }

        public Vector2 Vector2
        {
            get { return _v; }
        }
    }
}
