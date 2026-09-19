using Alteruna;
using Invector.vCharacterController;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour {
    [SerializeField] private VoiceManager _voiceManager;
    [SerializeField] private SettingsManager _settingsManager;
    [SerializeField] private ChatManager _chatManager;

    private void Start() {
        Multiplayer.Instance.AvatarSpawnLocation = transform;
        SpawnPlayer();
    }

    private void SpawnPlayer() {
        Alteruna.Avatar avatar = Multiplayer.Instance.SpawnAvatar();
        avatar.TryGetComponent(out vThirdPersonInput playerInput);
        playerInput.SetManagers(_voiceManager, _settingsManager, _chatManager);
    }
}
