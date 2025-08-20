using UnityEngine;
using System.Collections;

namespace HunterRush.GameModes
{
    /// <summary>
    /// Endless Run game mode - infinite runner with increasing difficulty
    /// </summary>
    public class EndlessRunMode : MonoBehaviour
    {
        [Header("Endless Run Settings")]
        public float baseSpeed = 10f;
        public float speedIncreaseRate = 0.5f;
        public float maxSpeed = 25f;
        public float scoreMultiplier = 1f;
        public float distanceScoreRate = 1f; // Points per unit distance
        
        [Header("Power-ups")]
        public float powerUpSpawnRate = 0.1f;
        public GameObject[] powerUpPrefabs;
        
        [Header("Difficulty Scaling")]
        public float enemySpawnRateIncrease = 0.02f;
        public float obstacleSpawnRateIncrease = 0.01f;
        
        // State
        private float currentSpeed;
        private float distanceTraveled = 0f;
        private float lastDistanceScore = 0f;
        private bool isRunning = false;
        private Transform player;
        private LevelGenerator levelGenerator;
        
        // Score tracking
        private int enemiesDefeated = 0;
        private int obstaclesDestroyed = 0;
        private int powerUpsCollected = 0;
        
        void Start()
        {
            Initialize();
        }
        
        void Update()
        {
            if (isRunning)
            {
                UpdateEndlessRun();
                UpdateScore();
            }
        }
        
        private void Initialize()
        {
            player = FindObjectOfType<BaseCharacter>()?.transform;
            levelGenerator = FindObjectOfType<LevelGenerator>();
            
            currentSpeed = baseSpeed;
            isRunning = true;
            
            // Set initial game state
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ChangeGameState(GameState.Playing);
            }
            
            // Show HUD
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowHUD();
            }
            
