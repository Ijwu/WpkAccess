using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
4 byte "r3d2"
4 byte version (1)
4 byte Amount of WEM Files

For WEMFile Count
4 Byte Offset To WEM
Loop

At each WEMOffset
4 byte Offset to WEM Sound File
4 Byte data length
4 Byte name length
Unicode String for name length

At each Offset for WEM Sound File is just the data for the WEM
*/

namespace WPKAccess
{
    public class WpkFile
    {
        private const int Signature = 845427570;
        private const int Version = 1;

        private int FileCount;

        public List<WemFile> SoundFiles = new List<WemFile>();

        public static WpkFile ReadFile(string path)
        {
            var file = new WpkFile();
            file.ReadFile(File.OpenRead(path));
            return file;
        }

        public void ReadFile(Stream fileStream)
        {
            using (BinaryReader reader = new BinaryReader(fileStream, Encoding.UTF8, true))
            {
                int signature = reader.ReadInt32();
                if (signature != Signature)
                {
                    throw new FormatException("Signature was incorrect for WPK file.");
                }

                int version = reader.ReadInt32();
                if (version != Version)
                {
                    throw new FormatException("Version was incorrect for WPK file.");
                }
            }

            List<int> offsets = ReadFileEntryOffsets(fileStream);
            SoundFiles = ReadWemFilesFromOffsets(fileStream, offsets);

            fileStream.Dispose();
        }

        private List<int> ReadFileEntryOffsets(Stream fileStream)
        {
            var ret = new List<int>();
            using (BinaryReader reader = new BinaryReader(fileStream, Encoding.UTF8, true))
            {
                FileCount = reader.ReadInt32();

                for (int i = 0; i < FileCount; i++)
                {
                    ret.Add(reader.ReadInt32());    
                }
            }

            return ret;
        }

        private List<WemFile> ReadWemFilesFromOffsets(Stream fileStream, List<int> offsets)
        {
            var ret = new List<WemFile>();
            using (BinaryReader reader = new BinaryReader(fileStream, Encoding.UTF8, true))
            {
                foreach (var offset in offsets)
                {
                    reader.BaseStream.Seek(offset, SeekOrigin.Begin);

                    var file = new WemFile();

                    var dataOffset = reader.ReadInt32();
                    var dataLength = reader.ReadInt32();
                    var nameLength = reader.ReadInt32();

                    var name = Encoding.UTF8.GetString(reader.ReadBytes(nameLength));

                    reader.BaseStream.Seek(dataOffset, SeekOrigin.Begin);

                    var fileData = reader.ReadBytes(dataLength);

                    file.DataLength = dataLength;
                    file.Name = name;
                    file.Data = fileData;

                    ret.Add(file);
                }
            }
            return ret;
        } 
    }
}
