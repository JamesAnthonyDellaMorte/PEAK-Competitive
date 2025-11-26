using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

namespace PEAKCompetitive.Util
{
    /// <summary>
    /// Debug fly mode for testing campfire arrivals and point scoring.
    /// Press F4 (keyboard) or L3+R3 (controller) to toggle fly mode.
    /// WASD/Left Stick to move, Space/A to go up, Shift/B to go down
    /// Hold Ctrl/LT for speed boost.
    ///
    /// Networked: When any player toggles fly mode, it syncs to all players with the mod.
    /// </summary>
    public class FlyModeManager : MonoBehaviourPunCallbacks
    {
        private static FlyModeManager _instance;
        public static FlyModeManager Instance => _instance;

        private const string FLY_MODE_KEY = "FlyModeEnabled";

        public bool IsFlyModeActive { get; private set; } = false;

        private float _flySpeed = 20f;
        private float _boostMultiplier = 3f;
        private Character _localCharacter;

        // Input System actions (same as game uses)
        private InputAction _moveAction;
        private InputAction _jumpAction;
        private InputAction _crouchAction;
        private InputAction _sprintAction;

        // Controller stick click detection for toggle
        private bool _leftStickPressed = false;
        private bool _rightStickPressed = false;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;

            // Initialize input actions
            InitializeInputActions();
        }

        public override void OnEnable()
        {
            base.OnEnable();
            PhotonNetwork.AddCallbackTarget(this);
        }

        public override void OnDisable()
        {
            base.OnDisable();
            PhotonNetwork.RemoveCallbackTarget(this);
        }

        public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
        {
            if (propertiesThatChanged.ContainsKey(FLY_MODE_KEY))
            {
                bool newState = (bool)propertiesThatChanged[FLY_MODE_KEY];
                Plugin.Logger.LogInfo($"FlyModeManager: Received fly mode sync: {newState}");

                if (newState != IsFlyModeActive)
                {
                    SetFlyModeState(newState);
                }
            }
        }

