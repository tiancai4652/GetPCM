using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GetPCM
{
    class Program
    {
        static void Main(string[] args)
        {
            string rgb2 = @"C:\Users\Sam\Desktop\95_84_24.rgb";
            string bmp = @"C:\Users\Sam\Desktop\无标题.bmp";
            string rgb = @"C:\Users\Sam\Desktop\1.rgb";
            string bmp2 = @"C:\Users\Sam\Desktop\x1.bmp";

            string bmp3 = @"C:\Users\Sam\Desktop\res\95_84_32.bmp";



            GetRGBFromBMP(bmp, rgb, out UInt32 biWidth, out UInt32 biHeight, out UInt16 biBitCount, out UInt32 biCompression
             , out UInt32 biXPelsPerMeter, out UInt32 biYPelsPerMeter);

            SetRGBToBMP(rgb, bmp2, biWidth, biHeight, biBitCount, biCompression
             , biXPelsPerMeter, biYPelsPerMeter);


            string wavFile = @"C:\Users\Sam\Desktop\月亮代表我的心-黄红英.wav";
            string tempPCMFile = @"C:\Users\Sam\Desktop\x1.pcm";
            string wavFile2 = @"C:\Users\Sam\Desktop\x1.wav";

            GetPCMFromWAV(wavFile, tempPCMFile, out UInt32 samplesPerSec, out UInt16 bitsPerSample, out UInt16 channels);
            SetPCMToWAV(tempPCMFile, (int)samplesPerSec, bitsPerSample, channels, wavFile2);



        }

        public static void GetPCMFromWAV(string wavFile, string pcmFile,out UInt32 samplesPerSec,out UInt16 bitsPerSample,out UInt16 channels)
        {
            using (FileStream stream = new FileStream(wavFile, FileMode.OpenOrCreate))
            {
                using (BinaryReader reader = new BinaryReader(stream, System.Text.Encoding.ASCII))
                {
                    //内容为"RIFF"
                    string chunkId = new string(reader.ReadChars(4));
                    //存储文件的字节数（不包含ChunkID和ChunkSize这8个字节）
                    UInt32 chunkSize = reader.ReadUInt32();
                    //内容为"WAVE"
                    string riffType = new string(reader.ReadChars(4));
                    //内容为"fmt"
                    string fmtId = new string(reader.ReadChars(4));
                    //存储该子块的字节数（不含前面的Subchunk1ID和Subchunk1Size这8个字节）
                    UInt32 fmtSize = reader.ReadUInt32();
                    ////存储音频文件的编码格式，例如若为PCM则其存储值为1，若为其他非PCM格式的则有一定的压缩。
                    UInt16 formatTag = reader.ReadUInt16();
                    ////通道数，单通道(Mono)值为1，双通道(Stereo)值为2，等等
                    channels = reader.ReadUInt16();
                    ////采样率，如8k，44.1k等
                    samplesPerSec = reader.ReadUInt32();
                    ////每秒存储的bit数，其值=SampleRate * NumChannels * BitsPerSample/8
                    UInt32 avgBytesPerSec = reader.ReadUInt32();
                    ////块对齐大小，其值=NumChannels * BitsPerSample/8
                    UInt16 blockAlign = reader.ReadUInt16();
                    ////每个采样点的bit数，一般为8,16,32等。
                    bitsPerSample = reader.ReadUInt16();
                    //内容为“data”
                    string dataID = new string(reader.ReadChars(4));
                    ////内容为接下来的正式的数据部分的字节数，其值=NumSamples * NumChannels * BitsPerSample/8
                    UInt32 dataSize = reader.ReadUInt32();

                    //try
                    //{
                    //    UInt32 other = reader.ReadUInt32();
                    //}
                    //catch(Exception ex)
                    //{ }

                    if (chunkId != "RIFF" || riffType != "WAVE" || fmtId != "fmt " || dataID != "data" || fmtSize != 16)
                    {
                        //Console.WriteLine("Malformed WAV header");
                        MessageBox.Show("Malformed WAV header");
                        return;
                    }

                    //if (channels != 1 || samplesPerSec != 11025 || avgBytesPerSec != 22050 || blockAlign != 2 || bitsPerSample != 16 || formatTag != 1 || chunkSize < 48)
                    //{
                    //    Console.WriteLine("Unexpected WAV format, need 11025 Hz mono 16 bit (little endian integers)");
                    //    return;
                    //}

                    //uint numberOfsamples = Math.Min(dataSize / 2, 330750); // max 30 seconds
                    uint numberOfsamples = dataSize;
                    var pcmData = new byte[numberOfsamples];
                    for (int i = 0; i < numberOfsamples; i++)
                    {
                        pcmData[i] = reader.ReadByte();
                    }

                    Stream streamByte = new MemoryStream(pcmData);

                    using (var fileStream = File.Create(pcmFile))
                    {
                        streamByte.Seek(0, SeekOrigin.Begin);
                        streamByte.CopyTo(fileStream);
                    }
                }

            }
        }

        public static void SetRGBToBMP(string rgbFile, string bmpFile, UInt32 biWidth, UInt32 biHeight, UInt16 biBitCount, UInt32 biCompression
          , UInt32 biXPelsPerMeter, UInt32 biYPelsPerMeter)
        {
            using (FileStream stream = new FileStream(rgbFile, FileMode.OpenOrCreate))
            {
                using (BinaryReader reader = new BinaryReader(stream, System.Text.Encoding.ASCII))
                {
                    List<byte> pcmData = new List<byte>();
                    try
                    {
                        while (true)
                        {
                            pcmData.Add(reader.ReadByte());
                        }
                    }
                    catch (Exception ex)
                    {

                    }

                    #region  位图文件头(bmp file header)：  提供文件的格式、大小等信息
                    //2Bytes，必须为"BM"，即0x424D 才是Windows位图文件
                    string bfType = "BM";
                    //4Bytes，整个BMP文件的大小
                    UInt32 bfSize = (UInt32)(14 + 40 + pcmData.Count);
                    //2Bytes，保留，为0
                    UInt16 bfReserved1 = 0;
                    //2Bytes，保留，为0
                    UInt16 bfReserved2 = 0;
                    //4Bytes，文件起始位置到图像像素数据的字节偏移量
                    UInt32 bfOffBits = 14 + 40;
                    #endregion

                    #region 位图信息头(bitmap information)：提供图像数据的尺寸、位平面数、压缩方式、颜色索引等信息
                    //4Bytes，INFOHEADER结构体大小，存在其他版本I NFOHEADER，用作区分
                    UInt32 biSize = 40;
                    //4Bytes，图像宽度（以像素为单位）
                    //UInt32 biWidth = reader.ReadUInt32();
                    //4Bytes，图像高度，+：图像存储顺序为Bottom2Top，-：Top2Bottom
                    //UInt32 biHeight = reader.ReadUInt32();
                    //2Bytes，图像数据平面，BMP存储RGB数据，因此总为1
                    UInt16 biPlanes = 1;
                    //2Bytes，图像像素位数
                    //UInt16 biBitCount = reader.ReadUInt16();
                    //4Bytes，0：不压缩，1：RLE8，2：RLE4
                    //UInt32 biCompression = reader.ReadUInt32();
                    //4Bytes，4字节对齐的图像数据大小
                    UInt32 biSizeImage = 0;
                    //4 Bytes，用象素/米表示的水平分辨率
                    //UInt32 biXPelsPerMeter = reader.ReadUInt32();
                    //4 Bytes，用象素/米表示的垂直分辨率
                    //UInt32 biYPelsPerMeter = reader.ReadUInt32();
                    //4 Bytes，实际使用的调色板索引数，0：使用所有的调色板索引
                    UInt32 biClrUsed = 0;
                    //4 Bytes，重要的调色板索引数，0：所有的调色板索引都重要
                    UInt32 biClrImportant = 0;
                    #endregion

                    #region 调色板(color palette)：可选，如使用索引来表示图像，调色板就是索引与其对应的颜色的映射表
                    //1，4，8位图像才会使用调色板数据，16,24,32位图像不需要调色板数据，即调色板最多只需要256项（索引0 - 255）。
                    if (biBitCount == 1 || biBitCount == 4 || biBitCount == 8)
                    {
                        ////指定蓝色强度
                        //byte rgbBlue = reader.ReadByte();
                        ////指定绿色强度
                        //byte rgbGreen = reader.ReadByte();
                        ////指定红色强度
                        //byte rgbRed = reader.ReadByte();
                        ////保留，设置为0
                        //byte rgbReserved = reader.ReadByte();
                        MessageBox.Show("error:biBitCount == 1 || biBitCount == 4 || biBitCount == 8");
                        return;
                    }

                    #endregion



                    List<byte> result = new List<byte>();
                    result.AddRange(System.Text.Encoding.ASCII.GetBytes(bfType));
                    result.AddRange(BitConverter.GetBytes(bfSize));
                    result.AddRange(BitConverter.GetBytes(bfReserved1));
                    result.AddRange(BitConverter.GetBytes(bfReserved2));
                    result.AddRange(BitConverter.GetBytes(bfOffBits));

                    result.AddRange(BitConverter.GetBytes(biSize));
                    result.AddRange(BitConverter.GetBytes(biWidth));
                    result.AddRange(BitConverter.GetBytes(biHeight));
                    result.AddRange(BitConverter.GetBytes(biPlanes));

                    result.AddRange(BitConverter.GetBytes(biBitCount));
                    result.AddRange(BitConverter.GetBytes(biCompression));
                    result.AddRange(BitConverter.GetBytes(biSizeImage));
                    result.AddRange(BitConverter.GetBytes(biPlanes));
                    result.AddRange(BitConverter.GetBytes(biXPelsPerMeter));
                    result.AddRange(BitConverter.GetBytes(biYPelsPerMeter));
                    result.AddRange(BitConverter.GetBytes(biClrUsed));
                    result.AddRange(BitConverter.GetBytes(biClrImportant));
                    result.AddRange(pcmData);

                    Stream streamByte = new MemoryStream(result.ToArray());

                    using (var fileStream = File.Create(bmpFile))
                    {
                        streamByte.Seek(0, SeekOrigin.Begin);
                        streamByte.CopyTo(fileStream);
                    }
                }

            }


        }

        public static void GetRGBFromBMP(string bmpFile, string rgbFile, out UInt32 biWidth,out UInt32 biHeight, out UInt16 biBitCount,out UInt32 biCompression
             ,out UInt32 biXPelsPerMeter,out UInt32 biYPelsPerMeter)
        {
            using (FileStream stream = new FileStream(bmpFile, FileMode.OpenOrCreate))
            {
                using (BinaryReader reader = new BinaryReader(stream, System.Text.Encoding.ASCII))
                {
                    #region  位图文件头(bmp file header)：  提供文件的格式、大小等信息
                    //2Bytes，必须为"BM"，即0x424D 才是Windows位图文件
                    string bfType = new string(reader.ReadChars(2));
                    //4Bytes，整个BMP文件的大小
                    UInt32 bfSize = reader.ReadUInt32();
                    //2Bytes，保留，为0
                    UInt16 bfReserved1 = reader.ReadUInt16();
                    //2Bytes，保留，为0
                    UInt16 bfReserved2 = reader.ReadUInt16();
                    //4Bytes，文件起始位置到图像像素数据的字节偏移量
                    UInt32 bfOffBits = reader.ReadUInt32();
                    #endregion

                    #region 位图信息头(bitmap information)：提供图像数据的尺寸、位平面数、压缩方式、颜色索引等信息
                    //4Bytes，INFOHEADER结构体大小，存在其他版本I NFOHEADER，用作区分
                    UInt32 biSize = reader.ReadUInt32();
                    //4Bytes，图像宽度（以像素为单位）
                     biWidth = reader.ReadUInt32();
                    //4Bytes，图像高度，+：图像存储顺序为Bottom2Top，-：Top2Bottom
                     biHeight= reader.ReadUInt32();
                    //2Bytes，图像数据平面，BMP存储RGB数据，因此总为1
                    UInt16 biPlanes = reader.ReadUInt16();
                    //2Bytes，图像像素位数
                     biBitCount = reader.ReadUInt16();
                    //4Bytes，0：不压缩，1：RLE8，2：RLE4
                     biCompression = reader.ReadUInt32();
                    //4Bytes，4字节对齐的图像数据大小
                    UInt32 biSizeImage = reader.ReadUInt32();
                    //4 Bytes，用象素/米表示的水平分辨率
                     biXPelsPerMeter = reader.ReadUInt32();
                    //4 Bytes，用象素/米表示的垂直分辨率
                     biYPelsPerMeter = reader.ReadUInt32();
                    //4 Bytes，实际使用的调色板索引数，0：使用所有的调色板索引
                    UInt32 biClrUsed = reader.ReadUInt32();
                    //4 Bytes，重要的调色板索引数，0：所有的调色板索引都重要
                    UInt32 biClrImportant = reader.ReadUInt32();
                    #endregion

                    #region 调色板(color palette)：可选，如使用索引来表示图像，调色板就是索引与其对应的颜色的映射表
                    //1，4，8位图像才会使用调色板数据，16,24,32位图像不需要调色板数据，即调色板最多只需要256项（索引0 - 255）。
                    if (biBitCount == 1 || biBitCount == 4 || biBitCount == 8)
                    {
                        //指定蓝色强度
                        byte rgbBlue = reader.ReadByte();
                        //指定绿色强度
                        byte rgbGreen = reader.ReadByte();
                        //指定红色强度
                        byte rgbRed = reader.ReadByte();
                        //保留，设置为0
                        byte rgbReserved = reader.ReadByte();
                    }

                    #endregion

                    #region 位图数据(bitmap data)：图像数据区
                    //当biBitCount = 1时，8个像素占1个字节;
                    //当biBitCount = 4时，2个像素占1个字节;
                    //当biBitCount = 8时，1个像素占1个字节;
                    //当biBitCount = 24时,1个像素占3个字节;



                    //Windows规定一个扫描行所占的字节数必须是4的倍数(即以long为单位),不足的以0填充，
                    //DataSizePerLine = (biWidth * biBitCount + 31) / 8;
                    //DataSizePerLine = DataSizePerLine / 4 * 4;
                    var dataSizePerLine = (biWidth * biBitCount) / 8;
                    if (dataSizePerLine % 4 != 0)
                    {
                        dataSizePerLine = (dataSizePerLine / 4 + 1) * 4;
                    }
                    var dataSize = dataSizePerLine * biHeight;


                    #endregion
             
                    var dataBytes = new byte[dataSize];
                    for (int i = 0; i < dataSize; i++)
                    {
                        dataBytes[i] = reader.ReadByte();
                    }

                    Stream streamByte = new MemoryStream(dataBytes);

                    using (var fileStream = File.Create(rgbFile))
                    {
                        streamByte.Seek(0, SeekOrigin.Begin);
                        streamByte.CopyTo(fileStream);
                    }

                    ////内容为"RIFF"
                    //string chunkId = new string(reader.ReadChars(4));
                    ////存储文件的字节数（不包含ChunkID和ChunkSize这8个字节）
                    //UInt32 chunkSize = reader.ReadUInt32();
                    ////内容为"WAVE"
                    //string riffType = new string(reader.ReadChars(4));
                    ////内容为"fmt"
                    //string fmtId = new string(reader.ReadChars(4));
                    ////存储该子块的字节数（不含前面的Subchunk1ID和Subchunk1Size这8个字节）
                    //UInt32 fmtSize = reader.ReadUInt32();
                    //////存储音频文件的编码格式，例如若为PCM则其存储值为1，若为其他非PCM格式的则有一定的压缩。
                    //UInt16 formatTag = reader.ReadUInt16();
                    //////通道数，单通道(Mono)值为1，双通道(Stereo)值为2，等等
                    //channels = reader.ReadUInt16();
                    //////采样率，如8k，44.1k等
                    //samplesPerSec = reader.ReadUInt32();
                    //////每秒存储的bit数，其值=SampleRate * NumChannels * BitsPerSample/8
                    //UInt32 avgBytesPerSec = reader.ReadUInt32();
                    //////块对齐大小，其值=NumChannels * BitsPerSample/8
                    //UInt16 blockAlign = reader.ReadUInt16();
                    //////每个采样点的bit数，一般为8,16,32等。
                    //bitsPerSample = reader.ReadUInt16();
                    ////内容为“data”
                    //string dataID = new string(reader.ReadChars(4));
                    //////内容为接下来的正式的数据部分的字节数，其值=NumSamples * NumChannels * BitsPerSample/8
                    //UInt32 dataSize = reader.ReadUInt32();

                    //try
                    //{
                    //    UInt32 other = reader.ReadUInt32();
                    //}
                    //catch(Exception ex)
                    //{ }

                    //    if (chunkId != "RIFF" || riffType != "WAVE" || fmtId != "fmt " || dataID != "data" || fmtSize != 16)
                    //    {
                    //        //Console.WriteLine("Malformed WAV header");
                    //        MessageBox.Show("Malformed WAV header");
                    //        return;
                    //    }

                    //    //if (channels != 1 || samplesPerSec != 11025 || avgBytesPerSec != 22050 || blockAlign != 2 || bitsPerSample != 16 || formatTag != 1 || chunkSize < 48)
                    //    //{
                    //    //    Console.WriteLine("Unexpected WAV format, need 11025 Hz mono 16 bit (little endian integers)");
                    //    //    return;
                    //    //}

                    //    //uint numberOfsamples = Math.Min(dataSize / 2, 330750); // max 30 seconds
                    //    uint numberOfsamples = dataSize;
                    //    var pcmData = new byte[numberOfsamples];
                    //    for (int i = 0; i < numberOfsamples; i++)
                    //    {
                    //        pcmData[i] = reader.ReadByte();
                    //    }

                    //    Stream streamByte = new MemoryStream(pcmData);

                    //    using (var fileStream = File.Create(pcmFile))
                    //    {
                    //        streamByte.Seek(0, SeekOrigin.Begin);
                    //        streamByte.CopyTo(fileStream);
                    //    }
                }

            }
        }

      
        public static void SetPCMToWAV(string pcmFile, int sampleRate, int bits, int channelsCount, string wavFile)
        {
            using (FileStream stream = new FileStream(pcmFile, FileMode.OpenOrCreate))
            {
                using (BinaryReader reader = new BinaryReader(stream, System.Text.Encoding.ASCII))
                {
                    List<byte> pcmData = new List<byte>();
                    try
                    {
                        while (true)
                        {
                            pcmData.Add(reader.ReadByte());
                        }
                    }
                    catch (Exception ex)
                    {

                    }


                    //内容为"RIFF"
                    string chunkId = "RIFF";
                    //存储文件的字节数（不包含ChunkID和ChunkSize这8个字节）
                    UInt32 chunkSize = (UInt32)pcmData.Count + 44 - 8;
                    //内容为"WAVE"
                    string riffType = "WAVE";
                    //内容为"fmt"
                    string fmtId = "fmt ";
                    //存储该子块的字节数（不含前面的Subchunk1ID和Subchunk1Size这8个字节）
                    UInt32 fmtSize = 16;
                    //存储音频文件的编码格式，例如若为PCM则其存储值为1，若为其他非PCM格式的则有一定的压缩。
                    UInt16 formatTag = 1;
                    //通道数，单通道(Mono)值为1，双通道(Stereo)值为2，等等
                    UInt16 channels = (UInt16)channelsCount;
                    //采样率，如8k，44.1k等
                    UInt32 samplesPerSec = (UInt32)sampleRate;
                    //每秒存储的bit数，其值=SampleRate * NumChannels * BitsPerSample/8
                    /// 每样本的数据位数，表示每个声道中各个样本的数据位数。如果有多个声道，对每个声道而言，样本大小都一样。
                    UInt32 avgBytesPerSec = (UInt32)(sampleRate * /*channelsCount **/ bits / 8);
                    //块对齐大小，其值=NumChannels * BitsPerSample/8
                    UInt16 blockAlign = (UInt16)(channelsCount * bits / 8);
                    //每个采样点的bit数，一般为8,16,32等。
                    UInt16 bitsPerSample = (UInt16)bits;
                    //内容为“data”
                    string dataID = "data";
                    //内容为接下来的正式的数据部分的字节数，其值=NumSamples * NumChannels * BitsPerSample/8
                    UInt32 dataSize = (UInt32)pcmData.Count;

                    if (chunkId != "RIFF" || riffType != "WAVE" || fmtId != "fmt " || dataID != "data" || fmtSize != 16)
                    {
                        Console.WriteLine("Malformed WAV header");
                        MessageBox.Show("Malformed WAV header");
                        return;
                    }

                    List<byte> result = new List<byte>();
                    result.AddRange(System.Text.Encoding.ASCII.GetBytes(chunkId));
                    result.AddRange(BitConverter.GetBytes(chunkSize));
                    result.AddRange(System.Text.Encoding.ASCII.GetBytes(riffType));
                    result.AddRange(System.Text.Encoding.ASCII.GetBytes(fmtId));
                    result.AddRange(BitConverter.GetBytes(fmtSize));
                    result.AddRange(BitConverter.GetBytes(formatTag));
                    result.AddRange(BitConverter.GetBytes(channels));
                    result.AddRange(BitConverter.GetBytes(samplesPerSec));
                    result.AddRange(BitConverter.GetBytes(avgBytesPerSec));
                    result.AddRange(BitConverter.GetBytes(blockAlign));
                    result.AddRange(BitConverter.GetBytes(bitsPerSample));
                    result.AddRange(System.Text.Encoding.ASCII.GetBytes(dataID));
                    result.AddRange(BitConverter.GetBytes(dataSize));
                    result.AddRange(pcmData);

                    Stream streamByte = new MemoryStream(result.ToArray());

                    using (var fileStream = File.Create(wavFile))
                    {
                        streamByte.Seek(0, SeekOrigin.Begin);
                        streamByte.CopyTo(fileStream);
                    }
                }

            }


        }
    }
}
