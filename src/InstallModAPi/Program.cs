﻿using Microsoft.Win32;
using System;
using System.IO;
using System.IO.Compression;
using System.Reflection;

namespace InstallModApi
{
    internal class Program
    {
        [STAThread]
        
        static void Main(string[] args)
        {
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Console.WriteLine("Looking for TMNT.exe in " + path);
            if (!File.Exists(Path.Combine(path, "TMNT.exe")))
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("TMNT.exe was not found, press any key to locate it.");
                Console.ReadKey();
                OpenFileDialog OFD = new OpenFileDialog();
                OFD.Multiselect = false;
                OFD.Title = "Locate TMNT.exe";
                OFD.Filter = "TMNT|TMNT.exe";
                OFD.ShowDialog();
                string filePath = OFD.FileName;
                path = Path.GetDirectoryName(filePath.ToString());

                if(!File.Exists(Path.Combine(path, "TMNT.exe")))
                    CloseWithError("Could not find TMNT.exe in " + path);
            }
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("TMNT.exe found in " + path);

            if (!File.Exists("modapidata.zip"))
                CloseWithError("Could not locate modapidata.dat");

            Console.Write("Installing TMNT Mod Api...");

            if (File.Exists(Path.Combine(path, "ParisSerializers.mod.dll")))
                File.Delete(Path.Combine(path, "ParisSerializers.mod.dll"));

            if (File.Exists(Path.Combine(path, "0Harmony.dll")))
                File.Delete(Path.Combine(path, "0Harmony.dll"));

            if (File.Exists(Path.Combine(path, "0ModApi.dll")))
                File.Delete(Path.Combine(path, "0ModApi.dll"));

            if (File.Exists(Path.Combine(path, "0ModApi.json")))
                File.Delete(Path.Combine(path, "0ModApi.json"));

            if (File.Exists(Path.Combine(path, "ParisSerializers.org.dll")))
                File.Delete(Path.Combine(path, "ParisSerializers.dll"));
            else
            {
                File.Copy(Path.Combine(path, "ParisSerializers.dll"), Path.Combine(path, "ParisSerializers.org.dll"));
                File.Delete(Path.Combine(path, "ParisSerializers.dll"));
            }

            string zipFilePath = "modapidata.zip";
            string extractionPath = path;
            ZipFile.ExtractToDirectory(zipFilePath, extractionPath);

            File.Copy(Path.Combine(path, "ParisSerializers.mod.dll"), Path.Combine(path, "ParisSerializers.dll"));
            File.Delete(Path.Combine(path, "ParisSerializers.mod.dll"));

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("\tDone");

            Console.WriteLine();

            Console.ReadKey();
        }

        static void CloseWithError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine(message);

            Console.ReadKey();
            Environment.Exit(0);
        }
    }
}
