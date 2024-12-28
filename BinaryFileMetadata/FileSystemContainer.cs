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

        public void CopyFileIntoContainer(string sourcePath, string fullPath)
        {
            if (!File.Exists(sourcePath))
                throw new FileNotFoundException($"Source file '{sourcePath}' not found.");

            var bytes = File.ReadAllBytes(sourcePath);
            using (var stream = new FileStream(containerPath, FileMode.Append, FileAccess.Write))
            {
                WriteString(stream, fullPath);
                WriteBytes(stream, bytes);
            }

            // Debug statement
            Console.WriteLine($"Debug: Stored file '{fullPath}' with size {bytes.Length} bytes.");
        }

        public void CopyFileOutFromContainer(string fullPath, string destinationPath)
        {
            using (var stream = new FileStream(containerPath, FileMode.Open, FileAccess.Read))
            {
                while (stream.Position < stream.Length)
                {
                    string entryName = ReadString(stream);
                    if (entryName == null) break;

                    int entryLength = ReadInt(stream);

                    // Skip directories
                    if (entryName.StartsWith("D:"))
                    {
                        stream.Seek(entryLength, SeekOrigin.Current);
                        continue;
                    }

                    if (entryName == fullPath)
                    {
                        byte[] fileData = ReadBytes(stream, entryLength);
                        File.WriteAllBytes(destinationPath, fileData);
                        return;
                    }
                    else
                    {
                        stream.Seek(entryLength, SeekOrigin.Current);
                    }
                }
            }
            throw new FileNotFoundException($"File '{fullPath}' not found in the container.");
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

        public void RemoveFile(string fullPath)
        {
            string tempPath = containerPath + ".tmp";

            using (var input = new FileStream(containerPath, FileMode.Open, FileAccess.Read))
            using (var output = new FileStream(tempPath, FileMode.Create, FileAccess.Write))
            {
                while (input.Position < input.Length)
                {
                    string currentName = ReadString(input);
                    int dataLength = ReadInt(input);

                    if (currentName == fullPath)
                    {
                        // Skip this entry
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

        public void CreateDirectoryEntry(string fullPath)
        {
            using (var stream = new FileStream(containerPath, FileMode.Append, FileAccess.Write))
            {
                string dirEntryName = "D:" + fullPath;
                WriteString(stream, dirEntryName);

                // Store an empty payload for directories
                byte[] emptyPayload = new byte[0];
                WriteBytes(stream, emptyPayload);
            }
        }


        public long GetFileSizeInContainer(string fullPath)
        {

            using (var stream = new FileStream(containerPath, FileMode.Open, FileAccess.Read))
            {
                while (stream.Position < stream.Length)
                {
                    string entryName = ReadString(stream);
                    if (entryName == null)
                    {
                        Console.WriteLine("Debug: Reached end of container without finding the file.");
                        break;
                    }

                    int entryLength = ReadInt(stream);

                    
                    // Skip directories
                    if (entryName.StartsWith("D:"))
                    {
                        stream.Seek(entryLength, SeekOrigin.Current);
                        continue;
                    }

                    // It's a file
                    if (String.Equals(entryName, fullPath, StringComparison.OrdinalIgnoreCase))
                    {
                        // Move past the file data
                        stream.Seek(entryLength, SeekOrigin.Current);
                        return entryLength;
                    }
                    else
                    {
                        // Not a match; skip the file data
                        stream.Seek(entryLength, SeekOrigin.Current);
                    }
                }
            }

            // Not found in container
            return -1;
        }




        // Helper methods
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
