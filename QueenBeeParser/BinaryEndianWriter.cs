using System;
using System.IO;

namespace Nanook.QueenBee.Parser
{

    public class BinaryEndianWriter : BinaryWriter
    {
        public BinaryEndianWriter(Stream input)
            : base(input)
        {
        }
        
        public void Write(uint value, EndianType endianType)
        {
            if ((BitConverter.IsLittleEndian && endianType != EndianType.Little) || (!BitConverter.IsLittleEndian && endianType != EndianType.Big))
            {
                byte[] b = BitConverter.GetBytes(value);
                Array.Reverse(b);
                base.Write(b, 0, 4);
            }
            else
                base.Write(value);
        }

        public void Write(int value, EndianType endianType)
        {
            if ((BitConverter.IsLittleEndian && endianType != EndianType.Little) || (!BitConverter.IsLittleEndian && endianType != EndianType.Big))
            {
                byte[] b = BitConverter.GetBytes(value);
                Array.Reverse(b);
                base.Write(b, 0, 4);
            }
            else
                base.Write(value);
        }

        public void Write(ushort value, EndianType endianType)
        {
            if ((BitConverter.IsLittleEndian && endianType != EndianType.Little) || (!BitConverter.IsLittleEndian && endianType != EndianType.Big))
            {
                byte[] b = BitConverter.GetBytes(value);
                Array.Reverse(b);
                base.Write(b, 0, 2);
            }
            else
                base.Write(value);
        }

        public void Write(short value, EndianType endianType)
        {
            if ((BitConverter.IsLittleEndian && endianType != EndianType.Little) || (!BitConverter.IsLittleEndian && endianType != EndianType.Big))
            {
                byte[] b = BitConverter.GetBytes(value);
                Array.Reverse(b);
                base.Write(b, 0, 2);
            }
            else
                base.Write(value);
        }

        public void Write(float value, EndianType endianType)
        {
            if ((BitConverter.IsLittleEndian && endianType != EndianType.Little) || (!BitConverter.IsLittleEndian && endianType != EndianType.Big))
            {
                byte[] b = BitConverter.GetBytes(value);
                Array.Reverse(b);
                base.Write(b, 0, 4);
            }
            else
                base.Write(value);
        }
    }
}
