using Alteruna;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoomManager : MonoBehaviour {
    [SerializeField] private GameObject _loadingScreen;
    [SerializeField] private TextMeshProUGUI _roomText;
    [SerializeField] private VoiceManager _voiceManager;

    private Multiplayer _multiplayer;

    public void Start() {
        _multiplayer = Multiplayer.Instance;
        if (_multiplayer.InRoom) {
            OnRoomJoin(_multiplayer.CurrentRoom.Name);
        } else {
            ChangeLoadingState(true);
            _multiplayer.RoomJoined.AddListener((mult, room, user) => OnRoomJoin(room.Name));
        }
    }

    public void Leave() {
        _multiplayer.CurrentRoom?.Leave();
        _voiceManager.LeaveVoice();
        SceneManager.LoadScene(0);
    }

    public void ChangeLoadingState(bool state) {
        _loadingScreen.SetActive(state);
    }

    public void OnRoomJoin(string text) {
        ChangeLoadingState(false);
        _roomText.text = text;
    }
}
