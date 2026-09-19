using Invector.vCharacterController;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UserListUI : MonoBehaviour {
    [SerializeField] private VoiceManager _voiceManager;
    [SerializeField] private RectTransform _usersContainer;
    [SerializeField] private TextMeshProUGUI _textPrefab;

    private void Start() {
        StartCoroutine(UpdateListOfUsers());
    }

    private void Update() {
        if (EventSystem.current.currentSelectedGameObject == null && Input.GetKeyDown(KeyCode.Tab))
            ChangeUserListState();
    }

    private IEnumerator UpdateListOfUsers() {
        WaitForSeconds wait = new(3);
        while (true) {
            GenerateListOfNames();
            yield return wait;
        }
    }

    private void GenerateListOfNames() {
        for (int i = 1; i < _usersContainer.childCount; i++)
            Destroy(_usersContainer.GetChild(i).gameObject);

        foreach (var userName in _voiceManager.GetUsersNames()) {
            var nameText = Instantiate(_textPrefab, _usersContainer.transform);
            nameText.text = userName;
        }
    }

    private void ChangeUserListState() {
        _usersContainer.anchoredPosition = new Vector2(_usersContainer.anchoredPosition.x, -612.46f - _usersContainer.anchoredPosition.y);
    }
}
