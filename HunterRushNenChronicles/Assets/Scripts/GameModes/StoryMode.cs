using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace HunterRush.GameModes
{
    /// <summary>
    /// Story Mode following Hunter x Hunter anime arcs with cutscenes and progression
    /// </summary>
    public class StoryMode : MonoBehaviour
    {
        [Header("Story Progression")]
        public StoryArc[] storyArcs;
        public int currentArcIndex = 0;
        public int currentChapterIndex = 0;
        
        [Header("Cutscenes")]
        public CutsceneData[] cutscenes;
        public bool skipCutscenes = false;
        
        [Header("Progression")]
        public bool saveProgress = true;
        public string saveKey = "StoryProgress";
        
        // Current state
        private StoryArc currentArc;
        private StoryChapter currentChapter;
        private bool isInCutscene = false;
        private bool chapterCompleted = false;
        
        // Events
        public System.Action<StoryArc> OnArcStarted;
        public System.Action<StoryArc> OnArcCompleted;
        public System.Action<StoryChapter> OnChapterStarted;
        public System.Action<StoryChapter> OnChapterCompleted;
        
        void Start()
        {
            LoadProgress();
            StartCurrentArc();
        }
        
        private void LoadProgress()
        {
            if (saveProgress)
            {
                currentArcIndex = PlayerPrefs.GetInt($"{saveKey}_Arc", 0);
                currentChapterIndex = PlayerPrefs.GetInt($"{saveKey}_Chapter", 0);
            }
            
            // Ensure valid indices
            currentArcIndex = Mathf.Clamp(currentArcIndex, 0, storyArcs.Length - 1);
            if (storyArcs.Length > currentArcIndex)
            {
                currentChapterIndex = Mathf.Clamp(currentChapterIndex, 0, storyArcs[currentArcIndex].chapters.Length - 1);
            }
        }
        
        private void SaveProgress()
        {
            if (saveProgress)
            {
                PlayerPrefs.SetInt($"{saveKey}_Arc", currentArcIndex);
                PlayerPrefs.SetInt($"{saveKey}_Chapter", currentChapterIndex);
                PlayerPrefs.Save();
            }
        }
        
        private void StartCurrentArc()
        {
            if (currentArcIndex >= storyArcs.Length) return;
            
            currentArc = storyArcs[currentArcIndex];
            OnArcStarted?.Invoke(currentArc);
            
            Debug.Log($"Starting Arc: {currentArc.arcName}");
            
            // Play arc intro cutscene
            if (currentArc.introCutscene != null && !skipCutscenes)
            {
                PlayCutscene(currentArc.introCutscene);
            }
            else
            {
                StartCurrentChapter();
            }
        }
        
        private void StartCurrentChapter()
        {
            if (currentChapterIndex >= currentArc.chapters.Length)
            {
                CompleteCurrentArc();
                return;
            }
            
            currentChapter = currentArc.chapters[currentChapterIndex];
            OnChapterStarted?.Invoke(currentChapter);
            
            Debug.Log($"Starting Chapter: {currentChapter.chapterName}");
            
            // Setup chapter environment
            SetupChapterEnvironment();
            
            // Play chapter intro cutscene
            if (currentChapter.introCutscene != null && !skipCutscenes)
            {
                PlayCutscene(currentChapter.introCutscene);
            }
            else
            {
                StartChapterGameplay();
            }
        }
        
        private void SetupChapterEnvironment()
        {
            // Load chapter-specific level
            if (currentChapter.levelPrefab != null)
            {
                // Clear existing level
                GameObject existingLevel = GameObject.FindGameObjectWithTag("Level");
                if (existingLevel != null)
                {
                    Destroy(existingLevel);
                }
                
                // Instantiate new level
                GameObject level = Instantiate(currentChapter.levelPrefab);
                level.tag = "Level";
            }
            
            // Setup chapter objectives
            SetupObjectives();
            
            // Configure level generator for chapter theme
            if (levelGenerator != null && currentChapter.locationTheme != null)
            {
                levelGenerator.currentTheme = currentChapter.locationTheme;
            }
        }
        
        private void SetupObjectives()
        {
            // Clear previous objectives
            // Setup new objectives based on chapter
            
            foreach (ChapterObjective objective in currentChapter.objectives)
            {
                switch (objective.type)
                {
                    case ObjectiveType.DefeatEnemies:
                        SetupDefeatObjective(objective);
                        break;
                    case ObjectiveType.ReachLocation:
                        SetupLocationObjective(objective);
                        break;
                    case ObjectiveType.CollectItems:
                        SetupCollectionObjective(objective);
                        break;
                    case ObjectiveType.SurviveTime:
                        SetupSurvivalObjective(objective);
                        break;
                    case ObjectiveType.DefeatBoss:
                        SetupBossObjective(objective);
                        break;
                }
            }
        }
        
        private void SetupDefeatObjective(ChapterObjective objective)
        {
            // Spawn specific enemies to defeat
            Debug.Log($"Objective: Defeat {objective.targetCount} enemies");
        }
        
        private void SetupLocationObjective(ChapterObjective objective)
        {
            // Create location marker
            Debug.Log($"Objective: Reach {objective.targetLocation}");
        }
        
        private void SetupCollectionObjective(ChapterObjective objective)
        {
            // Spawn collectible items
            Debug.Log($"Objective: Collect {objective.targetCount} items");
        }
        
        private void SetupSurvivalObjective(ChapterObjective objective)
        {
            // Setup survival timer
            StartCoroutine(SurvivalTimer(objective.targetTime));
        }
        
        private void SetupBossObjective(ChapterObjective objective)
        {
            // Spawn boss enemy
            if (objective.bossPrefab != null)
            {
                Vector3 bossPosition = transform.position + Vector3.forward * 20f;
                GameObject boss = Instantiate(objective.bossPrefab, bossPosition, Quaternion.identity);
                
                // Setup boss event listeners
                BaseCharacter bossChar = boss.GetComponent<BaseCharacter>();
                if (bossChar != null)
                {
                    bossChar.OnCharacterDeath += OnBossDefeated;
                }
            }
        }
        
        private IEnumerator SurvivalTimer(float duration)
        {
            yield return new WaitForSeconds(duration);
            CompleteCurrentChapter();
        }
        
        private void PlayCutscene(CutsceneData cutscene)
        {
            if (cutscene == null) return;
            
            isInCutscene = true;
            
            // Pause gameplay
            Time.timeScale = 0f;
            
            // Play cutscene
            StartCoroutine(CutsceneCoroutine(cutscene));
        }
        
        private IEnumerator CutsceneCoroutine(CutsceneData cutscene)
        {
            // Play cutscene video or animation
            Debug.Log($"Playing cutscene: {cutscene.cutsceneName}");
            
            // Play cutscene audio
            if (cutscene.audioClip != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.voiceSource.PlayOneShot(cutscene.audioClip);
            }
            
            // Wait for cutscene duration (or skip input)
            float elapsed = 0f;
            while (elapsed < cutscene.duration && !Input.GetKeyDown(KeyCode.Space))
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
            
            // Resume gameplay
            Time.timeScale = 1f;
            isInCutscene = false;
            
            // Continue with story progression
            if (cutscene == currentArc.introCutscene)
            {
                StartCurrentChapter();
            }
            else if (cutscene == currentChapter.introCutscene)
            {
                StartChapterGameplay();
            }
            else if (cutscene == currentChapter.outroCutscene)
            {
                AdvanceToNextChapter();
            }
            else if (cutscene == currentArc.outroCutscene)
            {
                AdvanceToNextArc();
            }
        }
        
        private void StartChapterGameplay()
        {
            // Enable player control and start chapter
            BaseCharacter player = FindObjectOfType<BaseCharacter>();
            if (player != null)
            {
                player.enabled = true;
            }
            
            // Start chapter-specific gameplay
            switch (currentChapter.chapterType)
            {
                case ChapterType.Combat:
                    StartCombatChapter();
                    break;
                case ChapterType.Exploration:
                    StartExplorationChapter();
                    break;
                case ChapterType.Chase:
                    StartChaseChapter();
                    break;
                case ChapterType.Boss:
                    StartBossChapter();
                    break;
            }
        }
        
        private void StartCombatChapter()
        {
            // Setup combat encounter
            Debug.Log("Starting combat chapter");
        }
        
        private void StartExplorationChapter()
        {
            // Setup exploration objectives
            Debug.Log("Starting exploration chapter");
        }
        
        private void StartChaseChapter()
        {
            // Setup chase sequence
            Debug.Log("Starting chase chapter");
        }
        
        private void StartBossChapter()
        {
            // Setup boss battle
            Debug.Log("Starting boss chapter");
            
            // Play boss music
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayBattleMusic(true);
            }
        }
        
        public void CompleteCurrentChapter()
        {
            if (chapterCompleted) return;
            
            chapterCompleted = true;
            OnChapterCompleted?.Invoke(currentChapter);
            
            Debug.Log($"Chapter completed: {currentChapter.chapterName}");
            
            // Play outro cutscene
            if (currentChapter.outroCutscene != null && !skipCutscenes)
            {
                PlayCutscene(currentChapter.outroCutscene);
            }
            else
            {
                AdvanceToNextChapter();
            }
        }
        
        private void AdvanceToNextChapter()
        {
            currentChapterIndex++;
            chapterCompleted = false;
            SaveProgress();
            
            if (currentChapterIndex >= currentArc.chapters.Length)
            {
                CompleteCurrentArc();
            }
            else
            {
                StartCurrentChapter();
            }
        }
        
        private void CompleteCurrentArc()
        {
            OnArcCompleted?.Invoke(currentArc);
            
            Debug.Log($"Arc completed: {currentArc.arcName}");
            
            // Play arc outro cutscene
            if (currentArc.outroCutscene != null && !skipCutscenes)
            {
                PlayCutscene(currentArc.outroCutscene);
            }
            else
            {
                AdvanceToNextArc();
            }
        }
        
        private void AdvanceToNextArc()
        {
            currentArcIndex++;
            currentChapterIndex = 0;
            SaveProgress();
            
            if (currentArcIndex >= storyArcs.Length)
            {
                // Story completed!
                CompleteStory();
            }
            else
            {
                StartCurrentArc();
            }
        }
        
        private void CompleteStory()
        {
            Debug.Log("Story Mode Completed!");
            
            // Award completion bonus
            GameManager.Instance?.AddScore(10000);
            
            // Return to main menu
            GameManager.Instance?.ReturnToMenu();
        }
        
        // Event handlers
        private void OnBossDefeated()
        {
            CompleteCurrentChapter();
        }
        
        // Public methods
        public void SkipCutscene()
        {
            skipCutscenes = true;
        }
        
        public StoryArc GetCurrentArc()
        {
            return currentArc;
        }
        
        public StoryChapter GetCurrentChapter()
        {
            return currentChapter;
        }
        
        public float GetProgressPercentage()
        {
            if (storyArcs.Length == 0) return 100f;
            
            float totalChapters = 0f;
            float completedChapters = 0f;
            
            for (int i = 0; i < storyArcs.Length; i++)
            {
                totalChapters += storyArcs[i].chapters.Length;
                
                if (i < currentArcIndex)
                {
                    completedChapters += storyArcs[i].chapters.Length;
                }
                else if (i == currentArcIndex)
                {
                    completedChapters += currentChapterIndex;
                }
            }
            
            return (completedChapters / totalChapters) * 100f;
        }
    }
    
    [System.Serializable]
    public class StoryArc
    {
        public string arcName;
        public string description;
        public StoryChapter[] chapters;
        public CutsceneData introCutscene;
        public CutsceneData outroCutscene;
        public Sprite arcImage;
        public LocationTheme arcTheme;
    }
    
    [System.Serializable]
    public class StoryChapter
    {
        public string chapterName;
        public string description;
        public ChapterType chapterType;
        public GameObject levelPrefab;
        public ChapterObjective[] objectives;
        public CutsceneData introCutscene;
        public CutsceneData outroCutscene;
        public LocationTheme locationTheme;
        public float timeLimit = 0f; // 0 = no time limit
    }
    
    [System.Serializable]
    public class ChapterObjective
    {
        public string objectiveName;
        public string description;
        public ObjectiveType type;
        public int targetCount = 1;
        public float targetTime = 0f;
        public string targetLocation;
        public GameObject bossPrefab;
        public bool isOptional = false;
        public bool isCompleted = false;
    }
    
    [System.Serializable]
    public class CutsceneData
    {
        public string cutsceneName;
        public float duration = 5f;
        public AudioClip audioClip;
        public VideoClip videoClip;
        public Sprite[] storyImages;
        public string[] dialogueLines;
        public CharacterType[] speakers;
    }
    
    public enum ChapterType
    {
        Combat,
        Exploration,
        Chase,
        Boss,
        Stealth,
        Puzzle
    }
    
    public enum ObjectiveType
    {
        DefeatEnemies,
        ReachLocation,
        CollectItems,
        SurviveTime,
        DefeatBoss,
        ProtectAlly,
        SolveEuzzle,
        StealthMission
    }
}