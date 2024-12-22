using System;
using System.IO;
using System.Text;

namespace BinaryFileMetadata
{
    public class DirectoryManager
    {
        private string currentDirectory;
        private readonly FileSystemContainer fileSystemContainer;

        public DirectoryManager(FileSystemContainer fileSystem)
        {
            fileSystemContainer = fileSystem;
            // Start at the root directory
            currentDirectory = "/"; 
        }

        public void CreateDirectory(string directoryName)
        {
            if (string.IsNullOrWhiteSpace(directoryName))
                throw new ArgumentException("Directory name cannot be empty.");

            string normalizedPath = NormalizePath(Path.Combine(currentDirectory, directoryName));
            string directoryRecordName = "D:" + normalizedPath;

            if (DirectoryExists(directoryRecordName))
                throw new IOException($"Directory '{directoryName}' already exists.");

            using (var stream = new FileStream(fileSystemContainer.ContainerPath, FileMode.Append, FileAccess.Write))
            {
                WriteString(stream, directoryRecordName);
                WriteInt(stream, 0);
            }
        }

        public void ChangeDirectory(string targetDirectory)
        {
            if (targetDirectory == "/")
            {
                currentDirectory = "/";
                return;
            }

            if (targetDirectory == "..")
            {
                if (currentDirectory == "/")
                {
                    Console.WriteLine("Already at the root directory.");
                    return;
                }
                currentDirectory = NormalizePath(Path.GetDirectoryName(currentDirectory) ?? "/");
                return;
            }

            string newDirectoryPath = NormalizePath(Path.Combine(currentDirectory, targetDirectory));
            string directoryRecordName = "D:" + newDirectoryPath;

            if (!DirectoryExists(directoryRecordName))
                throw new DirectoryNotFoundException($"Directory '{targetDirectory}' not found.");

            currentDirectory = newDirectoryPath;
        }

        public void RemoveDirectory(string directoryName)
        {
            if (string.IsNullOrWhiteSpace(directoryName))
                throw new ArgumentException("Directory name cannot be empty.");

            string normalizedPath = NormalizePath(Path.Combine(currentDirectory, directoryName));
            string directoryRecordName = "D:" + normalizedPath;

            if (!DirectoryExists(directoryRecordName))
                throw new DirectoryNotFoundException($"Directory '{directoryName}' not found.");

            MarkAsDeleted(directoryRecordName);
        }

        private bool DirectoryExists(string directoryRecordName)
        {
            using (var stream = new FileStream(fileSystemContainer.ContainerPath, FileMode.Open, FileAccess.Read))
            {
                while (stream.Position < stream.Length)
                {
                    string recordName = ReadString(stream);
                    if (recordName == null) break; // End of file or error
                    int dataLength = ReadInt(stream);

                    if (recordName == directoryRecordName)
                        return true;

                    // Skip the data (file or directory data). For directories, dataLength should be 0.
                    if (dataLength > 0)
                        stream.Seek(dataLength, SeekOrigin.Current);
                }
            }
            return false;
        }

        private void MarkAsDeleted(string directoryRecordName)
        {
            string tempPath = fileSystemContainer.ContainerPath + ".tmp";

            using (var input = new FileStream(fileSystemContainer.ContainerPath, FileMode.Open, FileAccess.Read))
            using (var output = new FileStream(tempPath, FileMode.Create, FileAccess.Write))
            {
                while (input.Position < input.Length)
                {
                    string recordName = ReadString(input);
                    if (recordName == null) break; // End of data
                    int dataLength = ReadInt(input);

                    if (recordName == directoryRecordName)
                    {
                        // This is the directory to remove, so skip writing it out.
                        // Also skip the data (which should be 0).
                        if (dataLength > 0)
                            input.Seek(dataLength, SeekOrigin.Current);
                    }
                    else
                    {
                        // Keep this record
                        WriteString(output, recordName);
                        WriteInt(output, dataLength);

                        // Copy existing data
                        if (dataLength > 0)
                        {
                            byte[] buffer = new byte[4096];
                            int remaining = dataLength;
                            while (remaining > 0)
                            {
                                int toRead = Math.Min(buffer.Length, remaining);
                                int readBytes = input.Read(buffer, 0, toRead);
                                if (readBytes <= 0) break;
                                output.Write(buffer, 0, readBytes);
                                remaining -= readBytes;
                            }
                        }
                    }
                }
            }

            File.Delete(fileSystemContainer.ContainerPath);
            File.Move(tempPath, fileSystemContainer.ContainerPath);
        }

        private string NormalizePath(string path)
        {
            // Convert backslashes to forward slashes
            path = path.Replace("\\", "/");
            // Trim trailing slashes unless it's the root "/"
            if (path.EndsWith("/") && path != "/")
                path = path.TrimEnd('/');

            return path;
        }


        private void WriteString(FileStream stream, string value)
        {
            // If value is null, treat as empty
            if (value == null) value = "";
            byte[] stringBytes = Encoding.UTF8.GetBytes(value);
            WriteInt(stream, stringBytes.Length);
            stream.Write(stringBytes, 0, stringBytes.Length);
        }

        private string ReadString(FileStream stream)
        {
            byte[] lengthBytes = new byte[4];
            int readLen = stream.Read(lengthBytes, 0, lengthBytes.Length);
            if (readLen < 4) return null; // Not enough data to read length

            int length = BitConverter.ToInt32(lengthBytes, 0);
            if (length < 0) return null; // Invalid length

            byte[] stringBytes = new byte[length];
            int actuallyRead = stream.Read(stringBytes, 0, length);
            if (actuallyRead < length) return null; // Not enough data

            return Encoding.UTF8.GetString(stringBytes);
        }

        private void WriteInt(FileStream stream, int value)
        {
            byte[] intBytes = BitConverter.GetBytes(value);
            stream.Write(intBytes, 0, intBytes.Length);
        }

        private int ReadInt(FileStream stream)
        {
            byte[] intBytes = new byte[4];
            int readLen = stream.Read(intBytes, 0, intBytes.Length);
            if (readLen < 4) return -1; // Could throw an exception or handle gracefully
            return BitConverter.ToInt32(intBytes, 0);
        }

    }
}
