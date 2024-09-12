//using System.Collections;
//using System.IO;
//using UnityEngine;
//using UnityEngine.Networking;
//using UnityEngine.UI;
//using TMPro;

//public class VoiceRecorder : MonoBehaviour
//{
//    public Button recordButton;
//    public TextMeshProUGUI statusText;

//    private AudioSource audioSource;
//    private AudioClip recordedClip;
//    private bool isRecording = false;
//    private string microphoneDevice;
//    private const float silenceThreshold = 0.02f;
//    private const float silenceDuration = 3f;
//    private float silenceTimer = 0f;

//    private string serverUrl = "https://xfojojrxiv9w43-5000.proxy.runpod.net";
//    private const int timeoutDuration = 10;

//    void Start()
//    {
//        audioSource = gameObject.AddComponent<AudioSource>();
//        recordButton.onClick.AddListener(ToggleRecording);
//    }

//    void ToggleRecording()
//    {
//        if (!isRecording)
//        {
//            StartRecording();
//        }
//        else
//        {
//            StopRecording();
//        }
//    }

//    void StartRecording()
//    {
//        if (Microphone.devices.Length > 0)
//        {
//            microphoneDevice = Microphone.devices[0];
//            recordedClip = Microphone.Start(microphoneDevice, false, 10, 44100);
//            isRecording = true;
//            silenceTimer = 0f;
//            statusText.text = "Recording...";
//        }
//        else
//        {
//            Debug.LogError("No microphone detected!");
//            statusText.text = "Error: No microphone detected!";
//        }
//    }

//    void StopRecording()
//    {
//        if (isRecording)
//        {
//            Microphone.End(microphoneDevice);
//            isRecording = false;
//            statusText.text = "Processing...";

//            // Start the upload coroutine
//            StartCoroutine(UploadAudioClip(recordedClip));
//        }
//    }

//    IEnumerator UploadAudioClip(AudioClip audioClip)
//    {
//        if (audioClip == null)
//        {
//            Debug.LogError("AudioClip is null, cannot upload.");
//            statusText.text = "Error: No audio clip to upload.";
//            yield break;
//        }

//        // Convert AudioClip to WAV byte array
//        byte[] audioData = WavUtility.FromAudioClip(audioClip);
//        if (audioData == null || audioData.Length == 0)
//        {
//            Debug.LogError("Failed to convert AudioClip to WAV.");
//            statusText.text = "Error: Failed to process audio.";
//            yield break;
//        }

//        // Create a UnityWebRequest for the upload
//        WWWForm form = new WWWForm();
//        form.AddBinaryData("audio", audioData, "recordedAudio.wav", "audio/wav");

//        UnityWebRequest request = UnityWebRequest.Post($"{serverUrl}/upload", form);
//        request.timeout = timeoutDuration;

//        yield return request.SendWebRequest();

//        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
//        {
//            Debug.LogError("Upload failed: " + request.error);
//            statusText.text = "Error: Upload failed.";
//        }
//        else if (request.result == UnityWebRequest.Result.Success)
//        {
//            Debug.Log("Upload successful! Response: " + request.downloadHandler.text);
//            string ssid = ExtractSsidFromResponse(request.downloadHandler.text);
//            if (!string.IsNullOrEmpty(ssid))
//            {
//                StartCoroutine(FetchAudioFiles(ssid));
//            }
//            else
//            {
//                Debug.LogError("Failed to retrieve session ID.");
//                statusText.text = "Error: Failed to retrieve session.";
//            }
//        }
//    }

//    IEnumerator FetchAudioFiles(string ssid)
//    {
//        int index = 0;
//        while (true)
//        {
//            string fetchUrl = $"{serverUrl}/fetch?ssid={ssid}&index={index}";

//            UnityWebRequest request = UnityWebRequest.Get(fetchUrl);
//            request.timeout = timeoutDuration;

//            yield return request.SendWebRequest();

//            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
//            {
//                Debug.LogError("Fetch failed: " + request.error);
//                statusText.text = "Error: Fetch failed.";
//                break;
//            }
//            else if (request.result == UnityWebRequest.Result.Success)
//            {
//                if (request.responseCode == 404) { Debug.Log("No more files available for SSID: " + ssid); statusText.text = "All audio files received."; break; }
//                // Save the audio file to disk (optional)
//                string fileName = $"audio_{index}.wav";
//                SaveAudioToFile(request.downloadHandler.data, fileName);

//                // Play the fetched audio
//                AudioClip fetchedClip = WavUtility.ToAudioClip(request.downloadHandler.data);
//                PlayFetchedAudio(fetchedClip);

//                index++;
//                yield return new WaitForSeconds(1); // Delay between requests
//            }
//        }
//    }

//    void SaveAudioToFile(byte[] audioData, string fileName)
//    {
//        string filePath = Path.Combine(Application.persistentDataPath, fileName);
//        File.WriteAllBytes(filePath, audioData);
//        Debug.Log($"Saved audio file to: {filePath}");
//    }

//    void PlayFetchedAudio(AudioClip clip)
//    {
//        if (clip != null)
//        {
//            audioSource.clip = clip;
//            audioSource.Play();
//            statusText.text = "Playing fetched audio...";
//        }
//        else
//        {
//            Debug.LogError("No audio clip available to play!");
//            statusText.text = "Error: No audio to play.";
//        }
//    }

//    // Helper function to extract ssid from the JSON response
//    string ExtractSsidFromResponse(string jsonResponse)
//    {
//        var response = JsonUtility.FromJson<UploadResponse>(jsonResponse);
//        return response.ssid;
//    }

//    [System.Serializable]
//    public class UploadResponse
//    {
//        public string ssid;
//    }

//    void Update()
//    {
//        if (isRecording)
//        {
//            MonitorMicrophone();
//        }
//    }

//    void MonitorMicrophone()
//    {
//        float[] microphoneData = new float[128];
//        int microphonePosition = Microphone.GetPosition(microphoneDevice);

//        if (microphonePosition > microphoneData.Length)
//        {
//            recordedClip.GetData(microphoneData, microphonePosition - microphoneData.Length);
//            float averageLevel = 0f;

//            foreach (float sample in microphoneData)
//            {
//                averageLevel += Mathf.Abs(sample);
//            }
//            averageLevel /= microphoneData.Length;

//            if (averageLevel < silenceThreshold)
//            {
//                silenceTimer += Time.deltaTime;

//                if (silenceTimer >= silenceDuration)
//                {
//                    StopRecording();
//                    Debug.Log("Recording stopped due to silence.");
//                }
//            }
//            else
//            {
//                silenceTimer = 0f;
//            }
//        }
//    }
//}