            // Start background music
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayBattleMusic(false);
            }
        }
        
        private void UpdateEndlessRun()
        {
            if (player == null) return;
            
            // Update distance traveled
            distanceTraveled = player.position.z;
            
            // Increase speed over time
            float targetSpeed = Mathf.Min(maxSpeed, baseSpeed + (distanceTraveled * speedIncreaseRate / 100f));
            currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * 0.5f);
            
            // Apply speed to player movement
            MovementController movement = player.GetComponent<MovementController>();
            if (movement != null)
            {
                movement.baseRunSpeed = currentSpeed;
            }
            
            // Update level generator difficulty
            if (levelGenerator != null)
            {
                levelGenerator.difficultyIncreaseRate = 0.1f + (distanceTraveled * 0.001f);
            }
            
            // Spawn power-ups occasionally
            if (Random.Range(0f, 1f) < powerUpSpawnRate * Time.deltaTime)
            {
                SpawnPowerUp();
            }
        }
        
        private void UpdateScore()
        {
            // Award points for distance traveled
            float distanceSinceLastScore = distanceTraveled - lastDistanceScore;
            if (distanceSinceLastScore >= 1f)
            {
                int distancePoints = Mathf.FloorToInt(distanceSinceLastScore * distanceScoreRate);
                GameManager.Instance?.AddScore(distancePoints);
                lastDistanceScore = distanceTraveled;
            }
        }
        
        private void SpawnPowerUp()
        {
            if (powerUpPrefabs.Length == 0 || player == null) return;
            
            GameObject powerUpPrefab = powerUpPrefabs[Random.Range(0, powerUpPrefabs.Length)];
            Vector3 spawnPosition = player.position + Vector3.forward * 20f + Vector3.right * Random.Range(-5f, 5f);
            
            GameObject powerUp = Instantiate(powerUpPrefab, spawnPosition, Quaternion.identity);
            
            // Auto-destroy if not collected
            Destroy(powerUp, 10f);
        }
        
        // Event handlers
        public void OnEnemyDefeated(BaseCharacter enemy)
        {
            enemiesDefeated++;
            
            // Award points based on enemy type and current speed
            int points = Mathf.RoundToInt(50f * scoreMultiplier * (currentSpeed / baseSpeed));
            GameManager.Instance?.AddScore(points);
            
            // Play victory voice line occasionally
            if (Random.Range(0f, 1f) < 0.3f && AudioManager.Instance != null)
            {
                CharacterType playerChar = GameManager.Instance?.selectedCharacter ?? CharacterType.Gon;
                AudioManager.Instance.PlayVoiceLine(playerChar, VoiceLineType.Victory);
            }
        }
        
        public void OnObstacleDestroyed()
        {
            obstaclesDestroyed++;
            
            // Award points for destruction
            int points = Mathf.RoundToInt(25f * scoreMultiplier);
            GameManager.Instance?.AddScore(points);
        }
        
        public void OnPowerUpCollected(PowerUpType type)
        {
            powerUpsCollected++;
            
            // Award points for collection
            int points = Mathf.RoundToInt(100f * scoreMultiplier);
            GameManager.Instance?.AddScore(points);
            
            // Apply power-up effect
            ApplyPowerUp(type);
        }
        
        private void ApplyPowerUp(PowerUpType type)
        {
            if (player == null) return;
            
            BaseCharacter character = player.GetComponent<BaseCharacter>();
            if (character == null) return;
            
            switch (type)
            {
                case PowerUpType.SpeedBoost:
                    StartCoroutine(SpeedBoostCoroutine(character));
                    break;
                case PowerUpType.InvincibilityBoost:
                    StartCoroutine(InvincibilityCoroutine(character));
                    break;
                case PowerUpType.NenBoost:
                    character.currentNen = character.maxNenCapacity;
                    break;
                case PowerUpType.HealthBoost:
                    character.Heal(character.maxHealth * 0.5f);
                    break;
                case PowerUpType.ScoreMultiplier:
                    StartCoroutine(ScoreMultiplierCoroutine());
                    break;
            }
        }
        
        private IEnumerator SpeedBoostCoroutine(BaseCharacter character)
        {
            float originalSpeed = character.runSpeed;
            character.runSpeed *= 1.5f;
            
            // Visual effect
            if (VisualEffectsManager.Instance != null)
            {
                VisualEffectsManager.Instance.ActivateSpeedLines(10f);
            }
            
            yield return new WaitForSeconds(10f);
            
            character.runSpeed = originalSpeed;
        }
        
        private IEnumerator InvincibilityCoroutine(BaseCharacter character)
        {
            // Make player invincible
            Physics.IgnoreLayerCollision(character.gameObject.layer, LayerMask.NameToLayer("Enemy"), true);
            
            // Visual feedback
            Renderer[] renderers = character.GetComponentsInChildren<Renderer>();
            StartCoroutine(FlashEffect(renderers, 8f));
            
            yield return new WaitForSeconds(8f);
            
            // Remove invincibility
            Physics.IgnoreLayerCollision(character.gameObject.layer, LayerMask.NameToLayer("Enemy"), false);
        }
        
        private IEnumerator FlashEffect(Renderer[] renderers, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                bool visible = (Mathf.FloorToInt(elapsed * 10f) % 2) == 0;
                
                foreach (Renderer renderer in renderers)
                {
                    renderer.enabled = visible;
                }
                
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            // Ensure visible at end
            foreach (Renderer renderer in renderers)
            {
                renderer.enabled = true;
            }
        }
        
        private IEnumerator ScoreMultiplierCoroutine()
        {
            float originalMultiplier = scoreMultiplier;
            scoreMultiplier *= 2f;
            
            yield return new WaitForSeconds(15f);
            
            scoreMultiplier = originalMultiplier;
        }
        
        public void EndRun()
        {
            isRunning = false;
            
            // Calculate final score bonus
            int finalBonus = CalculateFinalBonus();
            GameManager.Instance?.AddScore(finalBonus);
            
            // Trigger game over
            GameManager.Instance?.GameOver();
        }
        
        private int CalculateFinalBonus()
        {
            int bonus = 0;
            
            // Distance bonus
            bonus += Mathf.RoundToInt(distanceTraveled * 2f);
            
            // Performance bonuses
            bonus += enemiesDefeated * 50;
            bonus += obstaclesDestroyed * 25;
            bonus += powerUpsCollected * 100;
            
            // Survival bonus based on final speed
            bonus += Mathf.RoundToInt((currentSpeed / baseSpeed) * 500f);
            
            return bonus;
        }
        
        // Public getters for UI display
        public float GetDistanceTraveled() => distanceTraveled;
        public float GetCurrentSpeed() => currentSpeed;
        public int GetEnemiesDefeated() => enemiesDefeated;
        public int GetObstaclesDestroyed() => obstaclesDestroyed;
        public int GetPowerUpsCollected() => powerUpsCollected;
    }
    
    public enum PowerUpType
    {
        SpeedBoost,
        InvincibilityBoost,
        NenBoost,
        HealthBoost,
        ScoreMultiplier
    }
}