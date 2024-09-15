using System.IO;
using UnityEngine;

public class AudioSaver
{
    private string directoryPath;

    public AudioSaver(string directoryPath)
    {
        this.directoryPath = directoryPath;
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
    }

    public void SaveAudio(string fileName, AudioClip clip)
    {
        string filePath = Path.Combine(directoryPath, fileName);

        // Convert the AudioClip to WAV bytes
        string tempFilePath;
        byte[] wavBytes = WavUtility.FromAudioClip(clip, out tempFilePath, true, Path.GetDirectoryName(filePath));

        // Save the WAV bytes to the specified file path
        File.WriteAllBytes(filePath, wavBytes);

        Debug.Log("Recording saved as " + filePath);
    }
}
