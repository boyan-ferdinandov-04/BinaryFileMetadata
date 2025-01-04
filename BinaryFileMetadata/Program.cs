using System;

namespace BinaryFileMetadata
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to the Binary File System Simulator (with block-level deduplication).");
            Console.Write("Enter block size for container (bytes): ");
            string? blockSizeInput = Console.ReadLine();

            int blockSize;
            if (!int.TryParse(blockSizeInput, out blockSize) || blockSize <= 0)
            {
                blockSize = 1024;
                Console.WriteLine("Invalid input. Using default block size of 1024 bytes.");
            }

            // Create or open the container with the user-specified block size
            var fileSystem = new FileSystemContainer("demo5.bin", blockSize);
            var directoryManager = new DirectoryManager(fileSystem);

            Console.WriteLine("\nCommands:");
            Console.WriteLine("  cpin <sourcePath> <fileNameInContainer>    - Copy file into container");
            Console.WriteLine("  cpout <fileNameInContainer> <destinationPath> - Copy file out of container");
            Console.WriteLine("  rm <fileNameInContainer>                   - Remove file from container");
            Console.WriteLine("  ls                                         - List contents of current directory");
            Console.WriteLine("  md <directoryName>                         - Make a new directory");
            Console.WriteLine("  cd <directoryName|..|\\>                    - Change directory");
            Console.WriteLine("  rd <directoryName>                         - Remove directory");
            Console.WriteLine("  exit                                       - Exit the program");

            while (true)
            {
                Console.Write("\nEnter command: ");
                string? input = Console.ReadLine();

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
                                Console.WriteLine("Usage: cpin <sourcePath> <fileNameInContainer>");
                                break;
                            }
                            {
                                string sourcePath = inputArgs[1];
                                string fileName = inputArgs[2];
                                directoryManager.CopyFileIn(sourcePath, fileName);
                                break;
                            }

                        case "cpout":
                            if (inputArgs.Length != 3)
                            {
                                Console.WriteLine("Usage: cpout <fileNameInContainer> <destinationPath>");
                                break;
                            }
                            {
                                string containerFileName = inputArgs[1];
                                string destinationPath = inputArgs[2];
                                directoryManager.CopyFileOut(containerFileName, destinationPath);
                                break;
                            }

                        case "rm":
                            if (inputArgs.Length != 2)
                            {
                                Console.WriteLine("Usage: rm <fileNameInContainer>");
                                break;
                            }
                            {
                                string fileNameToRemove = inputArgs[1];
                                directoryManager.RemoveFile(fileNameToRemove);
                                break;
                            }

                        case "ls":
                            directoryManager.ListCurrentDirectory();
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
                                break;
                            }

                        case "cd":
                            if (inputArgs.Length != 2)
                            {
                                Console.WriteLine("Usage: cd <directoryName|..|\\>");
                                break;
                            }
                            {
                                string targetDir = inputArgs[1];
                                directoryManager.ChangeDirectory(targetDir);
                                break;
                            }

                        case "rd":
                            if (inputArgs.Length != 2)
                            {
                                Console.WriteLine("Usage: rd <directoryName>");
                                break;
                            }
                            {
                                string dirName = inputArgs[1];
                                directoryManager.RemoveDirectory(dirName);
                                break;
                            }

                        case "exit":
                            Console.WriteLine("Exiting the program. Goodbye!");
                            return;

                        default:
                            Console.WriteLine("Invalid command. Supported commands: cpin, cpout, rm, ls, md, cd, rd, exit.");
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
