using System;
using UnityEngine;

public class AudioRecorder
{
    private AudioClip recordedClip;
    private float startTime;
    private float silenceDuration;
    private bool isRecording = false;

    private const float silenceThreshold = 0.06f;
    private const float maxSilenceTime = 3f;
    private const int sampleWindow = 1024;

    public event Action OnSilenceExceeded;

    public void StartRecording()
    {
        string device = Microphone.devices[0];
        int sampleRate = 44100;
        int lengthSec = 3599;

        recordedClip = Microphone.Start(device, false, lengthSec, sampleRate);
        startTime = Time.realtimeSinceStartup;
        isRecording = true;
        silenceDuration = 0f;

        Debug.Log("Recording started...");
    }

    public void UpdateRecording()
    {
        if (isRecording)
        {
            CheckSilence();
        }
    }

    private void CheckSilence()
    {
        int micPosition = Microphone.GetPosition(null);
        if (micPosition < sampleWindow) return;

        float[] clipData = new float[sampleWindow];
        int sampleOffset = micPosition - sampleWindow;
        if (sampleOffset < 0) sampleOffset = recordedClip.samples + sampleOffset;

        recordedClip.GetData(clipData, sampleOffset);

        float maxLevel = 0f;
        foreach (float sample in clipData)
        {
            maxLevel = Mathf.Max(maxLevel, Mathf.Abs(sample));
        }

        if (maxLevel < silenceThreshold)
        {
            silenceDuration += Time.deltaTime;
            if (silenceDuration >= maxSilenceTime)
            {
                OnSilenceExceeded?.Invoke();
            }
        }
        else
        {
            silenceDuration = 0f;
        }
    }

    public AudioClip StopRecording()
    {
        if (!isRecording) return null;

        Microphone.End(null);
        isRecording = false;

        float recordingLength = Time.realtimeSinceStartup - startTime;
        return TrimClip(recordedClip, recordingLength);
    }

    private AudioClip TrimClip(AudioClip clip, float length)
    {
        int samples = (int)(clip.frequency * length);
        float[] data = new float[samples];
        clip.GetData(data, 0);

        AudioClip trimmedClip = AudioClip.Create(clip.name, samples, clip.channels, clip.frequency, false);
        trimmedClip.SetData(data, 0);

        return trimmedClip;
    }
}
