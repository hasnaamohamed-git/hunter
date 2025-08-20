using UnityEngine;
using HunterRush.Characters;

namespace HunterRush.Demo
{
    /// <summary>
    /// Simple demo player for immediate gameplay testing
    /// Works with basic cube models and provides instant playability
    /// </summary>
    public class DemoPlayer : MonoBehaviour
    {
        [Header("Demo Settings")]
        public CharacterType characterType = CharacterType.Gon;
        public Material[] characterMaterials; // Different colors for each character
        
        [Header("Movement")]
        public float moveSpeed = 10f;
        public float jumpPower = 15f;
        public float dashDistance = 5f;
        
        [Header("Combat")]
        public float attackRange = 3f;
        public float attackDamage = 25f;
        public GameObject attackEffectPrefab;
        
        [Header("Visual Effects")]
        public ParticleSystem auraEffect;
        public GameObject jumpEffect;
        public GameObject attackEffect;
        
        // Components
        private Rigidbody rb;
        private Renderer renderer;
        private Animator animator;
        private AudioSource audioSource;
        
        // State
        private bool isGrounded = true;
        private bool canAttack = true;
        private bool canDash = true;
        private Vector3 lastPosition;
        private float currentSpeed;
        
        // Demo stats
        private float health = 100f;
        private float nen = 100f;
        private int score = 0;
        
        void Start()
        {
            InitializeDemoPlayer();
            SetupVisuals();
            SetupUI();
        }
        
        void Update()
        {
            HandleInput();
            UpdateMovement();
            UpdateUI();
            CheckGrounded();
        }
        
        private void InitializeDemoPlayer()
        {
            // Get components
            rb = GetComponent<Rigidbody>();
            renderer = GetComponent<Renderer>();
            audioSource = GetComponent<AudioSource>();
            
            // Add components if missing
            if (rb == null)
                rb = gameObject.AddComponent<Rigidbody>();
            
            if (renderer == null)
                renderer = gameObject.AddComponent<MeshRenderer>();
            
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();
            
            // Setup physics
            rb.freezeRotation = true;
            
            // Set player tag
            gameObject.tag = "Player";
            gameObject.layer = LayerMask.NameToLayer("Default");
            
            lastPosition = transform.position;
        }
        
        private void SetupVisuals()
        {
            // Create basic cube if no mesh
            MeshFilter meshFilter = GetComponent<MeshFilter>();
            if (meshFilter == null)
            {
                meshFilter = gameObject.AddComponent<MeshFilter>();
                meshFilter.mesh = CreateCubeMesh();
            }
            
            // Set character-specific material
            if (characterMaterials != null && characterMaterials.Length > (int)characterType)
            {
                renderer.material = characterMaterials[(int)characterType];
            }
            else
            {
                // Create default material with character color
                Material mat = new Material(Shader.Find("Standard"));
                switch (characterType)
                {
                    case CharacterType.Gon:
                        mat.color = Color.green;
                        break;
                    case CharacterType.Killua:
                        mat.color = Color.cyan;
                        break;
                    case CharacterType.Kurapika:
                        mat.color = Color.blue;
                        break;
                    case CharacterType.Leorio:
                        mat.color = Color.yellow;
                        break;
                }
                renderer.material = mat;
            }
            
            // Setup aura effect
            if (auraEffect == null)
            {
                GameObject auraGO = new GameObject("Aura Effect");
                auraGO.transform.SetParent(transform);
                auraGO.transform.localPosition = Vector3.zero;
                auraEffect = auraGO.AddComponent<ParticleSystem>();
                
                var main = auraEffect.main;
                main.startColor = renderer.material.color;
                main.startSize = 0.5f;
                main.startLifetime = 2f;
                main.simulationSpace = ParticleSystemSimulationSpace.World;
                
                var emission = auraEffect.emission;
                emission.rateOverTime = 10f;
                
                var shape = auraEffect.shape;
                shape.shapeType = ParticleSystemShapeType.Sphere;
                shape.radius = 1.5f;
            }
        }
        
