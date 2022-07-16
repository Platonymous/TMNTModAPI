using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModLoader.Utilities
{
    internal class ModVersion
    {
        int Major { get; set; }

        int Minor { get; set; }

        int Patch { get; set; }

        public string Version => Major.ToString() + "." + Minor.ToString() + "." + Patch.ToString();

        public ModVersion(string version)
        {
            if (string.IsNullOrEmpty(version))
                version = "0.0.0";

            List<string> parts = new List<string>(version.Split('.'));

            if (parts.Count < 2)
                parts.Add("0");

            if(parts.Count < 3)
                parts.Add("0");

            Major = int.Parse(parts[0]);
            Minor = int.Parse(parts[1]);
            Patch = int.Parse(parts[2].Contains("-") ? parts[2].Split('-')[0] : parts[2]);
        }

        public bool IsLowerOrEqualTo(ModVersion version)
        {

            if (Major < version.Major)
                return true;

            if (Major > version.Major)
                return false;

            if (Minor < version.Minor)
                return true;

            if (Minor > version.Minor)
                return false;

            if (Patch <= version.Patch)
                return true;

            return false;
        }
    }
}
