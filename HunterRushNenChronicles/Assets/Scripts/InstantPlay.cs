using UnityEngine;
using UnityEngine.SceneManagement;

namespace HunterRush
{
    /// <summary>
    /// Instant play setup - makes the game immediately playable when opened in Unity
    /// Automatically creates a demo scene with player, enemies, and environment
    /// </summary>
    [System.Serializable]
    public class InstantPlay : MonoBehaviour
    {
        [Header("Instant Play Settings")]
        public bool enableInstantPlay = true;
        public bool createDemoScene = true;
        public CharacterType startCharacter = CharacterType.Gon;
        
        [Header("Demo Level")]
        public Vector3 levelBounds = new Vector3(100f, 20f, 100f);
        public int enemyCount = 15;
        public int platformCount = 8;
        public int collectibleCount = 20;
        
        [Header("Player Spawn")]
        public Vector3 playerSpawnPosition = new Vector3(0, 2f, 0);
        
        // Runtime objects
        private GameObject player;
        private Camera playerCamera;
        private GameObject demoLevel;
        
        void Awake()
        {
            // Set as default scene if no scene is set
            if (enableInstantPlay)
            {
                Debug.Log("🎮 HUNTER RUSH: Setting up instant play mode...");
                SetupInstantPlay();
            }
        }
        
        void Start()
        {
            if (createDemoScene)
            {
                CreateCompleteDemo();
            }
        }
        
        private void SetupInstantPlay()
        {
            // Ensure this scene is the startup scene
            if (SceneManager.GetActiveScene().name == "PlayableDemo" || SceneManager.GetActiveScene().name == "DemoScene")
            {
                Debug.Log("✅ Instant play mode activated!");
            }
        }
        
        [ContextMenu("Create Complete Demo")]
        public void CreateCompleteDemo()
        {
            Debug.Log("🎮 Creating complete Hunter Rush demo...");
            
            // Clear existing demo objects
            ClearExistingDemo();
            
            // Create demo environment
            CreateDemoLevel();
            CreateDemoPlayer();
            CreateDemoEnemies();
            CreateDemoPlatforms();
            CreateDemoCollectibles();
            SetupDemoCamera();
            CreateDemoUI();
            
            Debug.Log("✅ Demo created! Ready to play!");
            ShowInstructions();
        }
        
        private void ClearExistingDemo()
        {
            // Remove existing demo objects
            GameObject[] demoObjects = GameObject.FindGameObjectsWithTag("Demo");
            foreach (GameObject obj in demoObjects)
            {
                DestroyImmediate(obj);
            }
            
            // Remove existing player
            GameObject existingPlayer = GameObject.FindGameObjectWithTag("Player");
            if (existingPlayer != null)
            {
                DestroyImmediate(existingPlayer);
            }
        }
        
        private void CreateDemoLevel()
        {
            // Create ground plane
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Demo Ground";
            ground.tag = "Demo";
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = Vector3.one * 10f;
            
            // Style the ground
            Renderer groundRenderer = ground.GetComponent<Renderer>();
            Material groundMat = new Material(Shader.Find("Standard"));
            groundMat.color = new Color(0.2f, 0.8f, 0.2f); // Hunter green
            groundMat.metallic = 0f;
            groundMat.smoothness = 0.3f;
            groundRenderer.material = groundMat;
            
            // Create boundaries
            CreateBoundaryWalls();
            
            demoLevel = ground;
        }
        
        private void CreateBoundaryWalls()
        {
            Vector3[] wallPositions = {
                new Vector3(0, 5f, 50f),   // North wall
                new Vector3(0, 5f, -50f),  // South wall
                new Vector3(50f, 5f, 0),   // East wall
                new Vector3(-50f, 5f, 0)   // West wall
            };
            
            Vector3[] wallScales = {
                new Vector3(100f, 10f, 1f), // North wall
                new Vector3(100f, 10f, 1f), // South wall
                new Vector3(1f, 10f, 100f), // East wall
                new Vector3(1f, 10f, 100f)  // West wall
            };
            
            for (int i = 0; i < wallPositions.Length; i++)
            {
                GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                wall.name = $"Boundary Wall {i}";
                wall.tag = "Demo";
                wall.transform.position = wallPositions[i];
                wall.transform.localScale = wallScales[i];
                
                // Style walls
                Renderer wallRenderer = wall.GetComponent<Renderer>();
                Material wallMat = new Material(Shader.Find("Standard"));
                wallMat.color = new Color(0.5f, 0.5f, 0.5f);
                wallRenderer.material = wallMat;
            }
        }
        
