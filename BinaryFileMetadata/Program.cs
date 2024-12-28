using System;

namespace BinaryFileMetadata
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to the Binary File System Simulator (with directories).");
            Console.WriteLine("Commands: cpin <src> <fileName>, cpout <fileName> <dest>, rm <fileName>, ls, md <dir>, cd <dir>, rd <dir>, exit");

            var fileSystem = new FileSystemContainer("container3.bin");
            var directoryManager = new DirectoryManager(fileSystem);

            while (true)
            {
                Console.Write("\nEnter command: ");
                var input = Console.ReadLine();

                if (StringImplementations.IsNullOrWhiteSpace(input))
                {
                    Console.WriteLine("No command entered. Try again.");
                    continue;
                }

                var inputArgs = StringImplementations.Split(input, ' ');
                var command = StringImplementations.ToLower(inputArgs[0]);

                try
                {
                    switch (command)
                    {
                        case "cpin":
                            if (inputArgs.Length != 3)
                            {
                                Console.WriteLine("Usage: cpin <sourcePath> <fileName>");
                                break;
                            }
                            {
                                string sourcePath = inputArgs[1];
                                string fileName = inputArgs[2];
                                string currentDirPath = directoryManager.GetCurrentDirectoryFullPath();
                                string fileFullPath = currentDirPath == "\\" ? "\\" + fileName : currentDirPath + "\\" + fileName;

                                fileSystem.CopyFileIntoContainer(sourcePath, fileFullPath);
                                directoryManager.AddFileToCurrentDirectory(fileName);
                                Console.WriteLine($"File '{sourcePath}' copied into container as '{fileFullPath}'.");
                            }
                            break;


                        case "cpout":
                            if (inputArgs.Length != 3)
                            {
                                Console.WriteLine("Usage: cpout <containerFileName> <destinationPath>");
                                break;
                            }
                            {
                                string containerFileName = inputArgs[1];
                                string destinationPath = inputArgs[2];
                                fileSystem.CopyFileOutFromContainer(containerFileName, destinationPath);
                                Console.WriteLine($"File '{containerFileName}' copied to '{destinationPath}'.");
                            }
                            break;

                        case "rm":
                            if (inputArgs.Length != 2)
                            {
                                Console.WriteLine("Usage: rm <fileName>");
                                break;
                            }
                            {
                                string fileNameToRemove = inputArgs[1];
                                fileSystem.RemoveFile(fileNameToRemove);
                                directoryManager.RemoveFileFromCurrentDirectory(fileNameToRemove);
                                Console.WriteLine($"File '{fileNameToRemove}' removed from the container.");
                            }
                            break;

                        case "ls":
                            // Use the directory manager to list current dir
                            //directoryManager.ListCurrentDirectory();
                            fileSystem.ListFiles();
                            break;

                        case "md":
                            if (inputArgs.Length != 2)
                            {
                                Console.WriteLine("Usage: md <directoryName>");
                                break;
                            }
                            {
                                string dirName = inputArgs[1];
                                directoryManager.MakeDirectory(dirName);
                            }
                            break;

                        case "cd":
                            if (inputArgs.Length != 2)
                            {
                                Console.WriteLine("Usage: cd <directoryName|..|\\>");
                                break;
                            }
                            {
                                string targetDir = inputArgs[1];
                                directoryManager.ChangeDirectory(targetDir);
                            }
                            break;

                        case "rd":
                            if (inputArgs.Length != 2)
                            {
                                Console.WriteLine("Usage: rd <directoryName>");
                                break;
                            }
                            {
                                string dirName = inputArgs[1];
                                directoryManager.RemoveDirectory(dirName);
                            }
                            break;

                        case "exit":
                            Console.WriteLine("Exiting the program. Goodbye!");
                            return;

                        default:
                            Console.WriteLine("Invalid command. Supported commands: cpin, ls, rm, cpout, md, cd, rd, exit.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }
    }
}
