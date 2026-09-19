using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(PostProcessVolume))]
public class SettingsManager : MonoBehaviour {
    [SerializeField] private string _url;
    [SerializeField] private GameObject _menu;
    [SerializeField] private GameObject _settings;
    [SerializeField] private GameObject _controls;

    [Header("Настройки микрофона")]
    [SerializeField] private VoiceManager _voiceManager;
    [SerializeField] private GameObject _muteText;
    [SerializeField] private Slider _microphoneSlider;
    private bool _isMicrophoneMuted;

    [Header("Настройки музыки")]
    [SerializeField] private Slider _audioSlider;
    [SerializeField] private AudioMixerGroup _audioMixer;
    private bool _isMusicMuted;

    [Header("Настройки сенсы")]
    [SerializeField] private Slider _sensitivitySlider;
    private vThirdPersonCamera _vCamera;

    [Header("Настройки контраста")]
    [SerializeField] private Slider _contrastSlider;
    private PostProcessVolume _processVolume;
    private ColorGrading _gain;

    private bool _isMenuOpen;

    public bool IsMenuOpen => _isMenuOpen;
    private bool InRoom => SceneManager.GetActiveScene().buildIndex == 1;

    private void Start() {
        _microphoneSlider.maxValue = 400;
        _microphoneSlider.value = PlayerInformation.MicrophoneVolume;
        OnMicrophoneSliderChange(PlayerInformation.MicrophoneVolume);
        if (InRoom)
            _muteText.SetActive(PlayerInformation.MicrophoneVolume == 0);

        if (PlayerInformation.IsMusicMute)
            MuteMusic();
        _audioSlider.value = PlayerInformation.AudioVolume;

        if (InRoom)
            _vCamera = FindAnyObjectByType<vThirdPersonCamera>();
        _sensitivitySlider.value = PlayerInformation.Sensitivity;
        _sensitivitySlider.maxValue = 12f;
        _sensitivitySlider.minValue = 0.5f;
        OnSensitivitySliderChange(PlayerInformation.Sensitivity);

        _processVolume = GetComponent<PostProcessVolume>();
        _processVolume.profile.TryGetSettings(out _gain);

        _contrastSlider.value = PlayerInformation.Contrast;
        _contrastSlider.maxValue = 1;
        _contrastSlider.minValue = -1;
        OnContrastSliderChange(PlayerInformation.Contrast);

        _menu.SetActive(false);
        _settings.SetActive(false);
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            if (_controls.activeSelf)
                Controls();
            else if (_settings.activeSelf)
                Settings();
            else
                Menu();
        }

        if (EventSystem.current.currentSelectedGameObject != null)
            return;

        if (Input.GetKeyDown(KeyCode.Q) && InRoom)
            MuteMicrophone();

        if (Input.GetKeyDown(KeyCode.M))
            MuteMusic();
    }

    public void Menu() {
        _menu.SetActive(!_menu.activeSelf);
        _isMenuOpen = _menu.activeSelf;
        if (InRoom)
            Cursor.visible = _isMenuOpen;
    }

    public void Exit() {
        Application.Quit();
    }

    public void Support() {
        Application.OpenURL(_url);
    }

    public void Settings() {
        _menu.SetActive(!_menu.activeSelf);
        _settings.SetActive(!_settings.activeSelf);
    }

    public void Controls() {
        _menu.SetActive(!_menu.activeSelf);
        _controls.SetActive(!_controls.activeSelf);
    }

    private void MuteMicrophone() {
        _muteText.SetActive(!_muteText.activeSelf);
        _isMicrophoneMuted = !_isMicrophoneMuted;

        if (!_muteText.activeSelf) {
            _voiceManager.RtcEngine.AdjustRecordingSignalVolume(PlayerInformation.MicrophoneVolume);
            _microphoneSlider.value = PlayerInformation.MicrophoneVolume;
        } else {
            _voiceManager.RtcEngine.AdjustRecordingSignalVolume(0);
            _microphoneSlider.value = 0;
        }
    }

    private void MuteMusic() {
        _isMusicMuted = !_isMusicMuted;
        PlayerInformation.IsMusicMute = _isMusicMuted;

        if (!_isMusicMuted) {
            _audioMixer.audioMixer.SetFloat("Background", Mathf.Lerp(-60, 20, PlayerInformation.AudioVolume));
            _audioSlider.value = PlayerInformation.AudioVolume;
        } else {
            _audioMixer.audioMixer.SetFloat("Background", -80);
            _audioSlider.value = 0;
        }
    }

    public void OnMicrophoneSliderChange(float value) {
        if (_isMicrophoneMuted)
            return;

        PlayerInformation.MicrophoneVolume = (int)value;
        if (InRoom) {
            _voiceManager.RtcEngine.AdjustRecordingSignalVolume((int)value);
            _muteText.SetActive((int)value == 0);
        }
    }

    public void OnAudioSliderChange(float value) {
        if (_isMusicMuted)
            return;

        _isMusicMuted = false;
        PlayerInformation.IsMusicMute = false;
        PlayerInformation.AudioVolume = value;
        _audioMixer.audioMixer.SetFloat("Background", Mathf.Lerp(-60, 20, value));
    }

    public void OnSensitivitySliderChange(float value) {
        PlayerInformation.Sensitivity = value;
        if (InRoom) {
            _vCamera.xMouseSensitivity = value;
            _vCamera.yMouseSensitivity = value;
        }
    }

    public void OnContrastSliderChange(float value) {
        PlayerInformation.Contrast = value;
        _gain.gain.value = new Vector4(1, 1, 1, value);
    }
}
