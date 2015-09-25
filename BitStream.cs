using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace BitStreams
{
    public class BitStream
    {
        private long offset { get; set; }
        private int bit { get; set; }
        private bool MSB { get; set; }
        private Stream stream;
        private Encoding encoding;
        /// <summary>
        /// Allows the <see cref="BitStream"/> auto increase in size when needed
        /// </summary>
        public bool AutoIncreaseStream { get; set; }

        /// <summary>
        /// Get the stream length
        /// </summary>
        public long Length
        {
            get
            {
                return stream.Length;
            }
        }

        /// <summary>
        /// Get the current bit position in the stream
        /// </summary>
        public long BitPosition
        {
            get
            {
                return bit;
            }
        }

        /// <summary>
        /// Check if <see cref="BitStream"/> offset is inside the stream length
        /// </summary>
        private bool ValidPosition
        {
            get
            {
                return offset < Length;
            }
        }

        #region Constructors

        /// <summary>
        /// Creates a BitStream using a Stream
        /// </summary>
        /// <param name="stream">Stream to use</param>
        /// <param name="MSB">true if Most Significant Bit will be used, if false LSB will be used</param>
        public BitStream(Stream stream, bool MSB = false)
        {
            this.stream = new MemoryStream();
            stream.CopyTo(this.stream);
            this.MSB = MSB;
            offset = 0;
            bit = 0;
            encoding = Encoding.UTF8;
            AutoIncreaseStream = false;
        }

        /// <summary>
        /// Creates a BitStream using a Stream
        /// </summary>
        /// <param name="stream">Stream to use</param>
        /// <param name="encoding">Encoding to use with chars</param>
        /// <param name="MSB">true if Most Significant Bit will be used, if false LSB will be used</param>
        public BitStream(Stream stream, Encoding encoding, bool MSB = false)
        {
            this.stream = new MemoryStream();
            stream.CopyTo(this.stream);
            this.MSB = MSB;
            offset = 0;
            bit = 0;
            this.encoding = encoding;
            AutoIncreaseStream = false;
        }

        /// <summary>
        /// Creates a BitStream using a byte[]
        /// </summary>
        /// <param name="buffer">byte[] to use</param>
        /// <param name="MSB">true if Most Significant Bit will be used, if false LSB will be used</param>
        public BitStream(byte[] buffer, bool MSB = false)
        {
            this.stream = new MemoryStream();
            MemoryStream m = new MemoryStream(buffer);
            m.CopyTo(this.stream);
            this.MSB = MSB;
            offset = 0;
            bit = 0;
            encoding = Encoding.UTF8;
            AutoIncreaseStream = false;
        }

        /// <summary>
        /// Creates a BitStream using a byte[]
        /// </summary>
        /// <param name="buffer">byte[] to use</param>
        /// <param name="encoding">Encoding to use with chars</param>
        /// <param name="MSB">true if Most Significant Bit will be used, if false LSB will be used</param>
        public BitStream(byte[] buffer, Encoding encoding, bool MSB = false)
        {
            this.stream = new MemoryStream();
            MemoryStream m = new MemoryStream(buffer);
            this.MSB = MSB;
            offset = 0;
            bit = 0;
            this.encoding = encoding;
            AutoIncreaseStream = false;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Seek through the stream selecting the offset and bit using <see cref="SeekOrigin.Begin"/>
        /// </summary>
        /// <param name="offset">offset on the stream</param>
        /// <param name="bit">bit position</param>
        public void Seek(long offset, int bit)
        {
            if (offset > Length)
            {
                this.offset = Length;
            }
            else
            {
                if (offset >= 0)
                {
                    this.offset = offset;
                }
                else
                {
                    offset = 0;
                }
            }
            if (bit >= 8)
            {
                this.bit = 0;
            }
            else
            {
                this.bit = bit;
            }
            stream.Seek(offset, SeekOrigin.Begin);
        }

        /// <summary>
        /// Advances the stream by one bit
        /// </summary>
        public void AdvanceBit()
        {
            bit = (bit + 1) % 8;
            if (bit == 0)
            {
                offset++;
            }
        }

        /// <summary>
        /// Returns the stream by one bit
        /// </summary>
        public void ReturnBit()
        {
            bit = ((bit - 1) == -1 ? 7 : bit - 1);
            if (bit == 7)
            {
                offset--;
            }
            if(offset < 0)
            {
                offset = 0;
            }
        }

        /// <summary>
        /// Get the edited stream
        /// </summary>
        /// <returns>Modified stream</returns>
        public Stream GetStream()
        {
            return stream;
        }

        /// <summary>
        /// Get the stream data as a byte[]
        /// </summary>
        /// <returns>Stream as byte[]</returns>
        public byte[] GetStreamData()
        {
            stream.Seek(0, SeekOrigin.Begin);
            MemoryStream s = new MemoryStream();
            stream.CopyTo(s);
            return s.ToArray();
        }

        /// <summary>
        /// Get the <see cref="Encoding"/> used for chars and strings
        /// </summary>
        /// <returns><see cref="Encoding"/> used</returns>
        public Encoding GetEncoding()
        {
            return encoding;
        }

        /// <summary>
        /// Set the <see cref="Encoding"/> that will be used for chars and strings
        /// </summary>
        /// <param name="encoding"><see cref="Encoding"/> to use</param>
        public void SetEncoding(Encoding encoding)
        {
            this.encoding = encoding;
        }

        public bool ChangeLength(long length)
        {
            if (stream.CanSeek && stream.CanWrite)
            {
                stream.SetLength(length);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Checks if the <see cref="BitStream"/> will be in a valid position on its last bit read/write
        /// </summary>
        /// <param name="bits">Number of bits it will advance</param>
        /// <returns>true if <see cref="BitStream"/> will be inside the stream length</returns>
        private bool ValidPositionWhen(int bits)
        {
            long o = offset;
            int b = bit;
            bit = (bit + 1) % 8;
            if (bit == 0)
            {
                o++;
            }
            return o < Length;
        }

        #endregion

        #region BitRead/Write

        /// <summary>
        /// Read current position bit and advances the position within the stream by one bit
        /// </summary>
        /// <returns>Returns the current position bit as 0 or 1</returns>
        public Bit ReadBit()
        {
            if (!ValidPosition)
            {
                throw new IOException("Cannot read in an offset bigger than the length of the stream");
            }
            stream.Seek(offset, SeekOrigin.Begin);
            byte value;
            if (!MSB)
            {
                value = (byte)((stream.ReadByte() >> (bit)) & 1);
            }
            else
            {
                value = (byte)((stream.ReadByte() >> (7 - bit)) & 1);
            }
            AdvanceBit();
            stream.Seek(offset, SeekOrigin.Begin);
            return value;
        }

        /// <summary>
        /// Writes a bit in the current position
        /// </summary>
        /// <param name="data">Bit to write, it data is not 0 or 1 data = data & 1</param>
        public void WriteBit(Bit data)
        {
            stream.Seek(offset, SeekOrigin.Begin);
            byte value = (byte)stream.ReadByte();
            stream.Seek(offset, SeekOrigin.Begin);
            if (!MSB)
            {
                value &= (byte)~(1 << bit);
                value |= (byte)(data << bit);
            }
            else
            {
                value &= (byte)~(1 << (7 - bit));
                value |= (byte)(data << (7 - bit));
            }
            if (ValidPosition)
            {
                stream.WriteByte(value);
            }
            else
            {
                if (AutoIncreaseStream)
                {
                    if (ChangeLength(Length + (offset - Length) + 1))
                    {
                        stream.WriteByte(value);
                    }
                    else
                    {
                        throw new IOException("Cannot write in an offset bigger than the length of the stream");
                    }
                }
                else
                {
                    throw new IOException("Cannot write in an offset bigger than the length of the stream");
                }
            }
            AdvanceBit();
            stream.Seek(offset, SeekOrigin.Begin);
        }

        #endregion

        #region Read

        /// <summary>
        /// Read from the current position bit the specified number of bits and creates a byte[] 
        /// </summary>
        /// <param name="bits">Number of bits</param>
        /// <returns>byte[] containing bytes created from current position</returns>
        public byte[] ReadBytes(long bits)
        {
            List<byte> data = new List<byte>();
            for (long i = 0; i < bits;)
            {
                byte value = 0;
                for (int p = 0; p < 8 && i < bits; i++, p++)
                {
                    if (!MSB)
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

        /// <summary>
        /// Read a byte based on the current stream and bit position
        /// </summary>
        public byte ReadByte()
        {
            return ReadBytes(8)[0];
        }

        /// <summary>
        /// Read a byte based on the current stream and bit position and check if it is 0
        /// </summary>
        public bool ReadBool()
        {
            return ReadBytes(8)[0] == 0 ? false : true;
        }

        public char ReadChar()
        {
            return encoding.GetChars(ReadBytes(encoding.GetMaxByteCount(1) * 8))[0];
        }

        public string ReadString(int length)
        {
            int bitsPerChar = encoding.GetByteCount(" ") * 8;
            return encoding.GetString(ReadBytes(bitsPerChar*length),0,length);
        }

        /// <summary>
        /// Read a short based on the current stream and bit position
        /// </summary>
        public short ReadInt16()
        {
            short value = BitConverter.ToInt16(ReadBytes(16), 0);
            return value;
        }

        /// <summary>
        /// Read a 24bit value based on the current stream and bit position
        /// </summary>
        public int ReadInt24()
        {
            int value = BitConverter.ToInt32(ReadBytes(24), 0);
            return value;
        }

        /// <summary>
        /// Read an int based on the current stream and bit position
        /// </summary>
        public int ReadInt32()
        {
            int value = BitConverter.ToInt32(ReadBytes(32), 0);
            return value;
        }

        /// <summary>
        /// Read a 48bit value based on the current stream and bit position
        /// </summary>
        public long ReadInt48()
        {
            long value = BitConverter.ToInt64(ReadBytes(48), 0);
            return value;
        }

        /// <summary>
        /// Read a long based on the current stream and bit position
        /// </summary>
        public long ReadInt64()
        {
            long value = BitConverter.ToInt64(ReadBytes(64), 0);
            return value;
        }

        /// <summary>
        /// Read a ushort based on the current stream and bit position
        /// </summary>
        public ushort ReadUInt16()
        {
            ushort value = BitConverter.ToUInt16(ReadBytes(16), 0);
            return value;
        }

        /// <summary>
        /// Read an unsigned 24bit value based on the current stream and bit position
        /// </summary>
        public uint ReadUInt24()
        {
            uint value = BitConverter.ToUInt16(ReadBytes(24), 0);
            return value;
        }

        /// <summary>
        /// Read an uint based on the current stream and bit position
        /// </summary>
        public uint ReadUInt32()
        {
            uint value = BitConverter.ToUInt32(ReadBytes(32), 0);
            return value;
        }

        /// <summary>
        /// Read an unsigned 48bit value based on the current stream and bit position
        /// </summary>
        public ulong ReadUInt48()
        {
            ulong value = BitConverter.ToUInt64(ReadBytes(48), 0);
            return value;
        }

        /// <summary>
        /// Read an ulong based on the current stream and bit position
        /// </summary>
        public ulong ReadUInt64()
        {
            ulong value = BitConverter.ToUInt64(ReadBytes(64), 0);
            return value;
        }

        #endregion

        #region Write

        /// <summary>
        /// Writes as bits a byte[] by a specified number of bits
        /// </summary>
        /// <param name="data">byte[] to write</param>
        /// <param name="bits">Number of bits to use from the array</param>
        public void WriteBytes(byte[] data, long bits)
        {
            int position = 0;
            for (long i = 0; i < bits;)
            {
                byte value = 0;
                for (int p = 0; p < 8 && i < bits; i++, p++)
                {
                    if (!MSB)
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

        /// <summary>
        /// Write a byte value based on the current stream and bit position
        /// </summary>
        public void WriteByte(byte value)
        {
            WriteBytes(new byte[] { value }, 8);
        }

        /// <summary>
        /// Write a bool value as 0:false, 1:true as byte based on the current stream and bit position
        /// </summary>
        public void WriteBool(bool value)
        {
            WriteBytes(new byte[] { value ? (byte)1 : (byte)0 }, 8);
        }

        public void WriteChar(char value)
        {
            byte[] bytes = encoding.GetBytes(new char[] { value }, 0, 1);
            WriteBytes(bytes, bytes.Length*8);
        }

        public void WriteString(string value)
        {
            byte[] bytes = encoding.GetBytes(value);
            WriteBytes(bytes, bytes.Length * 8);
        }

        /// <summary>
        /// Write a short value based on the current stream and bit position
        /// </summary>
        public void WriteInt16(short value)
        {
            WriteBytes(BitConverter.GetBytes(value), 16);
        }

        /// <summary>
        /// Write a 24bit value based on the current stream and bit position
        /// </summary>
        public void WriteInt24(int value)
        {
            WriteBytes(BitConverter.GetBytes(value), 24);
        }

        /// <summary>
        /// Write an int value based on the current stream and bit position
        /// </summary>
        public void WriteInt32(int value)
        {
            WriteBytes(BitConverter.GetBytes(value), 32);
        }

        /// <summary>
        /// Write a 48bit value based on the current stream and bit position
        /// </summary>
        public void WriteInt48(long value)
        {
            WriteBytes(BitConverter.GetBytes(value), 48);
        }

        /// <summary>
        /// Write a long value based on the current stream and bit position
        /// </summary>
        public void WriteInt64(long value)
        {
            WriteBytes(BitConverter.GetBytes(value), 64);
        }

        /// <summary>
        /// Write an ushort value based on the current stream and bit position
        /// </summary>
        public void WriteUInt16(ushort value)
        {
            WriteBytes(BitConverter.GetBytes(value), 16);
        }

        /// <summary>
        /// Write an unsigned 24bit value based on the current stream and bit position
        /// </summary>
        public void WriteUInt24(uint value)
        {
            WriteBytes(BitConverter.GetBytes(value), 24);
        }

        /// <summary>
        /// Write an uint value based on the current stream and bit position
        /// </summary>
        public void WriteUInt32(uint value)
        {
            WriteBytes(BitConverter.GetBytes(value), 32);
        }

        /// <summary>
        /// Write an unsigned 48bit value based on the current stream and bit position
        /// </summary>
        public void WriteUInt48(ulong value)
        {
            WriteBytes(BitConverter.GetBytes(value), 48);
        }

        /// <summary>
        /// Write an ulong value based on the current stream and bit position
        /// </summary>
        public void WriteUInt64(ulong value)
        {
            WriteBytes(BitConverter.GetBytes(value), 64);
        }

        #endregion

        #region Shifts

        /// <summary>
        /// Do a bitwise shift on the current position of the stream on bit 0
        /// </summary>
        /// <param name="bits">bits to shift</param>
        /// <param name="leftShift">true to left shift, false to right shift</param>
        public void bitwiseShift(int bits, bool leftShift)
        {
            if (!ValidPositionWhen(8))
            {
                throw new IOException("Cannot write in an offset bigger than the length of the stream");
            }
            Seek(offset, 0);
            if (bits != 0 && bits <= 7)
            {
                byte value = (byte)stream.ReadByte();
                if (leftShift)
                {
                    value = (byte)(value << bits);
                }
                else
                {
                    value = (byte)(value >> bits);
                }
                Seek(offset, 0);
                stream.WriteByte(value);
            }
            bit = 0;
            offset++;
        }

        /// <summary>
        /// Do a bitwise shift on the current position of the stream on current bit
        /// </summary>
        /// <param name="bits">bits to shift</param>
        /// <param name="leftShift">true to left shift, false to right shift</param>
        public void bitwiseShiftOnBit(int bits, bool leftShift)
        {
            if (!ValidPositionWhen(8))
            {
                throw new IOException("Cannot write in an offset bigger than the length of the stream");
            }
            Seek(offset, bit);
            if (bits != 0 && bits <= 7)
            {
                byte value = ReadByte();
                if (leftShift)
                {
                    value = (byte)(value << bits);
                }
                else
                {
                    value = (byte)(value >> bits);
                }
                offset--;
                Seek(offset, bit);
                WriteByte(value);
            }
            offset++;
        }

        /// <summary>
        /// Do a circular shift on the current position of the stream on bit 0
        /// </summary>
        /// <param name="bits">bits to shift</param>
        /// <param name="leftShift">true to left shift, false to right shift</param>
        public void circularShift(int bits, bool leftShift)
        {
            if (!ValidPositionWhen(8))
            {
                throw new IOException("Cannot write in an offset bigger than the length of the stream");
            }
            Seek(offset, 0);
            if (bits != 0 && bits <= 7)
            {
                byte value = (byte)stream.ReadByte();
                if (leftShift)
                {
                    value = (byte)(value << bits | value >> (8 - bits));
                }
                else
                {
                    value = (byte)(value >> bits | value << (8 - bits));
                }
                Seek(offset, 0);
                stream.WriteByte(value);
            }
            bit = 0;
            offset++;
        }

        /// <summary>
        /// Do a circular shift on the current position of the stream on current bit
        /// </summary>
        /// <param name="bits">bits to shift</param>
        /// <param name="leftShift">true to left shift, false to right shift</param>
        public void circularShiftOnBit(int bits, bool leftShift)
        {
            if (!ValidPositionWhen(8))
            {
                throw new IOException("Cannot write in an offset bigger than the length of the stream");
            }
            Seek(offset, bit);
            if (bits != 0 && bits <= 7)
            {
                byte value = ReadByte();
                if (leftShift)
                {
                    value = (byte)(value << bits | value >> (8 - bits));
                }
                else
                {
                    value = (byte)(value >> bits | value << (8 - bits));
                }
                offset--;
                Seek(offset, bit);
                WriteByte(value);
            }
            offset++;
        }

        #endregion

        #region Bitwise Operators

        /// <summary>
        /// Apply an and operator on the current stream and bit position byte and advances one byte position
        /// </summary>
        /// <param name="x">Byte value to apply and</param>
        public void And(byte x)
        {
            if (!ValidPositionWhen(8))
            {
                throw new IOException("Cannot read in an offset bigger than the length of the stream");
            }
            Seek(offset, bit);
            byte value = ReadByte();
            offset--;
            Seek(offset, bit);
            WriteByte((byte)(value & x));
        }

        /// <summary>
        /// Apply an or operator on the current stream and bit position byte and advances one byte position
        /// </summary>
        /// <param name="x">Byte value to apply or</param>
        public void Or(byte x)
        {
            if (!ValidPositionWhen(8))
            {
                throw new IOException("Cannot read in an offset bigger than the length of the stream");
            }
            Seek(offset, bit);
            byte value = ReadByte();
            offset--;
            Seek(offset, bit);
            WriteByte((byte)(value | x));
        }

        /// <summary>
        /// Apply a xor operator on the current stream and bit position byte and advances one byte position
        /// </summary>
        /// <param name="x">Byte value to apply xor</param>
        public void Xor(byte x)
        {
            if (!ValidPositionWhen(8))
            {
                throw new IOException("Cannot read in an offset bigger than the length of the stream");
            }
            Seek(offset, bit);
            byte value = ReadByte();
            offset--;
            Seek(offset, bit);
            WriteByte((byte)(value ^ x));
        }

        /// <summary>
        /// Apply a not operator on the current stream and bit position byte and advances one byte position
        /// </summary>
        public void Not()
        {
            if (!ValidPositionWhen(8))
            {
                throw new IOException("Cannot read in an offset bigger than the length of the stream");
            }
            Seek(offset, bit);
            byte value = ReadByte();
            offset--;
            Seek(offset, bit);
            WriteByte((byte)(~value));
        }

        /// <summary>
        /// Apply an and operator on the current stream and bit position and advances one bit position
        /// </summary>
        /// <param name="bit">Bit value to apply and</param>
        public void BitAnd(Bit x)
        {
            if (!ValidPosition)
            {
                throw new IOException("Cannot read in an offset bigger than the length of the stream");
            }
            Seek(offset, bit);
            Bit value = ReadBit();
            ReturnBit();
            Seek(offset, bit);
            WriteBit(x & value);
        }

        /// <summary>
        /// Apply an or operator on the current stream and bit position and advances one bit position
        /// </summary>
        /// <param name="bit">Bit value to apply or</param>
        public void BitOr(Bit x)
        {
            if (!ValidPosition)
            {
                throw new IOException("Cannot read in an offset bigger than the length of the stream");
            }
            Seek(offset, bit);
            Bit value = ReadBit();
            ReturnBit();
            Seek(offset, bit);
            WriteBit(x | value);
        }

        /// <summary>
        /// Apply a xor operator on the current stream and bit position and advances one bit position
        /// </summary>
        /// <param name="bit">Bit value to apply xor</param>
        public void BitXor(Bit x)
        {
            if (!ValidPosition)
            {
                throw new IOException("Cannot read in an offset bigger than the length of the stream");
            }
            Seek(offset, bit);
            Bit value = ReadBit();
            ReturnBit();
            Seek(offset, bit);
            WriteBit(x ^ value);
        }

        /// <summary>
        /// Apply a not operator on the current stream and bit position and advances one bit position
        /// </summary>
        public void BitNot()
        {
            if (!ValidPosition)
            {
                throw new IOException("Cannot read in an offset bigger than the length of the stream");
            }
            Seek(offset, bit);
            Bit value = ReadBit();
            ReturnBit();
            Seek(offset, bit);
            WriteBit(~value);
        }

        #endregion

    }
}
