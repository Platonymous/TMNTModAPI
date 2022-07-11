using System;

namespace ModLoader.Events
{
    public class GameInitializedEventArgs : EventArgs
    {
        public Paris.Paris Game { get; private set; }

        public GameInitializedEventArgs(Paris.Paris game)
        {
            Game = game;
        }
    }
}
