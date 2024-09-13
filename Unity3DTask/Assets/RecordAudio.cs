using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using UnityEngine.Networking;

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
    private const float silenceThreshold = 0.08f;
    private const float maxSilenceTime = 3f;
    private const int sampleWindow = 1024;
    private Queue<string> audioFileQueue = new Queue<string>();

    private string serverUrl = "https://xfojojrxiv9w43-5000.proxy.runpod.net";
    private string ssid = null;

    [System.Serializable]
    public class UploadResponse
    {
        public string ssid;
    }

    private void Awake()
    {
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
    }

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

    private void Update()
    {
        if (isRecording)
        {
            CheckSilence();
        }
    }

    private void CheckSilence()
    {
        int micPosition = Microphone.GetPosition(null);
        if (micPosition < sampleWindow)
        {
            Debug.Log("Not enough data to analyze silence yet.");
            return;
        }

        float[] clipData = new float[sampleWindow];
        int sampleOffset = micPosition - sampleWindow;
        if (sampleOffset < 0)
        {
            sampleOffset = recordedClip.samples + sampleOffset;
        }

        recordedClip.GetData(clipData, sampleOffset);

        float maxLevel = 0f;
        foreach (float sample in clipData)
        {
            if (Mathf.Abs(sample) > maxLevel)
            {
                maxLevel = Mathf.Abs(sample);
            }
        }

        Debug.Log("Max audio level detected: " + maxLevel);

        if (maxLevel < silenceThreshold)
        {
            silenceDuration += Time.deltaTime;
            Debug.Log("Silence detected for " + silenceDuration + " seconds");
        }
        else
        {
            silenceDuration = 0f;
            Debug.Log("Sound detected, resetting silence timer.");
        }

        if (silenceDuration >= maxSilenceTime)
        {
            Debug.Log("Silence exceeded 3 seconds. Stopping recording...");
            StopRecording();
        }
    }

    public void StopRecording()
    {
        if (!isRecording) return;

        Microphone.End(null);
        isRecording = false;
        recordingLength = Time.realtimeSinceStartup - startTime;
        recordedClip = TrimClip(recordedClip, recordingLength);
        SaveRecording();
        Debug.Log("Recording stopped.");

        StartCoroutine(UploadAndFetchAudio(filePath));
    }

    public void SaveRecording()
    {
        if (recordedClip != null)
        {
            filePath = Path.Combine(directoryPath, filePath);
            WavUtility.Save(filePath, recordedClip);
            Debug.Log("Recording saved as " + filePath);
        }
        else
        {
            Debug.LogError("No recording found to save.");
        }
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

    private IEnumerator UploadAndFetchAudio(string filePath)
    {
        Debug.Log("Uploading the recorded file...");

        byte[] audioBytes = File.ReadAllBytes(filePath);
        WWWForm form = new WWWForm();
        form.AddBinaryData("audio", audioBytes, "recording.wav", "audio/wav");

        using (UnityWebRequest www = UnityWebRequest.Post(serverUrl + "/upload", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error uploading file: " + www.error);
                yield break;
            }

            UploadResponse uploadResponse = JsonUtility.FromJson<UploadResponse>(www.downloadHandler.text);
            ssid = uploadResponse.ssid;
            Debug.Log("File uploaded successfully, SSID: " + ssid);

            StartCoroutine(FetchResponseAudio());
        }
    }

    private IEnumerator FetchResponseAudio()
    {
        int index = 0;
        bool processing = true;

        while (processing)
        {
            string fetchUrl = $"{serverUrl}/fetch?ssid={UnityWebRequest.EscapeURL(ssid)}&index={index}";
            Debug.Log("Fetching response audio with URL: " + fetchUrl);

            using (UnityWebRequest www = UnityWebRequest.Get(fetchUrl))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.Log("Error fetching audio file: " + www.error);
                    processing = false;
                    break;
                }

                if (www.responseCode == 404)
                {
                    Debug.Log("No more files available, starting to play audio...");
                    processing = false;
                    break;
                }

                byte[] audioData = www.downloadHandler.data;

                // Save the file to StreamingAssets folder
                string responseFilePath = Path.Combine(Application.streamingAssetsPath, "response_" + index + ".wav");
                File.WriteAllBytes(responseFilePath, audioData);

                audioFileQueue.Enqueue(responseFilePath);

                Debug.Log("Response file saved: " + responseFilePath);

                index++;
            }
        }

        if (audioFileQueue.Count > 0)
        {
            PlayAllFetchedAudio();
        }
        else
        {
            Debug.LogWarning("No audio files found to play.");
        }
    }

    private void PlayAllFetchedAudio()
    {
        StartCoroutine(PlayQueuedAudio());
    }

    private IEnumerator PlayQueuedAudio()
    {
        while (audioFileQueue.Count > 0)
        {
            string filePath = audioFileQueue.Dequeue();
            Debug.Log("Attempting to load audio from: " + filePath);

            // Play from StreamingAssets
            string fullPath = "file://" + filePath;  // Add file:// prefix for UnityWebRequest
            Debug.Log("Full path: " + fullPath);

            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(fullPath, AudioType.WAV))
            {
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.Log("Error loading audio file: " + www.error);
                }
                else
                {
                    AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                    if (clip == null)
                    {
                        Debug.LogError("Error: AudioClip is null for file: " + filePath);
                    }
                    else
                    {
                        audioSource.clip = clip;
                        Debug.Log("AudioClip assigned. Length: " + clip.length);

                        audioSource.Play();
                        Debug.Log("Playing audio...");

                        yield return new WaitForSeconds(clip.length);
                    }
                }
            }
        }
    }
}
