using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            using var stream = new FileStream(containerPath, FileMode.Open, FileAccess.Read);
            while (stream.Position < stream.Length)
            {
                string fileName = ReadString(stream);
                int fileLength = ReadInt(stream);

                if (fileName == containerFileName)
                {
                    byte[] fileData = new byte[fileLength];
                    stream.Read(fileData, 0, fileLength);

                    File.WriteAllBytes(destinationPath, fileData);
                    return;
                }
                else
                {
                    stream.Seek(fileLength, SeekOrigin.Current);
                }
            }

            throw new FileNotFoundException($"File '{containerFileName}' not found in the container.");
        }


        public void ListFiles()
        {
            using var stream = new FileStream(containerPath, FileMode.Open, FileAccess.Read);
            while (stream.Position < stream.Length)
            {
                string entryName = ReadString(stream);
                if (entryName == null) break;

                int entryLength = ReadInt(stream);

                // If it's a directory (starts with "D:"), skip it
                if (entryName.StartsWith("D:"))
                {
                    // Skip the data (which should be 0 for a directory)
                    if (entryLength > 0)
                        stream.Seek(entryLength, SeekOrigin.Current);

                    // Optionally print "[directory] <name>"
                    // or simply continue to ignore it.
                    continue;
                }

                // It's a file. Print the file listing
                Console.WriteLine($"File: {entryName}, Size: {entryLength} bytes");
                stream.Seek(entryLength, SeekOrigin.Current);
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
                        // Skip the file data
                        input.Seek(fileLength, SeekOrigin.Current);
                    }
                }
            }

            File.Delete(containerPath);
            File.Move(tempPath, containerPath);
        }
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
            stream.Read(lengthBytes, 0, lengthBytes.Length);
            int length = BitConverter.ToInt32(lengthBytes, 0);
            byte[] stringBytes = new byte[length];
            stream.Read(stringBytes, 0, stringBytes.Length);
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
