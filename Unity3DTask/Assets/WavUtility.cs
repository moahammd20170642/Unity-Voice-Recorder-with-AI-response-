using UnityEngine;
using System.Text;
using System.IO;
using System;

/// <summary>
/// WAV utility for recording and audio playback functions in Unity.
/// Version: 1.0 alpha 1
///
/// - Use "ToAudioClip" method for loading wav file / bytes.
/// Loads .wav (PCM uncompressed) files at 8,16,24 and 32 bits and converts data to Unity's AudioClip.
///
/// - Use "FromAudioClip" method for saving wav file / bytes.
/// Converts an AudioClip's float data into wav byte array at 16 bit.
/// </summary>
/// <remarks>
/// For documentation and usage examples: https://github.com/deadlyfingers/UnityWav
/// </remarks>
public class WavUtility
{
    // Force save as 16-bit .wav
    const int BlockSize_16Bit = 2;

    /// <summary>
    /// Load PCM format *.wav audio file (using Unity's Application data path) and convert to AudioClip.
    /// </summary>
    /// <returns>The AudioClip.</returns>
    /// <param name="filePath">Local file path to .wav file</param>
    public static AudioClip ToAudioClip(string filePath)
    {
        if (!filePath.StartsWith(Application.persistentDataPath) && !filePath.StartsWith(Application.dataPath))
        {
            Debug.LogWarning("This only supports files that are stored using Unity's Application data path. \nTo load bundled resources use 'Resources.Load(\"filename\") typeof(AudioClip)' method. \nhttps://docs.unity3d.com/ScriptReference/Resources.Load.html");
            return null;
        }
        byte[] fileBytes = File.ReadAllBytes(filePath);
        return ToAudioClip(fileBytes, 0);
    }

    public static AudioClip ToAudioClip(byte[] fileBytes, int offsetSamples = 0, string name = "wav")
    {
        //string riff = Encoding.ASCII.GetString (fileBytes, 0, 4);
        //string wave = Encoding.ASCII.GetString (fileBytes, 8, 4);
        int subchunk1 = BitConverter.ToInt32(fileBytes, 16);
        UInt16 audioFormat = BitConverter.ToUInt16(fileBytes, 20);

        // NB: Only uncompressed PCM wav files are supported.
        string formatCode = FormatCode(audioFormat);
        Debug.AssertFormat(audioFormat == 1 || audioFormat == 65534, "Detected format code '{0}' {1}, but only PCM and WaveFormatExtensable uncompressed formats are currently supported.", audioFormat, formatCode);

        UInt16 channels = BitConverter.ToUInt16(fileBytes, 22);
        int sampleRate = BitConverter.ToInt32(fileBytes, 24);
        //int byteRate = BitConverter.ToInt32 (fileBytes, 28);
        //UInt16 blockAlign = BitConverter.ToUInt16 (fileBytes, 32);
        UInt16 bitDepth = BitConverter.ToUInt16(fileBytes, 34);

        int headerOffset = 16 + 4 + subchunk1 + 4;
        int subchunk2 = BitConverter.ToInt32(fileBytes, headerOffset);
        //Debug.LogFormat ("riff={0} wave={1} subchunk1={2} format={3} channels={4} sampleRate={5} byteRate={6} blockAlign={7} bitDepth={8} headerOffset={9} subchunk2={10} filesize={11}", riff, wave, subchunk1, formatCode, channels, sampleRate, byteRate, blockAlign, bitDepth, headerOffset, subchunk2, fileBytes.Length);

        float[] data;
        switch (bitDepth)
        {
            case 8:
                data = Convert8BitByteArrayToAudioClipData(fileBytes, headerOffset, subchunk2);
                break;
            case 16:
                data = Convert16BitByteArrayToAudioClipData(fileBytes, headerOffset, subchunk2);
                break;
            case 24:
                data = Convert24BitByteArrayToAudioClipData(fileBytes, headerOffset, subchunk2);
                break;
            case 32:
                data = Convert32BitByteArrayToAudioClipData(fileBytes, headerOffset, subchunk2);
                break;
            default:
                throw new Exception(bitDepth + " bit depth is not supported.");
        }

        // Create the AudioClip with preloadAudioData set to true
        AudioClip audioClip = AudioClip.Create(name, data.Length, (int)channels, sampleRate, true);
        audioClip.SetData(data, 0);
        return audioClip;
    }

    #region wav file bytes to Unity AudioClip conversion methods

    private static float[] Convert8BitByteArrayToAudioClipData(byte[] source, int headerOffset, int dataSize)
    {
        int wavSize = BitConverter.ToInt32(source, headerOffset);
        headerOffset += sizeof(int);
        Debug.AssertFormat(wavSize > 0 && wavSize == dataSize, "Failed to get valid 8-bit wav size: {0} from data bytes: {1} at offset: {2}", wavSize, dataSize, headerOffset);

        float[] data = new float[wavSize];

        sbyte maxValue = sbyte.MaxValue;

        int i = 0;
        while (i < wavSize)
        {
            data[i] = (float)source[i] / maxValue;
            ++i;
        }

        return data;
    }

