using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class AudioPlayer : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;

    public IEnumerator PlayQueuedAudio(Queue<string> audioFileQueue)
    {
        int index = 0;

        while (audioFileQueue.Count > 0)
        {
            string filePath = audioFileQueue.Dequeue();
            AudioType audioType = (index == 0||index ==1) ? AudioType.WAV : AudioType.MPEG;
            string fullPath = "file://" + filePath;

            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(fullPath, audioType))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                    audioSource.clip = clip;
                    audioSource.Play();
                    yield return new WaitForSeconds(clip.length);
                }
            }

            index++;
        }
    }
}
