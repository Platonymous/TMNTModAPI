using System;

namespace ModLoader.Events
{
    public interface IEventHelper
    {
        event EventHandler<GameInitializedEventArgs> GameInitialized;

        event EventHandler<RequestingAssetEventArgs> RequestingAsset;

        event EventHandler<AssetLoadedEventArgs> AssetLoaded;

        event EventHandler<UpdateTickEventArgs> UpdateTick;

        event EventHandler<UpdateTickEventArgs> UpdateTicked;

        event EventHandler<DrawEventArgs> BeforeDraw;

        event EventHandler<DrawEventArgs> AfterDraw;

        event EventHandler<ContextSwitchedEventArgs> ContextSwitched;
    }
}
