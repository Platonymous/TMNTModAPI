namespace ModLoader.Content
{
    public class AudioContent
    {
        public byte[] Data { get; private set; }

        public string Id { get; private set; }
        public AudioContent(string id, byte[] data)
        {
            Id = id;
            Data = data;
        }

    }
}
