using Agora.Rtc;
using Alteruna;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Invector.vCharacterController;
using Newtonsoft.Json;


public class VoiceManager : MonoBehaviour {
    private const string URI = "https://agora-token-service-production-c683.up.railway.app/rtc/";

    [SerializeField] private string _appID = "";

    private vThirdPersonController _mainUser;
    private readonly List<vThirdPersonController> _users = new();

    public IRtcEngine RtcEngine { get; private set; }
    public uint Uid { get; private set; }

    private void Start() {
        Multiplayer.Instance.RoomJoined.AddListener(CreateToken);

        InitRtcEngine();
        SetBasicConfiguration();
        RtcEngine.EnableAudioVolumeIndication(200, 3, true);
    }

    private void Update() {
        if (_mainUser == null)
            return;

        var player = _mainUser.transform;
        foreach (vThirdPersonController user in _users) {
            if (user.Uid == 0)
                continue;

            var heading = user.transform.position - player.position;
            var distance = heading.magnitude;

            float volume;
            if (distance <= 2.5f)
                volume = 100;
            else if (distance >= 10.5f)
                volume = 0;
            else
                volume = -12.5f * distance + 131.25f;
            
            RtcEngine.AdjustUserPlaybackSignalVolume(user.Uid, (int)volume);
        }
    }

    private void InitRtcEngine() {
        RtcEngine = Agora.Rtc.RtcEngine.CreateAgoraRtcEngine();
        RtcEngineContext context = new(_appID, 0,
                                    CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING,
                                    AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT);
        UserEventHandler handler = new(this);
        RtcEngine.Initialize(context);
        RtcEngine.InitEventHandler(handler);
    }

    private void SetBasicConfiguration() {
        RtcEngine.EnableAudio();
        RtcEngine.SetChannelProfile(CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_COMMUNICATION);
        RtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
    }

    public void CreateToken(Multiplayer multiplayer, Room room, User user) {
        var typeUser = room.GetUserCount() == 1 ? "publisher" : "subscriber";
        Cursor.visible = false;
        StartCoroutine(GetData_Coroutine(room, typeUser));
    }

    public void LeaveVoice() {
        RtcEngine.LeaveChannel();
    }

    IEnumerator GetData_Coroutine(Room room, string user) {
        using UnityWebRequest request = UnityWebRequest.Get(URI + room.Name + "/" + user + "/uid/0");
        yield return request.SendWebRequest();
        Dictionary<string, string> values = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.downloadHandler.text);

        if (values is null || !values.ContainsKey("rtcToken")) {
            Debug.LogError("Cannot connect to voice room");
            yield break;
        }

        RtcEngine.JoinChannel(values["rtcToken"], room.Name);
    }

    public void SetMain(vThirdPersonController player) {
        _mainUser = player;
    }

    public void AddUser(vThirdPersonController cc) {
        _users.Add(cc);
    }

    public void RemoveUser(vThirdPersonController cc) {
        _users.Remove(cc);
    }

    public void ChangeSpeaking(bool isSpeaking) {
        _mainUser.IsSpeak = isSpeaking;
    }

    public IEnumerable<string> GetUsersNames() {
        if (_mainUser != null)
            yield return _mainUser.PlayerName;
        foreach (var user in _users)
            yield return user.PlayerName;
    }

    internal class UserEventHandler : IRtcEngineEventHandler {
        private readonly VoiceManager _voiceManager;

        public UserEventHandler(VoiceManager voiceManager) {
            _voiceManager = voiceManager;
        }

        public override void OnError(int err, string msg) {
            Debug.Log(string.Format("OnError err: {0}, msg: {1}", err, msg));
        }

        public override void OnJoinChannelSuccess(RtcConnection connection, int elapsed) {
            Debug.Log("Success!");
            _voiceManager.Uid = connection.localUid;
        }

        public override void OnRejoinChannelSuccess(RtcConnection connection, int elapsed) {
            Debug.Log("Success!");
        }

        public override void OnLeaveChannel(RtcConnection connection, RtcStats stats) {
            Debug.Log("Leave");
        }

        public override void OnClientRoleChanged(RtcConnection connection, CLIENT_ROLE_TYPE oldRole, CLIENT_ROLE_TYPE newRole, ClientRoleOptions newRoleOptions) {
            Debug.Log("Change");
        }

        public override void OnUserJoined(RtcConnection connection, uint uid, int elapsed) {
            Debug.Log(uid + "- join!");
        }

        public override void OnUserOffline(RtcConnection connection, uint uid, USER_OFFLINE_REASON_TYPE reason) {
            Debug.Log(uid + "- offline!");
        }

        public override void OnAudioVolumeIndication(RtcConnection connection, AudioVolumeInfo[] speakers, uint speakerNumber, int totalVolume) {
            if (speakerNumber == 0)
                return;

           _voiceManager.ChangeSpeaking(speakers[0].vad == 1);
        }
    }
}
