using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FlvMerge
{
    /// <summary>
    /// Class <c>FlvFile</c> models flv files and provids read, write and edit methods.
    /// Author: Xuan525
    /// Date: 25/07/2019
    /// Reference: https://www.adobe.com/content/dam/acom/en/devnet/flv/video_file_format_spec_v10_1.pdf
    /// </summary>
    public class FlvFile : IDisposable
    {
        #region Properties

        public FileStream FlvFileStream { get; private set; }
        public FlvHeader Header { get; private set; }

        #endregion

        #region Constructor

        public FlvFile(string filename)
        {
            FlvFileStream = new FileStream(filename, FileMode.Open);
            Header = FlvHeader.ReadHeader(FlvFileStream);
        }

        #endregion

        #region IDisposable Support

        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    FlvFileStream.Close();
                    FlvFileStream.Dispose();
                    FlvFileStream = null;
                }

                Header = null;

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion

        #region Public method

        public Tag ReadTag()
        {
            if (disposedValue)
                throw new ObjectDisposedException(this.ToString());
            return Tag.ReadTag(FlvFileStream);
        }

        public void Close()
        {
            Dispose();
        }

        #endregion

        #region Exceptions

        public class UnsupportedFormat : Exception
        {
            public UnsupportedFormat() : base()
            {

            }

            public UnsupportedFormat(string message) : base(message)
            {

            }
        }

        #endregion

        #region Util

        internal static class Util
        {
            public enum UintType
            {
                Uint16 = 2,
                Uint24,
                Uint32
            }

            public static uint ToUintBe(byte[] bytes, int startIndex, UintType uintType)
            {
                uint value = 0;
                for (int i = 0; i < (int)uintType; i++)
                {
                    value |= (uint)bytes[startIndex + i] << ((int)uintType - i - 1) * 8;
                }
                return value;
            }

            public static double ToDoubleBe(byte[] bytes)
            {
                byte[] buffer = new byte[8];
                for (int i = 0; i < 8; i++)
                    buffer[i] = bytes[7 - i];
                return BitConverter.ToDouble(buffer, 0);
            }

            public static byte[] ToBytesLe(uint value, UintType uintType)
            {
                byte[] bytes = new byte[(int)uintType];
                for(int i = 0; i < (int)uintType; i++)
                {
                    bytes[i] = (byte)(value >> ((int)uintType - i - 1) * 8);
                }
                return bytes;
            }

            public static byte[] ToBytesLe(double value)
            {
                byte[] bytes = BitConverter.GetBytes(value);
                byte temp;
                for (int i = 0; i < 4; i++)
                {
                    temp = bytes[i];
                    bytes[i] = bytes[7 - i];
                    bytes[7 - i] = temp;
                }
                return bytes;
            }
        }

        #endregion

        #region Public classes

        public class FlvHeader
        {
            #region StreamFlag Enum

            public enum StreamFlag
            {
                Video = 0x01,
                Audio = 0x04,
                VideoAndAudio = 0x05
            }

            #endregion

            #region Properties

            public const uint HeaderLengthWithPts = 13;

            public byte[] HeaderBytes { get; private set; }

            public string Signature
            {
                get
                {
                    return Encoding.UTF8.GetString(HeaderBytes, 0, 3);
                }
            }

            public bool IsAvaliableFlvFile
            {
                get
                {
                    if (HeaderBytes[0] == 'F' && HeaderBytes[1] == 'L' && HeaderBytes[2] == 'V' && Version == 1)
                        return true;
                    return false;
                }
            }

            public int Version
            {
                get
                {
                    return HeaderBytes[3];
                }
            }

            public StreamFlag StreamType
            {
                get
                {
                    return (StreamFlag)(HeaderBytes[4] & 0b00000101);
                }
                set
                {
                    HeaderBytes[4] = (byte)((HeaderBytes[4] & (0b00000101 ^ 0b11111111)) | (byte)value);
                }
            }

            public uint DataOffset
            {
                get
                {
                    return Util.ToUintBe(HeaderBytes, 5, Util.UintType.Uint32);
                }
            }

            public uint PreviousTagSize
            {
                get
                {
                    return Util.ToUintBe(HeaderBytes, 9, Util.UintType.Uint32);
                }
            }

            public int Length
            {
                get
                {
                    return HeaderBytes.Length;
                }
            }

            #endregion

            #region Constructors

            public FlvHeader(byte[] headerBytes)
            {
                HeaderBytes = headerBytes;
            }

            public FlvHeader(StreamFlag streamFlag)
            {
                HeaderBytes = new byte[] { (byte)'F', (byte)'L', (byte)'V', 0x01, 0x05, 0x00, 0x00, 0x00, 0x09, 0x00, 0x00, 0x00, 0x00 };
                StreamType = streamFlag;
            }

            #endregion

            #region Static method

            public static FlvHeader ReadHeader(Stream stream)
            {
                byte[] headerBytes = new byte[HeaderLengthWithPts];
                if (stream.Read(headerBytes, 0, headerBytes.Length) == headerBytes.Length)
                {
                    FlvHeader flvHeader = new FlvHeader(headerBytes);
                    return flvHeader;
                }
                else
                {
                    throw new Exception();
                }
            }

            #endregion
        }

        public abstract class Tag
        {
            #region TagType Enum

            public enum TagType
            {
                Audio = 0x08,
                Video = 0x09,
                Script = 0x12
            }

            #endregion

            #region Properties
            
            public TagHeader Header { get; private set; }
            public abstract TagType Type { get; }
            public abstract uint BodyLength { get; }
            public abstract byte[] BodyBytes { get; }

            public uint TagLength
            {
                get
                {
                    return Header.HeaderLength + BodyLength;
                }
            }

            public byte[] TagBytes
            {
                get
                {
                    byte[] bytes = new byte[TagLength];
                    Header.HeaderBytes.CopyTo(bytes, 0);
                    BodyBytes.CopyTo(bytes, Header.HeaderLength);
                    return bytes;
                }
            }

            public uint TagLengthWithPts
            {
                get
                {
                    return TagLength + (uint)Util.UintType.Uint32;
                }
            }

            public byte[] TagBytesWithPts
            {
                get
                {
                    byte[] bytes = new byte[TagLengthWithPts];
                    Header.HeaderBytes.CopyTo(bytes, 0);
                    BodyBytes.CopyTo(bytes, Header.HeaderLength);
                    Util.ToBytesLe((uint)bytes.Length - 4, Util.UintType.Uint32).CopyTo(bytes, bytes.Length - 4);
                    return bytes;
                }
            }

            #endregion

            #region Constructor

            public Tag(byte[] headerBytes)
            {
                Header = new TagHeader(headerBytes);
            }

            #endregion

            #region Static Methods

            public static Tag ReadTag(Stream stream)
            {
                if (stream.Position < stream.Length - 1)
                {
                    byte[] headerBytes = new byte[11];
                    if (stream.Read(headerBytes, 0, headerBytes.Length) == headerBytes.Length)
                    {
                        uint bodysize = Util.ToUintBe(headerBytes, 1, Util.UintType.Uint24);
                        byte[] bodyBytes = new byte[bodysize];
                        if (stream.Read(bodyBytes, 0, bodyBytes.Length) == bodyBytes.Length)
                        {
                            byte[] ptsBytes = new byte[4];
                            if (stream.Read(ptsBytes, 0, 4) == 4)
                            {
                                return Parse(headerBytes, bodyBytes);
                            }
                            else
                            {
                                throw new UnsupportedFormat("Cannot read previous tag size");
                            }
                        }
                        else
                        {
                            throw new UnsupportedFormat("Cannot read tag body");
                        }
                    }
                    else
                    {
                        throw new UnsupportedFormat("Cannot read tag header");
                    }
                }
                else
                {
                    return null;
                }
            }

            public static Tag Parse(byte[] headerBytes, byte[] bodyBytes)
            {
                TagType tagType = (TagType)(headerBytes[0] & 0b00011111);
                Tag tagBase;
                switch (tagType)
                {
                    case TagType.Audio:
                        tagBase = new AudioTag(headerBytes, bodyBytes);
                        break;
                    case TagType.Video:
                        tagBase = new VideoTag(headerBytes, bodyBytes);
                        break;
                    case TagType.Script:
                        tagBase = new ScriptTag(headerBytes, bodyBytes);
                        break;
                    default:
                        throw new UnsupportedFormat(string.Format("Unsupported Tag type 0x{0}", tagType.ToString("X2")));
                }
                return tagBase;
            }

            #endregion

            public class TagHeader
            {
                #region Properties

                public uint HeaderLength
                {
                    get
                    {
                        return 11;
                    }
                }

                public byte[] HeaderBytes { get; private set; }

                public uint Reversed
                {
                    get
                    {
                        return ((uint)HeaderBytes[0] & 0b11000000) >> 6;
                    }
                    set
                    {
                        HeaderBytes[0] = (byte)((HeaderBytes[0] & (0b11000000 ^ 0b11111111)) | ((byte)value << 6));
                    }
                }

                public uint Filtered
                {
                    get
                    {
                        return ((uint)HeaderBytes[0] & 0b00100000) >> 5;
                    }
                    set
                    {
                        HeaderBytes[0] = (byte)((HeaderBytes[0] & (0b00100000 ^ 0b11111111)) | ((byte)value << 5));
                    }
                }

                public TagType Type
                {
                    get
                    {
                        return (TagType)(HeaderBytes[0] & 0b00011111);
                    }
                    set
                    {
                        HeaderBytes[0] = (byte)((HeaderBytes[0] & (0b00011111 ^ 0b11111111)) | (byte)value);
                    }
                }

                public uint DataSize
                {
                    get
                    {
                        return Util.ToUintBe(HeaderBytes, 1, Util.UintType.Uint24);
                    }
                    set
                    {
                        byte[] bytes = Util.ToBytesLe(value, Util.UintType.Uint24);
                        bytes.CopyTo(HeaderBytes, 1);
                    }
                }

                public uint Timestamp
                {
                    get
                    {
                        return ((uint)HeaderBytes[7] << 24) | ((uint)HeaderBytes[4] << 16) | ((uint)HeaderBytes[5] << 8) | (HeaderBytes[6]);
                    }
                    set
                    {
                        HeaderBytes[7] = (byte)(value >> 24);
                        HeaderBytes[4] = (byte)(value >> 16);
                        HeaderBytes[5] = (byte)(value >> 8);
                        HeaderBytes[6] = (byte)(value);
                    }
                }

                public uint StreamId
                {
                    get
                    {
                        return Util.ToUintBe(HeaderBytes, 8, Util.UintType.Uint24);
                    }
                    set
                    {
                        byte[] bytes = Util.ToBytesLe(value, Util.UintType.Uint24);
                        bytes.CopyTo(HeaderBytes, 8);
                    }
                }

                #endregion

                #region Constructors

                public TagHeader(byte[] headerBytes)
                {
                    HeaderBytes = headerBytes;
                }

                public TagHeader(TagType tagType)
                {
                    HeaderBytes = new byte[11];
                    Type = tagType;
                }

                #endregion
            }

            public class ScriptTag : Tag
            {
                #region DataTypes

                public enum ScriptDataType
                {
                    Number = 0x00,
                    Boolean,
                    String,
                    Object,
                    MovieClip,
                    Null,
                    Undefined,
                    Reference,
                    EcmaArray,
                    ObjectEndMark,
                    StrictArray,
                    Date,
                    LongString
                }

                public abstract class ScriptData
                {
                    public ScriptDataType ScriptDataType { get; internal set; }
                    public virtual byte[] ScriptDataBytes { get; internal set; }
                    public virtual uint ScriptDataLength { get; }

                    public ScriptData()
                    {

                    }

                    public static ScriptData ReadData(Stream stream)
                    {
                        ScriptDataType scriptDataType = (ScriptDataType)stream.ReadByte();
                        if (!(stream.Position < stream.Length - 4))
                        {
                            return null;
                        }
                        ScriptData scriptData;
                        switch (scriptDataType)
                        {
                            case ScriptDataType.Number:
                                scriptData = new Number(stream);
                                break;
                            case ScriptDataType.Boolean:
                                scriptData = new Boolean(stream);
                                break;
                            case ScriptDataType.String:
                                scriptData = new String(stream);
                                break;
                            case ScriptDataType.Object:
                                scriptData = new Object(stream);
                                break;
                            case ScriptDataType.Reference:
                                scriptData = new Reference(stream);
                                break;
                            case ScriptDataType.EcmaArray:
                                scriptData = new EcmaArray(stream);
                                break;
                            case ScriptDataType.ObjectEndMark:
                                scriptData = new ObjectEndMark(stream);
                                break;
                            case ScriptDataType.StrictArray:
                                scriptData = new StrictArray(stream);
                                break;
                            default:
                                throw new UnsupportedFormat(string.Format("Unsupported Script data type 0x{0}", scriptDataType.ToString("X2")));
                        }
                        return scriptData;
                    }
                }

                public class Number : ScriptData
                {
                    public Number() : base()
                    {
                        ScriptDataType = ScriptDataType.Number;
                        ScriptDataBytes = new byte[8];
                    }

                    public Number(Stream stream) : this()
                    {
                        stream.Read(ScriptDataBytes, 0, ScriptDataBytes.Length);
                    }

                    public Number(double value) : this()
                    {
                        Value = value;
                    }

                    public double Value
                    {
                        get
                        {
                            double value = Util.ToDoubleBe(ScriptDataBytes);
                            return value;
                        }
                        set
                        {
                            ScriptDataBytes = Util.ToBytesLe(value);
                        }
                    }

                    public override uint ScriptDataLength
                    {
                        get
                        {
                            return 8;
                        }
                    }

                    public override string ToString()
                    {
                        return Value.ToString();
                    }
                }

                public class Boolean : ScriptData
                {
                    public Boolean() : base()
                    {
                        ScriptDataType = ScriptDataType.Boolean;
                        ScriptDataBytes = new byte[1];
                    }

                    public Boolean(Stream stream) : this()
                    {
                        stream.Read(ScriptDataBytes, 0, ScriptDataBytes.Length);
                    }

                    public Boolean(bool value) : this()
                    {
                        Value = value;
                    }

                    public bool Value
                    {
                        get
                        {
                            return ScriptDataBytes[0] != 0;
                        }
                        set
                        {
                            if (value)
                                ScriptDataBytes[0] = 1;
                            else
                                ScriptDataBytes[0] = 0;
                        }
                    }

                    public override uint ScriptDataLength
                    {
                        get
                        {
                            return 1;
                        }
                    }

                    public override string ToString()
                    {
                        if (Value)
                            return "true";
                        else
                            return "false";
                    }
                }

                public class String : ScriptData
                {
                    public String() : base()
                    {
                        ScriptDataType = ScriptDataType.String;
                        Value = string.Empty;
                    }

                    public String(Stream stream) : this()
                    {
                        byte[] lengthBytes = new byte[(int)Util.UintType.Uint16];
                        stream.Read(lengthBytes, 0, lengthBytes.Length);
                        uint length = Util.ToUintBe(lengthBytes, 0, Util.UintType.Uint16);
                        ScriptDataBytes = new byte[(int)Util.UintType.Uint16 + length];
                        lengthBytes.CopyTo(ScriptDataBytes, 0);
                        stream.Read(ScriptDataBytes, (int)Util.UintType.Uint16, ScriptDataBytes.Length - (int)Util.UintType.Uint16);
                    }

                    public String(string value) : this()
                    {
                        Value = value;
                    }

                    public string Value
                    {
                        get
                        {
                            return Encoding.ASCII.GetString(ScriptDataBytes, (int)Util.UintType.Uint16, ScriptDataBytes.Length - (int)Util.UintType.Uint16);
                        }
                        set
                        {
                            byte[] lengthBytes = Util.ToBytesLe((uint)value.Length, Util.UintType.Uint16);
                            byte[] stringBytes = Encoding.ASCII.GetBytes(value);
                            ScriptDataBytes = new byte[lengthBytes.Length + stringBytes.Length];
                            lengthBytes.CopyTo(ScriptDataBytes, 0);
                            stringBytes.CopyTo(ScriptDataBytes, lengthBytes.Length);
                        }
                    }

                    public override uint ScriptDataLength
                    {
                        get
                        {
                            return (uint)Util.UintType.Uint16 + Util.ToUintBe(ScriptDataBytes, 0, Util.UintType.Uint16);
                        }
                    }

                    public override string ToString()
                    {
                        return string.Format("\"{0}\"", Value.ToString());
                    }
                }

                public class Object : ScriptData, IEnumerable
                {
                    public Dictionary<string, ScriptData> Items { get; private set; }

                    public Object() : base()
                    {
                        ScriptDataType = ScriptDataType.Object;
                        Items = new Dictionary<string, ScriptData>();
                    }

                    public Object(Stream stream) : this()
                    {
                        while (true)
                        {
                            byte[] nameLengthBytes = new byte[2];
                            stream.Read(nameLengthBytes, 0, nameLengthBytes.Length);
                            uint nameLength = Util.ToUintBe(nameLengthBytes, 0, Util.UintType.Uint16);
                            byte[] nameBytes = new byte[nameLength];
                            stream.Read(nameBytes, 0, nameBytes.Length);
                            string name = Encoding.ASCII.GetString(nameBytes);

                            ScriptData scriptData = ScriptData.ReadData(stream);
                            if (scriptData == null || scriptData.ScriptDataType == ScriptDataType.ObjectEndMark)
                                break;
                            Items.Add(name, scriptData);
                        }
                    }

                    public override byte[] ScriptDataBytes
                    {
                        get
                        {
                            List<byte> bytes = new List<byte>();
                            foreach (KeyValuePair<string, ScriptData> keyValuePair in Items)
                            {
                                String name = new String(keyValuePair.Key);
                                byte[] nameBytes = name.ScriptDataBytes;
                                bytes.AddRange(nameBytes);
                                ScriptData data = keyValuePair.Value;
                                byte[] dataBytes = data.ScriptDataBytes;
                                bytes.Add((byte)data.ScriptDataType);
                                bytes.AddRange(dataBytes);
                            }
                            bytes.AddRange(new ObjectEndMark().ScriptDataBytes);
                            return bytes.ToArray();
                        }
                    }

                    public override uint ScriptDataLength
                    {
                        get
                        {
                            uint length = 0;
                            foreach (KeyValuePair<string, ScriptData> keyValuePair in Items)
                            {
                                String name = new String(keyValuePair.Key);
                                length += name.ScriptDataLength;
                                ScriptData data = keyValuePair.Value;
                                length += 1;
                                length += data.ScriptDataLength;
                            }
                            length += new ObjectEndMark().ScriptDataLength;
                            return length;
                        }
                    }

                    public IEnumerator GetEnumerator()
                    {
                        return ((IEnumerable)Items).GetEnumerator();
                    }

                    public override string ToString()
                    {
                        StringBuilder stringBuilder = new StringBuilder();
                        stringBuilder.Append("{");
                        int i = 0;
                        foreach (KeyValuePair<string, ScriptData> keyValuePair in Items)
                        {
                            if(i != 0)
                                stringBuilder.Append(",");
                            stringBuilder.Append(string.Format("\"{0}\":{1}", keyValuePair.Key, keyValuePair.Value));
                            i++;
                        }
                        stringBuilder.Append("}");
                        return stringBuilder.ToString();
                    }
                }

                public class Reference : ScriptData
                {
                    public Reference() : base()
                    {
                        ScriptDataType = ScriptDataType.Reference;
                        ScriptDataBytes = new byte[2];
                    }

                    public Reference(Stream stream) : this()
                    {
                        stream.Read(ScriptDataBytes, 0, ScriptDataBytes.Length);
                    }

                    public Reference(ushort value)
                    {
                        Value = value;
                    }

                    public uint Value
                    {
                        get
                        {
                            return Util.ToUintBe(ScriptDataBytes, 0, Util.UintType.Uint16);
                        }
                        set
                        {
                            ScriptDataBytes = Util.ToBytesLe(value, Util.UintType.Uint16);
                        }
                    }

                    public override uint ScriptDataLength
                    {
                        get
                        {
                            return (uint)Util.UintType.Uint16;
                        }
                    }

                    public override string ToString()
                    {
                        return Value.ToString();
                    }
                }

                public class EcmaArray : ScriptData, IEnumerable
                {
                    public Dictionary<string, ScriptData> Items { get; set; }

                    public EcmaArray() : base()
                    {
                        ScriptDataType = ScriptDataType.EcmaArray;
                        Items = new Dictionary<string, ScriptData>();
                    }

                    public EcmaArray(Stream stream) : this()
                    {
                        byte[] lengthBytes = new byte[4];
                        stream.Read(lengthBytes, 0, lengthBytes.Length);

                        while (true)
                        {
                            byte[] nameLengthBytes = new byte[2];
                            stream.Read(nameLengthBytes, 0, nameLengthBytes.Length);
                            uint nameLength = Util.ToUintBe(nameLengthBytes, 0, Util.UintType.Uint16);
                            byte[] nameBytes = new byte[nameLength];
                            stream.Read(nameBytes, 0, nameBytes.Length);
                            string name = Encoding.ASCII.GetString(nameBytes);

                            ScriptData scriptData = ScriptData.ReadData(stream);
                            if (scriptData == null || scriptData.ScriptDataType == ScriptDataType.ObjectEndMark)
                                break;
                            Items.Add(name, scriptData);
                        }
                    }

                    public override byte[] ScriptDataBytes
                    {
                        get
                        {
                            List<byte> bytes = new List<byte>();
                            bytes.AddRange(Util.ToBytesLe((uint)Items.Count, Util.UintType.Uint32));
                            foreach (KeyValuePair<string, ScriptData> keyValuePair in Items)
                            {
                                String name = new String(keyValuePair.Key);
                                byte[] nameBytes = name.ScriptDataBytes;
                                bytes.AddRange(nameBytes);
                                ScriptData data = keyValuePair.Value;
                                byte[] dataBytes = data.ScriptDataBytes;
                                bytes.Add((byte)data.ScriptDataType);
                                bytes.AddRange(dataBytes);
                            }
                            bytes.AddRange(new ObjectEndMark().ScriptDataBytes);
                            return bytes.ToArray();
                        }
                    }

                    public override uint ScriptDataLength
                    {
                        get
                        {
                            uint length = 0;
                            length += (int)Util.UintType.Uint32;
                            foreach (KeyValuePair<string, ScriptData> keyValuePair in Items)
                            {
                                String name = new String(keyValuePair.Key);
                                length += name.ScriptDataLength;
                                ScriptData data = keyValuePair.Value;
                                length += 1;
                                length += data.ScriptDataLength;
                            }
                            length += new ObjectEndMark().ScriptDataLength;
                            return length;
                        }
                    }

                    public IEnumerator GetEnumerator()
                    {
                        return ((IEnumerable)Items).GetEnumerator();
                    }

                    public override string ToString()
                    {
                        StringBuilder stringBuilder = new StringBuilder();
                        stringBuilder.Append("{");
                        int i = 0;
                        foreach (KeyValuePair<string, ScriptData> keyValuePair in Items)
                        {
                            if (i != 0)
                                stringBuilder.Append(",");
                            stringBuilder.Append(string.Format("\"{0}\":{1}", keyValuePair.Key, keyValuePair.Value));
                            i++;
                        }
                        stringBuilder.Append("}");
                        return stringBuilder.ToString();
                    }
                }

                public class ObjectEndMark : ScriptData
                {
                    public ObjectEndMark() : base()
                    {

                    }

                    public ObjectEndMark(Stream stream) : this()
                    {

                    }

                    public override uint ScriptDataLength
                    {
                        get
                        {
                            return 3;
                        }
                    }

                    public override byte[] ScriptDataBytes
                    {
                        get
                        {
                            return new byte[] { 0x00, 0x00, 0x09 };
                        }
                    }
                }

                public class StrictArray : ScriptData, IEnumerable
                {
                    public List<ScriptData> Items { get; private set; }

                    public StrictArray() : base()
                    {
                        ScriptDataType = ScriptDataType.StrictArray;
                        Items = new List<ScriptData>();
                    }

                    public StrictArray(Stream stream) : this()
                    {
                        byte[] lengthBytes = new byte[4];
                        stream.Read(lengthBytes, 0, lengthBytes.Length);
                        uint length = Util.ToUintBe(lengthBytes, 0, Util.UintType.Uint32);

                        for (int i = 0; i < length; i++)
                        {
                            ScriptData scriptData = ScriptData.ReadData(stream);
                            if (scriptData == null)
                                break;
                            Items.Add(scriptData);
                        }
                    }

                    public override byte[] ScriptDataBytes
                    {
                        get
                        {
                            List<byte> bytes = new List<byte>();
                            bytes.AddRange(Util.ToBytesLe((uint)Items.Count, Util.UintType.Uint32));
                            foreach (ScriptData scriptData in Items)
                            {
                                byte[] dataBytes = scriptData.ScriptDataBytes;
                                bytes.Add((byte)scriptData.ScriptDataType);
                                bytes.AddRange(dataBytes);
                            }
                            return bytes.ToArray();
                        }
                    }

                    public override uint ScriptDataLength
                    {
                        get
                        {
                            uint length = 0;
                            length += (uint)Util.UintType.Uint32;
                            foreach (ScriptData scriptData in Items)
                            {
                                length += 1;
                                length += scriptData.ScriptDataLength;
                            }
                            return length;
                        }
                    }

                    public IEnumerator GetEnumerator()
                    {
                        return ((IEnumerable)Items).GetEnumerator();
                    }

                    public override string ToString()
                    {
                        StringBuilder stringBuilder = new StringBuilder();
                        stringBuilder.Append("[");
                        int i = 0;
                        foreach (ScriptData scriptData in Items)
                        {
                            if (i != 0)
                                stringBuilder.Append(",");
                            stringBuilder.Append(string.Format("{0}", scriptData));
                            i++;
                        }
                        stringBuilder.Append("]");
                        return stringBuilder.ToString();
                    }
                }

                #endregion

                #region Properties

                public ScriptData Name { get; private set; }

                public ScriptData Value { get; private set; }

                public override TagType Type
                {
                    get
                    {
                        return TagType.Script;
                    }
                }

                public override uint BodyLength
                {
                    get
                    {
                        return 1 + Name.ScriptDataLength + 1 + Value.ScriptDataLength;
                    }
                }

                public override byte[] BodyBytes
                {
                    get
                    {
                        byte[] bytes = new byte[BodyLength];
                        bytes[0] = (byte)ScriptDataType.String;
                        Name.ScriptDataBytes.CopyTo(bytes, 1);
                        bytes[1 + Name.ScriptDataLength] = (byte)ScriptDataType.EcmaArray;
                        Value.ScriptDataBytes.CopyTo(bytes, 1 + Name.ScriptDataLength + 1);
                        return bytes;
                    }
                }

                #endregion

                #region Constructors

                public ScriptTag(byte[] headerBytes, byte[] bodyBytes) : base(headerBytes)
                {
                    if (Header.Filtered == 0)
                    {
                        MemoryStream memoryStream = new MemoryStream(bodyBytes);
                        Name = ScriptData.ReadData(memoryStream);
                        Value = ScriptData.ReadData(memoryStream);
                    }
                    else
                    {
                        throw new UnsupportedFormat("Script tag with filter is not supported");
                    }
                }

                public ScriptTag(string name, EcmaArray value) : base (new TagHeader(TagType.Script).HeaderBytes)
                {
                    Name = new String(name);
                    Value = value;
                }

                #endregion

                public override string ToString()
                {
                    return string.Format(
                        "{{\"Type\":\"{0}\"," +
                        "\"Timestamp\":{1}," +
                        "\"Name\":{2}," +
                        "\"Value\":{3}}}", 
                        Type, 
                        Header.Timestamp, 
                        Name, 
                        Value);
                }
            }

            public class VideoTag : Tag
            {
                #region Enums

                public enum FrameTypes
                {
                    KeyFrame = 1,
                    InterFrame,
                    DisposableInterFrame,
                    GeneratedKeyfFrame,
                    VideoInfoCommandFrame
                }

                public enum CodecIDs
                {
                    SorensonH263 = 2,
                    ScreenVideo,
                    On2Pv6,
                    On2Pv6WithAlphaChannel,
                    ScreenVideoVersion2,
                    AVC
                }

                #endregion

                #region Properties

                public FrameTypes FrameType
                {
                    get
                    {
                        return (FrameTypes)((uint)BodyBytes[0] >> 4);
                    }
                }

                public CodecIDs CodecID
                {
                    get
                    {
                        return (CodecIDs)((uint)BodyBytes[0] & 0b00001111);
                    }
                }

                public override TagType Type
                {
                    get
                    {
                        return TagType.Video;
                    }
                }

                public override byte[] BodyBytes { get; }

                public override uint BodyLength
                {
                    get
                    {
                        return (uint)BodyBytes.Length;
                    }
                }

                #endregion

                #region Constructor

                public VideoTag(byte[] headerBytes, byte[] bodyBytes) : base(headerBytes)
                {
                    BodyBytes = bodyBytes;
                }

                #endregion

                public override string ToString()
                {
                    return string.Format(
                        "{{\"Type\":\"{0}\"," +
                        "\"Timestamp\":{1}," +
                        "\"FrameType\":\"{2}\"," +
                        "\"CodecID\":\"{3}\"}}", 
                        Type, 
                        Header.Timestamp, 
                        FrameType, 
                        CodecID);
                }
            }

            public class AudioTag : Tag
            {
                #region Enums

                public enum SoundFormats
                {
                    LinearPcmPlatformEndian = 0,
                    AdPcm,
                    Mp3,
                    LinearPcmLittleEndian,
                    Nellymoser16kHzMono,
                    Nellymoser8kHzMono,
                    Nellymoser,
                    G711ALawLogarithmicPcm,
                    G711MuLawLogarithmicPcm,
                    Reserved,
                    AAC,
                    Speex,
                    Mp3At8kHz = 14,
                    DeviceSpecificSound
                }

                public enum SoundRates
                {
                    R5p5kHz = 0,
                    R11kHz,
                    R22kHz,
                    R44kHz,
                }

                public enum SoundSizes
                {
                    S8bit = 0,
                    S16bit,
                }

                public enum SoundTypes
                {
                    Mono = 0,
                    Stereo,
                }

                #endregion

                #region Properties

                public SoundFormats SoundFormat
                {
                    get
                    {
                        return (SoundFormats)((uint)BodyBytes[0] >> 4);
                    }
                }

                public SoundRates SoundRate
                {
                    get
                    {
                        return (SoundRates)(((uint)BodyBytes[0] & 0b00001100) >> 2);
                    }
                }

                public SoundSizes SoundSize
                {
                    get
                    {
                        return (SoundSizes)(((uint)BodyBytes[0] & 0b00000010) >> 1);
                    }
                }

                public SoundTypes SoundType
                {
                    get
                    {
                        return (SoundTypes)((uint)BodyBytes[0] & 0b00000001);
                    }
                }

                public override TagType Type
                {
                    get
                    {
                        return TagType.Audio;
                    }
                }

                public override byte[] BodyBytes { get; }

                public override uint BodyLength
                {
                    get
                    {
                        return (uint)BodyBytes.Length;
                    }
                }

                #endregion

                #region Constructor

                public AudioTag(byte[] headerBytes, byte[] bodyBytes) : base(headerBytes)
                {
                    BodyBytes = bodyBytes;
                }

                #endregion

                public override string ToString()
                {
                    return string.Format(
                        "{{\"Type\":\"{0}\"," +
                        "\"Timestamp\":{1}," +
                        "\"SoundFormat\":\"{2}\"," +
                        "\"SoundRate\":\"{3}\"," +
                        "\"SoundSize\":\"{4}\"," +
                        "\"SoundType\":\"{5}\"}}", 
                        Type, 
                        Header.Timestamp, 
                        SoundFormat, 
                        SoundRate, 
                        SoundSize, 
                        SoundType);
                }
            }
        }

        #endregion
    }
}
