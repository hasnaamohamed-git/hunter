using UnityEngine;
using System.Collections.Generic;

namespace HunterRush.Levels
{
    /// <summary>
    /// Individual level segment component that handles enemies, obstacles, and collectibles
    /// </summary>
    public class LevelSegment : MonoBehaviour
    {
        [Header("Segment Info")]
        public LevelSegmentData segmentData;
        public Transform triggerPoint;
        public Transform[] spawnPoints;
        public Transform[] obstaclePoints;
        public Transform[] collectiblePoints;
        
        [Header("Runtime Objects")]
        public List<GameObject> spawnedEnemies = new List<GameObject>();
        public List<GameObject> spawnedObstacles = new List<GameObject>();
        public List<GameObject> spawnedCollectibles = new List<GameObject>();
        
        // State
        private bool isInitialized = false;
        private float difficultyMultiplier = 1f;
        private LocationTheme theme;
        
        public void Initialize(LevelSegmentData data, float difficulty, LocationTheme locationTheme)
        {
            segmentData = data;
            difficultyMultiplier = difficulty;
            theme = locationTheme;
            
            if (!isInitialized)
            {
                SetupTriggerPoint();
                SetupSpawnPoints();
                isInitialized = true;
            }
            
            SpawnContent();
            ApplyTheme();
        }
        
        private void SetupTriggerPoint()
        {
            if (triggerPoint == null)
            {
                GameObject triggerGO = new GameObject("TriggerPoint");
                triggerGO.transform.SetParent(transform);
                triggerGO.transform.localPosition = Vector3.forward * (segmentData.length * 0.8f);
                triggerPoint = triggerGO.transform;
            }
        }
        
        private void SetupSpawnPoints()
        {
            if (spawnPoints == null || spawnPoints.Length == 0)
            {
                // Create default spawn points
                List<Transform> points = new List<Transform>();
                
                for (int i = 0; i < 5; i++)
                {
                    GameObject spawnGO = new GameObject($"SpawnPoint_{i}");
                    spawnGO.transform.SetParent(transform);
                    
                    float x = Random.Range(-10f, 10f);
                    float z = Random.Range(5f, segmentData.length - 5f);
                    spawnGO.transform.localPosition = new Vector3(x, 0, z);
                    
                    points.Add(spawnGO.transform);
                }
                
                spawnPoints = points.ToArray();
            }
        }
        
        private void SpawnContent()
        {
            ClearExistingContent();
            
            switch (segmentData.type)
            {
                case SegmentType.Running:
                    SpawnRunningContent();
                    break;
                case SegmentType.Combat:
                    SpawnCombatContent();
                    break;
                case SegmentType.Exploration:
                    SpawnExplorationContent();
                    break;
                case SegmentType.Boss:
                    SpawnBossContent();
                    break;
            }
        }
        
        private void SpawnRunningContent()
        {
            // Spawn obstacles and collectibles for running segments
            SpawnObstacles(0.3f);
            SpawnCollectibles(0.7f);
            SpawnEnemies(0.2f); // Few enemies in running segments
        }
        
        private void SpawnCombatContent()
        {
            // Spawn many enemies for combat segments
            SpawnEnemies(0.8f);
            SpawnObstacles(0.1f);
            SpawnCollectibles(0.4f);
        }
        
        private void SpawnExplorationContent()
        {
            // Spawn puzzles, hidden items, and secrets
            SpawnCollectibles(0.9f);
            SpawnObstacles(0.5f);
            SpawnEnemies(0.3f);
            SpawnSecrets();
        }
        
        private void SpawnBossContent()
        {
            // Spawn boss enemy and arena setup
            SpawnBossEnemy();
            SpawnCollectibles(0.3f); // Fewer collectibles in boss fights
        }
        
        private void SpawnEnemies(float spawnChance)
        {
            if (segmentData.enemySpawns == null) return;
            
            foreach (EnemySpawnData enemyData in segmentData.enemySpawns)
            {
                if (Random.Range(0f, 1f) <= spawnChance * enemyData.spawnChance)
                {
                    int spawnCount = Random.Range(enemyData.minCount, enemyData.maxCount + 1);
                    spawnCount = Mathf.RoundToInt(spawnCount * difficultyMultiplier);
                    
                    for (int i = 0; i < spawnCount; i++)
                    {
                        Vector3 spawnPosition = GetRandomSpawnPosition() + enemyData.spawnOffset;
                        GameObject enemy = Instantiate(enemyData.enemyPrefab, spawnPosition, Quaternion.identity);
                        enemy.transform.SetParent(transform);
                        
                        // Scale enemy stats with difficulty
                        BaseCharacter enemyChar = enemy.GetComponent<BaseCharacter>();
                        if (enemyChar != null)
                        {
                            enemyChar.maxHealth *= difficultyMultiplier;
                            enemyChar.currentHealth = enemyChar.maxHealth;
                            enemyChar.attackDamage *= difficultyMultiplier;
                        }
                        
                        spawnedEnemies.Add(enemy);
                    }
                }
            }
        }
        
