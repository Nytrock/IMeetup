using UnityEngine;
using TMPro;
using Alteruna;
using System.Collections;
using System;

namespace Invector.vCharacterController {
    [RequireComponent(typeof(Alteruna.Avatar))]
    public class vThirdPersonInput : MonoBehaviour {
        #region Variables       

        [Header("Controller Input")]
        public string horizontalInput = "Horizontal";
        public string verticallInput = "Vertical";
        public KeyCode jumpInput = KeyCode.Space;
        public KeyCode strafeInput = KeyCode.Tab;
        public KeyCode sprintInput = KeyCode.LeftShift;

        [Header("Camera Input")]
        public string rotateCameraXInput = "Mouse X";
        public string rotateCameraYInput = "Mouse Y";

        [HideInInspector] public vThirdPersonController cc;
        [HideInInspector] public vThirdPersonCamera tpCamera;
        [HideInInspector] public Camera cameraMain;

        [SerializeField] private GameObject namePrefab;
        public TextMeshProUGUI nameText;

        private Alteruna.Avatar _avatar;
        private string _playerName;
        private Animator[] _skins;
        private int _oldSkin;

        private VoiceManager _voiceManager;
        private SettingsManager _settingsManager;
        private ChatManager _chatManager;
        private const string SPEAK_SYMBOL = " ılı";

        #endregion

        public void SetManagers(VoiceManager voiceManager, SettingsManager settingsManager, ChatManager chatManager) {
            _voiceManager = voiceManager;
            _settingsManager = settingsManager;
            _chatManager = chatManager;
        }

        protected virtual void Start() {
            _avatar = GetComponent<Alteruna.Avatar>();
            InitilizeController();

            _skins = cc.GetComponentsInChildren<Animator>(true);
            if (_avatar.IsMe) {
                cc.PlayerSkin = PlayerInformation.SkinIndex;
                cc.animator = _skins[cc.PlayerSkin];
                cc.animator.gameObject.SetActive(true);
                Multiplayer.Instance.OtherUserJoined.AddListener(UpdateSkin);
            } else {
                cc.animator = _skins[cc.PlayerSkin];
                cc.animator.gameObject.SetActive(true);
                _oldSkin = cc.PlayerSkin;
            }

            if (cc != null)
                cc.Init();

            if (!_avatar.IsMe) {
                nameText = Instantiate(namePrefab, transform).GetComponentInChildren<TextMeshProUGUI>();
                _voiceManager.AddUser(cc);
                return;
            } else {
                _voiceManager.SetMain(cc);
            }

            cc.PlayerName = PlayerInformation.Name;
            _playerName = cc.PlayerName;
            cc.rtc = _voiceManager.RtcEngine;
           
            InitializeTpCamera();
        }

        private void UpdateSkin(Multiplayer multiplayer, User user) {
            StartCoroutine(Wait());
        }

        private IEnumerator Wait() {
            yield return new WaitForSeconds(Time.deltaTime);
            var id = cc.PlayerSkin;
            cc.PlayerSkin = 0;
            yield return new WaitForSeconds(1);
            cc.PlayerSkin = id;
        }

        private void OnDestroy() {
            if (!_avatar.IsMe)
                _voiceManager.RemoveUser(cc);
        }

        protected virtual void FixedUpdate() {
            if (!_avatar.IsMe) {
                nameText.text = cc.PlayerName;
                if (cc.PlayerSkin != _oldSkin) {
                    _skins[_oldSkin].gameObject.SetActive(false);
                    cc.animator = _skins[cc.PlayerSkin];
                    cc.animator.gameObject.SetActive(true);
                    _oldSkin = cc.PlayerSkin;
                }
                return;
            }

            if (cc.IsSpeak)
                cc.PlayerName = _playerName + SPEAK_SYMBOL;
            else
                cc.PlayerName = _playerName;
            cc.Uid = _voiceManager.Uid;
            cc.UpdateMotor();               // updates the ThirdPersonMotor methods
            cc.ControlLocomotionType();     // handle the controller locomotion type and movespeed
            cc.ControlRotationType();       // handle the controller rotation type
        }

        protected virtual void Update() {
            cc.UpdateAnimator();
            if (!_avatar.IsMe)
                return;

            if (_settingsManager.IsMenuOpen || _chatManager.IsChatOpen) {
                cc.input = Vector3.zero;
                cc.moveDirection = Vector3.zero;
                return;
            }

            InputHandle();                  // update the input methods
        }

        public virtual void OnAnimatorMove() {
            if (!_avatar.IsMe)
                return;

            cc.ControlAnimatorRootMotion(); // handle root motion animations 
        }

        #region Basic Locomotion Inputs

        protected virtual void InitilizeController() {
            cc = GetComponent<vThirdPersonController>();
        }

        protected virtual void InitializeTpCamera() {
            if (tpCamera == null) {
                tpCamera = FindAnyObjectByType<vThirdPersonCamera>();
                if (tpCamera == null)
                    return;
                if (tpCamera)
                {
                    tpCamera.SetMainTarget(this.transform);
                    tpCamera.Init();
                }
            }
        }

        protected virtual void InputHandle()
        {
            MoveInput();
            CameraInput();
            SprintInput();
            StrafeInput();
            JumpInput();
        }

        public virtual void MoveInput()
        {
            cc.input.x = Input.GetAxis(horizontalInput);
            cc.input.z = Input.GetAxis(verticallInput);
        }

        protected virtual void CameraInput()
        {
            if (!cameraMain)
            {
                if (!Camera.main) Debug.Log("Missing a Camera with the tag MainCamera, please add one.");
                else
                {
                    cameraMain = Camera.main;
                    cc.rotateTarget = cameraMain.transform;
                }
            }

            if (cameraMain)
            {
                cc.UpdateMoveDirection(cameraMain.transform);
            }

            if (tpCamera == null)
                return;

            var Y = Input.GetAxis(rotateCameraYInput);
            var X = Input.GetAxis(rotateCameraXInput);

            tpCamera.RotateCamera(X, Y);
        }

        protected virtual void StrafeInput()
        {
            if (Input.GetKeyDown(strafeInput))
                cc.Strafe();
        }

        protected virtual void SprintInput()
        {
            if (Input.GetKeyDown(sprintInput))
                cc.Sprint(true);
            else if (Input.GetKeyUp(sprintInput))
                cc.Sprint(false);
        }

        /// <summary>
        /// Conditions to trigger the Jump animation & behavior
        /// </summary>
        /// <returns></returns>
        protected virtual bool JumpConditions()
        {
            return cc.isGrounded && cc.GroundAngle() < cc.slopeLimit && !cc.isJumping && !cc.stopMove;
        }

        /// <summary>
        /// Input to trigger the Jump 
        /// </summary>
        protected virtual void JumpInput()
        {
            if (Input.GetKeyDown(jumpInput) && JumpConditions())
                cc.Jump();
        }

        #endregion
    }
}