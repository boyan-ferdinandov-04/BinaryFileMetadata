using System;
using System.IO;

namespace BinaryFileMetadata
{
    public class FileSystemContainer
    {
        private string containerPath;
        public string ContainerPath => containerPath;

        public FileSystemContainer(string path)
        {
            containerPath = path;
            if (!File.Exists(containerPath))
            {
                using (File.Create(containerPath))
                {
                    // Ensure container file exists
                }
            }
        }

        public void CopyFileIntoContainer(string sourcePath, string containerFileName)
        {
            if (!File.Exists(sourcePath))
                throw new FileNotFoundException($"Source file '{sourcePath}' not found.");

            var bytes = File.ReadAllBytes(sourcePath);
            using (var stream = new FileStream(containerPath, FileMode.Append, FileAccess.Write))
            {
                WriteString(stream, containerFileName);
                WriteBytes(stream, bytes);
            }
        }

        public void CopyFileOutFromContainer(string containerFileName, string destinationPath)
        {
            using (var stream = new FileStream(containerPath, FileMode.Open, FileAccess.Read))
            {
                while (stream.Position < stream.Length)
                {
                    string fileName = ReadString(stream);
                    int fileLength = ReadInt(stream);

                    // Skip directories
                    if (fileName.StartsWith("D:"))
                    {
                        stream.Seek(fileLength, SeekOrigin.Current);
                        continue;
                    }

                    if (fileName == containerFileName)
                    {
                        byte[] fileData = ReadBytes(stream, fileLength);
                        File.WriteAllBytes(destinationPath, fileData);
                        return;
                    }
                    else
                    {
                        stream.Seek(fileLength, SeekOrigin.Current);
                    }
                }
            }
            throw new FileNotFoundException($"File '{containerFileName}' not found in the container.");
        }

        public void ListFiles()
        {
            using (var stream = new FileStream(containerPath, FileMode.Open, FileAccess.Read))
            {
                while (stream.Position < stream.Length)
                {
                    string entryName = ReadString(stream);
                    if (entryName == null) break;

                    int entryLength = ReadInt(stream);

                    if (entryName.StartsWith("D:"))
                    {
                        Console.WriteLine($"[Dir ] {entryName.Substring(2)}");
                    }
                    else
                    {
                        Console.WriteLine($"[File] {entryName}, Size: {entryLength} bytes");
                    }

                    stream.Seek(entryLength, SeekOrigin.Current);
                }
            }
        }

        public void RemoveFile(string fileName)
        {
            string tempPath = containerPath + ".tmp";

            using (var input = new FileStream(containerPath, FileMode.Open, FileAccess.Read))
            using (var output = new FileStream(tempPath, FileMode.Create, FileAccess.Write))
            {
                while (input.Position < input.Length)
                {
                    string currentFileName = ReadString(input);
                    int fileLength = ReadInt(input);

                    if (currentFileName != fileName)
                    {
                        WriteString(output, currentFileName);
                        WriteBytes(output, ReadBytes(input, fileLength));
                    }
                    else
                    {
                        // skip the file data
                        input.Seek(fileLength, SeekOrigin.Current);
                    }
                }
            }

            File.Delete(containerPath);
            File.Move(tempPath, containerPath);
        }

        // --- NEW METHODS FOR DIRECTORIES BELOW ---

        /// <summary>
        /// Create a directory entry. We store a special name "D:MyFolder"
        /// plus a (possibly empty) payload.
        /// </summary>
        public void CreateDirectoryEntry(string directoryName)
        {
            using (var stream = new FileStream(containerPath, FileMode.Append, FileAccess.Write))
            {
                string dirEntryName = "D:" + directoryName;
                WriteString(stream, dirEntryName);

                // For now, we can store an empty payload or minimal metadata.
                // Just store 0 bytes for directory metadata.
                byte[] emptyPayload = new byte[0];
                WriteBytes(stream, emptyPayload);
            }
        }

        /// <summary>
        /// Remove a directory entry by name "D:<dirName>" (assuming sub-contents already removed).
        /// </summary>
        public void RemoveDirectoryEntry(string directoryName)
        {
            string dirFullName = "D:" + directoryName;
            string tempPath = containerPath + ".tmp";

            using (var input = new FileStream(containerPath, FileMode.Open, FileAccess.Read))
            using (var output = new FileStream(tempPath, FileMode.Create, FileAccess.Write))
            {
                while (input.Position < input.Length)
                {
                    string currentName = ReadString(input);
                    int dataLength = ReadInt(input);

                    if (currentName == dirFullName)
                    {
                        // Skip these directory bytes
                        input.Seek(dataLength, SeekOrigin.Current);
                    }
                    else
                    {
                        // Copy as-is
                        WriteString(output, currentName);
                        WriteBytes(output, ReadBytes(input, dataLength));
                    }
                }
            }

            File.Delete(containerPath);
            File.Move(tempPath, containerPath);
        }

        // --- HELPER METHODS BELOW ---
        private byte[] ReadBytes(FileStream stream, int length)
        {
            byte[] data = new byte[length];
            stream.Read(data, 0, length);
            return data;
        }

        private void WriteString(FileStream stream, string value)
        {
            byte[] lengthBytes = BitConverter.GetBytes(value.Length);
            stream.Write(lengthBytes, 0, lengthBytes.Length);

            byte[] stringBytes = System.Text.Encoding.UTF8.GetBytes(value);
            stream.Write(stringBytes, 0, stringBytes.Length);
        }

        private string ReadString(FileStream stream)
        {
            byte[] lengthBytes = new byte[4];
            int readCount = stream.Read(lengthBytes, 0, lengthBytes.Length);
            if (readCount < 4) return null;

            int length = BitConverter.ToInt32(lengthBytes, 0);
            byte[] stringBytes = new byte[length];
            stream.Read(stringBytes, 0, length);
            return System.Text.Encoding.UTF8.GetString(stringBytes);
        }

        private void WriteBytes(FileStream stream, byte[] data)
        {
            byte[] lengthBytes = BitConverter.GetBytes(data.Length);
            stream.Write(lengthBytes, 0, lengthBytes.Length);
            stream.Write(data, 0, data.Length);
        }

        private int ReadInt(FileStream stream)
        {
            byte[] intBytes = new byte[4];
            stream.Read(intBytes, 0, intBytes.Length);
            return BitConverter.ToInt32(intBytes, 0);
        }
    }
}
