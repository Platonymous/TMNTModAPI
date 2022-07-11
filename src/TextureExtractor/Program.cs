using System;
using System.IO;
using System.Reflection;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using Paris.Engine.Context;
using Paris.Engine;
using Microsoft.Xna.Framework;

namespace TextureExtractor
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
            game.RunOneFrame();

            var files = Directory.EnumerateFiles(_pathToContent, "*.zxnb", SearchOption.AllDirectories);

            int count = files.Count();
            int done = 0;
            int fullbar = 50;

            foreach (string currentFile in files)
            {
                var extension = Path.GetExtension(currentFile);

                Console.Clear();

                var file = currentFile.Replace(@"Content/", "").Replace(@"Content\", "").Replace(extension, "");
                Console.WriteLine("TextureExtractor 1.0.0 by Platonymous");
                Console.WriteLine("Next File: " + file);

                Console.Write("Extracting Textures... [");

                float percent = (float)done / count;
                int bars = (int)Math.Ceiling(percent * fullbar);

                for (int i = 0; i < bars; i++)
                    Console.Write("#");

                for (int i = 0; i < fullbar - bars; i++)
                    Console.Write("-");

                Console.Write("] " + Math.Ceiling(percent * 100) + "%");
                try
                {
                    Texture2D texture = content.Load<Texture2D>(file);
                    string folder = Path.Combine(_exportFolderPath, Path.GetDirectoryName(file));

                    if (!Directory.Exists(folder))
                        Directory.CreateDirectory(folder);
                    UnpremultiplyTransparency(texture);
                    texture.SaveAsPng(File.Create(Path.Combine(folder, Path.GetFileNameWithoutExtension(file)) + ".png"), texture.Width, texture.Height);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                }
                done++;
            }
            Console.Write(" DONE");
            Environment.Exit(0);
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