        private void SpawnObstacles(float spawnChance)
        {
            if (segmentData.obstacles == null) return;
            
            foreach (ObstacleData obstacleData in segmentData.obstacles)
            {
                if (Random.Range(0f, 1f) <= spawnChance)
                {
                    Vector3 spawnPosition = transform.position + obstacleData.position;
                    GameObject obstacle = Instantiate(obstacleData.obstaclePrefab, spawnPosition, Quaternion.identity);
                    obstacle.transform.SetParent(transform);
                    
                    // Add destructible component if needed
                    if (obstacleData.canBeDestroyed)
                    {
                        DestructibleObstacle destructible = obstacle.GetComponent<DestructibleObstacle>();
                        if (destructible == null)
                        {
                            destructible = obstacle.AddComponent<DestructibleObstacle>();
                        }
                        destructible.Initialize(obstacleData.destructionReward);
                    }
                    
                    spawnedObstacles.Add(obstacle);
                }
            }
        }
        
        private void SpawnCollectibles(float spawnChance)
        {
            if (segmentData.collectibles == null) return;
            
            foreach (CollectibleData collectibleData in segmentData.collectibles)
            {
                if (Random.Range(0f, 1f) <= spawnChance)
                {
                    Vector3 spawnPosition = transform.position + collectibleData.position;
                    GameObject collectible = Instantiate(collectibleData.collectiblePrefab, spawnPosition, Quaternion.identity);
                    collectible.transform.SetParent(transform);
                    
                    // Setup collectible component
                    Collectible collectibleComponent = collectible.GetComponent<Collectible>();
                    if (collectibleComponent == null)
                    {
                        collectibleComponent = collectible.AddComponent<Collectible>();
                    }
                    collectibleComponent.Initialize(collectibleData.type, collectibleData.value);
                    
                    spawnedCollectibles.Add(collectible);
                }
            }
        }
        
        private void SpawnSecrets()
        {
            // Spawn hidden areas and secret collectibles
            int secretCount = Random.Range(1, 4);
            for (int i = 0; i < secretCount; i++)
            {
                Vector3 secretPosition = GetRandomSpawnPosition();
                secretPosition.y += Random.Range(2f, 8f); // Hidden in elevated positions
                
                // Create secret collectible with high value
                // TODO: Create secret collectible prefab
                Debug.Log($"Secret spawned at {secretPosition}");
            }
        }
        
        private void SpawnBossEnemy()
        {
            if (segmentData.enemySpawns != null && segmentData.enemySpawns.Length > 0)
            {
                EnemySpawnData bossData = segmentData.enemySpawns[0];
                Vector3 bossPosition = transform.position + Vector3.forward * (segmentData.length * 0.5f);
                
                GameObject boss = Instantiate(bossData.enemyPrefab, bossPosition, Quaternion.identity);
                boss.transform.SetParent(transform);
                
                // Scale boss significantly
                BaseCharacter bossChar = boss.GetComponent<BaseCharacter>();
                if (bossChar != null)
                {
                    bossChar.maxHealth *= difficultyMultiplier * 3f; // Bosses are much stronger
                    bossChar.currentHealth = bossChar.maxHealth;
                    bossChar.attackDamage *= difficultyMultiplier * 2f;
                }
                
                spawnedEnemies.Add(boss);
            }
        }
        
        private Vector3 GetRandomSpawnPosition()
        {
            if (spawnPoints != null && spawnPoints.Length > 0)
            {
                Transform randomPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
                return randomPoint.position;
            }
            else
            {
                // Generate random position within segment bounds
                float x = Random.Range(-8f, 8f);
                float z = Random.Range(5f, segmentData.length - 5f);
                return transform.position + new Vector3(x, 0, z);
            }
        }
        
        private void ApplyTheme()
        {
            if (theme == null) return;
            
            // Apply theme-specific visual elements
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                if (renderer.material != null)
                {
                    // Tint materials with theme color
                    renderer.material.color = Color.Lerp(renderer.material.color, theme.ambientColor, 0.3f);
                }
            }
            
