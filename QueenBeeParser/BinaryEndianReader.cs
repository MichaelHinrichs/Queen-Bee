using System;
using System.IO;

namespace Nanook.QueenBee.Parser
{
    public enum EndianType
    {
        Big,
        Little
    }

    public class BinaryEndianReader : BinaryReader
    {
        public BinaryEndianReader(Stream input)
            : base(input)
        {
        }

        public uint ReadUInt32(EndianType endianType)
        {
            if ((BitConverter.IsLittleEndian && endianType != EndianType.Little) || (!BitConverter.IsLittleEndian && endianType != EndianType.Big))
            {
                byte[] b = base.ReadBytes(4);
                Array.Reverse(b);
                return BitConverter.ToUInt32(b, 0);
            }
            else
                return base.ReadUInt32();
        }

        public int ReadInt32(EndianType endianType)
        {
            if ((BitConverter.IsLittleEndian && endianType != EndianType.Little) || (!BitConverter.IsLittleEndian && endianType != EndianType.Big))
            {
                byte[] b = base.ReadBytes(4);
                Array.Reverse(b);
                return BitConverter.ToInt32(b, 0);
            }
            else
                return base.ReadInt32();
        }

        public ushort ReadUInt16(EndianType endianType)
        {
            if ((BitConverter.IsLittleEndian && endianType != EndianType.Little) || (!BitConverter.IsLittleEndian && endianType != EndianType.Big))
            {
                byte[] b = base.ReadBytes(2);
                Array.Reverse(b);
                return BitConverter.ToUInt16(b, 0);
            }
            else
                return base.ReadUInt16();
        }

        public short ReadInt16(EndianType endianType)
        {
            if ((BitConverter.IsLittleEndian && endianType != EndianType.Little) || (!BitConverter.IsLittleEndian && endianType != EndianType.Big))
            {
                byte[] b = base.ReadBytes(2);
                Array.Reverse(b);
                return BitConverter.ToInt16(b, 0);
            }
            else
                return base.ReadInt16();
        }

        public float ReadSingle(EndianType endianType)
        {
            if ((BitConverter.IsLittleEndian && endianType != EndianType.Little) || (!BitConverter.IsLittleEndian && endianType != EndianType.Big))
            {
                byte[] b = base.ReadBytes(4);
                Array.Reverse(b);
                return BitConverter.ToSingle(b, 0);
            }
            else
                return base.ReadSingle();
        }

    }
}
