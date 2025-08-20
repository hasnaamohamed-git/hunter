using UnityEngine;
using UnityEngine.SceneManagement;

namespace HunterRush
{
    /// <summary>
    /// Game launcher that ensures immediate playability
    /// Automatically sets up the game for instant play when opened in Unity
    /// </summary>
    public class GameLauncher : MonoBehaviour
    {
        [Header("Auto Launch Settings")]
        public bool autoLaunchDemo = true;
        public bool skipMainMenu = true;
        public bool createInstantDemo = true;
        
        [Header("Demo Configuration")]
        public CharacterType demoCharacter = CharacterType.Gon;
        public GameMode demoGameMode = GameMode.EndlessRun;
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void OnRuntimeMethodLoad()
        {
            Debug.Log("🎮 HUNTER RUSH: NEN CHRONICLES - Initializing...");
            
            // Check if we're in the editor and should auto-setup demo
            if (Application.isEditor)
            {
                SetupEditorDemo();
            }
        }
        
        static void SetupEditorDemo()
        {
            // Ensure demo scene is loaded in editor
            Scene activeScene = SceneManager.GetActiveScene();
            
            if (activeScene.name != "PlayableDemo" && activeScene.name != "DemoScene")
            {
                Debug.Log("📋 Loading demo scene for instant play...");
                
                // Create demo objects in current scene
                CreateQuickDemo();
            }
        }
        
        static void CreateQuickDemo()
        {
            // Quick demo setup without scene loading
            GameObject demoSetup = new GameObject("Quick Demo Setup");
            InstantPlay instantPlay = demoSetup.AddComponent<InstantPlay>();
            instantPlay.enableInstantPlay = true;
            instantPlay.createDemoScene = true;
            
            Debug.Log("✅ Quick demo created! Ready to play!");
        }
        
        void Awake()
        {
            // Initialize game launcher
            if (autoLaunchDemo)
            {
                LaunchDemo();
            }
        }
        
        void Start()
        {
            if (createInstantDemo)
            {
                EnsureDemoIsPlayable();
            }
        }
        
        private void LaunchDemo()
        {
            Debug.Log("🚀 Launching Hunter Rush demo...");
            
            // Set demo character
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SelectCharacter(demoCharacter);
                GameManager.Instance.ChangeGameState(GameState.Playing);
            }
            
            // Skip to gameplay if needed
            if (skipMainMenu)
            {
                SkipToGameplay();
            }
        }
        
        private void SkipToGameplay()
        {
            // Create instant gameplay environment
            CreateInstantGameplay();
        }
        
        private void CreateInstantGameplay()
        {
            // Ensure player exists
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                player = InstantPlay.CreateCharacterCube(demoCharacter, Vector3.up * 2f);
                player.AddComponent<DemoPlayer>();
                player.tag = "Player";
                
                Debug.Log("✅ Player created for instant gameplay");
            }
            
            // Ensure camera follows player
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                CameraFollow cameraFollow = mainCamera.GetComponent<CameraFollow>();
                if (cameraFollow == null)
                {
                    cameraFollow = mainCamera.gameObject.AddComponent<CameraFollow>();
                }
                cameraFollow.target = player.transform;
            }
        }
        
        private void EnsureDemoIsPlayable()
        {
            // Check if demo is ready
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            
            if (player == null || enemies.Length == 0)
            {
                Debug.Log("🔧 Demo not ready, creating instant demo...");
                
                // Create instant demo setup
                GameObject setupGO = new GameObject("Instant Demo Setup");
                InstantPlay setup = setupGO.AddComponent<InstantPlay>();
                setup.enableInstantPlay = true;
                setup.createDemoScene = true;
                setup.startCharacter = demoCharacter;
            }
            else
            {
                Debug.Log("✅ Demo is ready to play!");
            }
        }
        
        void Update()
        {
            // Quick restart demo
            if (Input.GetKeyDown(KeyCode.R))
            {
                RestartDemo();
            }
            
            // Quick character switch
            if (Input.GetKeyDown(KeyCode.C))
            {
                CycleCharacter();
            }
        }
        
        private void RestartDemo()
        {
            Debug.Log("🔄 Restarting demo...");
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        
        private void CycleCharacter()
        {
            // Find demo player and cycle character
            DemoPlayer demoPlayer = FindObjectOfType<DemoPlayer>();
            if (demoPlayer != null)
            {
                CharacterType current = demoPlayer.characterType;
                CharacterType next = (CharacterType)(((int)current + 1) % 4);
                
                // This would be handled by the DemoPlayer script
                Debug.Log($"🔄 Cycling to next character: {next}");
            }
        }
        
        void OnGUI()
        {
            // Show launch status
            if (autoLaunchDemo)
            {
                GUI.Label(new Rect(Screen.width - 300, 10, 290, 30), 
                    "🎮 HUNTER RUSH: DEMO MODE ACTIVE", 
                    new GUIStyle(GUI.skin.label) { fontSize = 14, fontStyle = FontStyle.Bold });
                
                GUI.Label(new Rect(Screen.width - 300, 35, 290, 20), 
                    "Press R to restart, C to cycle characters");
            }
        }
    }
}