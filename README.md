Audio Manager System Overview
Introduction

This document provides a detailed overview of the Audio Manager system, which is used to
record audio, detect silence, upload the recorded audio to a server, fetch response audio,
play it back, and synchronize lip movements with audio .
Key Features
• Automatic Silence Detection: Stops recording automatically when the user stops
speaking for a specified duration.
• Audio Saving and Uploading: The recorded audio is saved locally as a WAV file and then
uploaded to a server.
• Fetching Response Audio: The system fetches multiple response audio files from the
server based on the unique session identifier (ssid). These responses are downloaded,
queued, and played sequentially.
• Queued Audio Playback: The fetched audio responses are played back one after the
other, with the first two files in WAV format and the rest in MP3 format.
• Lip-Sync and Animation Coordination: While the audio responses are played, the
character’s lips move in sync with the audio. The character&#39;s body animation continues
uninterrupted.
• Stop Functionality: The user can stop the audio playback at any point, reset the system,
and begin recording a new message.
• UI Language Selection: The user can navigate through and select their preferred
language for interactions.

System Components
1. AudioManager
The AudioManager class orchestrates the entire process of recording audio, uploading it,
fetching the responses, and playing them back. It interacts with the following components:
AudioRecorder, AudioSaver, AudioUploader, AudioFetcher, and AudioPlayer. It also controls
the AnimatorController for character animations.
2. AudioRecorder

The AudioRecorder class handles recording audio from the microphone. It starts the
recording process, monitors the audio for silence, and stops recording when a specified
duration of silence is detected. It also trims the recorded audio before returning it.
3. AudioSaver
The AudioSaver class is responsible for saving the recorded audio as a WAV file on the local
file system.
4. AudioUploader
The AudioUploader class uploads the saved audio file to a server using an HTTP POST
request. Upon successful upload, it receives a session identifier (ssid) that will be used to
fetch the response audio.
5. AudioFetcher
The AudioFetcher class is responsible for fetching multiple audio responses from the server
based on the ssid. It sends HTTP GET requests to fetch audio files and stores them in a
queue for playback. The files are fetched sequentially, and the fetching process stops when
no more responses are available.
6. AudioPlayer
The AudioPlayer class plays the queued audio files fetched from the server. It distinguishes
between WAV and MP3 formats and plays each file using Unity&#39;s AudioSource. It also
ensures the correct lip-syncing during playback.
7. AnimatorController
The AnimatorController class manages character animations. It switches between idle,
listening, and a custom coffee animation, which can be triggered via the UI. When audio
playback begins, the character&#39;s lip-sync animation is enabled without disabling other body
animations.
