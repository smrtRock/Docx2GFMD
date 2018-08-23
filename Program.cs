using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Docx2GFMD
{
    internal class Program
    {
        #region Properties
        private static DirectoryInfo[] Directories { get; set; }
        private static string ImageDirectory { get; set; }
        private static string ConvertedDirectory { get; set; } // = "C:\\Users\\marcusf\\source\\repos\\Docx2GFMD\\bin\\Debug\\ADR_Deployment_Expansion";
        private static string Dir { get; set; } 
        private static string Root { get; set; }

        #endregion

        private static void Main(string[] args)
        {
            // Check if they want help
            if (args.ToString().Contains("/?") || args.ToString().Contains("/h"))
            {
                Console.Error.WriteLine("USAGE: doc2xgfmd 'C:\\Directory\\To\\Convert");
                Console.Error.WriteLine();
                Console.Error.WriteLine("EXAMPLES:");
                Console.Error.WriteLine();
                Console.Error.WriteLine("Convert the C:\\Temp:  doc2gfmd C:\\Temp");
                Console.Error.WriteLine("Convert the current directory: doc2gfmd");
            }
            // Set default values and prepare environment for conversion
            ParseCommandArguments(args);
            ConvertDirectory();
        }

        #region Helpers
        private static void ParseCommandArguments(string[] args)
        {
            if (args[0].Equals(".") || args.Length == 0)
                Dir = $"{new DirectoryInfo(Environment.CurrentDirectory).FullName}";
            else if (args[0].Contains(".\\"))
                Dir = $"{new DirectoryInfo(Environment.CurrentDirectory).FullName}{args[0].Split('.')[1]}";
            else
                Dir = $"{new DirectoryInfo(args[0]).FullName}";
        }

        private static void ConvertDirectory()
        {
            PrepareFolderStructure();
            ConvertToMarkdown();
            CreateIndexMarkdown();
            CreateOrderFile();
            MoveImages();
            FixImageLinks();

        }

        private static void PrepareFolderStructure()
        {
            ConvertedDirectory = $"{new DirectoryInfo(Dir).FullName}\\converted\\{new DirectoryInfo(Dir).Name}";
            ImageDirectory = $"{Dir}\\converted\\.attachments";
            if (!Directory.Exists(ConvertedDirectory))
            {
                Directory.CreateDirectory(ConvertedDirectory);
            }
                        
            // The image folder is only set once
            if (!Directory.Exists(ImageDirectory) && !string.IsNullOrEmpty(Root))
            {
                Directory.CreateDirectory(ImageDirectory);
            }
        }

        //private static void CleanUpImages()
        //{
        //    // Move and rename image from the subdirectory created by pandoc
        //    foreach (var img in Directory.GetFiles($"{ImageDirectory}\\media"))
        //    {
        //        // Move and rename
        //        var newImg = $"{ImageDirectory}\\{new DirectoryInfo(ConvertedDirectory).Name}{img}";
        //        File.Move(img,newImg);
        //        foreach (var file in Directory.GetFiles(ConvertedDirectory))
        //        {
        //            List<string> currentFile = new List<string>(); 
        //            using (StreamReader sr = new StreamReader(file))
        //            {
        //                string line;
        //                while ((line = sr.ReadLine()) != null)
        //                {
        //                    currentFile.Add(line);
        //                }
        //            }

        //            if (currentFile.Contains(img))
        //            {
        //                foreach (var line in currentFile)
        //                {
        //                    if (line.Contains(img)) line.Replace(img, newImg);
        //                }
                        
        //                using (StreamWriter sw = new StreamWriter(file))
        //                {
        //                    foreach (var line in currentFile)
        //                    {
        //                        sw.WriteLine(line);
        //                    }
        //                }
        //            }
        //        }
        //    } 
        //}

        private static void ConvertToMarkdown()
        {
            foreach (var file in Directory.GetFiles(Dir))
            {
                try
                {
                    ProcessStartInfo StartInfo = new ProcessStartInfo
                    {
                        FileName = @"pandoc.exe",
                        Arguments = GetArguments(file)
                    };

                    using (Process exeProcess = Process.Start(StartInfo))
                    {
                        exeProcess.WaitForExit();
                    }
                }
                catch
                {
                    Console.Error.WriteLine(new Exception().Message);
                }
            }
        }

        private static string GetArguments(string file)
        {
            string arguments;
           
            var outName = GetName(file);
            arguments = $"{file} --to gfm " +
                $"--output {outName} --extract-media={ImageDirectory}";
            return arguments;
        }

        private static string GetName(string file)
        {
            return $"{ConvertedDirectory}\\{new FileInfo(file).Name.Split('.')[0].Trim()}.md";
        }
 
        private static void CreateIndexMarkdown()
        {

            string tempFile = Path.GetTempFileName();

            using (StreamWriter write = new StreamWriter(tempFile))
            {
                foreach (var file in Directory.EnumerateFileSystemEntries(Dir))
                {
                    write.WriteLine($"[{new FileInfo(file).Name.Split('.')[0]}](/{new DirectoryInfo(Dir).Name}/{new FileInfo(file).Name.Split('.')[0].Trim()})");
                }
            }
            if (!File.Exists($"{new DirectoryInfo(ConvertedDirectory).FullName}\\index.md"))
            {
                File.Delete($"{new DirectoryInfo(ConvertedDirectory).FullName}\\index.md");
            }
            File.Move(tempFile, $"{new DirectoryInfo(ConvertedDirectory).FullName}\\index.md");
        }

        private static void CreateOrderFile()
        {
            var tempFIle = Path.GetTempFileName();
            using (StreamWriter write = new StreamWriter(tempFIle))
            {
                foreach (var file in Directory.EnumerateFileSystemEntries(Dir))
                {
                    write.WriteLine($"{new FileInfo(file).Name.Split('.')[0]}");
                }
            }
            if (File.Exists($"{new DirectoryInfo(ConvertedDirectory).FullName}\\.order"))
            {
                File.Delete($"{new DirectoryInfo(ConvertedDirectory).FullName}\\.order");
            }
            File.Move(tempFIle, $"{new DirectoryInfo(ConvertedDirectory).FullName}\\.order");
        }

        private static void MoveImages()
        {
            foreach(var file in new DirectoryInfo($"{ImageDirectory}\\media").GetFiles())
            {
                FileInfo newFileName = new FileInfo($"{file.Directory.Parent.FullName}\\{new DirectoryInfo(Dir).Name}_{file.Name}");
                
                File.Move(file.FullName,newFileName.FullName);
            }

            Directory.Delete($"{ImageDirectory}\\media");
        }

        private static void FixImageLinks()
        {
            foreach (var image in new DirectoryInfo(ImageDirectory).GetFiles())
            {
                foreach (var file in new DirectoryInfo(ConvertedDirectory).GetFiles())
                {
                    bool changed = false;
                    var results = new List<string>();
                    using (StreamReader sr = new StreamReader(file.FullName))
                    {
                        string line = sr.ReadLine();
                        while (line != null)
                        {
                            results.Add(line);
                            if (line.Contains("![]"))
                            {
                                changed = true;
                                results.Remove(line);
                                results.Add($"![{image.Name}](/.attachments/{image.Name})");
                            }
                            line = sr.ReadLine();
                        }
                    }

                    if (changed)
                    {
                        File.Copy(file.FullName, $"{file.FullName}.copy");
                        File.Delete(file.FullName);
                        try
                        {
                            using (StreamWriter sw = new StreamWriter(file.FullName))
                            {
                                foreach (string line in results)
                                {
                                    sw.WriteLine(line);
                                }
                            }
                            File.Delete($"{file.FullName}.copy");
                        }
                        catch (IOException e)
                        {
                            Console.Error.WriteLine("The file failed to write");
                            Console.Error.WriteLine(e.InnerException.Message);
                            Console.Error.WriteLine("Everything is as it was");
                            File.Move($"{file.FullName}.copy", file.FullName);
                        }
                    }
                }
            }
        }
        #endregion
    }
}