        private void SetupUI()
        {
            // Create simple on-screen UI
            GameObject uiCanvas = GameObject.Find("DemoUI");
            if (uiCanvas == null)
            {
                uiCanvas = new GameObject("DemoUI");
                Canvas canvas = uiCanvas.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                uiCanvas.AddComponent<UnityEngine.UI.CanvasScaler>();
                uiCanvas.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            }
        }
        
        private Mesh CreateCubeMesh()
        {
            // Create a simple cube mesh for the character
            Mesh mesh = new Mesh();
            
            Vector3[] vertices = {
                new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(0.5f, -0.5f, -0.5f),
                new Vector3(0.5f, 0.5f, -0.5f), new Vector3(-0.5f, 0.5f, -0.5f),
                new Vector3(-0.5f, 0.5f, 0.5f), new Vector3(0.5f, 0.5f, 0.5f),
                new Vector3(0.5f, -0.5f, 0.5f), new Vector3(-0.5f, -0.5f, 0.5f)
            };
            
            int[] triangles = {
                0, 2, 1, 0, 3, 2, 2, 3, 4, 2, 4, 5, 1, 2, 5, 1, 5, 6,
                0, 7, 4, 0, 4, 3, 5, 4, 7, 5, 7, 6, 0, 6, 7, 0, 1, 6
            };
            
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            
            return mesh;
        }
        
        private void HandleInput()
        {
            // Movement input
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            Vector3 moveDirection = new Vector3(horizontal, 0, vertical).normalized;
            
            // Apply movement
            if (moveDirection.magnitude > 0.1f)
            {
                rb.velocity = new Vector3(moveDirection.x * moveSpeed, rb.velocity.y, moveDirection.z * moveSpeed);
                
                // Face movement direction
                transform.rotation = Quaternion.LookRotation(moveDirection);
                
                // Show movement aura
                if (auraEffect != null && !auraEffect.isPlaying)
                    auraEffect.Play();
            }
            else
            {
                // Stop horizontal movement
                rb.velocity = new Vector3(0, rb.velocity.y, 0);
                
                // Hide aura when not moving
                if (auraEffect != null && auraEffect.isPlaying)
                    auraEffect.Stop();
            }
            
            // Jump
            if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
            {
                PerformJump();
            }
            
            // Attack
            if (Input.GetMouseButtonDown(0) && canAttack)
            {
                PerformAttack();
            }
            
            // Special ability
            if (Input.GetKeyDown(KeyCode.Q))
            {
                PerformSpecialAbility();
            }
            
            // Dash
            if (Input.GetKeyDown(KeyCode.LeftShift) && canDash)
            {
                PerformDash();
            }
            
            // Character switching (for demo)
            if (Input.GetKeyDown(KeyCode.Alpha1))
                SwitchCharacter(CharacterType.Gon);
            else if (Input.GetKeyDown(KeyCode.Alpha2))
                SwitchCharacter(CharacterType.Killua);
            else if (Input.GetKeyDown(KeyCode.Alpha3))
                SwitchCharacter(CharacterType.Kurapika);
            else if (Input.GetKeyDown(KeyCode.Alpha4))
                SwitchCharacter(CharacterType.Leorio);
        }
        
        private void UpdateMovement()
        {
            // Calculate current speed
            Vector3 horizontalVelocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            currentSpeed = horizontalVelocity.magnitude;
            
            // Update position tracking
            lastPosition = transform.position;
        }
        
        private void CheckGrounded()
        {
            // Simple ground check
            isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.1f);
        }
        
        private void PerformJump()
        {
            rb.velocity = new Vector3(rb.velocity.x, jumpPower, rb.velocity.z);
            
            // Create jump effect
            CreateJumpEffect();
            
            // Play jump sound
            PlaySound("Jump");
            
            Debug.Log($"{characterType} jumped!");
        }
        
