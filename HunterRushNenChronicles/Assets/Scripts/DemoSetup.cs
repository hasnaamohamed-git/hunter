using UnityEngine;

namespace HunterRush.Demo
{
    /// <summary>
    /// Automatically sets up a playable demo scene when the game starts
    /// Creates player, ground, enemies, and camera for immediate gameplay
    /// </summary>
    public class DemoSetup : MonoBehaviour
    {
        [Header("Demo Configuration")]
        public bool autoSetupOnStart = true;
        public CharacterType defaultCharacter = CharacterType.Gon;
        
        [Header("Level Setup")]
        public Vector3 levelSize = new Vector3(50f, 1f, 50f);
        public int enemyCount = 10;
        public int platformCount = 5;
        
        void Start()
        {
            if (autoSetupOnStart)
            {
                SetupDemoScene();
            }
        }
        
        [ContextMenu("Setup Demo Scene")]
        public void SetupDemoScene()
        {
            Debug.Log("Setting up Hunter Rush demo scene...");
            
            CreateGround();
            CreatePlayer();
            CreateEnemies();
            CreatePlatforms();
            SetupCamera();
            CreateUI();
            
            Debug.Log("Demo scene setup complete! Use WASD to move, Space to jump, Mouse to attack!");
        }
        
        private void CreateGround()
        {
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(levelSize.x / 10f, 1f, levelSize.z / 10f);
            
            // Set ground material
            Renderer groundRenderer = ground.GetComponent<Renderer>();
            Material groundMat = new Material(Shader.Find("Standard"));
            groundMat.color = new Color(0.3f, 0.7f, 0.3f); // Green ground
            groundRenderer.material = groundMat;
            
            ground.tag = "Ground";
        }
        
        private void CreatePlayer()
        {
            // Check if player already exists
            GameObject existingPlayer = GameObject.FindGameObjectWithTag("Player");
            if (existingPlayer != null)
            {
                DestroyImmediate(existingPlayer);
            }
            
            // Create player cube
            GameObject player = GameObject.CreatePrimitive(PrimitiveType.Cube);
            player.name = "Player";
            player.transform.position = new Vector3(0, 2f, 0);
            player.tag = "Player";
            
            // Add Rigidbody
            Rigidbody rb = player.AddComponent<Rigidbody>();
            rb.mass = 1f;
            rb.drag = 2f;
            rb.freezeRotation = true;
            
            // Add demo player script
            DemoPlayer demoPlayer = player.AddComponent<DemoPlayer>();
            demoPlayer.characterType = defaultCharacter;
            
            // Set player material based on character
            Renderer playerRenderer = player.GetComponent<Renderer>();
            Material playerMat = new Material(Shader.Find("Standard"));
            
            switch (defaultCharacter)
            {
                case CharacterType.Gon:
                    playerMat.color = Color.green;
                    break;
                case CharacterType.Killua:
                    playerMat.color = Color.cyan;
                    break;
                case CharacterType.Kurapika:
                    playerMat.color = Color.blue;
                    break;
                case CharacterType.Leorio:
                    playerMat.color = Color.yellow;
                    break;
            }
            
            playerRenderer.material = playerMat;
            
            Debug.Log($"Player created as {defaultCharacter}");
        }
        
        private void CreateEnemies()
        {
            for (int i = 0; i < enemyCount; i++)
            {
                // Create enemy cube
                GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Cube);
                enemy.name = $"Enemy_{i}";
                
                // Random position around the level
                Vector3 randomPos = new Vector3(
                    Random.Range(-levelSize.x * 0.4f, levelSize.x * 0.4f),
                    2f,
                    Random.Range(-levelSize.z * 0.4f, levelSize.z * 0.4f)
                );
                enemy.transform.position = randomPos;
                enemy.tag = "Enemy";
                
                // Set enemy material
                Renderer enemyRenderer = enemy.GetComponent<Renderer>();
                Material enemyMat = new Material(Shader.Find("Standard"));
                enemyMat.color = Color.red;
                enemyRenderer.material = enemyMat;
                
                // Add simple AI
                SimpleEnemyAI ai = enemy.AddComponent<SimpleEnemyAI>();
            }
            
            Debug.Log($"Created {enemyCount} enemies");
        }
        
