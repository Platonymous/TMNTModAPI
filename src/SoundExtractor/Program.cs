using System;
using System.IO;
using System.Reflection;
using Paris.Engine.Context;
using Paris.Engine;
using Microsoft.Xna.Framework;
using System.Linq;

namespace SoundExtractor
{
    public class Program
    {
        const string _exportFolderPath = "Extracted";
        const string _pathToContent = "Content";

        static void Main(string[] args)
        {
            Game game = new Game();
            typeof(Game).GetField("graphicsDeviceService", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(game, new GraphicsDeviceManager(game));
            ParisContentManager content = new ParisContentManager((IServiceProvider)game.Services, "Content");
            ContextManager context = new ContextManager(game);

            if (File.Exists("ParisSerializers.org.dll"))
            {
                var ass = Assembly.LoadFrom("ParisSerializers.org.dll");
                var bcm = BinaryContentManager.Singleton;

               typeof(BinaryContentManager).GetField("_parisSerializerAssembly", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(bcm, ass);
                AppDomain.CurrentDomain.Load(ass.GetName());

               typeof(BinaryContentManager).GetMethod("AddAssemblyReadWriters", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(bcm, new object[] { ass });
            }

            game.RunOneFrame();
            Console.Clear();
            SoundSettings soundSettings = content.Load<SoundSettings>(@"Audio\SoundsSettings.zpbn");

            int count = soundSettings.Sounds.Count;
            int done = 0;
            int fullbar = 50;

            var path = Path.Combine(_pathToContent, "Audio", "SFXPack.pbn");
            if (File.Exists(path))
                using (FileStream input = File.OpenRead(path))
                using (BinaryReader binaryReader = new BinaryReader((Stream)input))
                    for (int index = 0; index < soundSettings.Sounds.Count; ++index)
                    {
                        string file = Path.Combine("Audio",soundSettings.Sounds[index].SoundID);
                        Console.Clear();


                        Console.WriteLine("SoundExtractor 1.0.3 by Platonymous");
                        Console.WriteLine("Next File: " + file);

                        Console.Write("Extracting SFX... [");
                        float percent = (float)done / count;
                        int bars = (int)Math.Ceiling(percent * fullbar);

                        for (int i = 0; i < bars; i++)
                            Console.Write("#");

                        for (int i = 0; i < fullbar - bars; i++)
                            Console.Write("-");

                        Console.Write("] " + Math.Ceiling(percent * 100) + "%");
                        SoundSettings.SoundInfo sound = soundSettings.Sounds[index];
                        int bytecount = binaryReader.ReadInt32();
                        byte[] data = binaryReader.ReadBytes(bytecount);

                        string output = Path.Combine(_exportFolderPath, file + ".wav");

                        string folder = Path.GetDirectoryName(output);

                        if (!Directory.Exists(folder))
                            Directory.CreateDirectory(folder);

                        using (FileStream fileout = File.OpenWrite(output))
                        using (BinaryWriter binaryWriter = new BinaryWriter(fileout))
                            WriteWave(binaryWriter, 2, 48000, data);
                        done++;
                    }

            var files = Directory.EnumerateFiles(_pathToContent, "*.ogg", SearchOption.AllDirectories);

            count = files.Count();
            done = 0;
            fullbar = 50;

            foreach (string currentFile in files)
            {
                var extension = Path.GetExtension(currentFile);

                Console.Clear();

                var file = currentFile.Replace(@"Content/", "").Replace(@"Content\", "").Replace(extension, "");
                Console.WriteLine("SoundExtractor 1.0.3 by Platonymous");
                Console.WriteLine("Next File: " + file);

                Console.Write("Copying Music... [");

                float percent = (float)done / count;
                int bars = (int)Math.Ceiling(percent * fullbar);

                for (int i = 0; i < bars; i++)
                    Console.Write("#");

                for (int i = 0; i < fullbar - bars; i++)
                    Console.Write("-");

                Console.Write("] " + Math.Ceiling(percent * 100) + "%");

                string folder = Path.Combine(_exportFolderPath, Path.GetDirectoryName(file));

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                File.Copy(currentFile, Path.Combine(folder, Path.GetFileName(currentFile)), true);

                done++;
            }

            Console.Write(" DONE");
            Environment.Exit(0);
        }

        private static void WriteWave(BinaryWriter writer, int channels, int rate, byte[] data)
        {
            writer.Write(new char[4] { 'R', 'I', 'F', 'F' });
            writer.Write((36 + data.Length));
            writer.Write(new char[4] { 'W', 'A', 'V', 'E' });
            writer.Write(new char[4] { 'f', 'm', 't', ' ' });
            writer.Write(16);
            writer.Write((short)1);
            writer.Write((short)channels);
            writer.Write(rate);
            writer.Write((rate * ((16 * channels) / 8)));
            writer.Write((short)((16 * channels) / 8));
            writer.Write((short)16);
            writer.Write(new char[4] { 'd', 'a', 't', 'a' });
            writer.Write(data.Length);
            writer.Write(data);
        }
    }
}
