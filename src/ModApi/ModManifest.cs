using System;
using System.Reflection;

namespace ModLoader
{
    internal class ModManifest : IModManifest
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public string Author { get; set; } = "?";

        public string Description { get; set; } = "";

        public string Version { get; set; } = "1.0.0";

        public string EntryFile { get; set; }

        public string EntryMethod { get; set; } = "ModEntry";

        public string ContentPackFor { get; set; }

        internal bool IsContentPack => !string.IsNullOrEmpty(ContentPackFor);

        internal bool IsMod => !string.IsNullOrEmpty(EntryFile);

        public string Folder { get; internal set; }

    }
}
