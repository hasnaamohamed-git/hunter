using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace HunterRush.Levels
{
    /// <summary>
    /// Procedural level generation system for iconic Hunter x Hunter locations
    /// Creates seamless endless runner levels with combat zones and exploration areas
    /// </summary>
    public class LevelGenerator : MonoBehaviour
    {
        [Header("Generation Settings")]
        public float segmentLength = 50f;
        public int activeSegments = 3;
        public float playerTriggerDistance = 25f;
        public Transform player;
        
        [Header("Location Themes")]
        public LocationTheme[] availableThemes;
        public LocationTheme currentTheme;
        public float themeChangeDistance = 200f;
        
        [Header("Segment Types")]
        public LevelSegmentData[] runningSegments;
        public LevelSegmentData[] combatSegments;
        public LevelSegmentData[] explorationSegments;
        public LevelSegmentData[] bossSegments;
        
        [Header("Difficulty Scaling")]
        public float difficultyIncreaseRate = 0.1f;
        public float maxDifficulty = 3f;
        public AnimationCurve difficultyScaling;
        
        [Header("Special Events")]
        public float bossChance = 0.05f;
        public float explorationChance = 0.15f;
        public float combatChance = 0.4f;
        
        // Generation state
        private Queue<LevelSegment> activeSegmentQueue = new Queue<LevelSegment>();
        private List<LevelSegment> segmentPool = new List<LevelSegment>();
        private Vector3 nextSegmentPosition = Vector3.zero;
        private float distanceTraveled = 0f;
        private float currentDifficulty = 1f;
        private int segmentsGenerated = 0;
        
        // Location tracking
        private float lastThemeChange = 0f;
        private int currentThemeIndex = 0;
        
        void Start()
        {
            if (player == null)
                player = FindObjectOfType<BaseCharacter>()?.transform;
            
            InitializeGeneration();
        }
        
        void Update()
        {
            if (player != null)
            {
                UpdateGeneration();
                UpdateDifficulty();
            }
        }
        
        private void InitializeGeneration()
        {
            // Select initial theme
            if (availableThemes.Length > 0)
            {
                currentTheme = availableThemes[0];
            }
            
            // Generate initial segments
            for (int i = 0; i < activeSegments; i++)
            {
                GenerateNextSegment();
            }
        }
        
        private void UpdateGeneration()
        {
            if (activeSegmentQueue.Count == 0) return;
            
            // Check if player is close to trigger point
            LevelSegment firstSegment = activeSegmentQueue.Peek();
            float distanceToTrigger = Vector3.Distance(player.position, firstSegment.triggerPoint.position);
            
            if (distanceToTrigger <= playerTriggerDistance)
            {
                GenerateNextSegment();
                RemoveOldestSegment();
            }
            
            // Update distance traveled
            distanceTraveled = player.position.z;
            
            // Check for theme change
            if (distanceTraveled - lastThemeChange >= themeChangeDistance)
            {
                ChangeLocationTheme();
            }
        }
        
        private void UpdateDifficulty()
        {
            float targetDifficulty = Mathf.Min(maxDifficulty, 1f + (distanceTraveled * difficultyIncreaseRate / 100f));
            currentDifficulty = Mathf.Lerp(currentDifficulty, targetDifficulty, Time.deltaTime * 0.5f);
        }
        
        private void GenerateNextSegment()
        {
            SegmentType segmentType = DetermineNextSegmentType();
            LevelSegmentData segmentData = GetRandomSegmentData(segmentType);
            
            if (segmentData != null)
            {
                CreateSegment(segmentData);
            }
        }
        
        private SegmentType DetermineNextSegmentType()
        {
            // Determine segment type based on distance and randomness
            float random = Random.Range(0f, 1f);
            
            // Boss segments at specific intervals
            if (segmentsGenerated > 0 && segmentsGenerated % 20 == 0)
            {
                return SegmentType.Boss;
            }
            
            // Regular segment distribution
            if (random < explorationChance)
                return SegmentType.Exploration;
            else if (random < explorationChance + combatChance)
                return SegmentType.Combat;
            else
                return SegmentType.Running;
        }
        
        private LevelSegmentData GetRandomSegmentData(SegmentType type)
        {
            LevelSegmentData[] availableSegments = null;
            
            switch (type)
            {
                case SegmentType.Running:
                    availableSegments = runningSegments;
                    break;
                case SegmentType.Combat:
                    availableSegments = combatSegments;
                    break;
                case SegmentType.Exploration:
                    availableSegments = explorationSegments;
                    break;
                case SegmentType.Boss:
                    availableSegments = bossSegments;
                    break;
            }
            
            if (availableSegments != null && availableSegments.Length > 0)
            {
                // Filter by current theme
                List<LevelSegmentData> themeSegments = new List<LevelSegmentData>();
                foreach (LevelSegmentData segment in availableSegments)
                {
                    if (segment.compatibleThemes.Contains(currentTheme.themeName))
                    {
                        themeSegments.Add(segment);
                    }
                }
                
                if (themeSegments.Count > 0)
                {
                    return themeSegments[Random.Range(0, themeSegments.Count)];
                }
                else
                {
                    return availableSegments[Random.Range(0, availableSegments.Length)];
                }
            }
            
            return null;
        }
        
        private void CreateSegment(LevelSegmentData segmentData)
        {
            // Get segment from pool or instantiate new one
            GameObject segmentObject = GetPooledSegment(segmentData);
            if (segmentObject == null)
            {
                segmentObject = Instantiate(segmentData.prefab);
            }
            
            // Position the segment
            segmentObject.transform.position = nextSegmentPosition;
            segmentObject.SetActive(true);
            
            // Create level segment component
            LevelSegment segment = segmentObject.GetComponent<LevelSegment>();
            if (segment == null)
            {
                segment = segmentObject.AddComponent<LevelSegment>();
            }
            
            segment.Initialize(segmentData, currentDifficulty, currentTheme);
            
            // Add to active queue
            activeSegmentQueue.Enqueue(segment);
            
            // Update next position
            nextSegmentPosition += Vector3.forward * segmentLength;
            segmentsGenerated++;
        }
        
        private void RemoveOldestSegment()
        {
            if (activeSegmentQueue.Count > 0)
            {
                LevelSegment oldSegment = activeSegmentQueue.Dequeue();
                ReturnSegmentToPool(oldSegment);
            }
        }
        
        private GameObject GetPooledSegment(LevelSegmentData segmentData)
        {
            foreach (LevelSegment segment in segmentPool)
            {
                if (!segment.gameObject.activeInHierarchy && segment.segmentData == segmentData)
                {
                    segmentPool.Remove(segment);
                    return segment.gameObject;
                }
            }
            return null;
        }
        
        private void ReturnSegmentToPool(LevelSegment segment)
        {
            if (segment != null)
            {
                segment.gameObject.SetActive(false);
                segmentPool.Add(segment);
            }
        }
        
        private void ChangeLocationTheme()
        {
            if (availableThemes.Length <= 1) return;
            
            // Select next theme
            currentThemeIndex = (currentThemeIndex + 1) % availableThemes.Length;
            currentTheme = availableThemes[currentThemeIndex];
            lastThemeChange = distanceTraveled;
            
            Debug.Log($"Changed to location theme: {currentTheme.themeName}");
            
            // Play theme transition effect
            StartCoroutine(ThemeTransitionEffect());
        }
        
        private IEnumerator ThemeTransitionEffect()
        {
            // Visual transition effect
            // TODO: Implement screen fade or other transition effect
            yield return new WaitForSeconds(1f);
        }
        
        // Public methods
        public float GetCurrentDifficulty()
        {
            return currentDifficulty;
        }
        
        public LocationTheme GetCurrentTheme()
        {
            return currentTheme;
        }
        
        public void SetPlayer(Transform newPlayer)
        {
            player = newPlayer;
        }
        
        public void ResetGeneration()
        {
            // Clear all segments
            while (activeSegmentQueue.Count > 0)
            {
                LevelSegment segment = activeSegmentQueue.Dequeue();
                if (segment != null)
                    Destroy(segment.gameObject);
            }
            
            // Reset state
            nextSegmentPosition = Vector3.zero;
            distanceTraveled = 0f;
            currentDifficulty = 1f;
            segmentsGenerated = 0;
            lastThemeChange = 0f;
            
            // Regenerate initial segments
            InitializeGeneration();
        }
    }
    
    [System.Serializable]
    public class LocationTheme
    {
        public string themeName;
        public Color ambientColor;
        public Material skyboxMaterial;
        public AudioClip ambientSound;
        public GameObject[] environmentPrefabs;
        public ParticleSystem[] atmosphereEffects;
    }
    
    [System.Serializable]
    public class LevelSegmentData
    {
        public string segmentName;
        public GameObject prefab;
        public SegmentType type;
        public float length = 50f;
        public float difficulty = 1f;
        public string[] compatibleThemes;
        public EnemySpawnData[] enemySpawns;
        public ObstacleData[] obstacles;
        public CollectibleData[] collectibles;
    }
    
    [System.Serializable]
    public class EnemySpawnData
    {
        public GameObject enemyPrefab;
        public Vector3 spawnOffset;
        public float spawnChance = 1f;
        public int minCount = 1;
        public int maxCount = 3;
    }
    
    [System.Serializable]
    public class ObstacleData
    {
        public GameObject obstaclePrefab;
        public Vector3 position;
        public bool canBeDestroyed = false;
        public float destructionReward = 10f;
    }
    
    [System.Serializable]
    public class CollectibleData
    {
        public GameObject collectiblePrefab;
        public Vector3 position;
        public CollectibleType type;
        public float value = 10f;
    }
    
    public enum SegmentType
    {
        Running,
        Combat,
        Exploration,
        Boss
    }
    
    public enum CollectibleType
    {
        Score,
        Health,
        Nen,
        PowerUp
    }
}