using ModLoader.Events;
using Paris.Engine;
using System;
using System.IO;
using System.Linq;

namespace ModLoader.Content
{
    internal class ModContentManager : ParisContentManager
    {
        public ModContentManager(IServiceProvider i_service, string i_sRoot) : base(i_service, i_sRoot)
        {
        }

        public override T Load<T>(string i_assetName)
        {
            if (typeof(T) == typeof(AudioContent))
                if (Path.Combine(RootDirectory, i_assetName + ".wav") is String file && File.Exists(file))
                    return (T)(object)new AudioContent(i_assetName, File.ReadAllBytes(file));
                else
                    return (T)(object)null;

            T loaded = base.Load<T>(i_assetName);

            return loaded;
        }
    }
}
