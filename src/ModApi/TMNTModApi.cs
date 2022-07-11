﻿using HarmonyLib;
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


        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool AllocConsole();

        [STAThread]
        public void Main()
        {
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
                Console.Title = "TMNT (Modded)";
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

        internal string GetRelativePath(string path)
        {
            return new Uri(path).AbsolutePath.Replace(new Uri(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).AbsolutePath, "").Substring(1);
        }

        public void LoadMods()
        {
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

                ModHelper h = new ModHelper(m);

                foreach (var cp in contentPacks.Where(c => c.ContentPackFor.Equals(m.Id, StringComparison.OrdinalIgnoreCase)))
                    h.ContentPacks.Add(new ModHelper(cp));

                if (entryType.GetMethod(m.EntryMethod, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance) is MethodInfo entryMethod)
                {
                    try
                    {
                        var instance = Activator.CreateInstance(entryType);
                        entryMethod.Invoke(instance, new object[] { h });
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

        internal void LogModLoad(IModManifest m, bool success)
        {
            string modString = $"{m.Name} ({m.Version}) by {m.Author}";

            if (success)
                modHelper.Console.Success(modString, "Success");
            else
                modHelper.Console.Failure(modString, "Failed");
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
        }

        internal static void DisplayMessage(string id, string message, float time)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"[{id}][{time}] {message}");
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
