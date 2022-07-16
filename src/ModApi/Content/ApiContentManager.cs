using ModLoader.Events;
using Paris.Engine;
using System;
using System.IO;
using System.Linq;

namespace ModLoader.Content
{
    internal class ApiContentManager : ModContentManager
    {
        public ApiContentManager(IServiceProvider i_service, string i_sRoot) : base(i_service, i_sRoot)
        {
        }

        public override T Load<T>(string i_assetName)
        {
            if (EventManager.TriggerRequestingAssetEvent(i_assetName, typeof(T)) is T result)
                return result;

            DirectoryInfo modInfo = new DirectoryInfo(TMNTModApi.MODCONTENT);
            DirectoryInfo contentInfo = new DirectoryInfo("Content");
            var orgFolder = contentInfo.EnumerateDirectories().Select(d => d.Name).ToList();

            if (typeof(T) == typeof(AudioContent))
            {
                if (TMNTModApi.Singleton.modHelper.Content.LoadContent<AudioContent>(i_assetName) is T ac)
                    return ac;

                foreach (var directory in modInfo.EnumerateDirectories())
                    if (!orgFolder.Contains(directory.Name) && TMNTModApi.Singleton.modHelper.Content.LoadContent<T>(Path.Combine(directory.Name, "Content", i_assetName)) is T mAsset)
                        return mAsset;
            }
            else
            {
                if (DoesFileExist(TMNTModApi.MODCONTENT, i_assetName) && TMNTModApi.Singleton.modHelper.Content.LoadContent<T>(i_assetName) is T asset)
                    return asset;

                foreach (var directory in modInfo.EnumerateDirectories())
                    if (!orgFolder.Contains(directory.Name) && DoesFileExist(Path.Combine(TMNTModApi.MODCONTENT, directory.Name, "Content"), i_assetName))
                        if (TMNTModApi.Singleton.modHelper.Content.LoadContent<T>(Path.Combine(directory.Name, "Content", i_assetName)) is T mAsset)
                            return mAsset;

                foreach (var pack in TMNTModApi.Singleton.modHelper.GetContentPacks())
                    if (DoesFileExist(pack.Manifest.Folder, i_assetName) && pack.Content.LoadContent<T>(i_assetName) is T packasset)
                        return packasset;
            }

            T loaded = base.Load<T>(i_assetName);

            return EventManager.TriggerAssetLoadedEvent(i_assetName, loaded);
        }

        public bool DoesFileExist(string folder, string assetName)
        {
            string directory = Path.GetDirectoryName(Path.Combine(folder, assetName));
            if (!Directory.Exists(directory))
                return false;
            DirectoryInfo root = new DirectoryInfo(directory);
            FileInfo[] listfiles = root.GetFiles(Path.GetFileNameWithoutExtension(assetName) + ".*");
            return listfiles.Length > 0;
        }
    }
}
