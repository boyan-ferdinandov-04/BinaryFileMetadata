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

            // Build a flat tree: everything under "root"
            root = new DirectoryEntry("\\", null);
            currentDirectory = root;

            LoadDirectoryTree();
        }

        /// Reads all entries from the container. If name starts with "D:", treat as a directory; otherwise treat as a file.
        /// Places them all under 'root' for simplicity.

        private void LoadDirectoryTree()
        {
            using (var stream = new FileStream(container.ContainerPath, FileMode.Open, FileAccess.Read))
            {
                while (stream.Position < stream.Length)
                {
                    string entryName = ReadString(stream);
                    if (entryName == null) break;

                    int entryLength = ReadInt(stream);

                    if (entryName.StartsWith("D:"))
                    {
                        // It's a directory
                        string dirName = StringImplementations.Substring(entryName, 2, entryName.Length - 2);
                        DirectoryEntry dir = new DirectoryEntry(dirName, root);
                        root.AddSubdirectory(dir);
                    }
                    else
                    {
                        // It's a file
                        root.AddFile(entryName);
                    }

                    // skip the file/directory data
                    stream.Seek(entryLength, SeekOrigin.Current);
                }
            }
        }


        public void MakeDirectory(string name)
        {
            // 1) In-memory
            var newDir = new DirectoryEntry(name, currentDirectory);
            currentDirectory.AddSubdirectory(newDir);

            // 2) In-container
            container.CreateDirectoryEntry(name);

            Console.WriteLine($"Directory '{name}' created in '{currentDirectory.Name}'.");
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

            // Remove the "D:<name>" entry from container
            container.RemoveDirectoryEntry(name);

            // Remove from parent's memory
            currentDirectory.RemoveSubdirectory(name);

            Console.WriteLine($"Directory '{name}' removed.");
        }

        /// Remove files and subdirs that belong to 'dir' from the container. 
        /// This must happen before we remove the actual directory entry itself.

        private void RecursiveDeleteDirectory(DirectoryEntry dir)
        {
            // 1) Remove all files in this directory
            string[] childFiles = dir.GetFiles();
            for (int i = 0; i < childFiles.Length; i++)
            {
                container.RemoveFile(childFiles[i]);
            }

            // 2) Remove all subdirectories
            DirectoryEntry[] childDirs = dir.GetDirectories();
            for (int i = 0; i < childDirs.Length; i++)
            {
                // Recursively remove children
                RecursiveDeleteDirectory(childDirs[i]);
                container.RemoveDirectoryEntry(childDirs[i].Name);
            }
        }


        /// Lists contents (files + subdirectories) of the current directory.

        public void ListCurrentDirectory()
        {
            Console.WriteLine($"Contents of directory '{currentDirectory.Name}':");
            // subdirs
            var dirs = currentDirectory.GetDirectories();
            for (int i = 0; i < dirs.Length; i++)
            {
                Console.WriteLine($"  [DIR ] {dirs[i].Name}");
            }
            // files
            var files = currentDirectory.GetFiles();
            for (int i = 0; i < files.Length; i++)
            {
                Console.WriteLine($"  [FILE] {files[i]}");
            }
        }

        // =========== FILE UTILS (to keep memory in sync) ===========

        /// Called after cpin, to add the file to the current directory in memory.

        public void AddFileToCurrentDirectory(string fileName)
        {
            currentDirectory.AddFile(fileName);
        }


        /// Called after rm or similar, to remove the file from the current directory in memory.

        public void RemoveFileFromCurrentDirectory(string fileName)
        {
            currentDirectory.RemoveFile(fileName);
        }

        // =========== HELPER READ METHODS (avoid collisions) ===========

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
            int rc = stream.Read(intBytes, 0, 4);
            if (rc < 4) return -1;
            return BitConverter.ToInt32(intBytes, 0);
        }
    }
}
