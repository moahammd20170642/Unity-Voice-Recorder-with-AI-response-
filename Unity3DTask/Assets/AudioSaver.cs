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
        WavUtility.Save(filePath, clip);
        Debug.Log("Recording saved as " + filePath);
    }
}
