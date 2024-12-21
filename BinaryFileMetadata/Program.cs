namespace BinaryFileMetadata;

internal class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Welcome to the Binary File System Simulator.");
        Console.WriteLine("Commands: cpin <sourcePath> <containerFileName>, ls, rm, exit");

        var fileSystem = new FileSystemContainer("container2.bin");
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
                        fileSystem.ListFiles();
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

                    case "exit":
                        Console.WriteLine("Exiting the program. Goodbye!");
                        return;

                    default:
                        Console.WriteLine("Invalid command. Supported commands: cpin, ls, rm, exit.");
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