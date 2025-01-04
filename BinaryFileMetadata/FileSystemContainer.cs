using BinaryFileMetadata.CustomDataStructs;
using System;
using System.IO;

namespace BinaryFileMetadata
{
    // A container that stores files in blocks of fixed size, with deduplication via BlockIndex.
    // It serializes/deserializes the BlockIndex and FileRecords to/from the container file.
    public class FileSystemContainer
    {
        private string containerPath;
        public string ContainerPath => containerPath;

        private int blockSize;
        public int BlockSize => blockSize;

        // The global block index for deduplication
        private BlockIndex blockIndex;

        // Each file stored as a list of references to blocks in blockIndex
        public CustomList<FileRecord> fileRecords;


        public FileSystemContainer(string path, int blockSize)
        {
            this.containerPath = path;
            this.blockSize = blockSize;

            blockIndex = new BlockIndex();
            fileRecords = new CustomList<FileRecord>();

            if (!File.Exists(containerPath))
            {
                // Create an empty container with initial metadata
                using (FileStream fs = new FileStream(containerPath, FileMode.Create, FileAccess.Write))
                using (BinaryWriter writer = new BinaryWriter(fs))
                {
                    // Write blockSize
                    writer.Write(blockSize);
                    // Write empty BlockIndex
                    blockIndex.Serialize(writer);
                    // Write empty FileRecords
                    writer.Write(fileRecords.Count);
                }
            }
            else
            {
                // Load existing data
                using (FileStream fs = new FileStream(containerPath, FileMode.Open, FileAccess.Read))
                using (BinaryReader reader = new BinaryReader(fs))
                {
                    // Read blockSize
                    blockSize = reader.ReadInt32();
                    // Read BlockIndex
                    blockIndex.Deserialize(reader);
                    // Read FileRecords
                    int numFiles = reader.ReadInt32();
                    for (int i = 0; i < numFiles; i++)
                    {
                        FileRecord fr = new FileRecord("");
                        fr.Deserialize(reader);
                        fileRecords.Add(fr);
                    }
                }
            }
        }

        public void CopyFileIntoContainer(string sourcePath, string fullPath)
        {
            if (!File.Exists(sourcePath))
                throw new FileNotFoundException($"Source file '{sourcePath}' not found.");

            // Read entire file from the local filesystem
            byte[] allBytes = File.ReadAllBytes(sourcePath);

            // Create a new FileRecord
            FileRecord fileRecord = new FileRecord(fullPath);

            // Break the data into blocks of size 'blockSize'
            int offset = 0;
            while (offset < allBytes.Length)
            {
                int chunkSize = Math.Min(blockSize, allBytes.Length - offset);
                byte[] blockData = new byte[chunkSize];
                for (int i = 0; i < chunkSize; i++)
                {
                    blockData[i] = allBytes[offset + i];
                }
                offset += chunkSize;

                // Add the block to the BlockIndex (dedup)
                int blockIndexId = blockIndex.AddBlock(blockData);

                // Add that block index to the FileRecord
                fileRecord.Blocks.Add(blockIndexId);
            }

            // Add the FileRecord to our in-memory list
            fileRecords.Add(fileRecord);

            // Serialize the updated BlockIndex and FileRecords to the container
            SerializeContainer();

            Console.WriteLine($"Debug: '{sourcePath}' stored as '{fullPath}' in blocks of size {blockSize}.");
        }

        public void CopyFileOutFromContainer(string fullPath, string destinationPath)
        {
            int fileIndex = FindFileRecordIndex(fullPath);
            if (fileIndex < 0)
            {
                throw new FileNotFoundException($"File '{fullPath}' not found in container.");
            }

            FileRecord record = fileRecords[fileIndex];

            using (var fs = new FileStream(destinationPath, FileMode.Create, FileAccess.Write))
            {
                // For each block ID in the file, fetch the data and write out
                for (int i = 0; i < record.Blocks.Count; i++)
                {
                    BlockRecord br = blockIndex.GetBlockRecord(record.Blocks[i]);
                    if (br != null)
                    {
                        fs.Write(br.BlockData, 0, br.BlockData.Length);
                    }
                }
            }
            Console.WriteLine($"Debug: Copied file '{fullPath}' to '{destinationPath}'.");
        }

        public void RemoveFile(string fullPath)
        {
            int fileIndex = FindFileRecordIndex(fullPath);
            if (fileIndex < 0)
            {
                Console.WriteLine($"File '{fullPath}' not found in container.");
                return;
            }

            FileRecord record = fileRecords[fileIndex];

            // Decrement the reference count for each block
            for (int i = 0; i < record.Blocks.Count; i++)
            {
                blockIndex.DecrementRefCount(record.Blocks[i]);
            }

            // Remove the FileRecord from our list
            fileRecords.RemoveAt(fileIndex);

            // Serialize the updated BlockIndex and FileRecords to the container
            SerializeContainer();

            Console.WriteLine($"Debug: Removed file '{fullPath}' from container.");
        }

        public long GetFileSizeInContainer(string fullPath)
        {
            int fileIndex = FindFileRecordIndex(fullPath);
            if (fileIndex < 0)
                return -1;

            FileRecord record = fileRecords[fileIndex];
            long totalSize = 0;
            for (int i = 0; i < record.Blocks.Count; i++)
            {
                BlockRecord br = blockIndex.GetBlockRecord(record.Blocks[i]);
                if (br != null)
                {
                    totalSize += br.BlockData.Length;
                }
            }
            return totalSize;
        }

        public void ListFiles()
        {
            Console.WriteLine("=== Files in Container (Deduplicated) ===");
            for (int i = 0; i < fileRecords.Count; i++)
            {
                FileRecord record = fileRecords[i];
                long size = GetFileSizeInContainer(record.FullPath);
                Console.WriteLine($"[File] {record.FullPath}, Size: {size} B, Blocks: {record.Blocks.Count}");
            }
            Console.WriteLine("=========================================");
        }

        private int FindFileRecordIndex(string fullPath)
        {
            for (int i = 0; i < fileRecords.Count; i++)
            {
                if (StringImplementations.CustomCompare(fileRecords[i].FullPath, fullPath) == 0)
                {
                    return i;
                }
            }
            return -1;
        }

        private void SerializeContainer()
        {
            using (FileStream fs = new FileStream(containerPath, FileMode.Create, FileAccess.Write))
            using (BinaryWriter writer = new BinaryWriter(fs))
            {
                // Write blockSize
                writer.Write(blockSize);
                // Serialize BlockIndex
                blockIndex.Serialize(writer);
                // Serialize FileRecords
                writer.Write(fileRecords.Count);
                for (int i = 0; i < fileRecords.Count; i++)
                {
                    fileRecords[i].Serialize(writer);
                }
            }
        }

        public int FileRecordCount
        {
            get 
            { 
                return fileRecords.Count; 
            }
        }

        public FileRecord GetFileRecord(int index)
        {
            return fileRecords[index];
        }

    }
}