        private void CreateDemoPlayer()
        {
            // Create player cube
            player = GameObject.CreatePrimitive(PrimitiveType.Cube);
            player.name = "Demo Player";
            player.tag = "Player";
            player.transform.position = playerSpawnPosition;
            player.transform.localScale = new Vector3(1f, 2f, 1f); // Make it look more like a character
            
            // Add physics
            Rigidbody rb = player.AddComponent<Rigidbody>();
            rb.mass = 1f;
            rb.drag = 2f;
            rb.freezeRotation = true;
            
            // Add demo player script
            DemoPlayer demoScript = player.AddComponent<DemoPlayer>();
            demoScript.characterType = startCharacter;
            
            // Style player based on character
            Renderer playerRenderer = player.GetComponent<Renderer>();
            Material playerMat = new Material(Shader.Find("Standard"));
            playerMat.metallic = 0.2f;
            playerMat.smoothness = 0.8f;
            
            switch (startCharacter)
            {
                case CharacterType.Gon:
                    playerMat.color = Color.green;
                    playerMat.SetColor("_EmissionColor", Color.green * 0.3f);
                    break;
                case CharacterType.Killua:
                    playerMat.color = Color.cyan;
                    playerMat.SetColor("_EmissionColor", Color.cyan * 0.3f);
                    break;
                case CharacterType.Kurapika:
                    playerMat.color = Color.blue;
                    playerMat.SetColor("_EmissionColor", Color.blue * 0.3f);
                    break;
                case CharacterType.Leorio:
                    playerMat.color = Color.yellow;
                    playerMat.SetColor("_EmissionColor", Color.yellow * 0.3f);
                    break;
            }
            
            playerMat.EnableKeyword("_EMISSION");
            playerRenderer.material = playerMat;
            
            Debug.Log($"✅ Demo player created as {startCharacter}");
        }
        
        private void CreateDemoEnemies()
        {
            for (int i = 0; i < enemyCount; i++)
            {
                // Create enemy sphere
                GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                enemy.name = $"Demo Enemy {i}";
                enemy.tag = "Enemy";
                
                // Random position
                Vector3 randomPos = new Vector3(
                    Random.Range(-40f, 40f),
                    2f,
                    Random.Range(-40f, 40f)
                );
                enemy.transform.position = randomPos;
                
                // Style enemy
                Renderer enemyRenderer = enemy.GetComponent<Renderer>();
                Material enemyMat = new Material(Shader.Find("Standard"));
                enemyMat.color = Color.red;
                enemyMat.SetColor("_EmissionColor", Color.red * 0.5f);
                enemyMat.EnableKeyword("_EMISSION");
                enemyRenderer.material = enemyMat;
                
                // Add simple AI
                enemy.AddComponent<SimpleEnemyAI>();
                
                // Add physics
                Rigidbody enemyRb = enemy.AddComponent<Rigidbody>();
                enemyRb.mass = 0.8f;
                enemyRb.freezeRotation = true;
            }
            
            Debug.Log($"✅ Created {enemyCount} demo enemies");
        }
        
