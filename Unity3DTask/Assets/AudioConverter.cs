using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System;
using System.Diagnostics;


public class AudioConverter
{
    private string ffmpegPath = @"path/to/ffmpeg"; // Path to FFmpeg executable

    public void ConvertToWav(string inputFilePath, string outputFilePath)
    {
        var arguments = $"-i \"{inputFilePath}\" -ar 44100 -ac 2 -f wav \"{outputFilePath}\"";

        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = ffmpegPath,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (Process process = Process.Start(startInfo))
        {
            using (StreamReader errorReader = process.StandardError)
            {
                string error = errorReader.ReadToEnd();
                if (!string.IsNullOrEmpty(error))
                {
                    //Debug.LogError("FFmpeg error: " + error);
                }
            }

            process.WaitForExit();
        }

        //Debug.Log("Conversion complete: " + outputFilePath);
    }
}