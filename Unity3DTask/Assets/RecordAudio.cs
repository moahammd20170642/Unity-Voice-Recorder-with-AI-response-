using System.Collections;
using System.Collections.Generic;
using System.IO;
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
            WavUtility.Save(filePath, recordedClip); // Assume WavUtility is a helper class for saving WAV files
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

    // Coroutine to upload the recorded file and then fetch the response audio files
    private IEnumerator UploadAndFetchAudio(string filePath)
    {
        // Upload the recorded audio file
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

            // Parse the JSON response to get the ssid
            UploadResponse uploadResponse = JsonUtility.FromJson<UploadResponse>(www.downloadHandler.text);
            ssid = uploadResponse.ssid;
            Debug.Log("File uploaded successfully, SSID: " + ssid);

            // Start fetching response audio files
            StartCoroutine(FetchResponseAudio());
        }
    }

    // Coroutine to fetch and play the response audio files
    private IEnumerator FetchResponseAudio()
    {
        int index = 0;
        int maxRetries = 10; // Maximum number of retries
        int retryCount = 0;
        bool processing = true;

        while (processing && retryCount < maxRetries)
        {
            // Directly use ssid as a plain string
            string fetchUrl = $"{serverUrl}/fetch?ssid={UnityWebRequest.EscapeURL(ssid)}&index={index}";
            Debug.Log("Fetching response audio with URL: " + fetchUrl);

            using (UnityWebRequest www = UnityWebRequest.Get(fetchUrl))
            {
                yield return www.SendWebRequest();

                // Handle connection or protocol errors
                if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError("Error fetching audio file: " + www.error);
                    retryCount++;
                    yield return new WaitForSeconds(5); // Wait 5 seconds before retrying
                    continue;
                }

                // If the server returns 404, wait and retry
                if (www.responseCode == 404)
                {
                    Debug.Log("No response available yet, retrying... (" + retryCount + "/" + maxRetries + ")");
                    retryCount++;
                    yield return new WaitForSeconds(5); // Wait 5 seconds before retrying
                    continue;
                }

                // If we successfully get a response, save and play the audio
                byte[] audioData = www.downloadHandler.data;
                string responseFilePath = Path.Combine(directoryPath, "response_" + index + ".wav");
                File.WriteAllBytes(responseFilePath, audioData);

                Debug.Log("Response file saved: " + responseFilePath);

                // Load and play the audio file
                StartCoroutine(PlayAudioFile(responseFilePath));

                // Reset retry count and move to the next index
                retryCount = 0;
                index++;
                yield return new WaitForSeconds(3); // Wait 3 seconds before fetching the next file
            }
        }

        if (retryCount >= maxRetries)
        {
            Debug.LogError("Max retries reached. No more response files available.");
        }
        else
        {
            Debug.Log("Finished fetching response files.");
        }
    }

    // Coroutine to play an audio file
    private IEnumerator PlayAudioFile(string filePath)
    {
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + filePath, AudioType.WAV))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error playing audio file: " + www.error);
            }
            else
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                audioSource.clip = clip;
                audioSource.Play();

                Debug.Log("Playing response audio...");
            }
        }
    }
}
