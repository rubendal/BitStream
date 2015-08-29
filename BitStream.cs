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

        public byte ReadBit()
        {
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
            for(long i=0;i< bits; )
            {
                byte value = 0;
                for(int p=0;p<8 && i < bits; i++,p++)
                {
                    if (!bigEndian)
                    {
                        value |= (byte)(ReadBit() << p);
                    }
                    else
                    {
                        value |= (byte)(ReadBit() << (7-p));
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

        public long Length
        {
            get
            {
                return stream.Length;
            }
        }
    }
}
