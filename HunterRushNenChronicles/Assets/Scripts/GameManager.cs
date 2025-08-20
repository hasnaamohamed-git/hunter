using UnityEngine;
using UnityEngine.SceneManagement;

namespace HunterRush
{
    /// <summary>
    /// Main game manager handling game states, scene transitions, and core game loop
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [Header("Game Settings")]
        public GameMode currentGameMode = GameMode.StoryMode;
        public GameState currentGameState = GameState.Menu;
        
        [Header("Character Selection")]
        public CharacterType selectedCharacter = CharacterType.Gon;
        
        [Header("Score System")]
        public int currentScore = 0;
        public int highScore = 0;
        
        [Header("Audio")]
        public AudioManager audioManager;
        
        public static GameManager Instance { get; private set; }
        
        // Events
        public System.Action<GameState> OnGameStateChanged;
        public System.Action<int> OnScoreChanged;
        public System.Action<CharacterType> OnCharacterChanged;
        
        private void Awake()
        {
            // Singleton pattern
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Initialize();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void Initialize()
        {
            // Load saved data
            LoadGameData();
            
            // Initialize audio
            if (audioManager == null)
                audioManager = FindObjectOfType<AudioManager>();
        }
        
        public void ChangeGameState(GameState newState)
        {
            currentGameState = newState;
            OnGameStateChanged?.Invoke(newState);
            
            switch (newState)
            {
                case GameState.Menu:
                    Time.timeScale = 1f;
                    break;
                case GameState.Playing:
                    Time.timeScale = 1f;
                    break;
                case GameState.Paused:
                    Time.timeScale = 0f;
                    break;
                case GameState.GameOver:
                    Time.timeScale = 0f;
                    break;
            }
        }
        
        public void SelectCharacter(CharacterType character)
        {
            selectedCharacter = character;
            OnCharacterChanged?.Invoke(character);
        }
        
        public void AddScore(int points)
        {
            currentScore += points;
            OnScoreChanged?.Invoke(currentScore);
            
            if (currentScore > highScore)
            {
                highScore = currentScore;
                SaveGameData();
            }
        }
        
        public void StartGame(GameMode mode)
        {
            currentGameMode = mode;
            currentScore = 0;
            
            switch (mode)
            {
                case GameMode.StoryMode:
                    SceneManager.LoadScene("StoryMode");
                    break;
                case GameMode.EndlessRun:
                    SceneManager.LoadScene("EndlessRun");
                    break;
                case GameMode.BossRush:
                    SceneManager.LoadScene("BossRush");
                    break;
                case GameMode.PvPArena:
                    SceneManager.LoadScene("PvPArena");
                    break;
            }
            
            ChangeGameState(GameState.Playing);
        }
        
        public void PauseGame()
        {
            ChangeGameState(GameState.Paused);
        }
        
        public void ResumeGame()
        {
            ChangeGameState(GameState.Playing);
        }
        
        public void GameOver()
        {
            ChangeGameState(GameState.GameOver);
        }
        
        public void ReturnToMenu()
        {
            SceneManager.LoadScene("MainMenu");
            ChangeGameState(GameState.Menu);
        }
        
        private void SaveGameData()
        {
            PlayerPrefs.SetInt("HighScore", highScore);
            PlayerPrefs.SetInt("SelectedCharacter", (int)selectedCharacter);
            PlayerPrefs.Save();
        }
        
        private void LoadGameData()
        {
            highScore = PlayerPrefs.GetInt("HighScore", 0);
            selectedCharacter = (CharacterType)PlayerPrefs.GetInt("SelectedCharacter", 0);
        }
        
        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus && currentGameState == GameState.Playing)
            {
                PauseGame();
            }
        }
    }
    
    public enum GameMode
    {
        StoryMode,
        EndlessRun,
        BossRush,
        MultiplayerCoop,
        PvPArena,
        DailyChallenges
    }
    
    public enum GameState
    {
        Menu,
        Playing,
        Paused,
        GameOver,
        Loading
    }
    
    public enum CharacterType
    {
        Gon,
        Killua,
        Kurapika,
        Leorio
    }
}