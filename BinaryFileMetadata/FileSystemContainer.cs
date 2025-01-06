using BinaryFileMetadata.CustomDataStructs;

namespace BinaryFileMetadata
{
    // A container that stores files in blocks of fixed size, with deduplication via BlockIndex.
    // It serializes/deserializes the BlockIndex and FileRecords to/from the container file.
    public class FileSystemContainer
    {
        private string containerPath;
        private int blockSize;
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
                    writer.Write(blockSize);
                    blockIndex.Serialize(writer);
                    writer.Write(fileRecords.Count);
                }
            }
            else
            {
                // Loading existing data
                using (FileStream fs = new FileStream(containerPath, FileMode.Open, FileAccess.Read))
                using (BinaryReader reader = new BinaryReader(fs))
                {
                    blockSize = reader.ReadInt32();
                    blockIndex.Deserialize(reader);
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
            byte[] allBytes = File.ReadAllBytes(sourcePath);

            FileRecord fileRecord = new FileRecord(fullPath);

            // Breaking the data into blocks
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
                int blockIndexId = blockIndex.AddBlock(blockData);
                fileRecord.Blocks.Add(blockIndexId);
            }

            fileRecords.Add(fileRecord);

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

            for (int i = 0; i < record.Blocks.Count; i++)
            {
                blockIndex.DecrementRefCount(record.Blocks[i]);
            }

            // Removing the FileRecord from our list
            fileRecords.RemoveAt(fileIndex);

            // Serializing the updated BlockIndex and FileRecords to the container
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
                writer.Write(blockSize);
                blockIndex.Serialize(writer);
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