    private static float[] Convert16BitByteArrayToAudioClipData(byte[] source, int headerOffset, int dataSize)
    {
        int wavSize = BitConverter.ToInt32(source, headerOffset);
        headerOffset += sizeof(int);
        Debug.AssertFormat(wavSize > 0 && wavSize == dataSize, "Failed to get valid 16-bit wav size: {0} from data bytes: {1} at offset: {2}", wavSize, dataSize, headerOffset);

        int x = sizeof(Int16); // block size = 2
        int convertedSize = wavSize / x;

        float[] data = new float[convertedSize];

        Int16 maxValue = Int16.MaxValue;

        int offset = 0;
        int i = 0;
        while (i < convertedSize)
        {
            offset = i * x + headerOffset;
            data[i] = (float)BitConverter.ToInt16(source, offset) / maxValue;
            ++i;
        }

        Debug.AssertFormat(data.Length == convertedSize, "AudioClip .wav data is wrong size: {0} == {1}", data.Length, convertedSize);

        return data;
    }

    private static float[] Convert24BitByteArrayToAudioClipData(byte[] source, int headerOffset, int dataSize)
    {
        int wavSize = BitConverter.ToInt32(source, headerOffset);
        headerOffset += sizeof(int);
        Debug.AssertFormat(wavSize > 0 && wavSize == dataSize, "Failed to get valid 24-bit wav size: {0} from data bytes: {1} at offset: {2}", wavSize, dataSize, headerOffset);

        int x = 3; // block size = 3
        int convertedSize = wavSize / x;

        int maxValue = Int32.MaxValue;

        float[] data = new float[convertedSize];

        byte[] block = new byte[sizeof(int)]; // using a 4 byte block for copying 3 bytes, then copy bytes with 1 offset

        int offset = 0;
        int i = 0;
        while (i < convertedSize)
        {
            offset = i * x + headerOffset;
            Buffer.BlockCopy(source, offset, block, 1, x);
            data[i] = (float)BitConverter.ToInt32(block, 0) / maxValue;
            ++i;
        }

        Debug.AssertFormat(data.Length == convertedSize, "AudioClip .wav data is wrong size: {0} == {1}", data.Length, convertedSize);

        return data;
    }

    private static float[] Convert32BitByteArrayToAudioClipData(byte[] source, int headerOffset, int dataSize)
    {
        int wavSize = BitConverter.ToInt32(source, headerOffset);
        headerOffset += sizeof(int);
        Debug.AssertFormat(wavSize > 0 && wavSize == dataSize, "Failed to get valid 32-bit wav size: {0} from data bytes: {1} at offset: {2}", wavSize, dataSize, headerOffset);

        int x = sizeof(float); //  block size = 4
        int convertedSize = wavSize / x;

        Int32 maxValue = Int32.MaxValue;

        float[] data = new float[convertedSize];

        int offset = 0;
        int i = 0;
        while (i < convertedSize)
        {
            offset = i * x + headerOffset;
            data[i] = (float)BitConverter.ToInt32(source, offset) / maxValue;
            ++i;
        }

        Debug.AssertFormat(data.Length == convertedSize, "AudioClip .wav data is wrong size: {0} == {1}", data.Length, convertedSize);

        return data;
    }

    #endregion

    public static byte[] FromAudioClip(AudioClip audioClip)
    {
        string file;
        return FromAudioClip(audioClip, out file, false);
    }

    public static byte[] FromAudioClip(AudioClip audioClip, out string filepath, bool saveAsFile = true, string dirname = "recordings")
    {
        if (audioClip == null)
        {
            Debug.LogWarning("AudioClip is null");
            filepath = "";
            return null;
        }

        // Convert the audio clip data to byte array
        float[] audioData = new float[audioClip.samples * audioClip.channels];
        audioClip.GetData(audioData, 0);

        byte[] wavFile = ConvertAudioClipDataToWav(audioClip, audioData);

        filepath = "";
        if (saveAsFile)
        {
            if (!Directory.Exists(dirname))
            {
                Directory.CreateDirectory(dirname);
            }
            filepath = Path.Combine(dirname, audioClip.name + ".wav");
            File.WriteAllBytes(filepath, wavFile);
        }

        return wavFile;
    }

    private static byte[] ConvertAudioClipDataToWav(AudioClip audioClip, float[] audioData)
    {
        int sampleCount = audioData.Length;
        int byteRate = 16; // WAV format only supports 16-bit audio clips in this utility
        int blockAlign = 2; // 2 bytes per sample for 16-bit audio
        int dataChunkSize = sampleCount * blockAlign;
        int fileSize = 36 + dataChunkSize; // File size excluding header

        using (MemoryStream ms = new MemoryStream())
        {
            using (BinaryWriter writer = new BinaryWriter(ms))
            {
                writer.Write(Encoding.ASCII.GetBytes("RIFF"));
                writer.Write(fileSize);
                writer.Write(Encoding.ASCII.GetBytes("WAVE"));

                writer.Write(Encoding.ASCII.GetBytes("fmt "));
                writer.Write(16); // Subchunk1Size for PCM
                writer.Write((UInt16)1); // AudioFormat for PCM
                writer.Write((UInt16)audioClip.channels); // Number of Channels
                writer.Write(audioClip.frequency); // Sample Rate
                writer.Write(audioClip.frequency * byteRate); // Byte Rate
                writer.Write((UInt16)blockAlign); // Block Align
                writer.Write((UInt16)byteRate); // Bits per Sample

                writer.Write(Encoding.ASCII.GetBytes("data"));
                writer.Write(dataChunkSize);

                foreach (var sample in audioData)
                {
                    short sample16Bit = (short)(sample * short.MaxValue);
                    writer.Write(sample16Bit);
                }

                writer.Flush();
                return ms.ToArray();
            }
        }
    }

    private static string FormatCode(ushort audioFormat)
    {
        switch (audioFormat)
        {
            case 1:
                return "PCM";
            case 65534:
                return "WaveFormatExtensible";
            default:
                return "Unknown";
        }
    }
}
