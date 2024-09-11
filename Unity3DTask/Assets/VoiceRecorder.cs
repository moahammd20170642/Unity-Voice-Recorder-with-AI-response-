using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VoiceRecorder : MonoBehaviour
{
    public Button recordButton;
    public Button playButton;

    private AudioSource audioSource;
    private AudioClip recordedClip;
    private bool isRecording = false;
    private float silenceTimer = 0f;
    private const float silenceThreshold = 0.02f; // Threshold for detecting silence
    private const float silenceDuration = 3f; // Time in seconds of silence to stop recording
    private string microphoneDevice;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();

        // Assign functions to button clicks
        recordButton.onClick.AddListener(ToggleRecording);
        playButton.onClick.AddListener(PlayRecording);

        // Disable play button initially
        playButton.interactable = false;
    }

    void ToggleRecording()
    {
        if (!isRecording)
        {
            StartRecording();
        }
        else
        {
            StopRecording();
        }
    }

    void StartRecording()
    {
        if (Microphone.devices.Length > 0)
        {
            microphoneDevice = Microphone.devices[0];
            recordedClip = Microphone.Start(microphoneDevice, false, 10, 44100); // Max 10 seconds recording
            isRecording = true;
            silenceTimer = 0f;

            //recordButton.GetComponentInChildren<TMP_Text>().text = "Recording...";
            playButton.interactable = false;
        }
        else
        {
            Debug.LogError("No microphone detected!");
        }
    }

    void StopRecording()
    {
        if (isRecording)
        {
            Microphone.End(microphoneDevice);
            isRecording = false;

            //recordButton.GetComponentInChildren<TMP_Text>().text = "Start Recording";
            playButton.interactable = true;
        }
    }

    void PlayRecording()
    {
        if (recordedClip != null)
        {
            audioSource.clip = recordedClip;
            audioSource.Play();
        }
        else
        {
            Debug.LogError("No recording available to play!");
        }
    }

    void Update()
    {
        if (isRecording)
        {
            MonitorMicrophone();
        }
    }

    void MonitorMicrophone()
    {
        // Create an array to hold microphone data
        float[] microphoneData = new float[128];

        // Get the current position of the microphone recording
        int microphonePosition = Microphone.GetPosition(microphoneDevice);

        // Only proceed if the microphone has recorded enough data
        if (microphonePosition > microphoneData.Length)
        {
            // Get microphone data, ensuring valid range for GetData
            recordedClip.GetData(microphoneData, microphonePosition - microphoneData.Length);

            // Calculate the average sound level
            float averageLevel = 0f;
            for (int i = 0; i < microphoneData.Length; i++)
            {
                averageLevel += Mathf.Abs(microphoneData[i]);
            }
            averageLevel /= microphoneData.Length;

            // Check if the sound level is below the silence threshold
            if (averageLevel < silenceThreshold)
            {
                silenceTimer += Time.deltaTime;

                if (silenceTimer >= silenceDuration)
                {
                    StopRecording();
                    Debug.Log("Recording stopped due to silence.");
                }
            }
            else
            {
                // Reset the silence timer if sound is detected
                silenceTimer = 0f;
            }
        }
    }
}
