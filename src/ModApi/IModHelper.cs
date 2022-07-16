using ModLoader.Config;
using ModLoader.Content;
using ModLoader.Events;
using ModLoader.Logs;
using System.Collections.Generic;

namespace ModLoader
{
    public interface IModHelper
    {
        IModManifest Manifest { get;}

        IConsoleHelper Console { get; }

        IEventHelper Events { get; }

        IContentHelper Content { get; }

        IConfigHelper Config { get; }


        IEnumerable<IModHelper> GetContentPacks();

    }
}
