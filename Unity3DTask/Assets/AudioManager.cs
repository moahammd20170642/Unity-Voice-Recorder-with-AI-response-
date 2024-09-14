using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private AudioRecorder audioRecorder;
    private AudioSaver audioSaver;
    private AudioUploader audioUploader;
    private AudioFetcher audioFetcher;
 

    [SerializeField] private AudioPlayer audioPlayer;

    [SerializeField] private AnimatorController animatorController;

    private string directoryPath = "Recordings";
    private string serverUrl = "https://xfojojrxiv9w43-5000.proxy.runpod.net";
    private string filePath = "recording.wav";
    private string ssid = null;

    private void Start()
    {
       
    }
    private void Awake()
    {
        Debug.Log("Initializing AudioManager...");

        audioRecorder = new AudioRecorder();
        audioSaver = new AudioSaver(directoryPath);
        audioUploader = new AudioUploader(serverUrl);
        audioFetcher = new AudioFetcher(serverUrl);

        Debug.Log("AudioManager initialized successfully.");
    }

    private void Update()
    {
        // Constantly update the recording process to detect silence.
        audioRecorder.UpdateRecording();
    }

    public void StartRecording()
    {
        Debug.Log("Starting recording...");
        animatorController.StartListening();
        // Start recording and set up silence detection.
        audioRecorder.StartRecording();
        audioRecorder.OnSilenceExceeded += StopRecording;

         Debug.Log("Recording started and waiting for silence detection.");
    }

    public void StopRecording()
    {
        animatorController.SetIdle();
        Debug.Log("Stopping recording due to silence or user command...");

        // Stop recording and get the audio clip.
        AudioClip clip = audioRecorder.StopRecording();
        if (clip != null)
        {
            Debug.Log("Recording stopped. Saving audio...");
            audioSaver.SaveAudio(filePath, clip);
            StartCoroutine(audioUploader.UploadAudio(Path.Combine(directoryPath, filePath), OnUploadSuccess, OnUploadError));
        }
        else
        {
            Debug.LogError("Recording failed or no audio to save.");
        }
    }

    private void OnUploadSuccess(string ssid)
    {
        Debug.Log("Audio uploaded successfully. Received SSID: " + ssid);

        this.ssid = ssid;
        Debug.Log("Fetching response audio from server...");
        StartCoroutine(audioFetcher.FetchAudioFiles(ssid, OnFetchComplete));
    }

    private void OnFetchComplete()
    {
        Debug.Log("All response audio files fetched successfully.");
        Queue<string> queuedFiles = audioFetcher.GetQueuedFiles();

        if (queuedFiles.Count > 0)
        {
            Debug.Log($"Playing {queuedFiles.Count} fetched audio file(s)...");
            animatorController.gameObject.GetComponent<Animator>().enabled = false;
            StartCoroutine(audioPlayer.PlayQueuedAudio(queuedFiles));
        }
        else
        {
            Debug.LogWarning("No audio files found to play.");
        }
    }

    private void OnUploadError(string error)
    {
        Debug.LogError("Upload failed with error: " + error);
    }
}
