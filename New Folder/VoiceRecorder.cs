using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class VoiceRecorder : MonoBehaviour
{
    private AudioSource audioSource;
    private string serverUrl = "https://xfojojrxiv9w43-5000.proxy.runpod.net";
    private string ssid;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // Start recording from the microphone and reset session if needed
    public void StartRecording()
    {
        // Reset session before starting a new recording
        if (!string.IsNullOrEmpty(ssid))
        {
            Debug.Log("Resetting existing session: " + ssid);
            ResetSession();
        }

        AudioClip clip = Microphone.Start(null, false, 10, 44100);
        StartCoroutine(StopRecordingAfterSilence(clip));
    }

    // Stop recording after silence or timeout and trigger the upload
    private IEnumerator StopRecordingAfterSilence(AudioClip clip)
    {
        yield return new WaitForSeconds(10); // Adjust this duration as needed
        Microphone.End(null);

        byte[] audioData = WavUtility.FromAudioClip(clip);
        StartCoroutine(UploadAudioClip(audioData));
    }

    // Upload the recorded audio clip to the server
    private IEnumerator UploadAudioClip(byte[] audioData)
    {
        WWWForm form = new WWWForm();
        form.AddBinaryData("audio", audioData, "audio.wav", "audio/wav");

        using (UnityWebRequest www = UnityWebRequest.Post(serverUrl + "/upload", form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                ssid = www.downloadHandler.text.Trim(); // Trim any extra whitespace
                Debug.Log("Upload success. SSID: " + ssid);
                StartCoroutine(FetchAudioFiles());
            }
            else
            {
                Debug.LogError("Upload failed: " + www.error);
            }
        }
    }

    // Fetch audio files from the server using the ssid and play them
    private IEnumerator FetchAudioFiles()
    {
        int index = 0;

        while (true)
        {
            string url = $"{serverUrl}/fetch?ssid={ssid}&index={index}";
            Debug.Log("Fetching from URL: " + url);

            using (UnityWebRequest www = UnityWebRequest.Get(url))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    byte[] audioData = www.downloadHandler.data;

                    if (audioData == null || audioData.Length == 0)
                    {
                        Debug.Log("No more files available.");
                        break;
                    }

                    AudioClip clip = WavUtility.ToAudioClip(audioData);
                    if (clip != null)
                    {
                        Debug.Log("Playing server response.");
                        audioSource.PlayOneShot(clip);
                    }
                    else
                    {
                        Debug.LogError("Failed to convert audio data to AudioClip.");
                    }

                    index++;
                }
                else if (www.responseCode == 404)
                {
                    Debug.Log("No more files available (404).");
                    break;
                }
                else
                {
                    Debug.LogError("Error fetching audio file: " + www.error);
                    break;
                }
            }

            yield return new WaitForSeconds(1); // Delay between fetch requests if necessary
        }
    }

    // Reset the current session if it exists
    private void ResetSession()
    {
        ssid = null;
        Debug.Log("Session has been reset.");
    }

    // Call this function when stopping or closing the app
    public void CloseSession()
    {
        if (!string.IsNullOrEmpty(ssid))
        {
            Debug.Log("Closing session: " + ssid);
            StartCoroutine(CloseSessionRequest());
        }
    }

    // Send a request to the server to close the session
    private IEnumerator CloseSessionRequest()
    {
        string url = $"{serverUrl}/close?ssid={ssid}";

        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Session closed successfully.");
                ssid = null;
            }
            else
            {
                Debug.LogError("Failed to close session: " + www.error);
            }
        }
    }

    void OnApplicationQuit()
    {
        // Ensure the session is closed when the app is quit
        CloseSession();
    }
}