        private void PerformAttack()
        {
            canAttack = false;
            
            // Create attack effect
            CreateAttackEffect();
            
            // Check for enemies in range
            Collider[] enemies = Physics.OverlapSphere(transform.position + transform.forward * 2f, attackRange);
            foreach (Collider enemy in enemies)
            {
                if (enemy.CompareTag("Enemy"))
                {
                    DamageEnemy(enemy.gameObject);
                }
            }
            
            // Play attack sound
            PlaySound("Attack");
            
            // Attack cooldown
            Invoke("ResetAttack", 0.5f);
            
            Debug.Log($"{characterType} attacked!");
        }
        
        private void PerformSpecialAbility()
        {
            if (nen >= 20f)
            {
                nen -= 20f;
                
                switch (characterType)
                {
                    case CharacterType.Gon:
                        PerformJajanken();
                        break;
                    case CharacterType.Killua:
                        PerformLightning();
                        break;
                    case CharacterType.Kurapika:
                        PerformChainAttack();
                        break;
                    case CharacterType.Leorio:
                        PerformRemotePunch();
                        break;
                }
                
                Debug.Log($"{characterType} used special ability!");
            }
        }
        
        private void PerformDash()
        {
            if (nen >= 10f)
            {
                nen -= 10f;
                canDash = false;
                
                // Dash forward
                Vector3 dashDirection = transform.forward;
                rb.AddForce(dashDirection * dashDistance, ForceMode.VelocityChange);
                
                // Create dash effect
                CreateDashEffect();
                
                // Dash cooldown
                Invoke("ResetDash", 1f);
                
                Debug.Log($"{characterType} dashed!");
            }
        }
        
        private void PerformJajanken()
        {
            // Gon's Rock attack
            CreateExplosionEffect(transform.position + transform.forward * 2f, Color.green);
            
            // Damage enemies in front
            Collider[] enemies = Physics.OverlapSphere(transform.position + transform.forward * 3f, 3f);
            foreach (Collider enemy in enemies)
            {
                if (enemy.CompareTag("Enemy"))
                {
                    DamageEnemy(enemy.gameObject, attackDamage * 2f);
                }
            }
        }
        
        private void PerformLightning()
        {
            // Killua's lightning attack
            CreateExplosionEffect(transform.position, Color.cyan);
            
            // Damage all nearby enemies
            Collider[] enemies = Physics.OverlapSphere(transform.position, 5f);
            foreach (Collider enemy in enemies)
            {
                if (enemy.CompareTag("Enemy"))
                {
                    DamageEnemy(enemy.gameObject, attackDamage * 1.5f);
                }
            }
        }
        
        private void PerformChainAttack()
        {
            // Kurapika's chain attack
            CreateExplosionEffect(transform.position + transform.forward * 4f, Color.blue);
            
            // Line attack
            RaycastHit[] hits = Physics.RaycastAll(transform.position, transform.forward, 8f);
            foreach (RaycastHit hit in hits)
            {
                if (hit.collider.CompareTag("Enemy"))
                {
                    DamageEnemy(hit.collider.gameObject, attackDamage * 1.8f);
                }
            }
        }
        
        private void PerformRemotePunch()
        {
            // Leorio's remote punch
            Vector3 targetPos = transform.position + transform.forward * 10f;
            CreateExplosionEffect(targetPos, Color.yellow);
            
            // Heal self
            health = Mathf.Min(100f, health + 20f);
            
            // Damage enemies at target location
            Collider[] enemies = Physics.OverlapSphere(targetPos, 3f);
            foreach (Collider enemy in enemies)
            {
                if (enemy.CompareTag("Enemy"))
                {
                    DamageEnemy(enemy.gameObject, attackDamage * 2f);
                }
            }
        }
        
        private void SwitchCharacter(CharacterType newCharacter)
        {
            characterType = newCharacter;
            SetupVisuals();
            
            Debug.Log($"Switched to {characterType}!");
        }
        
        private void CreateJumpEffect()
        {
            CreateExplosionEffect(transform.position, Color.white);
        }
        
        private void CreateAttackEffect()
        {
            Vector3 effectPos = transform.position + transform.forward * 2f;
            CreateExplosionEffect(effectPos, renderer.material.color);
        }
        
