
# Unity Voice Recorder with AI Response

This Unity project demonstrates how to record voice, send it to an AI API for processing, and receive a response to be played back to the user. The system also handles audio playback and interaction with a Unity Animator for controlling animations during recording and response playback.

## Features
- **Voice Recording:** Capture audio from the user's microphone and save it as a WAV file.
- **AI Response:** Send the recorded audio to an AI service for processing and get a text or audio response.
- **Audio Playback:** Play back both the AI-generated response and the original audio using Unity's `AudioSource`.
- **Lip-Sync Integration:** Optionally integrates with Unity's OVR Lip Sync for facial animations during speech.
- **Animation Control:** Trigger animations during voice recording, AI response playback, and user interaction.
- **Silence Detection:** Automatically stop recording when silence is detected, improving user experience.

## Requirements
- Unity 2020.3 or later.
- Any microphone input for voice recording.
- An AI service or API to handle the speech processing (e.g., OpenAI, Google Speech API, etc.).

## Setup

1. Clone the repository:

   ```bash
   git clone https://github.com/moahammd20170642/Unity-Voice-Recorder-with-AI-response
   ```

2. Open the project in Unity.

3. Set up the necessary AI API in the script where the recording is sent for processing. You can use services like OpenAI or Google Cloud Speech-to-Text.

4. Attach the required scripts to your Unity objects (such as the `AudioSource`, `Animator`, and `LipSync` components).

5. Customize the Animator Controller to include animations like "Idle," "Listen," and "Response" based on your use case.

6. Configure the microphone input and recording button in the Unity UI.

## Usage

- **Record:** Press the "Record" button to start recording the user's voice.
- **Stop:** Press the "Stop" button to stop recording and send the audio for processing.
- **Play Response:** Once the AI response is received, it will automatically be played back to the user.
- **Lip Sync:** If lip sync is enabled, the character will perform facial animations during the AI response playback.

## Example Code Snippets

### Voice Recording

```csharp
void StartRecording()
{
    // Start recording logic here
}

void StopRecording()
{
    // Stop recording and process the audio
}
```

### AI Response Handling

```csharp
void ProcessAudioAndGetResponse(AudioClip recordedClip)
{
    // Send audio to AI API and handle the response
}
```

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- [Unity](https://unity.com/) for the game development engine.
- [OpenAI](https://openai.com/) for the AI language model.
- [Google Cloud Speech-to-Text](https://cloud.google.com/speech-to-text) for speech recognition services.
