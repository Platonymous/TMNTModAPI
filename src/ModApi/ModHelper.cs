using ModLoader.Config;
using ModLoader.Content;
using ModLoader.Events;
using ModLoader.Logs;
using Paris.Engine;
using Paris.Engine.Context;
using System.Collections.Generic;
using System.Reflection;

namespace ModLoader
{
    internal class ModHelper : IModHelper
    {
        public IModManifest Manifest { get; private set; }

        internal List<IModHelper> ContentPacks { get; private set; }

        public IConsoleHelper Console { get; private set; }

        public IEventHelper Events => EventManager.Singleton;

        public IContentHelper Content { get; private set; }

        public IConfigHelper Config { get; private set; }

        public ModHelper(ModManifest modManifest)
        {
            Manifest = modManifest;
            Console = new ConsoleManager(this);
            Content = new ContentHelper(this);
            ContentPacks = new List<IModHelper>();
            Config = new ConfigHelper(this); 
        }

        public IEnumerable<IModHelper> GetContentPacks()
        {
            return ContentPacks;
        }


    }
}
