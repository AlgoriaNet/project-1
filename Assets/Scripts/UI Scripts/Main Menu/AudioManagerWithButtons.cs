using UnityEngine;
using UnityEngine.UI;

public class AudioManagerWithButtons : MonoBehaviour
{
    // Button references
    public Button musicButton; // Music toggle button
    public Button voiceButton; // Voice toggle button

    // Button images (on/off states)
    public Image musicOnImage;  // Green ON image for music
    public Image musicOffImage; // Grey OFF image for music
    public Image voiceOnImage;  // Green ON image for voice
    public Image voiceOffImage; // Grey OFF image for voice

    private void Start()
    {
        // Load saved preferences or default to ON
        bool isMusicOn = PlayerPrefs.GetInt("MusicOn", 1) == 1; // Default is ON
        bool isVoiceOn = PlayerPrefs.GetInt("VoiceOn", 1) == 1; // Default is ON

        // Initialize audio and button visuals
        SetMusicState(isMusicOn);
        SetVoiceState(isVoiceOn);

        // Add listeners to buttons
        musicButton.onClick.AddListener(ToggleMusic);
        voiceButton.onClick.AddListener(ToggleVoice);
    }

    private void ToggleMusic()
    {
        // Get current music state and toggle it
        bool isMusicOn = PlayerPrefs.GetInt("MusicOn", 1) == 1;
        SetMusicState(!isMusicOn);
    }

    private void ToggleVoice()
    {
        // Get current voice state and toggle it
        bool isVoiceOn = PlayerPrefs.GetInt("VoiceOn", 1) == 1;
        SetVoiceState(!isVoiceOn);
    }

    private void SetMusicState(bool isOn)
    {
        // Control global music by enabling/disabling the AudioListener
        AudioListener.volume = isOn ? 1 : 0; // 1 for full volume, 0 for mute

        // Update button visuals
        musicOnImage.gameObject.SetActive(isOn);
        musicOffImage.gameObject.SetActive(!isOn);

        // Save the state
        PlayerPrefs.SetInt("MusicOn", isOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void SetVoiceState(bool isOn)
    {
        // For global voice control, you would need additional handling for specific audio groups (e.g., voice channels).
        // For simplicity, this example assumes "voice" is part of overall game audio.
        AudioListener.volume = isOn ? 1 : 0; // Same as music, global audio control

        // Update button visuals
        voiceOnImage.gameObject.SetActive(isOn);
        voiceOffImage.gameObject.SetActive(!isOn);

        // Save the state
        PlayerPrefs.SetInt("VoiceOn", isOn ? 1 : 0);
        PlayerPrefs.Save();
    }
}
