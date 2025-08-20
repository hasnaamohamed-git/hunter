using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace HunterRush.GameModes
{
    /// <summary>
    /// Boss Rush mode - fight all major Hunter x Hunter villains consecutively
    /// </summary>
    public class BossRushMode : MonoBehaviour
    {
        [Header("Boss Rush Settings")]
        public BossData[] bosses;
        public int currentBossIndex = 0;
        public float timeBetweenBosses = 3f;
        public float healthRestorePercent = 0.3f;
        public float nenRestorePercent = 0.5f;
        
        [Header("Arena Settings")]
        public Transform[] arenaSpawnPoints;
        public GameObject[] arenaPrefabs;
        
        [Header("Rewards")]
        public int bossDefeatScore = 1000;
        public float scoreMultiplier = 1f;
        public int perfectBonusMultiplier = 2;
        
        // State
        private GameObject currentBoss;
        private GameObject currentArena;
        private BaseCharacter player;
        private bool bossRushActive = false;
        private bool isPerfectRun = true; // No damage taken
        private float totalTime = 0f;
        private int bossesDefeated = 0;
        
        // Statistics
        private float[] bossFightTimes;
        private bool[] perfectFights;
        
        void Start()
        {
            Initialize();
        }
        
        void Update()
        {
            if (bossRushActive)
            {
                totalTime += Time.deltaTime;
                
                // Check if current boss is defeated
                if (currentBoss == null && currentBossIndex < bosses.Length)
                {
                    OnBossDefeated();
                }
            }
        }
        
        private void Initialize()
        {
            player = FindObjectOfType<BaseCharacter>();
            bossFightTimes = new float[bosses.Length];
            perfectFights = new bool[bosses.Length];
            
            // Initialize arrays
            for (int i = 0; i < perfectFights.Length; i++)
            {
                perfectFights[i] = true;
            }
            
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ChangeGameState(GameState.Playing);
            }
            
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowHUD();
            }
            
            StartBossRush();
        }
        
        private void StartBossRush()
        {
            bossRushActive = true;
            currentBossIndex = 0;
            
            // Setup player health monitoring for perfect run tracking
            if (player != null)
            {
                player.OnHealthChanged += OnPlayerHealthChanged;
            }
            
            SpawnNextBoss();
        }
        
        private void SpawnNextBoss()
        {
            if (currentBossIndex >= bosses.Length)
            {
                CompleteBossRush();
                return;
            }
            
            BossData bossData = bosses[currentBossIndex];
            
            // Setup arena
            SetupArena(bossData);
            
            // Spawn boss
            Vector3 spawnPosition = GetBossSpawnPosition();
            currentBoss = Instantiate(bossData.bossPrefab, spawnPosition, Quaternion.identity);
            
            // Configure boss
            BaseCharacter bossCharacter = currentBoss.GetComponent<BaseCharacter>();
            if (bossCharacter != null)
            {
                // Scale boss based on current index for increasing difficulty
                float difficultyScale = 1f + (currentBossIndex * 0.3f);
                bossCharacter.maxHealth *= difficultyScale;
                bossCharacter.currentHealth = bossCharacter.maxHealth;
                bossCharacter.attackDamage *= difficultyScale;
                
                // Setup boss defeat listener
                bossCharacter.OnCharacterDeath += OnBossDefeated;
            }
            
            // Play boss intro
            PlayBossIntro(bossData);
            
            // Start boss fight timer
            bossFightTimes[currentBossIndex] = Time.time;
            
            Debug.Log($"Boss Rush: Fighting {bossData.bossName}");
        }
        
        private void SetupArena(BossData bossData)
        {
            // Destroy previous arena
            if (currentArena != null)
            {
                Destroy(currentArena);
            }
            
            // Create new arena
            if (bossData.arenaPrefab != null)
            {
                currentArena = Instantiate(bossData.arenaPrefab);
            }
            else if (arenaPrefabs.Length > 0)
            {
                GameObject randomArena = arenaPrefabs[Random.Range(0, arenaPrefabs.Length)];
                currentArena = Instantiate(randomArena);
            }
        }
        
        private Vector3 GetBossSpawnPosition()
        {
            if (arenaSpawnPoints != null && arenaSpawnPoints.Length > 0)
            {
                Transform spawnPoint = arenaSpawnPoints[Random.Range(0, arenaSpawnPoints.Length)];
                return spawnPoint.position;
            }
            else
            {
                return transform.position + Vector3.forward * 10f;
            }
        }
        
        private void PlayBossIntro(BossData bossData)
        {
            // Play boss music
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayBattleMusic(true);
            }
            
            // Play boss voice line
            if (bossData.introVoiceLine != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.voiceSource.PlayOneShot(bossData.introVoiceLine);
            }
            
            // Screen effect
            if (VisualEffectsManager.Instance != null)
            {
                VisualEffectsManager.Instance.TriggerScreenShake(0.3f, 1f);
            }
        }
        
        private void OnBossDefeated()
        {
            if (currentBossIndex >= bosses.Length) return;
            
            // Record fight time
            bossFightTimes[currentBossIndex] = Time.time - bossFightTimes[currentBossIndex];
            
            // Award score
            int score = bossDefeatScore;
            if (perfectFights[currentBossIndex])
            {
                score *= perfectBonusMultiplier;
            }
            
            score = Mathf.RoundToInt(score * scoreMultiplier);
            GameManager.Instance?.AddScore(score);
            
            bossesDefeated++;
            currentBossIndex++;
            
            // Restore player health and Nen between fights
            if (player != null)
            {
                player.Heal(player.maxHealth * healthRestorePercent);
                player.currentNen = Mathf.Min(player.maxNenCapacity, player.currentNen + (player.maxNenCapacity * nenRestorePercent));
            }
            
            // Play victory voice line
            if (AudioManager.Instance != null && GameManager.Instance != null)
            {
                AudioManager.Instance.PlayVoiceLine(GameManager.Instance.selectedCharacter, VoiceLineType.Victory);
            }
            
            // Prepare next boss
            StartCoroutine(NextBossDelay());
        }
        
        private IEnumerator NextBossDelay()
        {
            yield return new WaitForSeconds(timeBetweenBosses);
            
            if (currentBossIndex < bosses.Length)
            {
                SpawnNextBoss();
            }
            else
            {
                CompleteBossRush();
            }
        }
        
        private void CompleteBossRush()
        {
            bossRushActive = false;
            
            // Calculate final bonus
            int finalBonus = CalculateFinalBonus();
            GameManager.Instance?.AddScore(finalBonus);
            
            Debug.Log("Boss Rush Completed!");
            
            // Show completion stats
            ShowCompletionStats();
            
            // Return to menu after delay
            StartCoroutine(ReturnToMenuDelay());
        }
        
        private int CalculateFinalBonus()
        {
            int bonus = 0;
            
            // Perfect run bonus
            if (isPerfectRun)
            {
                bonus += 5000;
            }
            
            // Speed bonus
            float averageTime = totalTime / bosses.Length;
            if (averageTime < 60f) // Under 1 minute per boss
            {
                bonus += 2000;
            }
            
            // Consecutive boss bonus
            bonus += bossesDefeated * 500;
            
            return bonus;
        }
        
        private void ShowCompletionStats()
        {
            Debug.Log("=== BOSS RUSH COMPLETED ===");
            Debug.Log($"Total Time: {totalTime:F2} seconds");
            Debug.Log($"Bosses Defeated: {bossesDefeated}/{bosses.Length}");
            Debug.Log($"Perfect Run: {isPerfectRun}");
            
            for (int i = 0; i < bossesDefeated; i++)
            {
                Debug.Log($"{bosses[i].bossName}: {bossFightTimes[i]:F2}s {(perfectFights[i] ? "(Perfect)" : "")}");
            }
        }
        
        private IEnumerator ReturnToMenuDelay()
        {
            yield return new WaitForSeconds(5f);
            GameManager.Instance?.ReturnToMenu();
        }
        
        private void OnPlayerHealthChanged(float newHealth)
        {
            // Track if player takes damage during current boss fight
            if (currentBossIndex < perfectFights.Length && newHealth < player.maxHealth)
            {
                perfectFights[currentBossIndex] = false;
                isPerfectRun = false;
            }
        }
        
        void OnDestroy()
        {
            if (player != null)
            {
                player.OnHealthChanged -= OnPlayerHealthChanged;
            }
        }
        
        // Public methods
        public BossData GetCurrentBoss()
        {
            if (currentBossIndex < bosses.Length)
                return bosses[currentBossIndex];
            return null;
        }
        
        public int GetBossesRemaining()
        {
            return Mathf.Max(0, bosses.Length - currentBossIndex);
        }
        
        public bool IsLastBoss()
        {
            return currentBossIndex == bosses.Length - 1;
        }
    }
    
    [System.Serializable]
    public class BossData
    {
        public string bossName;
        public string description;
        public GameObject bossPrefab;
        public GameObject arenaPrefab;
        public AudioClip bossMusic;
        public AudioClip introVoiceLine;
        public AudioClip defeatVoiceLine;
        public Sprite bossPortrait;
        public Color bossAuraColor = Color.red;
    }
}