using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class AudioUploader
{
    private string serverUrl;

    public AudioUploader(string serverUrl)
    {
        this.serverUrl = serverUrl;
    }

    public IEnumerator UploadAudio(string filePath, Action<string> onSuccess, Action<string> onError)
    {
        byte[] audioBytes = File.ReadAllBytes(filePath);
        WWWForm form = new WWWForm();
        form.AddBinaryData("audio", audioBytes, "recording.wav", "audio/wav");

        using (UnityWebRequest www = UnityWebRequest.Post(serverUrl + "/upload", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke(www.error);
            }
            else
            {
                UploadResponse response = JsonUtility.FromJson<UploadResponse>(www.downloadHandler.text);
                onSuccess?.Invoke(response.ssid);
            }
        }
    }

    [Serializable]
    public class UploadResponse
    {
        public string ssid;
    }
}
