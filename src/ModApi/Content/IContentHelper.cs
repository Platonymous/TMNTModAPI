namespace ModLoader.Content
{
    public interface IContentHelper
    {
        T LoadJson<T>(string filename, T fallback = null, bool createFileIfMissing = false) where T : class;

        void SaveJson<T>(T data, string filename) where T : class;

        T LoadContent<T>(string assetName, bool fromModFolder = true);
    }
}
