using System;
using System.IO;

namespace BinaryFileMetadata
{
    public class DirectoryManager
    {
        private FileSystemContainer container;
        private DirectoryEntry root;
        private DirectoryEntry currentDirectory;

        public DirectoryManager(FileSystemContainer container)
        {
            this.container = container;

            // Initialize root
            root = new DirectoryEntry("\\", null);
            currentDirectory = root;

            LoadDirectoryTree();
        }

        public void LoadDirectoryTree()
        {
            using (var stream = new FileStream(container.ContainerPath, FileMode.Open, FileAccess.Read))
            {
                while (stream.Position < stream.Length)
                {
                    string entryName = ReadString(stream);
                    if (entryName == null) 
                        break;

                    int entryLength = ReadInt(stream);

                    // Debug statement
                    Console.WriteLine($"Debug: Loading entry '{entryName}' with length {entryLength} bytes.");

                    if (entryName.StartsWith("D:"))
                    {
                        // It's a directory with full path
                        string dirFullPath = StringImplementations.Substring(entryName, 2, entryName.Length - 2);
                        Console.WriteLine($"Debug: Processing directory '{dirFullPath}'.");
                        GetOrCreateDirectory(dirFullPath);
                    }
                    else
                    {
                        // It's a file with full path
                        string fileFullPath = entryName;
                        Console.WriteLine($"Debug: Processing file '{fileFullPath}'.");
                        // Extract the directory path and file name
                        string dirPath, fileName;
                        SplitPath(fileFullPath, out dirPath, out fileName);

                        // Get or create the directory
                        DirectoryEntry dir = GetOrCreateDirectory(dirPath);

                        // Add the file to the directory
                        dir.AddFile(fileName);
                    }

                    // Skip the file/directory data
                    stream.Seek(entryLength, SeekOrigin.Current);
                }
            }
        }

        private void SplitPath(string fullPath, out string directoryPath, out string fileName)
        {
            // Find the last '\' character
            int lastSlash = -1;
            for (int i = 0; i < fullPath.Length; i++)
            {
                if (fullPath[i] == '\\')
                {
                    lastSlash = i;
                }
            }

            if (lastSlash == -1)
            {
                // No directory, file is in root
                directoryPath = "\\";
                fileName = fullPath;
            }
            else if (lastSlash == 0)
            {
                // Directory is root
                directoryPath = "\\";
                fileName = StringImplementations.Substring(fullPath, lastSlash + 1, fullPath.Length - (lastSlash + 1));
            }
            else
            {
                directoryPath = StringImplementations.Substring(fullPath, 0, lastSlash);
                fileName = StringImplementations.Substring(fullPath, lastSlash + 1, fullPath.Length - (lastSlash + 1));
            }
        }

        private DirectoryEntry GetOrCreateDirectory(string dirFullPath)
        {
            if (StringImplementations.CustomCompare(dirFullPath, "\\") == 0)
            {
                return root;
            }

            // Split the path into components
            string[] parts = SplitFullPath(dirFullPath);
            DirectoryEntry current = root;

            for (int i = 0; i < parts.Length; i++)
            {
                string part = parts[i];
                DirectoryEntry sub = current.FindSubdirectory(part);
                if (sub == null)
                {
                    // Create new directory
                    sub = new DirectoryEntry(part, current);
                    current.AddSubdirectory(sub);
                }
                current = sub;
            }

            return current;
        }

        private string[] SplitFullPath(string fullPath)
        {
            // Count the number of '\' to determine the number of parts
            int count = 0;
            for (int i = 0; i < fullPath.Length; i++)
            {
                if (fullPath[i] == '\\') count++;
            }

            // Handle cases where fullPath is just "\" or empty
            if (count <= 0)
            {
                return new string[0];
            }

            // Allocate parts based on the number of backslashes
            string[] parts = new string[count];
            int partsIndex = 0;
            int start = 1; // Skip the first '\'

            for (int i = 1; i < fullPath.Length; i++)
            {
                if (fullPath[i] == '\\')
                {
                    int length = i - start;
                    if (length > 0)
                    {
                        parts[partsIndex++] = StringImplementations.Substring(fullPath, start, length);
                    }
                    else
                    {
                        // Handle consecutive backslashes or trailing backslash
                        parts[partsIndex++] = string.Empty;
                    }
                    start = i + 1;
                }
            }

            // Last part
            if (start < fullPath.Length)
            {
                int length = fullPath.Length - start;
                parts[partsIndex++] = StringImplementations.Substring(fullPath, start, length);
            }
            else if (partsIndex < parts.Length)
            {
                // If the path ends with a backslash, assign an empty string
                parts[partsIndex++] = string.Empty;
            }

            return parts;
        }


        public void MakeDirectory(string name)
        {
            // 1) In-memory
            var newDir = new DirectoryEntry(name, currentDirectory);
            currentDirectory.AddSubdirectory(newDir);

            // 2) In-container
            string currentDirPath = currentDirectory.GetFullPath();
            string newDirFullPath = currentDirPath == "\\" ? "\\" + name : currentDirPath + "\\" + name;
            container.CreateDirectoryEntry(newDirFullPath);

            // Debug statement
            Console.WriteLine($"Debug: Created directory '{newDirFullPath}'.");
        }