        private void CreateDashEffect()
        {
            CreateExplosionEffect(transform.position, renderer.material.color);
        }
        
        private void CreateExplosionEffect(Vector3 position, Color color)
        {
            // Create simple particle explosion
            GameObject effectGO = new GameObject("Effect");
            effectGO.transform.position = position;
            
            ParticleSystem ps = effectGO.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startColor = color;
            main.startSize = 1f;
            main.startLifetime = 1f;
            main.startSpeed = 5f;
            main.maxParticles = 50;
            
            var emission = ps.emission;
            emission.SetBursts(new ParticleSystem.Burst[]
            {
                new ParticleSystem.Burst(0f, 50)
            });
            
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.5f;
            
            // Auto destroy
            Destroy(effectGO, 2f);
        }
        
        private void DamageEnemy(GameObject enemy, float damage = 0f)
        {
            if (damage == 0f) damage = attackDamage;
            
            // Simple enemy destruction
            Destroy(enemy);
            
            // Add score
            score += 100;
            
            // Create hit effect
            CreateExplosionEffect(enemy.transform.position, Color.red);
            
            Debug.Log($"Enemy defeated! Score: {score}");
        }
        
        private void PlaySound(string soundType)
        {
            // Simple sound feedback
            if (audioSource != null)
            {
                // Create a simple beep sound
                audioSource.pitch = Random.Range(0.8f, 1.2f);
                audioSource.PlayOneShot(CreateBeepSound());
            }
        }
        
        private AudioClip CreateBeepSound()
        {
            // Create a simple procedural beep sound
            int sampleRate = 44100;
            float frequency = 440f;
            float duration = 0.1f;
            int samples = Mathf.RoundToInt(sampleRate * duration);
            
            AudioClip clip = AudioClip.Create("Beep", samples, 1, sampleRate, false);
            float[] data = new float[samples];
            
            for (int i = 0; i < samples; i++)
            {
                data[i] = Mathf.Sin(2f * Mathf.PI * frequency * i / sampleRate) * 0.3f;
            }
            
            clip.SetData(data, 0);
            return clip;
        }
        
        private void UpdateUI()
        {
            // Simple debug UI
            string uiText = $"Character: {characterType}\n";
            uiText += $"Health: {health:F0}/100\n";
            uiText += $"Nen: {nen:F0}/100\n";
            uiText += $"Score: {score}\n";
            uiText += $"Speed: {currentSpeed:F1}\n\n";
            uiText += "Controls:\n";
            uiText += "WASD - Move\n";
            uiText += "Space - Jump\n";
            uiText += "Mouse - Attack\n";
            uiText += "Q - Special Ability\n";
            uiText += "Shift - Dash\n";
            uiText += "1-4 - Switch Character";
            
            // This would be displayed on screen in a full implementation
        }
        
        private void ResetAttack()
        {
            canAttack = true;
        }
        
        private void ResetDash()
        {
            canDash = true;
        }
        
        void OnGUI()
        {
            // Simple on-screen display
            GUI.Box(new Rect(10, 10, 200, 200), "");
            
            GUILayout.BeginArea(new Rect(15, 15, 190, 190));
            
            GUILayout.Label($"Character: {characterType}", new GUIStyle(GUI.skin.label) { fontSize = 14, fontStyle = FontStyle.Bold });
            GUILayout.Label($"Health: {health:F0}/100");
            GUILayout.Label($"Nen: {nen:F0}/100");
            GUILayout.Label($"Score: {score}");
            GUILayout.Label($"Speed: {currentSpeed:F1}");
            
            GUILayout.Space(10);
            GUILayout.Label("Controls:", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold });
            GUILayout.Label("WASD - Move");
            GUILayout.Label("Space - Jump");
            GUILayout.Label("Mouse - Attack");
            GUILayout.Label("Q - Special");
            GUILayout.Label("Shift - Dash");
            GUILayout.Label("1-4 - Switch Char");
            
            GUILayout.EndArea();
        }
        
        void OnDrawGizmosSelected()
        {
            // Draw attack range
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position + transform.forward * 2f, attackRange);
        }
    }
}