        private void CreateDemoPlatforms()
        {
            for (int i = 0; i < platformCount; i++)
            {
                // Create platform
                GameObject platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
                platform.name = $"Demo Platform {i}";
                platform.tag = "Platform";
                
                // Random position and size
                Vector3 randomPos = new Vector3(
                    Random.Range(-30f, 30f),
                    Random.Range(3f, 12f),
                    Random.Range(-30f, 30f)
                );
                platform.transform.position = randomPos;
                
                Vector3 randomScale = new Vector3(
                    Random.Range(3f, 8f),
                    Random.Range(0.5f, 1.5f),
                    Random.Range(3f, 8f)
                );
                platform.transform.localScale = randomScale;
                
                // Style platform
                Renderer platformRenderer = platform.GetComponent<Renderer>();
                Material platformMat = new Material(Shader.Find("Standard"));
                platformMat.color = new Color(0.4f, 0.3f, 0.2f); // Brown
                platformRenderer.material = platformMat;
            }
            
            Debug.Log($"✅ Created {platformCount} demo platforms");
        }
        
        private void CreateDemoCollectibles()
        {
            for (int i = 0; i < collectibleCount; i++)
            {
                // Create collectible
                GameObject collectible = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                collectible.name = $"Demo Collectible {i}";
                collectible.tag = "Collectible";
                collectible.transform.localScale = Vector3.one * 0.5f;
                
                // Random position
                Vector3 randomPos = new Vector3(
                    Random.Range(-45f, 45f),
                    Random.Range(1f, 15f),
                    Random.Range(-45f, 45f)
                );
                collectible.transform.position = randomPos;
                
                // Style collectible
                Renderer collectibleRenderer = collectible.GetComponent<Renderer>();
                Material collectibleMat = new Material(Shader.Find("Standard"));
                collectibleMat.color = Color.gold;
                collectibleMat.SetColor("_EmissionColor", Color.yellow);
                collectibleMat.EnableKeyword("_EMISSION");
                collectibleRenderer.material = collectibleMat;
                
                // Make it a trigger
                Collider col = collectible.GetComponent<Collider>();
                col.isTrigger = true;
                
                // Add floating animation
                collectible.AddComponent<FloatingCollectible>();
            }
            
            Debug.Log($"✅ Created {collectibleCount} demo collectibles");
        }
        
        private void SetupDemoCamera()
        {
            playerCamera = Camera.main;
            if (playerCamera == null)
            {
                GameObject cameraGO = new GameObject("Demo Camera");
                playerCamera = cameraGO.AddComponent<Camera>();
                cameraGO.tag = "MainCamera";
                cameraGO.AddComponent<AudioListener>();
            }
            
            // Add camera follow
            CameraFollow cameraFollow = playerCamera.GetComponent<CameraFollow>();
            if (cameraFollow == null)
            {
                cameraFollow = playerCamera.gameObject.AddComponent<CameraFollow>();
            }
            
            if (player != null)
            {
                cameraFollow.target = player.transform;
                cameraFollow.offset = new Vector3(0, 8f, -12f);
                cameraFollow.followSpeed = 3f;
            }
            
            Debug.Log("✅ Demo camera setup complete");
        }
        
        private void CreateDemoUI()
        {
            // Create UI Canvas
            GameObject canvasGO = new GameObject("Demo UI Canvas");
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            
            canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            
            // Create title
            GameObject titleGO = new GameObject("Game Title");
            titleGO.transform.SetParent(canvasGO.transform);
            
            UnityEngine.UI.Text title = titleGO.AddComponent<UnityEngine.UI.Text>();
            title.text = "🎮 HUNTER RUSH: NEN CHRONICLES 🎮";
            title.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            title.fontSize = 24;
            title.fontStyle = FontStyle.Bold;
            title.color = Color.white;
            title.alignment = TextAnchor.MiddleCenter;
            
            RectTransform titleRect = title.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 1f);
            titleRect.anchorMax = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0, -30);
            titleRect.sizeDelta = new Vector2(600, 50);
            
            // Create instructions
            GameObject instructionsGO = new GameObject("Instructions");
            instructionsGO.transform.SetParent(canvasGO.transform);
            
