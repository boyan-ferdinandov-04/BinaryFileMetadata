using BinaryFileMetadata.CustomDataStructs;
using System;

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

            // Initialize an in-memory root directory
            root = new DirectoryEntry("\\", null);
            currentDirectory = root;

            // Load existing files from the container and build the directory tree
            LoadDirectoryTree();
        }

        /// <summary>
        /// Reads all FileRecords from the container and creates the necessary directories/files
        /// in the in-memory tree. Make sure the split logic handles leading slashes correctly.
        /// </summary>
        private void LoadDirectoryTree()
        {
            // container.fileRecords should be publicly accessible or use a method to iterate them.
            for (int i = 0; i < container.FileRecordCount; i++)
            {
                // In your code, you might do: var fr = container.GetFileRecordByIndex(i);
                // This example assumes there's a way to get each FileRecord.
                FileRecord fr = container.GetFileRecord(i);

                string fullPath = fr.FullPath;
                // e.g. "\MyPics\bigPicture.jpg" or "\myFile.txt"

                // Split into directory path + filename
                SplitPath(fullPath, out string directoryPath, out string fileName);

                // Create or get that directory
                DirectoryEntry dir = GetOrCreateDirectory(directoryPath);

                // Add the file to that directory
                dir.AddFile(fileName);
            }
        }

        /// Splits the full path into (directoryPath, fileName).
        /// Example: "\SomeDir\myFile.txt" => directoryPath="\SomeDir", fileName="myFile.txt"
        private void SplitPath(string fullPath, out string directoryPath, out string fileName)
        {
            // 1) Edge cases
            if (StringImplementations.IsNullOrWhiteSpace(fullPath)
                || StringImplementations.CustomCompare(fullPath, "\\") == 0)
            {
                directoryPath = "\\";
                fileName = ""; // No file name in this edge case
                return;
            }

            // 2) Find the last slash
            int lastSlash = -1;
            for (int i = 0; i < fullPath.Length; i++)
            {
                if (fullPath[i] == '\\')
                {
                    lastSlash = i;
                }
            }

            // 3) If no slash found, it means there's no directory, just a file in root
            if (lastSlash == -1)
            {
                directoryPath = "\\";
                fileName = fullPath;
                return;
            }

            // 4) If the last slash is the first char (0), that means something like "\file.txt"
            if (lastSlash == 0)
            {
                directoryPath = "\\";
                fileName = StringImplementations.Substring(fullPath, 1, fullPath.Length - 1);
                return;
            }

            // 5) Otherwise, normal case: "\SomeDir\myFile.txt"
            directoryPath = StringImplementations.Substring(fullPath, 0, lastSlash);
            fileName = StringImplementations.Substring(fullPath, lastSlash + 1, fullPath.Length - (lastSlash + 1));
        }

        /// Ensures the directory path exists in the in-memory tree. 
        /// If dirFullPath = "\SomeDir\Another", then we create subdirectories if missing.
        private DirectoryEntry GetOrCreateDirectory(string dirFullPath)
        {
            // If the path is just "\" => root
            if (StringImplementations.CustomCompare(dirFullPath, "\\") == 0)
            {
                return root;
            }

            // Convert the path into parts. e.g. "\SomeDir\Another" => ["SomeDir", "Another"]
            string[] parts = SplitFullPath(dirFullPath);
            DirectoryEntry current = root;

            for (int i = 0; i < parts.Length; i++)
            {
                string part = parts[i];
                if (StringImplementations.IsNullOrWhiteSpace(part))
                    continue; // skip empty segments if we have double slashes, etc.

                DirectoryEntry sub = current.FindSubdirectory(part);
                if (sub == null)
                {
                    // Create a new directory
                    sub = new DirectoryEntry(part, current);
                    current.AddSubdirectory(sub);
                }
                current = sub;
            }
            return current;
        }

        /// Split a full directory path (e.g. "\SomeDir\Another") into an array of directory names
        private string[] SplitFullPath(string fullPath)
        {
            // If it's just "\" or empty, return empty array => root
            if (StringImplementations.CustomCompare(fullPath, "\\") == 0
                || StringImplementations.IsNullOrWhiteSpace(fullPath))
            {
                return new string[0];
            }

            // Skip the first character if it's a backslash
            int startIndex = 0;
            if (fullPath.Length > 0 && fullPath[0] == '\\')
            {
                startIndex = 1;
            }

            // We can find the subdirectories by splitting on '\'
            string toSplit = StringImplementations.Substring(fullPath, startIndex, fullPath.Length - startIndex);

            return StringImplementations.Split(toSplit, '\\');
        }

        public void MakeDirectory(string name)
        {
            var newDir = new DirectoryEntry(name, currentDirectory);
            currentDirectory.AddSubdirectory(newDir);
            Console.WriteLine($"Debug: Created directory '{name}' in memory.");
        }

        public void ChangeDirectory(string target)
        {
            if (StringImplementations.CustomCompare(target, "\\") == 0)
            {
                currentDirectory = root;
                Console.WriteLine("Current directory is now '\\' (root).");
                return;
            }

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
            bool success = currentDirectory.RemoveSubdirectory(name);
            if (!success)
            {
                Console.WriteLine($"No subdirectory '{name}' in '{currentDirectory.Name}'.");
            }
            else
            {
                Console.WriteLine($"Directory '{name}' removed from memory.");
            }
        }

        public void ListCurrentDirectory()
        {
            Console.WriteLine($"Contents of directory '{currentDirectory.Name}':");

            var dirs = currentDirectory.GetDirectories();
            for (int i = 0; i < dirs.Length; i++)
            {
                Console.WriteLine($"[Dir ] {dirs[i].Name}");
            }

            var files = currentDirectory.GetFiles();
            for (int i = 0; i < files.Length; i++)
            {
                // Build the full path exactly as we do in CopyFileIn()
                string fullPath = (StringImplementations.CustomCompare(currentDirectory.GetFullPath(), "\\") == 0)
                    ? "\\" + files[i]
                    : currentDirectory.GetFullPath() + "\\" + files[i];

                long size = container.GetFileSizeInContainer(fullPath);
                Console.WriteLine($"[File] {files[i]}, Size: {size} B");
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

        /// Copy a local file into the container (uses block-based dedup).
        /// Also adds an entry to the current directory's file list.
        public void CopyFileIn(string sourcePath, string fileName)
        {
            string currentDirPath = currentDirectory.GetFullPath();
            // Build the fileFullPath with a leading slash if in root
            string fileFullPath = (StringImplementations.CustomCompare(currentDirPath, "\\") == 0)
                ? "\\" + fileName
                : currentDirPath + "\\" + fileName;

            container.CopyFileIntoContainer(sourcePath, fileFullPath);
            AddFileToCurrentDirectory(fileName);
        }

        /// Copy a file from the container out to the local file system.
        public void CopyFileOut(string containerFileName, string destinationPath)
        {
            // Build the path in the same consistent way
            string currentDirPath = currentDirectory.GetFullPath();
            string fileFullPath = (StringImplementations.CustomCompare(currentDirPath, "\\") == 0)
                ? "\\" + containerFileName
                : currentDirPath + "\\" + containerFileName;

            container.CopyFileOutFromContainer(fileFullPath, destinationPath);
        }

        /// Remove a file from the container and from the current directory listing.
        public void RemoveFile(string fileName)
        {
            string currentDirPath = currentDirectory.GetFullPath();
            string fileFullPath = (StringImplementations.CustomCompare(currentDirPath, "\\") == 0)
                ? "\\" + fileName
                : currentDirPath + "\\" + fileName;

            container.RemoveFile(fileFullPath);
            RemoveFileFromCurrentDirectory(fileName);
        }
    }
}
