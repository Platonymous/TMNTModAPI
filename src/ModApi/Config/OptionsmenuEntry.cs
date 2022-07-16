using System;
using System.Collections.Generic;
using System.Linq;

namespace ModLoader.Config
{
    internal class OptionsmenuEntry
    {
        public string Id { get; private set; }

        public string Name { get; private set; }

        public string[] Choices { get; private set; }

        public Action<int> Change { get; private set; }

        public Func<int> Current { get; private set; }

        internal IModHelper Helper { get; private set; }

        public OptionsmenuEntry(string id, string name, string[] choices, Action<IOptionsMenuChange> action, Func<string> current, IModHelper helper)
        {
            Id = helper.Manifest.Id + "." + id;
            Choices = choices;
            Name = name;
            Change = (c) => action.Invoke(new OptionsMenuChange(Id,Name,Choices[c]));
            Current = () => Choices.ToList().IndexOf(current());
            Helper = helper;
        }
    }
}
