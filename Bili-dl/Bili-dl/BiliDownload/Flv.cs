using System.Collections.Generic;
using System.IO;

namespace BiliDownload
{
    class Flv
    {
        public static void Merge(List<string> inputs, string output)
        {
            FileStream outputStream = new FileStream(output, FileMode.Create);
            int timeOffest = 0;
            for(int i=0; i<inputs.Count; i++)
            {
                FileStream inputStream = new FileStream(inputs[i], FileMode.Open);
                timeOffest = Merge(inputStream, outputStream, timeOffest);
                inputStream.Close();
            }
            outputStream.Close();
        }

        private const int FlvHeaderSize = 9;
        private const int FlvTagHeaderSize = 11;
        private const int FlvPreviousTagSize = 4;

        private static int ToInt24Be(byte b0, byte b1, byte b2)
        {
            return (int)((b0 << 16) | (b1 << 8) | (b2));
        }

        private static int GetTimestamp(byte b0, byte b1, byte b2, byte ex)
        {
            return ((ex << 24) | (b0 << 16) | (b1 << 8) | (b2));
        }

        private static void SetTimestamp(byte[] data, int offset, int newTimestamp)
        {
            data[offset + 3] = (byte)(newTimestamp >> 24);
            data[offset + 0] = (byte)(newTimestamp >> 16);
            data[offset + 1] = (byte)(newTimestamp >> 8);
            data[offset + 2] = (byte)(newTimestamp);
        }

        private static int Merge(FileStream inputStream, FileStream outputStream, int timestampOffset)
        {
            if (outputStream.Position == 0)
            {
                byte[] headerBuffer = new byte[FlvHeaderSize + FlvPreviousTagSize];
                inputStream.Read(headerBuffer, 0, FlvHeaderSize + FlvPreviousTagSize);
                outputStream.Write(headerBuffer, 0, FlvHeaderSize + FlvPreviousTagSize);
            }
            else
                inputStream.Position = FlvHeaderSize + FlvPreviousTagSize;
            int tagSize;
            int currentTimestamp;
            int mergedTimestamp = timestampOffset;
            int readSize;
            byte[] tagHeaderBuffer = new byte[FlvTagHeaderSize];
            while (inputStream.Read(tagHeaderBuffer, 0, FlvTagHeaderSize) > 0)
            {
                tagSize = ToInt24Be(tagHeaderBuffer[1], tagHeaderBuffer[2], tagHeaderBuffer[3]);
                currentTimestamp = GetTimestamp(tagHeaderBuffer[4], tagHeaderBuffer[5], tagHeaderBuffer[6], tagHeaderBuffer[7]);
                mergedTimestamp = currentTimestamp + timestampOffset;
                SetTimestamp(tagHeaderBuffer, 4, mergedTimestamp);
                outputStream.Write(tagHeaderBuffer, 0, FlvTagHeaderSize);
                readSize = tagSize + FlvPreviousTagSize;
                byte[] dataBuffer = new byte[readSize];
                inputStream.Read(dataBuffer, 0, readSize);
                outputStream.Write(dataBuffer, 0, readSize);
            }
            return mergedTimestamp;
        }
    }
}
