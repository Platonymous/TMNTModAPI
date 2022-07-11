using System;

namespace ModLoader.Events
{
    public class UpdateTickEventArgs : EventArgs
    {
        public Paris.Paris Game { get; private set; }

        public float DeltaTime { get; private set; }

        public UpdateTickEventArgs(Paris.Paris game, float deltaTime)
        {
            Game = game;
            DeltaTime = deltaTime;
        }
    }
}
