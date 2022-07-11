namespace ModLoader
{
    public class ModApiConfig
    {
        public bool Verbose { get; set; }
        public bool Console { get; set; }

        public ModApiConfig()
        {
            Verbose = false;
            Console = true;
        }
    }
}
