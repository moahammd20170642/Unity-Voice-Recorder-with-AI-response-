using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class RecordAudio : MonoBehaviour
{
    private AudioClip recordedClip;
    [SerializeField] private AudioSource audioSource;
    private string filePath = "recording.wav";
    private string directoryPath = "Recordings";
    private float startTime;
    private float recordingLength;
    private bool isRecording = false;
    private float silenceDuration = 0f;
    private const float silenceThreshold = 0.02f; // Adjust based on microphone sensitivity
    private const float maxSilenceTime = 3f; // Stop after 3 seconds of silence
    private const int sampleWindow = 1024; // Size of the sample to analyze for silence

    private void Awake()
    {
        // Create directory for recordings if it doesn't exist
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
    }

    // Start recording audio
    public void StartRecording()
    {
        string device = Microphone.devices[0]; // Get the default microphone
        int sampleRate = 44100;
        int lengthSec = 3599; // Max recording length

        recordedClip = Microphone.Start(device, false, lengthSec, sampleRate);
        startTime = Time.realtimeSinceStartup;
        isRecording = true;
        silenceDuration = 0f;

        Debug.Log("Recording started...");
    }

    // Called once per frame
    private void Update()
    {
        if (isRecording)
        {
            CheckSilence();
        }
    }

    // Check if the user is silent, and stop recording if silence exceeds 3 seconds
    private void CheckSilence()
    {
        int micPosition = Microphone.GetPosition(null); // Get the current position in the recording
        if (micPosition < sampleWindow)
        {
            // Not enough data to analyze yet
            Debug.Log("Not enough data to analyze silence yet.");
            return;
        }

        float[] clipData = new float[sampleWindow]; // Create a buffer to store audio samples
        int sampleOffset = micPosition - sampleWindow;
        if (sampleOffset < 0)
        {
            // Wrap the sample offset around the length of the recording
            sampleOffset = recordedClip.samples + sampleOffset;
        }

        // Get audio data from the recorded clip
        recordedClip.GetData(clipData, sampleOffset);

        // Find the maximum audio level in the sample
        float maxLevel = 0f;
        foreach (float sample in clipData)
        {
            if (Mathf.Abs(sample) > maxLevel)
            {
                maxLevel = Mathf.Abs(sample);
            }
        }

        Debug.Log("Max audio level detected: " + maxLevel);

        // If the max audio level is below the silence threshold, increase the silence duration
        if (maxLevel < silenceThreshold)
        {
            silenceDuration += Time.deltaTime;
            Debug.Log("Silence detected for " + silenceDuration + " seconds");
        }
        else
        {
            // Reset silence duration if sound is detected
            silenceDuration = 0f;
            Debug.Log("Sound detected, resetting silence timer.");
        }

        // Stop recording if silence has been detected for longer than 3 seconds
        if (silenceDuration >= maxSilenceTime)
        {
            Debug.Log("Silence exceeded 3 seconds. Stopping recording...");
            StopRecording();
        }
    }

    // Stop recording audio
    public void StopRecording()
    {
        if (!isRecording) return;

        Microphone.End(null); // Stop the microphone
        isRecording = false;
        recordingLength = Time.realtimeSinceStartup - startTime;
        recordedClip = TrimClip(recordedClip, recordingLength); // Trim the clip to the actual recording length
        SaveRecording();

        Debug.Log("Recording stopped.");
    }

    // Save the recorded audio clip to a file
    public void SaveRecording()
    {
        if (recordedClip != null)
        {
            filePath = Path.Combine(directoryPath, filePath); // Create full file path
            WavUtility.Save(filePath, recordedClip); // Save as WAV using WavUtility (assumed external class)
            Debug.Log("Recording saved as " + filePath);
        }
        else
        {
            Debug.LogError("No recording found to save.");
        }
    }

    // Trim the recorded audio clip to the actual recording length
    private AudioClip TrimClip(AudioClip clip, float length)
    {
        int samples = (int)(clip.frequency * length); // Calculate the number of samples based on length
        float[] data = new float[samples];
        clip.GetData(data, 0); // Get the recorded audio data

        // Create a new clip with the trimmed length
        AudioClip trimmedClip = AudioClip.Create(clip.name, samples, clip.channels, clip.frequency, false);
        trimmedClip.SetData(data, 0); // Set the data to the trimmed clip

        return trimmedClip;
    }
}
