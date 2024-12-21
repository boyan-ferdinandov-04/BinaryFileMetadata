using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinaryFileMetadata
{
    public class DirectoryManager
    {
        private FileSystemContainer _fileSystem;

        public DirectoryManager(FileSystemContainer fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public void CreateDirectory(string directoryName)
        {
            string directoryMaker = $"{directoryName}/";
            _fileSystem.CopyFileIntoContainer("", directoryMaker);
        }
    }
}
