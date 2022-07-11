using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModLoader.Events
{
    public class RequestingAssetEventArgs : EventArgs
    {
        public string AssetName { get; private set; }

        public Type AssetType { get; private set; }

        internal Action<object> setAssetAction;

        public RequestingAssetEventArgs(string assetName, Type type, Action<object> setAsset)
        {
            AssetName = assetName;
            setAssetAction = setAsset;
            AssetType = type;
        }

        public void SetAsset(object asset)
        {
            setAssetAction(asset);
        }
    }
}
