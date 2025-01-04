using System.Text;

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
            WriteString(writer, FullPath);
            writer.Write(Blocks.Count);
            for (int i = 0; i < Blocks.Count; i++)
            {
                writer.Write(Blocks[i]);
            }
        }

        public void Deserialize(BinaryReader reader)
        {
            FullPath = ReadString(reader);
            int numBlocks = reader.ReadInt32();
            for (int i = 0; i < numBlocks; i++)
            {
                int blockIndex = reader.ReadInt32();
                Blocks.Add(blockIndex);
            }
        }

        private void WriteString(BinaryWriter writer, string value)
        {
            writer.Write(StringImplementations.Length(value));
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
