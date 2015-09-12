# BitStream
C# .NET library to read/write bitstreams

## Introduction
Bitstreams unlike streams using `BinaryReader` and `BinaryWriter` use the stream at bit level, allowing read/write bits
This library project attempts to add bit manipulation to a stream while using known `Stream`, `BinaryReader` and `BinaryWriter` class methods

## Usage
Initialize a **BitStream** using a Stream or `byte[]` using the constructor
```
//Using Stream stream;
BitStream bitstream = new BitStream(stream);
//Using byte[] bytes;
BitStream bitstream = new BitStream(bytes);
```
You can set the **BitStream** to use most-significant bit or less-significant bit as bit 0, by default LSB is used
```
//Using Stream stream and LSB;
BitStream bitstream = new BitStream(bytes);
//Using Stream stream and MSB;
BitStream bitstream = new BitStream(bytes,true);
```
After reading/writing the stream use **GetStream()** to get the stream or **GetStreamData()** to get a `byte[]` of the data in the stream

## Features

### Seeking, advancing and returning bits
Seeking in a BitStream uses *Seek(long offset, int bit)* to specify the stream position

Using *AdvanceBit()* and *ReturnBit()* allows moving the bit offset forward or backwards by one

### Reading/Writing bits
Reading a bit is easy using *ReadBit()* method, it returns a **Bit** which can be assigned to a byte, int or bool
```
Bit bit = bitstream.ReadBit();
byte b = bitstream.ReadBit();
int i = bitstream.ReadBit();
bool boolean = bitstream.ReadBit();
```
Writing a bit using *WriteBit(Bit bit)* is also easy with byte, int and bool
```
bitstream.WriteBit(1);
bitstream.WriteBit(b);
bitstream.WriteBit(i);
bitstream.WriteBit(true);
```


### Reading/Writing data types
Just like `BinaryReader` and `BinaryWriter` can read/write data types like int, bool and string, **BitStream** can read/write this data types, currently these are supported:
* byte
* byte[]
* bool (Reading/Writing byte)
* short
* ushort
* int
* uint
* long
* ulong
* 24bit int/uint
* 48bit long/ulong

### Shifts
**BitStream** can do bitwise and circular shifts on current position byte using *bitwiseShift(int bits, bool leftShift)* and *circularShift(int bits, bool leftShift)* or using the current bit position create a byte and use it using *bitwiseShiftOnBit(int bits, bool leftShift)* and *circularShiftOnBit(int bits, bool leftShift)*

### Bitwise Operators
**BitStream** can do bitwise operations `AND`,`OR`,`XOR` and `NOT` at byte and bit level