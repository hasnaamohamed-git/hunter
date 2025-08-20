using UnityEngine;
using System.Collections.Generic;

namespace HunterRush.Managers
{
    /// <summary>
    /// Input management system supporting both mobile touch controls and gamepad input
    /// </summary>
    public class InputManager : MonoBehaviour
    {
        [Header("Input Settings")]
        public bool enableTouchControls = true;
        public bool enableGamepadControls = true;
        public float touchSensitivity = 1f;
        public float gamepadSensitivity = 1f;
        
        [Header("Touch Controls")]
        public float swipeThreshold = 50f;
        public float tapThreshold = 0.3f;
        public float holdThreshold = 0.5f;
        
        [Header("Gesture Recognition")]
        public bool enableGestures = true;
        public float gestureTimeout = 2f;
        
        // Singleton
        public static InputManager Instance { get; private set; }
        
        // Input state
        private Vector2 touchStartPosition;
        private float touchStartTime;
        private bool isTouching = false;
        private bool isHolding = false;
        
        // Gesture system
        private List<Vector2> gesturePoints = new List<Vector2>();
        private float lastGestureTime;
        
        // Input events
        public System.Action<Vector2> OnTouchStart;
        public System.Action<Vector2> OnTouchEnd;
        public System.Action<Vector2> OnSwipe;
        public System.Action OnTap;
        public System.Action OnHoldStart;
        public System.Action OnHoldEnd;
        public System.Action<GestureType> OnGestureRecognized;
        
        // Virtual input state
        public Vector2 MoveInput { get; private set; }
        public bool JumpInput { get; private set; }
        public bool AttackInput { get; private set; }
        public bool SpecialInput { get; private set; }
        public bool BlockInput { get; private set; }
        
        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        void Update()
        {
            // Clear frame-based inputs
            JumpInput = false;
            AttackInput = false;
            SpecialInput = false;
            
            if (enableTouchControls && Application.isMobilePlatform)
            {
                HandleTouchInput();
            }
            
            if (enableGamepadControls)
            {
                HandleGamepadInput();
            }
            
            HandleKeyboardInput(); // Always available for testing
        }
        
