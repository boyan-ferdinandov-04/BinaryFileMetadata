namespace BinaryFileMetadata;

internal class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Welcome to the Binary File System Simulator.");
        Console.WriteLine("Commands: cpin <sourcePath> <containerFileName>, ls, rm, cpout, md, cd, rd, exit");

        var fileSystem = new FileSystemContainer("container4.bin");
        var directoryManager = new DirectoryManager(fileSystem);

        while (true)
        {
            Console.Write("\nEnter command: ");
            var input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine("No command entered. Try again.");
                continue;
            }

            var inputArgs = StringImplementations.Split(input, ' ');
            var command = inputArgs[0].ToLower();

            try
            {
                switch (command)
                {
                    case "cpin":
                        if (inputArgs.Length != 3)
                        {
                            Console.WriteLine("Usage: cpin <sourcePath> <containerFileName>");
                            break;
                        }
                        string sourcePath = inputArgs[1];
                        string containerFileName = inputArgs[2];
                        fileSystem.CopyFileIntoContainer(sourcePath, containerFileName);
                        Console.WriteLine($"File '{sourcePath}' copied into container as '{containerFileName}'.");
                        break;

                    case "ls":
                        try
                        {
                            fileSystem.ListFiles();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error listing files: {ex.Message}");
                        }
                        break;

                    case "rm":
                        if (inputArgs.Length != 2)
                        {
                            Console.WriteLine("Usage: rm <fileName>");
                            break;
                        }
                        string fileNameToRemove = inputArgs[1];
                        fileSystem.RemoveFile(fileNameToRemove);
                        Console.WriteLine($"File '{fileNameToRemove}' removed from the container.");
                        break;

                    case "cpout":
                        if (inputArgs.Length != 3)
                        {
                            Console.WriteLine("Usage: cpout <containerFileName> <destinationPath>");
                            break;
                        }
                        string containerFileNameToCopy = inputArgs[1];
                        string destinationPath = inputArgs[2];
                        fileSystem.CopyFileOutFromContainer(containerFileNameToCopy, destinationPath);
                        Console.WriteLine($"File '{containerFileNameToCopy}' copied to '{destinationPath}'.");
                        break;

                    case "md":
                        if (inputArgs.Length != 2)
                        {
                            Console.WriteLine("Usage: md <directoryName>");
                            break;
                        }
                        string directoryName = inputArgs[1];
                        directoryManager.CreateDirectory(directoryName);
                        Console.WriteLine($"Directory '{directoryName}' created.");
                        break;

                    case "cd":
                        if (inputArgs.Length != 2)
                        {
                            Console.WriteLine("Usage: cd <directoryName>");
                            break;
                        }
                        try
                        {
                            string targetDirectory = inputArgs[1];
                            directoryManager.ChangeDirectory(targetDirectory);
                            Console.WriteLine($"Changed directory to '{targetDirectory}'.");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error changing directory: {ex.Message}");
                        }
                        break;

                    case "rd":
                        if (inputArgs.Length != 2)
                        {
                            Console.WriteLine("Usage: rd <directoryName>");
                            break;
                        }
                        string directoryToRemove = inputArgs[1];
                        directoryManager.RemoveDirectory(directoryToRemove);
                        Console.WriteLine($"Directory '{directoryToRemove}' removed.");
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