using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
Format of WPK file on disk. Provided by @SkinSpotlights. Check them out: https://twitter.com/SkinSpotlights

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
    /// <summary>
    /// Represents a WPK file. WPK is a format used by the WWise Audio Engine to retain an archive of sound files (WEM files).
    /// </summary>
    public class WpkFile : IList<WemFile>
    {
        private const int Signature = 845427570;
        private const int Version = 1;
        private const int IndexOffset = 12;

        private List<WemFile> _soundFiles = new List<WemFile>();

        #region I/O
        /// <summary>
        /// Reads a WPK file from a path.
        /// </summary>
        /// <param name="path">Path to the WPK file.,</param>
        /// <returns>An instance of <see cref="WpkFile"/> representing the file on the path.</returns>
        public static WpkFile ReadFile(string path)
        {
            var file = new WpkFile();
            file.ReadFile(File.OpenRead(path));
            return file;
        }

        /// <summary>
        /// Fills in this <see cref="WpkFile"/> instance from a <see cref="Stream"/> instance.
        /// </summary>
        /// <param name="stream">The stream to read the file from.</param>
        private void ReadFile(Stream stream)
        {
            using (BinaryReader reader = new BinaryReader(stream, Encoding.UTF8, true))
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

            List<int> offsets = ReadFileEntryOffsets(stream);
            _soundFiles = ReadWemFilesFromOffsets(stream, offsets);

            stream.Dispose();
        }

        /// <summary>
        /// Writes a WPK file representing this instance of <see cref="WpkFile"/> to the path on disk.
        /// </summary>
        /// <param name="path">Path to write the file to.</param>
        public void WriteFile(string path)
        {
            WriteFile(File.Open(path, FileMode.OpenOrCreate, FileAccess.Write));
        }

        /// <summary>
        /// Writes a WPK file representing this instance of <see cref="WpkFile"/> to the provided <see cref="Stream"/> instance.
        /// </summary>
        /// <param name="stream">The stream to write the file to.</param>
        public void WriteFile(Stream stream)
        {
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write(Signature);
                writer.Write(Version);
                writer.Write(_soundFiles.Count);

                foreach (var wem in _soundFiles)
                {
                    writer.Write(wem.MetadataOffset);
                }

                foreach (var wem in _soundFiles)
                {
                    writer.Write(wem.DataOffset);
                    writer.Write(wem.DataLength);
                    writer.Write(wem.Name.Length);
                    writer.Write(Encoding.Unicode.GetBytes(wem.Name));
                }

                foreach (var wem in _soundFiles)
                {
                    writer.Write(wem.Data);
                }
            }
        }

        /// <summary>
        /// Reads the offsets of the file entries in the <see cref="Stream"/> holding a WPK file.
        /// </summary>
        /// <param name="stream">The stream to read the offsets from.</param>
        /// <returns></returns>
        private List<int> ReadFileEntryOffsets(Stream stream)
        {
            var ret = new List<int>();
            using (BinaryReader reader = new BinaryReader(stream, Encoding.UTF8, true))
            {
                int count = reader.ReadInt32();

                for (int i = 0; i < count; i++)
                {
                    ret.Add(reader.ReadInt32());
                }
            }

            return ret;
        }

        /// <summary>
        /// Reads the WEM files from the offsets provided in the <see cref="Stream"/> holding a WPK file.
        /// </summary>
        /// <param name="stream">The stream to read the offsets from.</param>
        /// <param name="offsets">The offsets of the WEM files.</param>
        /// <returns>A list of <see cref="WemFile"/> representing the WEM file entries in the stream.</returns>
        private List<WemFile> ReadWemFilesFromOffsets(Stream stream, List<int> offsets)
        {
            var ret = new List<WemFile>();
            using (BinaryReader reader = new BinaryReader(stream, Encoding.UTF8, true))
            {
                foreach (var offset in offsets)
                {
                    reader.BaseStream.Seek(offset, SeekOrigin.Begin);

                    var file = new WemFile();

                    var dataOffset = reader.ReadInt32();
                    var dataLength = reader.ReadInt32();
                    //Times 2 because null bytes.
                    var nameLength = reader.ReadInt32() * 2;

                    var name = Encoding.Unicode.GetString(reader.ReadBytes(nameLength));

                    reader.BaseStream.Seek(dataOffset, SeekOrigin.Begin);

                    var fileData = reader.ReadBytes(dataLength);

                    file.Name = name;
                    file.Data = fileData;

                    ret.Add(file);
                }
            }
            return ret;
        }
        #endregion

        #region IList Implementation

        public IEnumerator<WemFile> GetEnumerator()
        {
            return _soundFiles.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_soundFiles).GetEnumerator();
        }

        public void Add(WemFile item)
        {
            _soundFiles.Add(item);
            UpdateIndex();
        }

        public void Clear()
        {
            _soundFiles.Clear();
            UpdateIndex();
        }

        public void Insert(int index, WemFile item)
        {
            _soundFiles.Insert(index, item);
            UpdateIndex();
        }

        public void RemoveAt(int index)
        {
            _soundFiles.RemoveAt(index);
            UpdateIndex();
        }

        public bool Remove(WemFile item)
        {
            var ret = _soundFiles.Remove(item);
            UpdateIndex();
            return ret;
        }

        public bool Contains(WemFile item) => _soundFiles.Contains(item);

        public void CopyTo(WemFile[] array, int arrayIndex) => _soundFiles.CopyTo(array, arrayIndex);

        public int Count => _soundFiles.Count;

        public bool IsReadOnly => false;

        public int IndexOf(WemFile item) => _soundFiles.IndexOf(item);

        public WemFile this[int index]
        {
            get { return _soundFiles[index]; }
            set
            {
                _soundFiles[index] = value;
                UpdateIndex();
            }
        }

        #endregion

        /// <summary>
        /// Updates the internal <see cref="WemFile"/> list to correct all file offsets.
        /// This is necessary after each modification to the list in order to maintain the correct file entry offsets.
        /// These offsets are necessary when writing the WPK file to disk.
        /// </summary>
        private void UpdateIndex()
        {
            int currentOffset = IndexOffset + 4 * _soundFiles.Count;
            foreach (var wem in _soundFiles)
            {
                wem.MetadataOffset = currentOffset;
                currentOffset += 12 + wem.NameLength;
            }

            foreach (var wem in _soundFiles)
            {
                wem.DataOffset = currentOffset;
                currentOffset += wem.DataLength;
            }
        }
    }
}