        private void HandleTouchInput()
        {
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                
                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        OnTouchBegan(touch);
                        break;
                        
                    case TouchPhase.Moved:
                        OnTouchMoved(touch);
                        break;
                        
                    case TouchPhase.Ended:
                    case TouchPhase.Canceled:
                        OnTouchEnded(touch);
                        break;
                        
                    case TouchPhase.Stationary:
                        OnTouchStationary(touch);
                        break;
                }
            }
        }
        
        private void OnTouchBegan(Touch touch)
        {
            touchStartPosition = touch.position;
            touchStartTime = Time.time;
            isTouching = true;
            isHolding = false;
            
            OnTouchStart?.Invoke(touch.position);
            
            // Start gesture recording
            if (enableGestures)
            {
                gesturePoints.Clear();
                gesturePoints.Add(touch.position);
                lastGestureTime = Time.time;
            }
        }
        
        private void OnTouchMoved(Touch touch)
        {
            if (!isTouching) return;
            
            // Update movement input based on touch movement
            Vector2 deltaPosition = touch.position - touchStartPosition;
            Vector2 normalizedDelta = deltaPosition / Screen.width;
            
            MoveInput = normalizedDelta * touchSensitivity;
            
            // Add to gesture
            if (enableGestures)
            {
                gesturePoints.Add(touch.position);
            }
        }
        
        private void OnTouchStationary(Touch touch)
        {
            if (isTouching && !isHolding && Time.time - touchStartTime > holdThreshold)
            {
                isHolding = true;
                OnHoldStart?.Invoke();
                
                // Holding can trigger special abilities
                SpecialInput = true;
            }
        }
        
        private void OnTouchEnded(Touch touch)
        {
            if (!isTouching) return;
            
            float touchDuration = Time.time - touchStartTime;
            Vector2 swipeVector = touch.position - touchStartPosition;
            
            if (isHolding)
            {
                OnHoldEnd?.Invoke();
            }
            else if (touchDuration < tapThreshold && swipeVector.magnitude < swipeThreshold)
            {
                // Tap detected
                OnTap?.Invoke();
                AttackInput = true;
            }
            else if (swipeVector.magnitude > swipeThreshold)
            {
                // Swipe detected
                Vector2 swipeDirection = swipeVector.normalized;
                OnSwipe?.Invoke(swipeDirection);
                
                // Interpret swipe direction
                InterpretSwipe(swipeDirection);
            }
            
            // Process gesture
            if (enableGestures && gesturePoints.Count > 2)
            {
                GestureType gesture = RecognizeGesture(gesturePoints);
                if (gesture != GestureType.None)
                {
                    OnGestureRecognized?.Invoke(gesture);
                    TriggerGestureAction(gesture);
                }
            }
            
            OnTouchEnd?.Invoke(touch.position);
            
            isTouching = false;
            isHolding = false;
            MoveInput = Vector2.zero;
        }
        
        private void InterpretSwipe(Vector2 direction)
        {
            // Interpret swipe direction for game actions
            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
            {
                // Horizontal swipe - dodge left/right
                MoveInput = new Vector2(direction.x, 0);
            }
            else
            {
                // Vertical swipe
                if (direction.y > 0)
                {
                    // Swipe up - jump
                    JumpInput = true;
                }
                else
                {
                    // Swipe down - slide/crouch
                    BlockInput = true;
                }
            }
        }
        
        private void HandleGamepadInput()
        {
            // Left stick for movement
            Vector2 leftStick = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            MoveInput = leftStick * gamepadSensitivity;
            
            // Button inputs
            if (Input.GetButtonDown("Jump"))
                JumpInput = true;
            
            if (Input.GetButtonDown("Fire1"))
                AttackInput = true;
            
            if (Input.GetButtonDown("Fire2"))
                SpecialInput = true;
            
            BlockInput = Input.GetButton("Fire3");
        }
        
        private void HandleKeyboardInput()
        {
            // WASD movement
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            MoveInput = new Vector2(horizontal, vertical);
            
            // Key inputs
            if (Input.GetKeyDown(KeyCode.Space))
                JumpInput = true;
            
            if (Input.GetMouseButtonDown(0))
                AttackInput = true;
            
            if (Input.GetKeyDown(KeyCode.Q))
                SpecialInput = true;
            
            BlockInput = Input.GetKey(KeyCode.LeftShift);
        }
        
        // Gesture Recognition
        private GestureType RecognizeGesture(List<Vector2> points)
        {
            if (points.Count < 3) return GestureType.None;
            
            // Simple gesture recognition
            Vector2 start = points[0];
            Vector2 end = points[points.Count - 1];
            Vector2 direction = (end - start).normalized;
            
            // Check for circle gesture (Nen activation)
            if (IsCircleGesture(points))
            {
                return GestureType.Circle;
            }
            
            // Check for specific Nen ability gestures
            if (IsZigzagGesture(points))
            {
                return GestureType.Lightning; // Killua's lightning
            }
            
            if (IsSpiralGesture(points))
            {
                return GestureType.Spiral; // Kurapika's chains
            }
            
            // Check for straight line gestures
            if (IsStraightLine(points))
            {
                if (direction.y > 0.7f)
                    return GestureType.UpwardStrike; // Gon's Rock
                else if (direction.x > 0.7f)
                    return GestureType.RightwardStrike; // Gon's Paper
            }
            
            return GestureType.None;
        }
        
        private bool IsCircleGesture(List<Vector2> points)
        {
            // Simple circle detection - check if path returns close to start
            Vector2 start = points[0];
            Vector2 end = points[points.Count - 1];
            float returnDistance = Vector2.Distance(start, end);
            
            return returnDistance < 100f && points.Count > 10;
        }
        
        private bool IsZigzagGesture(List<Vector2> points)
        {
            // Detect zigzag pattern
            int directionChanges = 0;
            bool goingUp = false;
            
            for (int i = 1; i < points.Count - 1; i++)
            {
                bool currentGoingUp = points[i + 1].y > points[i].y;
                if (i > 1 && currentGoingUp != goingUp)
                {
                    directionChanges++;
                }
                goingUp = currentGoingUp;
            }
            
            return directionChanges >= 3;
        }
        
        private bool IsSpiralGesture(List<Vector2> points)
        {
            // Simple spiral detection
            Vector2 center = Vector2.zero;
            foreach (Vector2 point in points)
            {
                center += point;
            }
            center /= points.Count;
            
            // Check if points spiral around center
            float totalAngle = 0f;
            for (int i = 1; i < points.Count; i++)
            {
                Vector2 prev = points[i - 1] - center;
                Vector2 curr = points[i] - center;
                float angle = Vector2.SignedAngle(prev, curr);
                totalAngle += angle;
            }
            
            return Mathf.Abs(totalAngle) > 360f;
        }
        
        private bool IsStraightLine(List<Vector2> points)
        {
            if (points.Count < 3) return false;
            
            Vector2 start = points[0];
            Vector2 end = points[points.Count - 1];
            Vector2 line = end - start;
            
            // Check if most points are close to the line
            int pointsOnLine = 0;
            float threshold = 50f;
            
            foreach (Vector2 point in points)
            {
                float distanceToLine = Vector2.Distance(point, start + Vector2.Project(point - start, line));
                if (distanceToLine < threshold)
                {
                    pointsOnLine++;
                }
            }
            
            return (float)pointsOnLine / points.Count > 0.8f;
        }
        
        private void TriggerGestureAction(GestureType gesture)
        {
            // Trigger character-specific actions based on gesture
            BaseCharacter player = FindObjectOfType<BaseCharacter>();
            if (player == null) return;
            
            switch (gesture)
            {
                case GestureType.Circle:
                    // Activate Nen state
                    player.ChangeNenState(NenState.Ren);
                    break;
                    
                case GestureType.Lightning:
                    // Killua's lightning ability
                    if (player.characterType == CharacterType.Killua)
                    {
                        player.PerformSpecialAbility();
                    }
                    break;
                    
                case GestureType.Spiral:
                    // Kurapika's chain ability
                    if (player.characterType == CharacterType.Kurapika)
                    {
                        player.PerformSpecialAbility();
                    }
                    break;
                    
                case GestureType.UpwardStrike:
                    // Gon's Jajanken Rock
                    if (player.characterType == CharacterType.Gon)
                    {
                        player.PerformHatsuAbility();
                    }
                    break;
                    
                case GestureType.RightwardStrike:
                    // Gon's Jajanken Paper
                    if (player.characterType == CharacterType.Gon)
                    {
                        // Set Paper mode and trigger
                        player.PerformHatsuAbility();
                    }
                    break;
            }
        }
        
        // Public methods for UI
        public bool IsTouchSupported()
        {
            return enableTouchControls && Input.touchSupported;
        }
        
        public bool IsGamepadConnected()
        {
            return enableGamepadControls && Input.GetJoystickNames().Length > 0;
        }
        
        public void SetTouchControlsEnabled(bool enabled)
        {
            enableTouchControls = enabled;
        }
        
        public void SetGamepadControlsEnabled(bool enabled)
        {
            enableGamepadControls = enabled;
        }
    }
    
    public enum GestureType
    {
        None,
        Circle,
        Lightning,
        Spiral,
        UpwardStrike,
        RightwardStrike,
        DownwardStrike,
        LeftwardStrike
    }
}