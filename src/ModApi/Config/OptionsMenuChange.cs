namespace ModLoader.Config
{
    internal class OptionsMenuChange : IOptionsMenuChange
    {
        public string Id { get; private set; }

        public string Name { get; private set; }

        public string Choice { get; private set; }

        public OptionsMenuChange(string id, string name, string choice)
        {
            Id = id;
            Name = name;
            Choice = choice;
        }
    }
}
