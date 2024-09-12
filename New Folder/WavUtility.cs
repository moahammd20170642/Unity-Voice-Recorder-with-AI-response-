using System;
using System.IO;
using UnityEngine;

public static class WavUtility
{
    public static byte[] FromAudioClip(AudioClip clip)
    {
        var samples = new float[clip.samples * clip.channels];
        clip.GetData(samples, 0);

        var bytesPerSample = 2; // 16-bit audio
        var fileSize = 44 + samples.Length * bytesPerSample;
        var buffer = new byte[fileSize];

        using (var stream = new MemoryStream(buffer))
        using (var writer = new BinaryWriter(stream))
        {
            WriteWavHeader(writer, clip, fileSize);
            foreach (var sample in samples)
            {
                short sampleData = (short)(sample * short.MaxValue);
                writer.Write(sampleData);
            }
        }

        return buffer;
    }

    private static void WriteWavHeader(BinaryWriter writer, AudioClip clip, int fileSize)
    {
        writer.Write(new[] { 'R', 'I', 'F', 'F' });
        writer.Write(fileSize - 8);
        writer.Write(new[] { 'W', 'A', 'V', 'E' });
        writer.Write(new[] { 'f', 'm', 't', ' ' });
        writer.Write(16); // Subchunk1Size
        writer.Write((short)1); // AudioFormat (1 = PCM)
        writer.Write((short)clip.channels);
        writer.Write(clip.frequency);
        writer.Write(clip.frequency * clip.channels * 2); // ByteRate
        writer.Write((short)(clip.channels * 2)); // BlockAlign
        writer.Write((short)16); // BitsPerSample
        writer.Write(new[] { 'd', 'a', 't', 'a' });
        writer.Write(fileSize - 44);
    }

    public static AudioClip ToAudioClip(byte[] data)
    {
        if (data == null || data.Length == 0)
        {
            Debug.LogError("Invalid audio data: The byte array is empty.");
            return null;
        }

        using (var stream = new MemoryStream(data))
        using (var reader = new BinaryReader(stream))
        {
            try
            {
                // Skip to channels and sample rate
                reader.BaseStream.Seek(22, SeekOrigin.Begin);
                var channels = reader.ReadInt16();
                var sampleRate = reader.ReadInt32();

                // Skip to bits per sample
                reader.BaseStream.Seek(34, SeekOrigin.Begin);
                var bitsPerSample = reader.ReadInt16();

                // Validate that we have 16-bit audio data
                if (bitsPerSample != 16)
                {
                    Debug.LogError($"Unsupported bits per sample: {bitsPerSample}. Only 16-bit audio is supported.");
                    return null;
                }

                // Skip to the data section
                reader.BaseStream.Seek(44, SeekOrigin.Begin);
                var sampleCount = (data.Length - 44) / (bitsPerSample / 8);

                if (sampleCount <= 0)
                {
                    Debug.LogError($"Invalid sample count: {sampleCount}");
                    return null;
                }

                float[] samples = new float[sampleCount];

                for (int i = 0; i < sampleCount; i++)
                {
                    short sampleData = reader.ReadInt16();
                    samples[i] = sampleData / (float)short.MaxValue;
                }

                var clip = AudioClip.Create("clip", sampleCount, channels, sampleRate, false);
                clip.SetData(samples, 0);
                return clip;
            }
            catch (Exception ex)
            {
                Debug.LogError("Error reading WAV file: " + ex.Message);
                return null;
            }
        }
    }
}
