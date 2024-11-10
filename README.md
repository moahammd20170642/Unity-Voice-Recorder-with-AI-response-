Audio Manager System
Overview
The Audio Manager system is designed for managing audio recordings, uploading to a server, fetching response audio, and coordinating lip-sync animations with character animations in Unity. This system allows seamless interaction by automating audio recording, playback, and synchronization with the character’s animations.

Key Features
Automatic Silence Detection: Stops recording automatically when the user stops speaking for a specified duration.
Audio Saving and Uploading: Saves recorded audio as a WAV file locally and uploads it to a server.
Fetching Response Audio: Fetches multiple audio files from the server based on a unique session identifier (SSID).
Queued Audio Playback: Sequential playback of fetched audio files, with the first two in WAV format and the rest in MP3.
Lip-Sync Synchronization: Syncs the character’s lip movements with the audio playback, while maintaining other body animations.
Stop Functionality: Allows stopping of the audio playback at any time, resets the system, and enables new audio recordings.
UI Language Selection: Provides UI elements to select the user’s preferred language for interaction.
System Components
1. AudioManager
The AudioManager class manages the recording, uploading, fetching, and playback of audio. It interfaces with the following components:

AudioRecorder
AudioSaver
AudioUploader
AudioFetcher
AudioPlayer
Additionally, it controls the character’s AnimatorController for animation handling.

2. AudioRecorder
The AudioRecorder class handles audio recording from the microphone, detects silence, and stops recording when no speech is detected for a given period. It also trims recorded audio.

3. AudioSaver
The AudioSaver class saves the recorded audio as a WAV file locally.

4. AudioUploader
The AudioUploader class uploads the saved WAV file to the server. It receives a session identifier (SSID) after the upload, which is used to fetch response audio.

5. AudioFetcher
The AudioFetcher class fetches response audio files from the server using the SSID. It queues them for sequential playback.

6. AudioPlayer
The AudioPlayer class plays the audio files from the queue. It distinguishes between WAV and MP3 formats and uses Unity's AudioSource for playback. Lip-sync animations are synchronized during playback.

7. AnimatorController
The AnimatorController class manages the character’s animation states. It switches between idle, listening, and a custom coffee animation, and ensures lip-syncing remains active during audio playback without affecting body animations.

Installation
Clone this repository:

bash
Copy code
git clone https://github.com/your-username/audio-manager-system.git
Import into Unity and attach the necessary components to your character and scene.

Configure the server URLs for audio uploading and fetching in the AudioUploader and AudioFetcher scripts.

Ensure your Unity project has the necessary assets for animations and lip-sync.

Usage
Trigger recording via a button in the UI.
Audio is automatically uploaded to the server after recording.
Response audio is fetched and played sequentially.
The character’s lips will sync with the audio during playback.
Contributing
Feel free to fork this project, create a branch for new features or fixes, and submit pull requests. Please ensure your code adheres to the existing style and includes tests where applicable.

