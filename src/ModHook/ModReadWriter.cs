using System;
using System.Reflection;
using Paris.Engine;
using ModLoader;

namespace ParisSerializer
{
    public class ModReadWriter : BinaryReadWriter
    {
        public static bool loaded = false;

        public static Assembly parisSerializerAssembly;

        public static TMNTModApi Api;

        public static Assembly GetSerializerAssembly()
        {
            if (parisSerializerAssembly == null)
                parisSerializerAssembly = Assembly.LoadFrom("ParisSerializers.org.dll");

            return parisSerializerAssembly;
        }

        public static Assembly CurrentDomain_TypeResolve(object sender, ResolveEventArgs args)
        {
            try
            {
                if (GetSerializerAssembly() is Assembly ass && ass.GetType(args.Name) is Type)
                    return ass;
            }
            catch
            {
                return null;
            }

            return null;
        }

        public ModReadWriter()
        {
            if (!loaded)
            {
                GetSerializerAssembly();
                AppDomain.CurrentDomain.TypeResolve += CurrentDomain_TypeResolve;

                Api = new TMNTModApi();
            }
            loaded = true;

        }
    }

}