using System;

namespace ModLoader.Events
{
    public class AssetLoadedEventArgs : EventArgs
    {
        public string AssetName { get; private set; }
        public object Asset { get; private set; }

        internal Action<object> setAssetAction;

        public AssetLoadedEventArgs(string assetName, object asset, Action<object> setAsset)
        {
            AssetName = assetName;
            Asset = asset;
            setAssetAction = setAsset;
        }

        public void SetAsset(object asset)
        {
            setAssetAction(asset);
        }

    }
}