            // Spawn theme-specific environment objects
            if (theme.environmentPrefabs != null && theme.environmentPrefabs.Length > 0)
            {
                int envObjectCount = Random.Range(2, 6);
                for (int i = 0; i < envObjectCount; i++)
                {
                    GameObject envPrefab = theme.environmentPrefabs[Random.Range(0, theme.environmentPrefabs.Length)];
                    Vector3 envPosition = GetRandomSpawnPosition();
                    envPosition.x += Random.Range(-15f, 15f); // Spread out more
                    
                    GameObject envObject = Instantiate(envPrefab, envPosition, Quaternion.identity);
                    envObject.transform.SetParent(transform);
                }
            }
        }
        
        private void ClearExistingContent()
        {
            // Clear previous spawned content
            foreach (GameObject enemy in spawnedEnemies)
            {
                if (enemy != null)
                    Destroy(enemy);
            }
            spawnedEnemies.Clear();
            
            foreach (GameObject obstacle in spawnedObstacles)
            {
                if (enemy != null)
                    Destroy(obstacle);
            }
            spawnedObstacles.Clear();
            
            foreach (GameObject collectible in spawnedCollectibles)
            {
                if (collectible != null)
                    Destroy(collectible);
            }
            spawnedCollectibles.Clear();
        }
        
        void OnDestroy()
        {
            ClearExistingContent();
        }
    }
    
    /// <summary>
    /// Destructible obstacle component
    /// </summary>
    public class DestructibleObstacle : MonoBehaviour
    {
        private float rewardValue;
        
        public void Initialize(float reward)
        {
            rewardValue = reward;
        }
        
        public void DestroyObstacle()
        {
            // Give reward to player
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(Mathf.RoundToInt(rewardValue));
            }
            
            // Create destruction effect
            // TODO: Add destruction particle effect
            
            Destroy(gameObject);
        }
        
        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                // Check if player is attacking
                BaseCharacter player = other.GetComponent<BaseCharacter>();
                if (player != null && player.currentState == CharacterState.Attacking)
                {
                    DestroyObstacle();
                }
            }
        }
    }
    
    /// <summary>
    /// Collectible item component
    /// </summary>
    public class Collectible : MonoBehaviour
    {
        private CollectibleType type;
        private float value;
        
        public void Initialize(CollectibleType collectibleType, float collectibleValue)
        {
            type = collectibleType;
            value = collectibleValue;
            
            // Add floating animation
            StartCoroutine(FloatingAnimation());
        }
        
        private IEnumerator FloatingAnimation()
        {
            Vector3 startPosition = transform.position;
            float time = 0f;
            
            while (gameObject != null)
            {
                time += Time.deltaTime * 2f;
                float yOffset = Mathf.Sin(time) * 0.5f;
                transform.position = startPosition + Vector3.up * yOffset;
                transform.Rotate(Vector3.up, 90f * Time.deltaTime);
                yield return null;
            }
        }
        
        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                BaseCharacter player = other.GetComponent<BaseCharacter>();
                if (player != null)
                {
                    CollectItem(player);
                }
            }
        }
        
        private void CollectItem(BaseCharacter player)
        {
            switch (type)
            {
                case CollectibleType.Score:
                    GameManager.Instance?.AddScore(Mathf.RoundToInt(value));
                    break;
                    
                case CollectibleType.Health:
                    player.Heal(value);
                    break;
                    
                case CollectibleType.Nen:
                    player.currentNen = Mathf.Min(player.maxNenCapacity, player.currentNen + value);
                    break;
                    
                case CollectibleType.PowerUp:
                    // Apply temporary power boost
                    StartCoroutine(ApplyPowerUp(player));
                    break;
            }
            
            // Create collection effect
            CreateCollectionEffect();
            
            // Play collection sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayUISound(UISound.Success);
            }
            
            Destroy(gameObject);
        }
        
        private IEnumerator ApplyPowerUp(BaseCharacter player)
        {
            // Temporary stat boost
            float originalAttack = player.attackDamage;
            float originalSpeed = player.runSpeed;
            
            player.attackDamage *= 1.5f;
            player.runSpeed *= 1.3f;
            
            yield return new WaitForSeconds(15f);
            
            player.attackDamage = originalAttack;
            player.runSpeed = originalSpeed;
        }
        
        private void CreateCollectionEffect()
        {
            // TODO: Create collection particle effect
            Debug.Log($"Collected {type}: {value}");
        }
    }
}