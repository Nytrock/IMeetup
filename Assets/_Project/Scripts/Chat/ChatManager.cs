using UnityEngine;
using TMPro;
using Alteruna;
using System;

public class ChatManager : AttributesSync {
    [SerializeField] private TMP_InputField _input;
    [SerializeField] private Transform _messagesContainer;
    [SerializeField] private ChatBubbleBehavior _messagesPrefab;
    [SerializeField] private SettingsManager _settingsManager;
    [SerializeField] private RectTransform _chatContainer;

    private string _playerName;
    private bool _isChatOpen;

    public bool IsChatOpen => _isChatOpen;

    void Start() {
        _playerName = PlayerInformation.Name;
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Return))
            CreateMessage();

        if (Input.GetKeyDown(KeyCode.T) && !_settingsManager.IsMenuOpen)
            Chat();
    }

    private void Chat() {
        _chatContainer.anchoredPosition = new Vector2(-800 - _chatContainer.anchoredPosition.x, _chatContainer.anchoredPosition.y);
        _isChatOpen = _chatContainer.anchoredPosition.x == 0;
        Cursor.visible = _isChatOpen;
    }


    public void CreateMessage() {
        if (_input.text == "")
            return;

        BroadcastRemoteMethod("SynchronizeMessage", _input.text, _playerName);
        _input.text = "";
    }

    [SynchronizableMethod] 
    public void SynchronizeMessage(string messageText, string name) {
        bool isMe = _playerName == name;
        var message = Instantiate(_messagesPrefab, _messagesContainer);
        message.SetMessage(DateTime.Now, name, messageText, isMe);
    }
}
