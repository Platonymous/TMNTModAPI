namespace ModLoader.Config
{
    public interface IOptionsMenuChange
    {
        string Id { get;  }

        string Name { get;  }

        string Choice { get; }
    }
}
