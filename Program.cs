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


        //        ypedef struct{
        //char ChunkID[4];//内容为"RIFF"
        //        unsigned long ChunkSize;//存储文件的字节数（不包含ChunkID和ChunkSize这8个字节）
        //        char Format[4];//内容为"WAVE"
        //    }
        //    WAVE_HEADER;
        //typedef struct{
        //char Subchunk1ID[4];//内容为"fmt"
        //    unsigned long Subchunk1Size;//存储该子块的字节数（不含前面的Subchunk1ID和Subchunk1Size这8个字节）
        //    unsigned short AudioFormat;//存储音频文件的编码格式，例如若为PCM则其存储值为1，若为其他非PCM格式的则有一定的压缩。
        //    unsigned short NumChannels;//通道数，单通道(Mono)值为1，双通道(Stereo)值为2，等等
        //    unsigned long SampleRate;//采样率，如8k，44.1k等
        //    unsigned long ByteRate;//每秒存储的bit数，其值=SampleRate * NumChannels * BitsPerSample/8
        //    unsigned short BlockAlign;//块对齐大小，其值=NumChannels * BitsPerSample/8
        //    unsigned short BitsPerSample;//每个采样点的bit数，一般为8,16,32等。
        //}
        //WAVE_FMT;
        //typedef struct{
        //char Subchunk2ID[4];//内容为“data”
        //unsigned long Subchunk2Size;//内容为接下来的正式的数据部分的字节数，其值=NumSamples * NumChannels * BitsPerSample/8
        //}WAVE_DATA;
        public static void SetPCMToWAV(string pcmFile, int sampleRate, int bits,int channelsCount ,string wavFile)
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
                    catch(Exception ex)
                    { 
                    
                    }
                    

                    //内容为"RIFF"
                    string chunkId = "RIFF";
                    //存储文件的字节数（不包含ChunkID和ChunkSize这8个字节）
                    UInt32 chunkSize = (UInt32)pcmData.Count + 44 - 8 ;
                    //内容为"WAVE"
                    string riffType = "WAVE";
                    //内容为"fmt"
                    string fmtId = "fmt ";
                    //存储该子块的字节数（不含前面的Subchunk1ID和Subchunk1Size这8个字节）
                    UInt32 fmtSize = 16;
                    ////存储音频文件的编码格式，例如若为PCM则其存储值为1，若为其他非PCM格式的则有一定的压缩。
                    UInt16 formatTag = 1;
                    ////通道数，单通道(Mono)值为1，双通道(Stereo)值为2，等等
                    UInt16 channels = (UInt16)channelsCount;
                    ////采样率，如8k，44.1k等
                    UInt32 samplesPerSec = (UInt32)sampleRate;
                    ////每秒存储的bit数，其值=SampleRate * NumChannels * BitsPerSample/8
                    ///每样本的数据位数，表示每个声道中各个样本的数据位数。如果有多个声道，对每个声道而言，样本大小都一样。
                    UInt32 avgBytesPerSec = (UInt32)(sampleRate* /*channelsCount **/ bits /8);
                    ////块对齐大小，其值=NumChannels * BitsPerSample/8
                    UInt16 blockAlign = (UInt16)(channelsCount* bits/8);
                    ////每个采样点的bit数，一般为8,16,32等。
                    UInt16 bitsPerSample = (UInt16)bits;
                    //内容为“data”
                    string dataID = "data";
                    ////内容为接下来的正式的数据部分的字节数，其值=NumSamples * NumChannels * BitsPerSample/8
                    UInt32 dataSize = (UInt32)pcmData.Count;

                    if (chunkId != "RIFF" || riffType != "WAVE" || fmtId != "fmt " || dataID != "data" || fmtSize != 16)
                    {
                        //Console.WriteLine("Malformed WAV header");
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
