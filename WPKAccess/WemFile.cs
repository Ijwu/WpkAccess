using System;
using System.Text;

namespace WPKAccess
{
    public class WemFile : IEquatable<WemFile>
    {
        public int DataOffset;
        public int MetadataOffset;
        public string Name;
        public int NameLength => Encoding.UTF8.GetByteCount(Name);
        public int DataLength => Data.Length;
        public byte[] Data;

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
            return Equals((WemFile) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (DataLength*397) ^ (Data != null ? Data.GetHashCode() : 0);
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
    }
}