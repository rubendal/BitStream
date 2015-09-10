using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace BitStream
{
    public class BitStream
    {
        private long index { get; set; }
        private int bit { get; set; }
        private bool bigEndian { get; set; }
        private Stream stream;

        public long Length
        {
            get
            {
                return stream.Length;
            }
        }

        #region Constructors

        public BitStream(Stream stream, bool bigEndian = false)
        {
            this.stream = stream;
            this.bigEndian = bigEndian;
            index = 0;
            bit = 0;
        }

        public BitStream(byte[] buffer, bool bigEndian = false)
        {
            this.stream = new MemoryStream(buffer);
            this.bigEndian = bigEndian;
            index = 0;
            bit = 0;
        }

        #endregion

        #region Methods

        public void Seek(long index, int bit)
        {
            if (index > Length)
            {
                this.index = Length;
            }
            else
            {
                this.index = index;
            }
            if (bit >= 8)
            {
                this.bit = 0;
            }
            else
            {
                this.bit = bit;
            }
            stream.Seek(index, SeekOrigin.Begin);
        }

        public Stream GetStream()
        {
            return stream;
        }

        public byte[] GetStreamData()
        {
            stream.Seek(0, SeekOrigin.Begin);
            MemoryStream s = new MemoryStream();
            stream.CopyTo(s);
            return s.ToArray();
        }

        #endregion

        #region BitRead/Write

        public byte ReadBit()
        {
            stream.Seek(index, SeekOrigin.Begin);
            byte value;
            if (!bigEndian)
            {
                value = (byte)((stream.ReadByte() >> (bit)) & 1);
            }
            else
            {
                value = (byte)((stream.ReadByte() >> (7 - bit)) & 1);
            }
            bit = (bit + 1) % 8;
            if (bit == 0)
            {
                index++;
            }
            stream.Seek(index, SeekOrigin.Begin);
            return value;
        }

        public byte[] ReadBits(long bits)
        {
            List<byte> data = new List<byte>();
            for (long i = 0; i < bits;)
            {
                byte value = 0;
                for (int p = 0; p < 8 && i < bits; i++, p++)
                {
                    if (!bigEndian)
                    {
                        value |= (byte)(ReadBit() << p);
                    }
                    else
                    {
                        value |= (byte)(ReadBit() << (7 - p));
                    }
                }
                data.Add(value);
            }
            return data.ToArray();
        }

        public void WriteBit(byte data)
        {
            stream.Seek(index, SeekOrigin.Begin);
            byte value = (byte)stream.ReadByte();
            stream.Seek(index, SeekOrigin.Begin);
            if (!bigEndian)
            {
                value &= (byte)~(1 << bit);
                value |= (byte)(data << bit);
            }
            else
            {
                value &= (byte)~(1 << (7 - bit));
                value |= (byte)(data << (7 - bit));
            }
            stream.WriteByte(value);
            bit = (bit + 1) % 8;
            if (bit == 0)
            {
                index++;
            }
            stream.Seek(index, SeekOrigin.Begin);
        }

        public void WriteBits(byte[] data, long bits)
        {
            int position = 0;
            for (long i = 0; i < bits;)
            {
                byte value = 0;
                for (int p = 0; p < 8 && i < bits; i++, p++)
                {
                    if (!bigEndian)
                    {
                        value = (byte)((data[position] >> p) & 1);
                    }
                    else
                    {
                        value = (byte)((data[position] >> (7 - p)) & 1);
                    }
                    WriteBit(value);
                }
                position++;
            }
        }

        #endregion

        #region Read

        public byte ReadByte()
        {
            return ReadBits(8)[0];
        }

        public bool ReadBool()
        {
            return ReadBits(8)[0] == 0 ? false : true;
        }

        public short ReadInt16()
        {
            short value = BitConverter.ToInt16(ReadBits(16), 0);
            return value;
        }

        public int ReadInt24()
        {
            int value = BitConverter.ToInt32(ReadBits(24), 0);
            return value;
        }

        public int ReadInt32()
        {
            int value = BitConverter.ToInt32(ReadBits(32), 0);
            return value;
        }

        public long ReadInt48()
        {
            long value = BitConverter.ToInt64(ReadBits(48), 0);
            return value;
        }

        public long ReadInt64()
        {
            long value = BitConverter.ToInt64(ReadBits(64), 0);
            return value;
        }

        public ushort ReadUInt16()
        {
            ushort value = BitConverter.ToUInt16(ReadBits(16), 0);
            return value;
        }

        public uint ReadUInt24()
        {
            uint value = BitConverter.ToUInt16(ReadBits(24), 0);
            return value;
        }

        public uint ReadUInt32()
        {
            uint value = BitConverter.ToUInt32(ReadBits(32), 0);
            return value;
        }

        public ulong ReadUInt48()
        {
            ulong value = BitConverter.ToUInt64(ReadBits(48), 0);
            return value;
        }

        public ulong ReadUInt64()
        {
            ulong value = BitConverter.ToUInt64(ReadBits(64), 0);
            return value;
        }

        #endregion

        #region Write

        public void WriteByte(byte value)
        {
            WriteBits(new byte[] { value }, 8);
        }

        public void WriteBool(bool value)
        {
            WriteBits(new byte[] { value ? (byte)1 : (byte)0 }, 8);
        }

        public void WriteInt16(short value)
        {
            WriteBits(BitConverter.GetBytes(value), 16);
        }

        public void WriteInt24(int value)
        {
            WriteBits(BitConverter.GetBytes(value), 24);
        }

        public void WriteInt32(int value)
        {
            WriteBits(BitConverter.GetBytes(value), 32);
        }

        public void WriteInt48(long value)
        {
            WriteBits(BitConverter.GetBytes(value), 48);
        }

        public void WriteInt64(long value)
        {
            WriteBits(BitConverter.GetBytes(value), 64);
        }

        public void WriteUInt16(ushort value)
        {
            WriteBits(BitConverter.GetBytes(value), 16);
        }

        public void WriteUInt24(uint value)
        {
            WriteBits(BitConverter.GetBytes(value), 24);
        }

        public void WriteUInt32(uint value)
        {
            WriteBits(BitConverter.GetBytes(value), 32);
        }

        public void WriteUInt48(ulong value)
        {
            WriteBits(BitConverter.GetBytes(value), 48);
        }

        public void WriteUInt64(ulong value)
        {
            WriteBits(BitConverter.GetBytes(value), 64);
        }

        #endregion

    }
}