        public override void OnJoinedRoom()
        {
            // Check if fly mode is already enabled in the room
            if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(FLY_MODE_KEY, out object value))
            {
                bool roomFlyMode = (bool)value;
                if (roomFlyMode && !IsFlyModeActive)
                {
                    Plugin.Logger.LogInfo("FlyModeManager: Joining room with fly mode enabled");
                    SetFlyModeState(true);
                }
            }
        }

        private void InitializeInputActions()
        {
            try
            {
                // Get existing input actions from the game's input system
                var inputActions = InputSystem.actions;
                if (inputActions != null)
                {
                    _moveAction = inputActions.FindAction("Move", false);
                    _jumpAction = inputActions.FindAction("Jump", false);
                    _crouchAction = inputActions.FindAction("Crouch", false);
                    _sprintAction = inputActions.FindAction("Sprint", false);

                    Plugin.Logger.LogInfo("FlyModeManager: Input actions initialized successfully");
                }
                else
                {
                    Plugin.Logger.LogWarning("FlyModeManager: InputSystem.actions is null - controller support may not work");
                }
            }
            catch (System.Exception ex)
            {
                Plugin.Logger.LogError($"FlyModeManager: Failed to initialize input actions: {ex.Message}");
            }
        }

        private void Update()
        {
            // Toggle fly mode with F4 (keyboard)
            if (Input.GetKeyDown(KeyCode.F4))
            {
                ToggleFlyMode();
            }

            // Toggle with L3+R3 (controller stick clicks)
            CheckControllerToggle();

            if (IsFlyModeActive)
            {
                HandleFlyMovement();
            }
        }

        private void CheckControllerToggle()
        {
            // Check for L3 (left stick click) - Button 8 on most controllers
            // Check for R3 (right stick click) - Button 9 on most controllers
            bool l3Pressed = Input.GetKey(KeyCode.JoystickButton8) || Input.GetKey(KeyCode.JoystickButton10);
            bool r3Pressed = Input.GetKey(KeyCode.JoystickButton9) || Input.GetKey(KeyCode.JoystickButton11);

            // Detect when both are pressed simultaneously
            if (l3Pressed && r3Pressed)
            {
                if (!_leftStickPressed || !_rightStickPressed)
                {
                    // First frame both are pressed
                    _leftStickPressed = true;
                    _rightStickPressed = true;
                    ToggleFlyMode();
                }
            }
            else
            {
                _leftStickPressed = l3Pressed;
                _rightStickPressed = r3Pressed;
            }
        }

        public void ToggleFlyMode()
        {
            bool newState = !IsFlyModeActive;

            // Always apply locally immediately
            SetFlyModeState(newState);

            // Also sync to other players via room properties
            if (PhotonNetwork.InRoom)
            {
                Plugin.Logger.LogInfo($"FlyModeManager: Syncing fly mode to other players: {newState}");
                var props = new Hashtable { { FLY_MODE_KEY, newState } };
                PhotonNetwork.CurrentRoom.SetCustomProperties(props);
            }
        }

        private void SetFlyModeState(bool enabled)
        {
            _localCharacter = CharacterHelper.GetLocalCharacter();

            if (_localCharacter == null)
            {
                Plugin.Logger.LogWarning("Cannot set fly mode - no local character found!");
                IsFlyModeActive = false;
                return;
            }

            IsFlyModeActive = enabled;

            if (IsFlyModeActive)
            {
                EnableFlyMode();
            }
            else
            {
                DisableFlyMode();
            }
        }

        private void EnableFlyMode()
        {
            Plugin.Logger.LogInfo("=== FLY MODE ENABLED ===");
            Plugin.Logger.LogInfo("Keyboard: WASD + Space/Shift, Ctrl for boost");
            Plugin.Logger.LogInfo("Controller: Left Stick + A(up)/B(down), LT for boost");

            // Disable gravity on all ragdoll parts
            if (_localCharacter?.refs?.ragdoll?.partList != null)
            {
                foreach (var part in _localCharacter.refs.ragdoll.partList)
                {
                    if (part?.Rig != null)
                    {
                        part.Rig.useGravity = false;
                        part.Rig.linearDamping = 5f; // Add drag to stop quickly
                    }
                }
            }
        }

        private void DisableFlyMode()
        {
            Plugin.Logger.LogInfo("=== FLY MODE DISABLED ===");

            // Re-enable gravity on all ragdoll parts
            if (_localCharacter?.refs?.ragdoll?.partList != null)
            {
                foreach (var part in _localCharacter.refs.ragdoll.partList)
                {
                    if (part?.Rig != null)
                    {
                        part.Rig.useGravity = true;
                        part.Rig.linearDamping = 0f; // Reset drag
                    }
                }
            }
        }

        private void HandleFlyMovement()
        {
            _localCharacter = CharacterHelper.GetLocalCharacter();
            if (_localCharacter == null) return;

            // Get input direction from both keyboard and controller
            Vector3 moveDir = Vector3.zero;

            // Get movement input from Input System (works with both keyboard and controller)
            Vector2 moveInput = Vector2.zero;
            if (_moveAction != null)
            {
                moveInput = _moveAction.ReadValue<Vector2>();
            }

            // Also check legacy keyboard input as fallback
            if (Input.GetKey(KeyCode.W)) moveInput.y = Mathf.Max(moveInput.y, 1f);
            if (Input.GetKey(KeyCode.S)) moveInput.y = Mathf.Min(moveInput.y, -1f);
            if (Input.GetKey(KeyCode.A)) moveInput.x = Mathf.Min(moveInput.x, -1f);
            if (Input.GetKey(KeyCode.D)) moveInput.x = Mathf.Max(moveInput.x, 1f);

            // Apply movement relative to camera
            if (Camera.main != null)
            {
                moveDir += Camera.main.transform.forward * moveInput.y;
                moveDir += Camera.main.transform.right * moveInput.x;
            }

            // Up/Down movement
            bool goUp = false;
            bool goDown = false;

            // Check Input System actions
            if (_jumpAction != null && _jumpAction.IsPressed()) goUp = true;
            if (_crouchAction != null && _crouchAction.IsPressed()) goDown = true;

            // Also check keyboard
            if (Input.GetKey(KeyCode.Space)) goUp = true;
            if (Input.GetKey(KeyCode.LeftShift)) goDown = true;

            // Also check controller buttons directly (A=0, B=1 for Xbox/PS layout)
            if (Input.GetKey(KeyCode.JoystickButton0)) goUp = true;    // A / Cross
            if (Input.GetKey(KeyCode.JoystickButton1)) goDown = true;  // B / Circle

            if (goUp) moveDir += Vector3.up;
            if (goDown) moveDir -= Vector3.up;

            // Apply speed boost if holding Ctrl or LT (Left Trigger)
            float speed = _flySpeed;
            bool boosting = false;

            // Check keyboard boost
            if (Input.GetKey(KeyCode.LeftControl)) boosting = true;

            // Check controller LT (axis 9 on most controllers, or sprint action)
            if (_sprintAction != null && _sprintAction.IsPressed()) boosting = true;
            if (Input.GetAxis("JoystickAxis9") > 0.5f) boosting = true;  // LT on some controllers
            if (Input.GetAxis("JoystickAxis3") < -0.5f) boosting = true; // LT on other controllers

            if (boosting)
                speed *= _boostMultiplier;

            // Normalize and apply force to all ragdoll parts
            if (moveDir.magnitude > 0.1f)
            {
                moveDir = moveDir.normalized * speed;

                if (_localCharacter?.refs?.ragdoll?.partList != null)
                {
                    foreach (var part in _localCharacter.refs.ragdoll.partList)
                    {
                        if (part?.Rig != null)
                        {
                            // Cancel existing velocity first for responsive control
                            part.Rig.linearVelocity = Vector3.Lerp(part.Rig.linearVelocity, moveDir, Time.deltaTime * 10f);
                        }
                    }
                }
            }
            else
            {
                // Slow down when no input
                if (_localCharacter?.refs?.ragdoll?.partList != null)
                {
                    foreach (var part in _localCharacter.refs.ragdoll.partList)
                    {
                        if (part?.Rig != null)
                        {
                            part.Rig.linearVelocity = Vector3.Lerp(part.Rig.linearVelocity, Vector3.zero, Time.deltaTime * 5f);
                        }
                    }
                }
            }
        }

        private void OnGUI()
        {
            if (!IsFlyModeActive) return;

            // Draw fly mode indicator
            var headerStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 16,
                richText = true
            };

            var subStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 12,
                richText = true
            };

            GUI.Label(
                new Rect(Screen.width / 2 - 150, 10, 300, 30),
                "<color=cyan><b>FLY MODE ACTIVE</b></color>",
                headerStyle
            );

            GUI.Label(
                new Rect(Screen.width / 2 - 200, 35, 400, 20),
                "<color=white>KB: WASD+Space/Shift | Ctrl=Boost | F4=Toggle</color>",
                subStyle
            );

            GUI.Label(
                new Rect(Screen.width / 2 - 200, 52, 400, 20),
                "<color=white>Controller: Stick+A/B | LT=Boost | L3+R3=Toggle</color>",
                subStyle
            );
        }

        private void OnDestroy()
        {
            // Make sure to disable fly mode if component is destroyed
            if (IsFlyModeActive)
            {
                IsFlyModeActive = false;
                DisableFlyMode();
            }
        }
    }
}
