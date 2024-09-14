using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class AudioFetcher
{
    private string serverUrl;
    private Queue<string> audioFileQueue = new Queue<string>();

    public AudioFetcher(string serverUrl)
    {
        this.serverUrl = serverUrl;
    }

    public IEnumerator FetchAudioFiles(string ssid, Action onFinished)
    {
        int index = 0;
        bool processing = true;

        while (processing)
        {
            string fetchUrl = $"{serverUrl}/fetch?ssid={UnityWebRequest.EscapeURL(ssid)}&index={index}";

            using (UnityWebRequest www = UnityWebRequest.Get(fetchUrl))
            {
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    processing = false;
                    break;
                }

                if (www.responseCode == 404)
                {
                    processing = false;
                    break;
                }

                byte[] audioData = www.downloadHandler.data;
                string responseFilePath = Path.Combine(Application.streamingAssetsPath, "response_" + index + ".wav");
                File.WriteAllBytes(responseFilePath, audioData);
                audioFileQueue.Enqueue(responseFilePath);

                index++;
            }
        }

        onFinished?.Invoke();
    }

    public Queue<string> GetQueuedFiles()
    {
        return audioFileQueue;
    }
}
