using HarmonyLib;
using Paris.Engine.Context;
using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.IO;
using Newtonsoft.Json;
using ModLoader.Events;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using ModLoader.Utilities;

namespace ModLoader
{
    public class TMNTModApi
    {
      

        public static TMNTModApi Singleton { get; private set; }

        internal EventManager EventManager { get; private set; }

        internal Harmony harmony;
        internal ModApiConfig config;
        internal ModHelper modHelper;

        const string CONFIG_FILENAME = "0ModApi.json";
        const string MODFOLDER = "Mods";
        const string APIVERSION = "1.1.0";
        internal const string MODCONTENT = "ModContent";

        internal ModVersion ApiVersion = new ModVersion(APIVERSION);


        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool AllocConsole();

        [STAThread]
        public void Main()
        {
            if (!Directory.Exists(MODCONTENT))
                Directory.CreateDirectory(MODCONTENT);

            modHelper = new ModHelper(new ModManifest()
            {
                Author = "Platonymous",
                Name = "Api",
                Id = "0ModApi",
                Folder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
            });


            config = modHelper.Content.LoadJson<ModApiConfig>(CONFIG_FILENAME, new ModApiConfig(), true);

            if (config.Console)
            {
                AllocConsole();
                Console.Title = "TMNT Mod Api " + APIVERSION;
                Thread inputThread = new Thread(() =>
                {
                    while (true)
                    {
                        string input = Console.ReadLine();

                        if (string.IsNullOrWhiteSpace(input))
                            continue;

                        modHelper.Console.Log(input);
                        EventManager.TriggerConsoleEvent(input);
                    }
                });
                inputThread.Start();
            }

            EventManager.Singleton.Init();
            EventManager.Singleton.GameInitialized += Singleton_GameInitialized;

           

            harmony = new Harmony("TMNTModApi.Main");

            if (config.Verbose)
            {
                try
                {
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    modHelper.Console.Warn("Verbose Log active");
                    Console.ForegroundColor = ConsoleColor.White;

                    harmony.Patch(
                        original: typeof(ContextManager).GetMethod(nameof(DisplayMessage), new Type[] { typeof(string), typeof(string), typeof(float) }),
                        postfix: new HarmonyMethod(GetType().GetMethod(nameof(DisplayMessage), BindingFlags.NonPublic | BindingFlags.Static))
                        );
                }
                catch (Exception ex)
                {
                    modHelper.Console.Error(ex.Message);
                    modHelper.Console.Trace(ex.StackTrace);
                }
            }

            modHelper.Console.Announcement("Loading Mods...");
            LoadMods();
       }

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            if(args.Name.Contains("TMNTModLoader"))
                return Assembly.GetExecutingAssembly();

            return null;
        }


        internal string GetRelativePath(string path)
        {
            return new Uri(path).AbsolutePath.Replace(new Uri(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).AbsolutePath, "").Substring(1);
        }

        public void LoadMods()
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve; ;

            JsonSerializer serializer = new JsonSerializer();

            if (!Directory.Exists(MODFOLDER))
                Directory.CreateDirectory(MODFOLDER);

            DirectoryInfo modInfo = new DirectoryInfo(MODFOLDER);
            List<ModManifest> mods = new List<ModManifest>();
            List<ModManifest> contentPacks = new List<ModManifest>();

            foreach (var directory in modInfo.EnumerateDirectories())
            {
                if (directory.GetFiles("manifest.json") is FileInfo[] files && files.Length > 0 && files[0] is FileInfo manifest)
                {
                    ModManifest m;
                    using (TextReader textreader = new StreamReader(manifest.OpenRead()))
                    using (JsonReader reader = new JsonTextReader(textreader))
                        m = serializer.Deserialize<ModManifest>(reader);

                    if (m == null)
                        continue;

                    if (string.IsNullOrEmpty(m.Id))
                        m.Id = m.Name;

                    if (m.IsMod)
                        mods.Add(m);

                    if (m.IsContentPack)
                        contentPacks.Add(m);

                    m.Folder = GetRelativePath(Path.GetDirectoryName(manifest.FullName));
                }
            }

