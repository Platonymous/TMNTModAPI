namespace ModLoader
{
    public interface IModManifest
    {
        string Id { get; }
        string Name { get; }

        string Author { get; }

        string Description { get; }

        string Version { get; }

        string EntryFile { get; }

        string EntryMethod { get; }

        string ContentPackFor { get; }

        string Folder { get; }

        string MinimumApiVersion { get; }
    }
}
