//-----------------------------------------------------------------------------
// GameTimer.cs
//
// Copyright 2011, BinaryConstruct.
// Licensed under the terms of the Ms-PL: http://www.microsoft.com/opensource/licenses.mspx#Ms-PL
//-----------------------------------------------------------------------------

using System;
using System.Diagnostics;

namespace TEdit.UI.Xaml.XnaContentHost
{
    public class GameTimer : Stopwatch
    {
        public void Update()
        {
            TimeSpan mark = Elapsed;
            _elapsedGameTime = mark - _lastUpdate;
            _lastUpdate = mark;
        }

        private TimeSpan _lastUpdate = TimeSpan.FromSeconds(0);
        private TimeSpan _elapsedGameTime = TimeSpan.FromSeconds(0);

        /// <summary>
        /// The amount of elapsed game time since the last update.
        /// </summary>
        public TimeSpan ElapsedGameTime
        {
            get { return _elapsedGameTime; }
        }

        /// <summary>
        /// The amount of game time since between the start of the game and the last time Update was called.
        /// </summary>
        public TimeSpan TotalGameTimeLastUpdate
        {
            get { return _lastUpdate; }
        }


        /// <summary>
        /// The amount of game time since the start of the game.
        /// </summary>
        public TimeSpan TotalGameTime
        {
            get { return base.Elapsed; }
        }

        /// <summary>
        /// Hidden....
        /// </summary>
        private new TimeSpan Elapsed
        {
            get { return base.Elapsed; }
        }

    }
}