            foreach (var cp in contentPacks.Where(c => c.ContentPackFor.Equals("Content", StringComparison.OrdinalIgnoreCase)))
            {
                modHelper.ContentPacks.Add(new ModHelper(cp));
                LogContentModLoad(modHelper.Manifest, true);
            }

            foreach (var m in mods)
            {
                var file = Path.Combine(m.Folder, m.EntryFile);

                Assembly a = Assembly.LoadFrom(file);

                if (a == null)
                {
                    LogModLoad(m, false);
                    continue;
                }

                AppDomain.CurrentDomain.Load(a.GetName());

                Type entryType = a.GetTypes().FirstOrDefault(t => t is IMod);

                if (entryType == null && !string.IsNullOrWhiteSpace(m.EntryMethod))
                    entryType = a.GetTypes().FirstOrDefault(t => t.GetMethod(m.EntryMethod, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance) is MethodInfo);

                if (entryType == null)
                {
                    LogModLoad(m, false);
                    continue;
                }

                var v = new ModVersion(m.MinumumApiVersion);

                if (!v.IsLowerOrEqualTo(ApiVersion))
                {
                    LogModLoad(m, false, m.Name + " requires Api Version " + m.Version + " or higher.");
                    continue;
                }

                ModHelper h = new ModHelper(m);

                foreach (var cp in contentPacks.Where(c => c.ContentPackFor.Equals(m.Id, StringComparison.OrdinalIgnoreCase)))
                    h.ContentPacks.Add(new ModHelper(cp));

                if (entryType.GetMethod(m.EntryMethod, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance) is MethodInfo entryMethod)
                {
                    try
                    {
                        var instance = Activator.CreateInstance(entryType);
                        var modp = typeof(IModHelper).FullName;
                        var entryParam = entryMethod.GetParameters().Select(p => p.ParameterType.FullName).FirstOrDefault();
                        object usep = null;
                        if (entryParam != null && entryParam == modp)
                            usep = h;

                        entryMethod.Invoke(instance, new object[] { usep });
                    }
                    catch (Exception ex)
                    {
                        h.Console.Error(ex.Message);
                        h.Console.Trace(ex.StackTrace);
                        LogModLoad(m, false);
                        continue;
                    }
                }

                LogModLoad(m, true);

                foreach (var c in h.ContentPacks)
                    LogContentModLoad(c.Manifest, true);

            }

           
        }

        internal void LogModLoad(IModManifest m, bool success, string message = "")
        {
            string modString = $"{m.Name} ({m.Version}) by {m.Author}";

            if (success)
                modHelper.Console.Success(modString, "Success");
            else
                modHelper.Console.Failure(modString, "Failed");

            if(!string.IsNullOrEmpty(message))
                modHelper.Console.Error(message);
        }

        internal void LogContentModLoad(IModManifest m, bool success)
        {
            string modString = $"ContentPack: {m.Name} ({m.Version}) by {m.Author}";

            if (success)
                modHelper.Console.Log(modString, "Loaded");
            else
                modHelper.Console.Failure(modString, "Failed");
        }

        public TMNTModApi()
        {
            Singleton = this;
            Main();
        }

        private void Singleton_GameInitialized(object sender, Events.GameInitializedEventArgs e)
        {
            modHelper.Console.Info("Game Initialized");

            modHelper.Config.SetOptionsMenuEntry("verbose", "Verbose Log", 
                (s) =>
                {
                    config.Verbose = (s.Choice == "ON");
                    modHelper.Content.SaveJson(config, CONFIG_FILENAME);
                }, () => config.Verbose ? "ON" : "OFF", "ON", "OFF");

            modHelper.Config.SetOptionsMenuEntry("console", "Show Console",
                (s) =>
                {
                    config.Console = (s.Choice == "ON");
                    modHelper.Content.SaveJson(config, CONFIG_FILENAME);
                }, () => config.Console ? "ON" : "OFF", "ON", "OFF");
        }

        internal static void DisplayMessage(string id, string message, float time)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"[{id}][{time}] {message}");
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
