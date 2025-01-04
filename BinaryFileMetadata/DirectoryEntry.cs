using System;

namespace BinaryFileMetadata
{
    public class DirectoryEntry
    {
        public string Name;
        public DirectoryEntry Parent;

        // Array to store subdirectories
        private DirectoryEntry[] subDirectories;
        private int dirCount;
        // Array to store file names
        private string[] files;
        private int fileCount;
        // Initial capacity for subdirectories and files arrays
        private const int INITIAL_CAPACITY = 8;

        public DirectoryEntry(string name, DirectoryEntry parent)
        {
            Name = name;
            Parent = parent;
            subDirectories = new DirectoryEntry[INITIAL_CAPACITY];
            files = new string[INITIAL_CAPACITY];
            dirCount = 0;
            fileCount = 0;
        }

        public void AddSubdirectory(DirectoryEntry directory)
        {
            // Checking if the subDirectories array needs to be resized
            if (dirCount == subDirectories.Length)
            {
                // Doubling the size of the subDirectories array
                DirectoryEntry[] bigger = new DirectoryEntry[subDirectories.Length * 2];
                for (int i = 0; i < subDirectories.Length; i++)
                {
                    bigger[i] = subDirectories[i];
                }
                subDirectories = bigger;
            }
            // Adding the new subdirectory and increment the count
            subDirectories[dirCount++] = directory;
        }

        // Name of the subdirectory to remove
        public bool RemoveSubdirectory(string directoryName)
        {
            for (int i = 0; i < dirCount; i++)
            {
                if (subDirectories[i].Name == directoryName)
                {
                    for (int j = i; j < dirCount - 1; j++)
                    {
                        subDirectories[j] = subDirectories[j + 1];
                    }
                    subDirectories[dirCount - 1] = null;
                    dirCount--;
                    return true;
                }
            }
            // Subdirectory is not found
            return false;
        }

        public DirectoryEntry FindSubdirectory(string name)
        {
            for (int i = 0; i < dirCount; i++)
            {
                if (subDirectories[i].Name == name)
                {
                    return subDirectories[i];
                }
            }
            return null;
        }

        public DirectoryEntry[] GetDirectories()
        {
            DirectoryEntry[] result = new DirectoryEntry[dirCount];
            for (int i = 0; i < dirCount; i++)
            {
                result[i] = subDirectories[i];
            }
            return result;
        }

        public void AddFile(string fileName)
        {
            if (fileCount == files.Length)
            {
                string[] bigger = new string[files.Length * 2];
                for (int i = 0; i < files.Length; i++)
                {
                    bigger[i] = files[i];
                }
                files = bigger;
            }
            files[fileCount++] = fileName;
        }

        public bool RemoveFile(string fileName)
        {
            for (int i = 0; i < fileCount; i++)
            {
                if (files[i] == fileName)
                {
                    for (int j = i; j < fileCount - 1; j++)
                    {
                        files[j] = files[j + 1];
                    }
                    files[fileCount - 1] = null;
                    fileCount--;
                    return true;
                }
            }
            return false;
        }

        public string[] GetFiles()
        {
            string[] result = new string[fileCount];
            for (int i = 0; i < fileCount; i++)
            {
                result[i] = files[i];
            }
            return result;
        }

        public string GetFullPath()
        {
            // If there is no parent then we are in the root
            if (Parent == null)
                return "\\";

            string parentPath = Parent.GetFullPath();
            if (StringImplementations.CustomCompare(parentPath, "\\") == 0)
                return "\\" + Name;
            else
                return parentPath + "\\" + Name;
        }
    }
}