            UnityEngine.UI.Text instructions = instructionsGO.AddComponent<UnityEngine.UI.Text>();
            instructions.text = "🎯 DEMO MODE - READY TO PLAY!\n\n" +
                               "🎮 CONTROLS:\n" +
                               "WASD - Move around\n" +
                               "SPACE - Jump\n" +
                               "MOUSE - Attack enemies\n" +
                               "Q - Special Nen ability\n" +
                               "SHIFT - Dash\n" +
                               "1-4 - Switch characters\n\n" +
                               "⚡ Defeat red enemies to score!\n" +
                               "💎 Collect gold orbs for points!\n" +
                               "🏃‍♂️ Try all 4 Hunter x Hunter characters!";
            
            instructions.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            instructions.fontSize = 16;
            instructions.color = Color.white;
            instructions.alignment = TextAnchor.UpperLeft;
            
            RectTransform instructionsRect = instructions.GetComponent<RectTransform>();
            instructionsRect.anchorMin = new Vector2(0f, 1f);
            instructionsRect.anchorMax = new Vector2(0f, 1f);
            instructionsRect.anchoredPosition = new Vector2(20, -80);
            instructionsRect.sizeDelta = new Vector2(350, 300);
            
            // Add background
            UnityEngine.UI.Image background = instructionsGO.AddComponent<UnityEngine.UI.Image>();
            background.color = new Color(0, 0, 0, 0.7f);
            
            Debug.Log("✅ Demo UI created");
        }
        
        private void ShowInstructions()
        {
            Debug.Log("🎮 HUNTER RUSH: NEN CHRONICLES - DEMO READY!");
            Debug.Log("✅ Use WASD to move, SPACE to jump, MOUSE to attack!");
            Debug.Log("⚡ Press Q for special abilities, SHIFT to dash!");
            Debug.Log("🔄 Press 1-4 to switch between Gon, Killua, Kurapika, Leorio!");
            Debug.Log("🎯 Defeat red enemies and collect gold orbs to score points!");
        }
        
        // Utility methods for runtime scene creation
        public static GameObject CreateCharacterCube(CharacterType character, Vector3 position)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.position = position;
            cube.transform.localScale = new Vector3(1f, 2f, 1f);
            
            // Set character color
            Renderer renderer = cube.GetComponent<Renderer>();
            Material mat = new Material(Shader.Find("Standard"));
            
            switch (character)
            {
                case CharacterType.Gon:
                    mat.color = Color.green;
                    cube.name = "Gon (Demo)";
                    break;
                case CharacterType.Killua:
                    mat.color = Color.cyan;
                    cube.name = "Killua (Demo)";
                    break;
                case CharacterType.Kurapika:
                    mat.color = Color.blue;
                    cube.name = "Kurapika (Demo)";
                    break;
                case CharacterType.Leorio:
                    mat.color = Color.yellow;
                    cube.name = "Leorio (Demo)";
                    break;
            }
            
            renderer.material = mat;
            return cube;
        }
    }
    
    /// <summary>
    /// Simple floating animation for collectibles
    /// </summary>
    public class FloatingCollectible : MonoBehaviour
    {
        private Vector3 startPosition;
        private float time;
        
        void Start()
        {
            startPosition = transform.position;
        }
        
        void Update()
        {
            time += Time.deltaTime * 2f;
            float yOffset = Mathf.Sin(time) * 0.5f;
            transform.position = startPosition + Vector3.up * yOffset;
            transform.Rotate(Vector3.up, 90f * Time.deltaTime);
        }
        
        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                // Simple collection effect
                Debug.Log("Collectible gathered! +50 points");
                
                // Create collection effect
                CreateCollectionEffect();
                
                Destroy(gameObject);
            }
        }
        
        private void CreateCollectionEffect()
        {
            GameObject effectGO = new GameObject("Collection Effect");
            effectGO.transform.position = transform.position;
            
            ParticleSystem ps = effectGO.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startColor = Color.gold;
            main.startSize = 0.3f;
            main.startLifetime = 1f;
            main.startSpeed = 3f;
            main.maxParticles = 20;
            
            var emission = ps.emission;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 20) });
            
            Destroy(effectGO, 2f);
        }
    }
}