using System;
using System.IO;
using System.Reflection;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using Paris.Engine.Context;
using Paris.Engine;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.IO.Compression;
using Newtonsoft.Json;
using Paris.Engine.Data;

namespace ContentExtractor
{
    public class Program
    {
        const string _exportFolderPath = "Extracted";
        const string _pathToContent = "Content";
        const string _title = "ContentExtractor 1.0.0 by Platonymous";
        const string _next = "Next File: ";
        static int done = 0;
        static int count = 0;
        const int fullbar = 50;
        static Assembly _parisSerializerAssembly;

        static List<string> AllTypes = new List<string>();
        static List<string> FailedTypes = new List<string>();

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            Game game = new Game();
            typeof(Game).GetField("graphicsDeviceService", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(game, new GraphicsDeviceManager(game));
            ParisContentManager content = new ParisContentManager((IServiceProvider)game.Services, "Content");
            ContextManager context = new ContextManager(game);
            context.GlobalContentManager = content;

            LoadParisSerializers();

            ParisObject.SerializingScope = GameObjectAttributes.PropertyScope.All;

                EngineSettings.Singleton = content.Load<EngineSettings>("Global\\EngineSettings");
            game.RunOneFrame();

            SoundSettings ss = content.Load<SoundSettings>(@"Audio\SoundsSettings.zpbn");

            var files = new List<string>(Directory.EnumerateFiles(_pathToContent, "*.*", SearchOption.AllDirectories));

            files.Add("sounds");
            done = 0;
            count = files.Count() + ss.Sounds.Count;

            foreach (string currentFile in files)
            {
                var extension = Path.GetExtension(currentFile);

                if (currentFile == "sounds")
                    ExtractSounds(ss);
                else
                {
                    var file = currentFile.Replace(@"Content/", "").Replace(@"Content\", "").Replace(extension, "");
                    if (extension == ".ogg" || extension == ".ogv" || extension == ".wav" || extension == ".mp4" || extension == ".json" || extension == ".pbn" || extension == ".png")
                    {
                        DrawBars(file, "Content");
                        CopyFile(file, currentFile);
                    }
                    else if (extension == ".zxnb" || extension == ".xnb")
                    {
                        DrawBars(file, "Content");
                        ExtractTexture(file, currentFile, content);
                    }
                    else if (extension == ".zpbn")
                    {
                        DrawBars(file, "Content");
                        ExtractData(file, currentFile);
                    }
                }

                done++;
            }
            Console.Write(" DONE");
            Environment.Exit(0);
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (args.Name == "Paris")
                try
                {
                    return Assembly.LoadFrom("TMNT.exe");
                }
                catch
                {
                    return null;
                }

            if (args.Name == "ParisSerializers")
                return _parisSerializerAssembly;

            return null;
        }

        static void ExtractData(string file, string currentFile)
        {
            string folder = EnsureDirectory(Path.Combine(_exportFolderPath, Path.GetDirectoryName(file)));
            bool handled = false;
            using (var fsin = File.OpenRead(currentFile))
            using (var deflate = new DeflateStream(fsin, CompressionMode.Decompress))
            using (var reader = new BinaryReader(deflate))
                handled = ExportAsJson(currentFile,folder,reader);

            if (!handled || true)
                using (var fsin = File.OpenRead(currentFile))
                using (var deflate = new DeflateStream(fsin, CompressionMode.Decompress))
                using (var reader = new BinaryReader(deflate))
                using (var fsout = File.Create(Path.Combine(folder, Path.GetFileName(currentFile.Replace(".zpbn", ".pbn")))))
                    deflate.CopyTo(fsout);
        }

        public static bool ExportAsJson(string currentFile, string folder, BinaryReader reader)
        {
            string readerType = "unknown";
            try
            {
                BinaryReadWriter readWriter = BinaryContentManager.Singleton.ReadTypeReader(reader);
                readerType = readWriter?.GetType().Name ?? readerType;
                object obj = null;

                readWriter.Read(reader, ref obj);
                
                File.WriteAllText(Path.Combine(folder, Path.GetFileName(currentFile.Replace(".zpbn", ".json"))), JsonConvert.SerializeObject(obj, Formatting.Indented));

                if (!AllTypes.Contains(readerType))
                    AllTypes.Add(readerType);

                return true;
            }
            catch(Exception ex)
            {
                if (!FailedTypes.Contains(readerType))
                    FailedTypes.Add(readerType);

                return false;
            }
        }

        public static void LoadParisSerializers()
        {
            if (_parisSerializerAssembly != null)
                return;
            if (File.Exists("ParisSerializers.org.dll"))
                _parisSerializerAssembly = Assembly.LoadFrom("ParisSerializers.org.dll");
            else
                _parisSerializerAssembly = Assembly.LoadFrom("ParisSerializers.dll");

            var bcm = BinaryContentManager.Singleton;

            typeof(BinaryContentManager).GetField("_parisSerializerAssembly", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(bcm, _parisSerializerAssembly);
            AppDomain.CurrentDomain.Load(_parisSerializerAssembly.GetName());

            typeof(BinaryContentManager).GetMethod("AddAssemblyReadWriters", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(bcm, new object[] { _parisSerializerAssembly });
        }

        static void ExtractSounds(SoundSettings ss)
        {
            var path = Path.Combine(_pathToContent, "Audio", "SFXPack.pbn");
            if (File.Exists(path))
                using (FileStream input = File.OpenRead(path))
                using (BinaryReader binaryReader = new BinaryReader((Stream)input))
                    for (int index = 0; index < ss.Sounds.Count; ++index)
                    {
                        string file = Path.Combine("Audio", ss.Sounds[index].SoundID);
                        DrawBars(file, "Content");
                        SoundSettings.SoundInfo sound = ss.Sounds[index];
                        int bytecount = binaryReader.ReadInt32();
                        byte[] data = binaryReader.ReadBytes(bytecount);

                        string output = Path.Combine(_exportFolderPath, file + ".wav");

                        string folder = EnsureDirectory(Path.GetDirectoryName(output));

                        using (FileStream fileout = File.OpenWrite(output))
                        using (BinaryWriter binaryWriter = new BinaryWriter(fileout))
                            WriteWaveData(binaryWriter, 2, 48000, data);
                        done++;
                    }
        }

        static string EnsureDirectory(string folder)
        {
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            return folder;
        }

        static void WriteWaveData(BinaryWriter writer, int channels, int rate, byte[] data)
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

        static void DrawBars(string file, string types)
        {
            Console.Clear();
            Console.WriteLine(_title);
            Console.WriteLine(_next + file);
            Console.Write("Extracting " + types + "... [");
            float percent = (float)done / count;
            int bars = (int)Math.Ceiling(percent * fullbar);

            for (int i = 0; i < bars; i++)
                Console.Write("#");

            for (int i = 0; i < fullbar - bars; i++)
                Console.Write("-");

            Console.Write("] " + Math.Ceiling(percent * 100) + "%");
        }

        static void CopyFile(string file, string currentFile)
        {
            string folder = EnsureDirectory(Path.Combine(_exportFolderPath, Path.GetDirectoryName(file)));
            File.Copy(currentFile, Path.Combine(folder, Path.GetFileName(currentFile)), true);
        }

        static void ExtractTexture(string file, string currentFile, ParisContentManager content)
        {
            try
            {
                Texture2D texture = content.Load<Texture2D>(file);
                string folder = EnsureDirectory(Path.Combine(_exportFolderPath, Path.GetDirectoryName(file)));

                UnpremultiplyTransparency(texture);
                texture.SaveAsPng(File.Create(Path.Combine(folder, Path.GetFileNameWithoutExtension(file)) + ".png"), texture.Width, texture.Height);
            }
            catch
            {
                CopyFile(file, currentFile);
            }
        }

        //https://github.com/Pathoschild/StardewXnbHack/blob/develop/StardewXnbHack/Framework/Writers/TextureWriter.cs
        static void UnpremultiplyTransparency(Texture2D texture)
        {
            Color[] data = new Color[texture.Width * texture.Height];
            texture.GetData(data);

            for (int i = 0; i < data.Length; i++)
            {
                Color pixel = data[i];
                if (pixel.A == 0)
                    continue;

                data[i] = new Color(
                    (byte)((pixel.R * 255) / pixel.A),
                    (byte)((pixel.G * 255) / pixel.A),
                    (byte)((pixel.B * 255) / pixel.A),
                    pixel.A
                );
            }

            texture.SetData(data);
        }

    }
}
