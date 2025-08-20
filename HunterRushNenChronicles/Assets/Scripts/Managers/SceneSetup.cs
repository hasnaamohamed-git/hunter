using UnityEngine;
using UnityEngine.SceneManagement;

namespace HunterRush.Managers
{
    /// <summary>
    /// Scene setup and management for different game scenes
    /// </summary>
    public class SceneSetup : MonoBehaviour
    {
        [Header("Scene References")]
        public string mainMenuScene = "MainMenu";
        public string gameplayScene = "Gameplay";
        public string characterSelectScene = "CharacterSelect";
        
        [Header("Managers")]
        public GameObject gameManagerPrefab;
        public GameObject audioManagerPrefab;
        public GameObject uiManagerPrefab;
        public GameObject inputManagerPrefab;
        
        [Header("Player Setup")]
        public GameObject[] characterPrefabs;
        public Transform playerSpawnPoint;
        
        [Header("Camera")]
        public GameObject cameraRigPrefab;
        public Vector3 cameraOffset = new Vector3(0, 5, -8);
        
        void Awake()
        {
            SetupEssentialManagers();
        }
        
        void Start()
        {
            SetupScene();
        }
        
        private void SetupEssentialManagers()
        {
            // Ensure essential managers exist
            if (GameManager.Instance == null && gameManagerPrefab != null)
            {
                Instantiate(gameManagerPrefab);
            }
            
            if (AudioManager.Instance == null && audioManagerPrefab != null)
            {
                Instantiate(audioManagerPrefab);
            }
            
            if (UIManager.Instance == null && uiManagerPrefab != null)
            {
                Instantiate(uiManagerPrefab);
            }
            
            if (InputManager.Instance == null && inputManagerPrefab != null)
            {
                Instantiate(inputManagerPrefab);
            }
        }
        
        private void SetupScene()
        {
            string currentScene = SceneManager.GetActiveScene().name;
            
            switch (currentScene)
            {
                case "MainMenu":
                    SetupMainMenuScene();
                    break;
                case "CharacterSelect":
                    SetupCharacterSelectScene();
                    break;
                case "Gameplay":
                case "EndlessRun":
                case "StoryMode":
                case "BossRush":
                    SetupGameplayScene();
                    break;
            }
        }
        
        private void SetupMainMenuScene()
        {
            // Setup main menu specific elements
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowMainMenu();
            }
            
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayMusic(AudioManager.Instance.mainMenuMusic);
            }
        }
        
        private void SetupCharacterSelectScene()
        {
            // Setup character selection
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowCharacterSelect();
            }
            
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayMusic(AudioManager.Instance.characterSelectMusic);
            }
        }
        
        private void SetupGameplayScene()
        {
            // Spawn player character
            SpawnPlayerCharacter();
            
            // Setup camera
            SetupCamera();
            
            // Initialize level generator
            SetupLevelGenerator();
            
            // Setup game mode
            SetupGameMode();
            
            // Show HUD
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowHUD();
            }
        }
        
        private void SpawnPlayerCharacter()
        {
            if (GameManager.Instance == null || characterPrefabs.Length == 0) return;
            
            CharacterType selectedChar = GameManager.Instance.selectedCharacter;
            int charIndex = (int)selectedChar;
            
            if (charIndex < characterPrefabs.Length && characterPrefabs[charIndex] != null)
            {
                Vector3 spawnPos = playerSpawnPoint != null ? playerSpawnPoint.position : Vector3.zero;
                GameObject player = Instantiate(characterPrefabs[charIndex], spawnPos, Quaternion.identity);
                player.tag = "Player";
                
                // Setup player components
                BaseCharacter character = player.GetComponent<BaseCharacter>();
                if (character != null)
                {
                    character.characterType = selectedChar;
                }
                
                Debug.Log($"Spawned player character: {selectedChar}");
            }
        }
        
        private void SetupCamera()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return;
            
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                // Create camera if none exists
                if (cameraRigPrefab != null)
                {
                    GameObject cameraRig = Instantiate(cameraRigPrefab);
                    mainCamera = cameraRig.GetComponentInChildren<Camera>();
                }
                else
                {
                    GameObject cameraGO = new GameObject("Main Camera");
                    mainCamera = cameraGO.AddComponent<Camera>();
                    cameraGO.tag = "MainCamera";
                }
            }
            
            // Setup camera follow
            CameraController cameraController = mainCamera.GetComponent<CameraController>();
            if (cameraController == null)
            {
                cameraController = mainCamera.gameObject.AddComponent<CameraController>();
            }
            
            cameraController.target = player.transform;
            cameraController.offset = cameraOffset;
        }
        
        private void SetupLevelGenerator()
        {
            LevelGenerator levelGen = FindObjectOfType<LevelGenerator>();
            if (levelGen == null)
            {
                GameObject levelGenGO = new GameObject("Level Generator");
                levelGen = levelGenGO.AddComponent<LevelGenerator>();
            }
            
            // Set player reference
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                levelGen.SetPlayer(player.transform);
            }
        }
        
        private void SetupGameMode()
        {
            string sceneName = SceneManager.GetActiveScene().name;
            
            switch (sceneName)
            {
                case "EndlessRun":
                    SetupEndlessRunMode();
                    break;
                case "StoryMode":
                    SetupStoryMode();
                    break;
                case "BossRush":
                    SetupBossRushMode();
                    break;
            }
        }
        
        private void SetupEndlessRunMode()
        {
            GameObject modeGO = new GameObject("Endless Run Mode");
            EndlessRunMode endlessMode = modeGO.AddComponent<EndlessRunMode>();
            
            Debug.Log("Endless Run Mode initialized");
        }
        
        private void SetupStoryMode()
        {
            GameObject modeGO = new GameObject("Story Mode");
            StoryMode storyMode = modeGO.AddComponent<StoryMode>();
            
            Debug.Log("Story Mode initialized");
        }
        
        private void SetupBossRushMode()
        {
            GameObject modeGO = new GameObject("Boss Rush Mode");
            BossRushMode bossMode = modeGO.AddComponent<BossRushMode>();
            
            Debug.Log("Boss Rush Mode initialized");
        }
    }
    
    /// <summary>
    /// Simple camera controller for following the player
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        [Header("Follow Settings")]
        public Transform target;
        public Vector3 offset = new Vector3(0, 5, -8);
        public float followSpeed = 5f;
        public float rotationSpeed = 2f;
        
        [Header("Look Ahead")]
        public float lookAheadDistance = 5f;
        public float lookAheadSpeed = 2f;
        
        private Vector3 velocity = Vector3.zero;
        
        void LateUpdate()
        {
            if (target == null) return;
            
            // Calculate target position with look-ahead
            Vector3 targetPosition = target.position + offset;
            
            // Add look-ahead based on player velocity
            Rigidbody targetRb = target.GetComponent<Rigidbody>();
            if (targetRb != null)
            {
                Vector3 lookAhead = targetRb.velocity.normalized * lookAheadDistance;
                targetPosition += lookAhead;
            }
            
            // Smooth follow
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, 1f / followSpeed);
            
            // Look at target
            Vector3 lookDirection = target.position - transform.position;
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        
        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }
        
        public void SetOffset(Vector3 newOffset)
        {
            offset = newOffset;
        }
    }
}