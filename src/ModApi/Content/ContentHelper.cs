using HarmonyLib;
using Newtonsoft.Json;
using Paris.Engine;
using Paris.Engine.Audio;
using Paris.Engine.Context;
using Paris.Engine.Types;
using System;
using System.IO;
using System.Reflection;

namespace ModLoader.Content
{
    internal class ContentHelper : IContentHelper
    {
        internal ModHelper helper;

        internal static bool initialized = false;

        internal ParisContentManager modContent;

        public ContentHelper(ModHelper mod)
        {
            helper = mod;
            Init();
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
                        modContent = new ModContentManager(ContextManager.Singleton.Services, TMNTModApi.MODCONTENT);
                    else
                        modContent = new ModContentManager(ContextManager.Singleton.Services, helper.Manifest.Folder);

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

        public static void Init()
        {
            if (initialized)
                return;

            Harmony harmony = new Harmony("TMNTModApi.ContentHelper");
            harmony.Patch(
                  original: typeof(SFX).GetConstructor(new Type[] { typeof(string), typeof(byte[]), typeof(float), typeof(int), typeof(SoundLoopType), typeof(SFXChannel[]), typeof(SoundCutOffType), typeof(Range), typeof(bool), typeof(float) }),
                  prefix: new HarmonyMethod(typeof(ContentHelper).GetMethod(nameof(ConstructSFX), BindingFlags.NonPublic | BindingFlags.Static))
                  );


            initialized = true;
        }

        internal static void ConstructSFX(string assetName, ref byte[] data)
        {
            if (ContextManager.Singleton.LoadContent<AudioContent>(Path.Combine("Audio",assetName)) is AudioContent a)
                data = a.Data;
        }
    }
}