        public void ChangeDirectory(string target)
        {
            // cd \
            if (StringImplementations.CustomCompare(target, "\\") == 0)
            {
                currentDirectory = root;
                Console.WriteLine("Current directory is now '\\' (root).");
                return;
            }

            // cd ..
            if (StringImplementations.CustomCompare(target, "..") == 0)
            {
                if (currentDirectory.Parent == null)
                {
                    Console.WriteLine("Already at root directory.");
                }
                else
                {
                    currentDirectory = currentDirectory.Parent;
                    Console.WriteLine($"Current directory is now '{currentDirectory.Name}'.");
                }
                return;
            }

            // cd <subdir>
            var sub = currentDirectory.FindSubdirectory(target);
            if (sub == null)
            {
                Console.WriteLine($"Subdirectory '{target}' not found in '{currentDirectory.Name}'.");
            }
            else
            {
                currentDirectory = sub;
                Console.WriteLine($"Current directory is now '{currentDirectory.Name}'.");
            }
        }

        public void RemoveDirectory(string name)
        {
            // find the subdir in memory
            var targetDir = currentDirectory.FindSubdirectory(name);
            if (targetDir == null)
            {
                Console.WriteLine($"No subdirectory '{name}' in '{currentDirectory.Name}'.");
                return;
            }

            // Recursively remove all children from container
            RecursiveDeleteDirectory(targetDir);

            // Remove the "D:<fullPath>" entry from container
            string dirFullPath = targetDir.GetFullPath();
            container.RemoveFile("D:" + dirFullPath);

            // Remove from parent's memory
            currentDirectory.RemoveSubdirectory(name);

            Console.WriteLine($"Directory '{name}' removed.");
        }

        private void RecursiveDeleteDirectory(DirectoryEntry dir)
        {
            // 1) Remove all files in this directory
            string[] childFiles = dir.GetFiles();
            for (int i = 0; i < childFiles.Length; i++)
            {
                // Get the full path of the file
                string fileFullPath = dir.GetFullPath() == "\\" ? "\\" + childFiles[i] : dir.GetFullPath() + "\\" + childFiles[i];
                container.RemoveFile(fileFullPath);
            }

            // 2) Remove all subdirectories
            DirectoryEntry[] childDirs = dir.GetDirectories();
            for (int i = 0; i < childDirs.Length; i++)
            {
                // Recursively remove children
                RecursiveDeleteDirectory(childDirs[i]);
                string childDirFullPath = childDirs[i].GetFullPath();
                container.RemoveFile("D:" + childDirFullPath);
            }
        }

        public void ListCurrentDirectory()
        {
            Console.WriteLine($"Contents of directory '{currentDirectory.Name}':");

            // 1) List subdirectories
            var dirs = currentDirectory.GetDirectories();
            for (int i = 0; i < dirs.Length; i++)
            {
                Console.WriteLine($"[Dir ] {dirs[i].Name}");
            }

            // 2) List files with size
            var files = currentDirectory.GetFiles();
            for (int i = 0; i < files.Length; i++)
            {
                // Get the full path of the file
                string fileFullPath = currentDirectory.GetFullPath() == "\\" ? "\\" + files[i] : currentDirectory.GetFullPath() + "\\" + files[i];
                long size = container.GetFileSizeInContainer(fileFullPath);

                // Handle not found
                if (size < 0)
                {
                    Console.WriteLine($"  [File] {files[i]}, Size: -1B;");
                }
                else
                {
                    // Use FormatFileListing to display file name and size
                    string formatted = StringImplementations.FormatFileListing(files[i], (int)size);
                    Console.WriteLine($"  [File] {formatted}");
                }
            }
        }



        public void AddFileToCurrentDirectory(string fileName)
        {
            currentDirectory.AddFile(fileName);
        }

        public void RemoveFileFromCurrentDirectory(string fileName)
        {
            currentDirectory.RemoveFile(fileName);
        }

        public string GetCurrentDirectoryFullPath()
        {
            return currentDirectory.GetFullPath();
        }

        //Helper functions

        private string ReadString(FileStream stream)
        {
            byte[] lengthBytes = new byte[4];
            int rc = stream.Read(lengthBytes, 0, 4);
            if (rc < 4) return null;

            int length = BitConverter.ToInt32(lengthBytes, 0);
            byte[] data = new byte[length];
            int readData = stream.Read(data, 0, length);
            if (readData < length) return null;

            return System.Text.Encoding.UTF8.GetString(data);
        }

        private int ReadInt(FileStream stream)
        {
            byte[] intBytes = new byte[4];
            int rc = stream.Read(intBytes, 0, intBytes.Length);
            if (rc < 4) return -1;
            return BitConverter.ToInt32(intBytes, 0);
        }
    }
}
