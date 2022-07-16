using Newtonsoft.Json;
using Paris.Engine;
using Paris.Engine.Context;
using System;
using System.IO;
namespace ModLoader.Content
{
    internal class ContentHelper : IContentHelper
    {
        internal ModHelper helper;

        internal ParisContentManager modContent;

        public ContentHelper(ModHelper mod)
        {
            helper = mod;
        }

        public void SaveJson<T>(T data, string filename) where T : class
        {
            string file = Path.Combine(helper.Manifest.Folder, filename);
            var settings = new JsonSerializerSettings() { Formatting = Formatting.Indented };
            File.WriteAllText(file, JsonConvert.SerializeObject(data, settings));
        }

            public T LoadJson<T>(string filename, T fallback = null, bool createFileIfMissing = false) where T : class
        {
            string file = Path.Combine(helper.Manifest.Folder, filename);
            JsonSerializer serializer = new JsonSerializer();
            var settings = new JsonSerializerSettings() { Formatting = Formatting.Indented };
            T result = null;
            try
            {
                if (File.Exists(file))
                    using (TextReader textreader = new StreamReader(File.OpenRead(file)))
                    using (JsonReader reader = new JsonTextReader(textreader))
                        result = serializer.Deserialize<T>(reader);
                else
                    result = fallback;

                if (createFileIfMissing && result is T)
                    SaveJson(result, filename);
            }
            catch (Exception ex)
            {
                helper.Console.Error(ex.Message);
                helper.Console.Trace(ex.StackTrace);
            }

            return result;
        }

        public T LoadContent<T>(string assetName, bool fromModFolder = true)
        {
            if (fromModFolder)
            {
                if (modContent == null)
                    if (helper.Manifest is ModManifest m && m.IsModApi)
                        modContent = new ParisContentManager(ContextManager.Singleton.Services, TMNTModApi.MODCONTENT);
                    else
                        modContent = new ParisContentManager(ContextManager.Singleton.Services, helper.Manifest.Folder);

                return modContent.Load<T>(assetName);
            }

            try
            {
                return ContextManager.Singleton.LoadContent<T>(assetName);
            }
            catch (Exception ex)
            {
                helper.Console.Error(ex.Message);
                helper.Console.Trace(ex.StackTrace);
            }

            return default(T);
        }
    }
}
