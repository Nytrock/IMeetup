using Alteruna;
using Footsteps;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour {
    [SerializeField] private TMP_InputField _nameInput;
    [SerializeField] private TextMeshProUGUI _error;
    [SerializeField] private Transform _skinContainer;
    [SerializeField] private GameObject _skinPrefab;

    [SerializeField] private List<Button> _connectionButtons;
    [SerializeField] private GameObject _mainPanel;
    [SerializeField] private GameObject _chooseRoomPanel;

    [SerializeField] private GameObject _lanEntryPrefab;
    [SerializeField] private GameObject _wanEntryPrefab;
    [SerializeField] private GameObject _roomsContainer;

    private readonly List<GameObject> _characters = new();
    private int _skinIndex;
    private int _roomID = -1;
    private readonly List<RoomObject> _roomObjects = new();
    private Multiplayer _multiplayer;

    private void Start() {
        Init();
        ChangeConnectButtonsState(false);
        SetCharacter();
    }

    public void CreateRoom() {
        if (IsNameCorrect())
            LoadScene(true);
    }

    public void JoinRoom() {
        if (IsNameCorrect())
            ChangePanels();
    }

    private bool IsNameCorrect() {
        bool isCorrect = true;
        if (_nameInput.text.Length > 20) {
            _error.text = "Cлишком длинное имя";
            isCorrect = false;
        } else if (_nameInput.text.Length < 3) {
            _error.text = "Слишком короткое имя";
            isCorrect = false;
        }
        return isCorrect;
    }

    private void LoadScene(bool isCreate) {
        PlayerInformation.Name = _nameInput.text;
        PlayerInformation.IsCreatingNewRoom = isCreate;
        PlayerInformation.SkinIndex = _skinIndex;
        SceneManager.LoadScene(1);
    }

    public void NextCharachter() {
        _characters[_skinIndex].SetActive(false);
        _skinIndex = (_skinIndex + 1) % _characters.Count;
        SetCharacter();
    }

    public void PreviousCharachter() {
        _characters[_skinIndex].SetActive(false);
        _skinIndex -= 1;
        if (_skinIndex < 0)
            _skinIndex += _characters.Count;
        SetCharacter();
    }

    public void UpdateListByButton() {
        _multiplayer.RefreshRoomList();
    }

    private void SetCharacter() {
        _characters[_skinIndex].SetActive(true);
    }

    private void Init() {
        _nameInput.text = PlayerInformation.Name;
        _error.text = "";
        _mainPanel.SetActive(true);
        _chooseRoomPanel.SetActive(false);

        _multiplayer = Multiplayer.Instance;
        _multiplayer.Connected.AddListener((m, e) => ChangeConnectButtonsState(true));
        _multiplayer.Disconnected.AddListener((m, e) => ChangeConnectButtonsState(false));
        _multiplayer.RoomListUpdated.AddListener(UpdateList);

        foreach (Animator skin in _skinPrefab.GetComponentsInChildren<Animator>(true)) {
            var character = Instantiate(skin, _skinContainer);
            foreach (FootstepTrigger trigger in character.GetComponentsInChildren<FootstepTrigger>())
                Destroy(trigger.gameObject);
            _characters.Add(character.gameObject);
        }
    }

    public void ChangePanels() {
        _mainPanel.SetActive(!_mainPanel.activeSelf);
        _chooseRoomPanel.SetActive(!_chooseRoomPanel.activeSelf);
        _multiplayer.RefreshRoomList();
    }

    private void ChangeConnectButtonsState(bool state) {
        foreach (Button button in _connectionButtons)
            button.interactable = state;
    }

    private void UpdateList(Multiplayer multiplayer) {
        if (_roomsContainer == null) 
            return;

        bool roomChange = false;
        {
            int roomID = multiplayer.InRoom ? (int)multiplayer.CurrentRoom.ID : -1;
            if (roomID != _roomID) {
                _roomID = roomID;
                roomChange = true;
            }
        }

        if (multiplayer.AvailableRooms.Count < _roomObjects.Count) {
            for (int i = _roomObjects.Count - 1; i >= multiplayer.AvailableRooms.Count; i--) {
                Destroy(_roomObjects[i].GameObject);
                _roomObjects.RemoveAt(i);
            }
        }

        for (int i = 0; i < multiplayer.AvailableRooms.Count; i++) {
            Room room = multiplayer.AvailableRooms[i];
            RoomObject entry;
            Button button;

            if (_roomObjects.Count > i) {
                if (room.Local != _roomObjects[i].Lan) {
                    Destroy(_roomObjects[i].GameObject);
                    if (room.Local) {
                        entry = new RoomObject(Instantiate(_lanEntryPrefab, _roomsContainer.transform), true);
                    } else {
                        entry = new RoomObject(Instantiate(_wanEntryPrefab, _roomsContainer.transform));
                    }
                    _roomObjects[i] = entry;
                    button = entry.GameObject.GetComponentInChildren<Button>();
                } else {
                    if (roomChange || (room.ID == multiplayer.AvailableRooms[i].ID && room.Name == multiplayer.AvailableRooms[i].Name)) continue;
                    entry = _roomObjects[i];
                    button = entry.GameObject.GetComponentInChildren<Button>();
                    button.onClick.RemoveAllListeners();
                }
            } else {
                if (room.Local) {
                    entry = new RoomObject(Instantiate(_lanEntryPrefab, _roomsContainer.transform), true);
                } else {
                    entry = new RoomObject(Instantiate(_wanEntryPrefab, _roomsContainer.transform));
                }

                button = entry.GameObject.GetComponentInChildren<Button>();

                _roomObjects.Add(entry);
            }

            if (room.InviteOnly && room.ID != _roomID) {
                entry.GameObject.SetActive(false);
                continue;
            }

            entry.GameObject.SetActive(true);
            entry.GameObject.name = room.Name;
            entry.GameObject.GetComponentInChildren<Text>().text = room.Name;

            if (room.ID == _roomID) {
                button.interactable = false;
            } else {
                button.interactable = true;
                button.onClick.AddListener(() => {
                    PlayerInformation.Room = room;
                    LoadScene(false);
                });
            }
        }
    }

    private readonly struct RoomObject {
        public readonly GameObject GameObject;
        public readonly bool Lan;

        public RoomObject(GameObject obj, bool lan = false) {
            GameObject = obj;
            Lan = lan;
        }
    }
}