        private void CreatePlatforms()
        {
            for (int i = 0; i < platformCount; i++)
            {
                // Create platform
                GameObject platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
                platform.name = $"Platform_{i}";
                
                // Random position and size
                Vector3 randomPos = new Vector3(
                    Random.Range(-levelSize.x * 0.3f, levelSize.x * 0.3f),
                    Random.Range(3f, 8f),
                    Random.Range(-levelSize.z * 0.3f, levelSize.z * 0.3f)
                );
                platform.transform.position = randomPos;
                
                Vector3 randomScale = new Vector3(
                    Random.Range(2f, 6f),
                    Random.Range(0.5f, 1f),
                    Random.Range(2f, 6f)
                );
                platform.transform.localScale = randomScale;
                
                // Set platform material
                Renderer platformRenderer = platform.GetComponent<Renderer>();
                Material platformMat = new Material(Shader.Find("Standard"));
                platformMat.color = new Color(0.6f, 0.4f, 0.2f); // Brown platforms
                platformRenderer.material = platformMat;
                
                platform.tag = "Platform";
            }
            
            Debug.Log($"Created {platformCount} platforms");
        }
        
        private void SetupCamera()
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                GameObject cameraGO = new GameObject("Main Camera");
                mainCamera = cameraGO.AddComponent<Camera>();
                cameraGO.tag = "MainCamera";
                cameraGO.AddComponent<AudioListener>();
            }
            
            // Add camera follow script
            CameraFollow cameraFollow = mainCamera.GetComponent<CameraFollow>();
            if (cameraFollow == null)
            {
                cameraFollow = mainCamera.gameObject.AddComponent<CameraFollow>();
            }
            
            // Set target to player
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                cameraFollow.target = player.transform;
            }
        }
        
        private void CreateUI()
        {
            // Create canvas for UI
            GameObject canvasGO = new GameObject("Demo Canvas");
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            
            // Add instructions text
            GameObject instructionsGO = new GameObject("Instructions");
            instructionsGO.transform.SetParent(canvasGO.transform);
            
            UnityEngine.UI.Text instructions = instructionsGO.AddComponent<UnityEngine.UI.Text>();
            instructions.text = "🎮 HUNTER RUSH: NEN CHRONICLES - DEMO\n\nControls:\nWASD - Move\nSpace - Jump\nMouse - Attack\nQ - Special Ability\nShift - Dash\n1-4 - Switch Character\n\nDefeat the red enemies to score points!";
            instructions.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            instructions.fontSize = 14;
            instructions.color = Color.white;
            
            RectTransform instructionsRect = instructions.GetComponent<RectTransform>();
            instructionsRect.anchorMin = new Vector2(0, 1);
            instructionsRect.anchorMax = new Vector2(0, 1);
            instructionsRect.anchoredPosition = new Vector2(10, -10);
            instructionsRect.sizeDelta = new Vector2(400, 200);
        }
    }
    
    /// <summary>
    /// Simple camera follow script for the demo
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        public Transform target;
        public Vector3 offset = new Vector3(0, 5, -8);
        public float followSpeed = 5f;
        
        void LateUpdate()
        {
            if (target != null)
            {
                Vector3 targetPosition = target.position + offset;
                transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
                transform.LookAt(target.position + Vector3.up * 2f);
            }
        }
    }
    
    /// <summary>
    /// Simple enemy AI for demo purposes
    /// </summary>
    public class SimpleEnemyAI : MonoBehaviour
    {
        public float moveSpeed = 3f;
        public float detectionRange = 8f;
        
        private Transform player;
        private Rigidbody rb;
        
        void Start()
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
            rb = gameObject.AddComponent<Rigidbody>();
            rb.mass = 1f;
            rb.freezeRotation = true;
            
            // Add simple floating animation
            StartCoroutine(FloatingAnimation());
        }
        
        void Update()
        {
            if (player != null)
            {
                float distanceToPlayer = Vector3.Distance(transform.position, player.position);
                
                if (distanceToPlayer <= detectionRange)
                {
                    // Move towards player
                    Vector3 direction = (player.position - transform.position).normalized;
                    direction.y = 0; // Keep on ground level
                    
                    rb.velocity = new Vector3(direction.x * moveSpeed, rb.velocity.y, direction.z * moveSpeed);
                    
                    // Face player
                    if (direction.magnitude > 0.1f)
                    {
                        transform.rotation = Quaternion.LookRotation(direction);
                    }
                }
            }
        }
        
        private System.Collections.IEnumerator FloatingAnimation()
        {
            Vector3 startPos = transform.position;
            float time = 0f;
            
            while (gameObject != null)
            {
                time += Time.deltaTime * 2f;
                float yOffset = Mathf.Sin(time) * 0.3f;
                transform.position = new Vector3(transform.position.x, startPos.y + yOffset, transform.position.z);
                yield return null;
            }
        }
        
        void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                // Simple damage to player
                Debug.Log("Player hit by enemy!");
            }
        }
    }
}