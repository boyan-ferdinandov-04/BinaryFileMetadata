namespace BinaryFileMetadata.CustomDataStructs
{
    public class BlockRecord
    {
        // a simple string representing the hash
        public string BlockHash;
        // Actual block contents
        public byte[] BlockData;
        // Number of files referencing this block
        public int ReferenceCount;  

        public BlockRecord(string hash, byte[] data)
        {
            BlockHash = hash;
            BlockData = data;
            ReferenceCount = 1;
        }
    }
}
