using System;
using System.Text;

namespace WPKAccess
{
    /// <summary>
    /// Represents a WEM file entry in a <see cref="WpkFile"/>.
    /// </summary>
    public class WemFile : IEquatable<WemFile>
    {
        /// <summary>
        /// The offset into the <see cref="WpkFile"/> where the data for this <see cref="WemFile"/> resides.
        /// </summary>
        public int DataOffset { get; set; }

        /// <summary>
        /// The offset into the <see cref="WpkFile"/> where the metadata for this <see cref="WemFile"/> resides.
        /// The metadata includes things such as data length and file name.
        /// </summary>
        public int MetadataOffset { get; set; }

        /// <summary>
        /// The name of this WEM file in the archive.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The length of the name of this file.
        /// </summary>
        public int NameLength => Encoding.UTF8.GetByteCount(Name);

        /// <summary>
        /// The length of the data of this file.
        /// </summary>
        public int DataLength => Data.Length;

        /// <summary>
        /// The binary data which this <see cref="WemFile"/> represents in a <see cref="WpkFile"/>.
        /// </summary>
        public byte[] Data { get; set; }

        #region IEquatable Implementation
        public bool Equals(WemFile other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return DataLength == other.DataLength && Equals(Data, other.Data);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((WemFile)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (DataLength * 397) ^ (Data?.GetHashCode() ?? 0);
            }
        }

        public static bool operator ==(WemFile left, WemFile right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(WemFile left, WemFile right)
        {
            return !Equals(left, right);
        }
        #endregion
    }
}