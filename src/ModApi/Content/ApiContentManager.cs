using ModLoader.Events;
using Paris.Engine;
using System;

namespace ModLoader.Content
{
    internal class ApiContentManager : ParisContentManager
    {
        public ApiContentManager(IServiceProvider i_service, string i_sRoot) : base(i_service, i_sRoot)
        {
        }

        public override T Load<T>(string i_assetName)
        {
            if(EventManager.TriggerRequestingAssetEvent(i_assetName, typeof(T)) is T result)
                return result;

            T loaded = base.Load<T>(i_assetName);

            return EventManager.TriggerAssetLoadedEvent(i_assetName, loaded);
        }
    }
}
