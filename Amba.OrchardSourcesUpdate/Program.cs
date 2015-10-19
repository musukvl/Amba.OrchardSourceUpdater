// Decompiled with JetBrains decompiler
// Type: Orchard.SourcesUpdate.Program
// Assembly: Orchard.SourcesUpdate, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B22794F-2108-4C75-9E6C-A10BCE0C9C5A
// Assembly location: D:\bin\orchard-soureces-update.exe

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Orchard.SourcesUpdate
{
    internal class Program
    {
        public static string oldOrchard;
        public static string newOrchard;

        private static void Main(string[] args)
        {
            if (Enumerable.Count<string>((IEnumerable<string>) args) < 2)
            {
                Console.WriteLine("orchard-sources-update old-src-path new-src-path");
                Console.WriteLine(" old-src-path and new-src-path should have /src and /lib folders in it.");
            }
            else
            {
                oldOrchard = Program.NormailizePath(args[0]);
                newOrchard = Program.NormailizePath(args[1]);

                if (!Directory.Exists(Path.Combine(oldOrchard, "src")) || !Directory.Exists(Path.Combine(newOrchard, "src")))
                {
                    Console.WriteLine("orchard-sources-update old-src-path new-src-path");
                    Console.WriteLine(" old-src-path and new-src-path should have /src and /lib folders in it.");
                    return;
                }

                Console.WriteLine("Updating:");
                Console.WriteLine("Old path: " + Program.oldOrchard);
                Console.WriteLine("New path: " + Program.newOrchard);

                Console.WriteLine("Save old sln file:");
                File.Copy(Path.Combine(oldOrchard, "src/Orchard.sln"), Path.Combine(oldOrchard, "src/Orchard.sln.bak"), true);

                Program.ReplaceFolder("lib");
                Program.ReplaceInternalFolders("src", "Orchard.Web");

                Program.ReplaceInternalFolders("src/Orchard.Web", "Media", "Modules", "Themes", "App_Data");

                Program.ReplaceModules("src/Orchard.Web/Modules");
                Program.ReplaceModules("src/Orchard.Web/Themes");

                Console.WriteLine("Done.");
            }
        }

        private static void ReplaceModules(string modulesPath)
        {
            foreach (string path in Directory.GetDirectories(Path.Combine(Program.newOrchard, modulesPath)))
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(path);
                Program.ReplaceFolder(modulesPath + "/" + directoryInfo.Name);
            }
        }

        private static void ReplaceInternalFolders(string folderPath, params string[] exeptions)
        {
            foreach (string newOrchardFiles in Directory.GetFiles(Path.Combine(Program.newOrchard, folderPath)))
            {
                string fileName = Path.GetFileName(newOrchardFiles);
                string oldOrchardFile = Path.Combine(Program.oldOrchard, folderPath, fileName);
                if (File.Exists(oldOrchardFile))
                    DeleteFile(oldOrchardFile);
                Console.Write("Copy {0}...", oldOrchardFile);
                File.Copy(newOrchardFiles, oldOrchardFile);
                Console.WriteLine("OK");
            }
            foreach (string path in Directory.GetDirectories(Path.Combine(Program.oldOrchard, folderPath)))
            {
                DirectoryInfo dirInfo = new DirectoryInfo(path);
                if (
                    !Enumerable.Any<string>((IEnumerable<string>) exeptions,
                        (Func<string, bool>) (x => dirInfo.Name.ToLower() == x.ToLower())))
                {
                    Console.Write("Deleting {0}...", dirInfo.Name);
                    Program.DeleteFolder(path);
                    Console.WriteLine("OK");
                }
            }
            foreach (string str in Directory.GetDirectories(Path.Combine(Program.newOrchard, folderPath)))
            {
                DirectoryInfo dirInfo = new DirectoryInfo(str);
                if (
                    !Enumerable.Any<string>((IEnumerable<string>) exeptions,
                        (Func<string, bool>) (x => dirInfo.Name.ToLower() == x.ToLower())))
                {
                    Console.Write("Copy {0}...", dirInfo.Name);
                    Program.CopyDirectory(str, Path.Combine(Program.oldOrchard, folderPath, dirInfo.Name));
                    Console.WriteLine("OK");
                }
            }
        }

        private static void ReplaceFolder(string folderPath)
        {
            Console.Write("Delete {0}...", folderPath);
            Program.DeleteFolder(Path.Combine(oldOrchard, folderPath));
            Console.WriteLine("OK");
            Console.Write("Copy {0}...", folderPath);
            Program.CopyDirectory(Path.Combine(newOrchard, folderPath), Path.Combine(oldOrchard, folderPath));
            Console.WriteLine("OK");
        }

        private static void DeleteFile(string path)
        {
            if (!File.Exists(path))
                return;
            try
            {
                File.Delete(path);
            }
            catch (Exception e)
            {
                Console.WriteLine("Cannot delete file. " + e.Message);
                Console.WriteLine("Let's try one more time.");
                Thread.Sleep(2000);
                File.Delete(path);
            }
        }

        private static void DeleteFolder(string path)
        {
            if (!Directory.Exists(path))
                return;
            try
            {
                Directory.Delete(path, true);
            }
            catch
            {
                Thread.Sleep(2000);
                Directory.Delete(path, true);
            }
        }

        private static string NormailizePath(string path)
        {
            if (!Path.IsPathRooted(path))
                return Path.Combine(Directory.GetCurrentDirectory(), path);
            return path;
        }

        private static void CopyDirectory(string sourcePath, string destPath)
        {
            if (!Directory.Exists(destPath))
                Directory.CreateDirectory(destPath);
            foreach (string str in Directory.GetFiles(sourcePath))
            {
                string destFileName = Path.Combine(destPath, Path.GetFileName(str));
                File.Copy(str, destFileName);
            }
            foreach (string str in Directory.GetDirectories(sourcePath))
            {
                string destPath1 = Path.Combine(destPath, Path.GetFileName(str));
                Program.CopyDirectory(str, destPath1);
            }
        }
    }
}