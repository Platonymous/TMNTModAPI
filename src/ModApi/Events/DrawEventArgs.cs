using Microsoft.Xna.Framework;
using System;

namespace ModLoader.Events
{
    public class DrawEventArgs : EventArgs
    {
        public GameTime Time { get; private set; }

        public DrawEventArgs(GameTime time)
        {
            Time = time;
        }
    }
}
