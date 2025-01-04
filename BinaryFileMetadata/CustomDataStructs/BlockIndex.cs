using System.Text;

namespace BinaryFileMetadata.CustomDataStructs
{
    public class BlockIndex
    {
        private CustomList<BlockRecord> blocks;

        public BlockIndex()
        {
            blocks = new CustomList<BlockRecord>();
        }

        public int FindBlockIndexByHash(string hash)
        {
            for (int i = 0; i < blocks.Count; i++)
            {
                if (StringImplementations.CustomCompare(blocks[i].BlockHash, hash) == 0)
                {
                    return i;
                }
            }
            return -1;
        }

        public int AddBlock(byte[] data)
        {
            string hash = ComputeHash(data);

            int existingIndex = FindBlockIndexByHash(hash);
            if (existingIndex >= 0)
            {
                blocks[existingIndex].ReferenceCount++;
                return existingIndex;
            }
            else
            {
                BlockRecord newBlock = new BlockRecord(hash, data);
                blocks.Add(newBlock);
                return blocks.Count - 1;
            }
        }

        public void DecrementRefCount(int blockIndex)
        {
            if (blockIndex < 0 || blockIndex >= blocks.Count)
                return;

            blocks[blockIndex].ReferenceCount--;
            if (blocks[blockIndex].ReferenceCount <= 0)
            {
                // Removing the block entirely
                blocks.RemoveAt(blockIndex);
            }
        }

        public BlockRecord GetBlockRecord(int blockIndex)
        {
            if (blockIndex < 0 || blockIndex >= blocks.Count)
                return null;
            return blocks[blockIndex];
        }

        public void Serialize(BinaryWriter writer)
        {
            // Writing the number of blocks
            writer.Write(blocks.Count);
            for (int i = 0; i < blocks.Count; i++)
            {
                BlockRecord br = blocks[i];
                WriteString(writer, br.BlockHash);
                writer.Write(br.BlockData.Length);
                writer.Write(br.BlockData);
                writer.Write(br.ReferenceCount);
            }
        }

        public void Deserialize(BinaryReader reader)
        {
            int numBlocks = reader.ReadInt32();
            for (int i = 0; i < numBlocks; i++)
            {
                string hash = ReadString(reader);
                int dataLength = reader.ReadInt32();
                byte[] data = reader.ReadBytes(dataLength);
                // Reading References
                int refCount = reader.ReadInt32();

                BlockRecord br = new BlockRecord(hash, data)
                {
                    ReferenceCount = refCount
                };
                blocks.Add(br);
            }
        }
        private string ComputeHash(byte[] data)
        {
            long sum = 0;
            for (int i = 0; i < data.Length; i++)
            {
                //1000000007 is a large prime number, which helps prevent overflow and reduces the chance of collisions in the hash values
                sum = (sum * 31 + data[i]) % 1000000007;
            }
            return "blk" + sum.ToString();
        }

        private void WriteString(BinaryWriter writer, string value)
        {
            writer.Write(value.Length);
            writer.Write(Encoding.UTF8.GetBytes(value));
        }

        private string ReadString(BinaryReader reader)
        {
            int length = reader.ReadInt32();
            byte[] bytes = reader.ReadBytes(length);
            return Encoding.UTF8.GetString(bytes);
        }
    }
}
