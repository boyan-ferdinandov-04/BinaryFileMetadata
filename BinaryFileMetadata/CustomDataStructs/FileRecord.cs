using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinaryFileMetadata.CustomDataStructs
{
    public class FileRecord
    {
        public string FullPath;         
        public CustomList<int> Blocks;   

        public FileRecord(string path)
        {
            FullPath = path;
            Blocks = new CustomList<int>();
        }


        public void Serialize(BinaryWriter writer)
        {
            // Write FullPath
            WriteString(writer, FullPath);
            // Write number of blocks
            writer.Write(Blocks.Count);
            // Write each block index
            for (int i = 0; i < Blocks.Count; i++)
            {
                writer.Write(Blocks[i]);
            }
        }

        public void Deserialize(BinaryReader reader)
        {
            // Read FullPath
            FullPath = ReadString(reader);
            // Read number of blocks
            int numBlocks = reader.ReadInt32();
            for (int i = 0; i < numBlocks; i++)
            {
                int blockIndex = reader.ReadInt32();
                Blocks.Add(blockIndex);
            }
        }

        private void WriteString(BinaryWriter writer, string value)
        {
            writer.Write(value.Length);
            writer.Write(System.Text.Encoding.UTF8.GetBytes(value));
        }


        private string ReadString(BinaryReader reader)
        {
            int length = reader.ReadInt32();
            byte[] bytes = reader.ReadBytes(length);
            return System.Text.Encoding.UTF8.GetString(bytes);
        }
    }
}
