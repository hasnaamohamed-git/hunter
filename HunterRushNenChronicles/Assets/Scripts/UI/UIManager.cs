using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace HunterRush.UI
{
    /// <summary>
    /// Main UI Manager for Hunter Rush: Nen Chronicles
    /// Handles all UI elements including menus, HUD, and character selection
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [Header("Main Menus")]
        public GameObject mainMenuPanel;
        public GameObject characterSelectPanel;
        public GameObject settingsPanel;
        public GameObject pauseMenuPanel;
        public GameObject gameOverPanel;
        
        [Header("HUD Elements")]
        public GameObject hudPanel;
        public Slider healthBar;
        public Slider nenBar;
        public TextMeshProUGUI scoreText;
        public TextMeshProUGUI comboText;
        public Image characterPortrait;
        public GameObject nenStateIndicator;
        
        [Header("Character Selection")]
        public CharacterSelectButton[] characterButtons;
        public TextMeshProUGUI characterNameText;
        public TextMeshProUGUI characterDescriptionText;
        public Image characterPreviewImage;
        public Button startGameButton;
        
        [Header("Settings")]
        public Slider masterVolumeSlider;
        public Slider musicVolumeSlider;
        public Slider sfxVolumeSlider;
        public Slider voiceVolumeSlider;
        public Toggle voiceLinesToggle;
        public Toggle subtitlesToggle;
        public TMP_Dropdown languageDropdown;
        
        [Header("Game Over")]
        public TextMeshProUGUI finalScoreText;
        public TextMeshProUGUI highScoreText;
        public Button retryButton;
        public Button menuButton;
        
        [Header("Subtitles")]
        public GameObject subtitlePanel;
        public TextMeshProUGUI subtitleText;
        
        [Header("Character Data")]
        public CharacterUIData[] characterData;
        
        // Singleton
        public static UIManager Instance { get; private set; }
        
        // State
        private CharacterType selectedCharacter = CharacterType.Gon;
        private bool isMenuActive = true;
        private Coroutine subtitleCoroutine;
        
        // Events
        public System.Action<CharacterType> OnCharacterSelected;
        public System.Action OnGameStartRequested;
        public System.Action OnRetryRequested;
        public System.Action OnMenuRequested;
        
        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                InitializeUI();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        void Start()
        {
            ShowMainMenu();
            SetupEventListeners();
            LoadSettings();
        }
        
        void Update()
        {
            UpdateHUD();
            HandleInput();
        }
        
        private void InitializeUI()
        {
            // Ensure all panels start inactive except main menu
            if (characterSelectPanel != null) characterSelectPanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false);
            if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
            if (hudPanel != null) hudPanel.SetActive(false);
            if (subtitlePanel != null) subtitlePanel.SetActive(false);
        }
        
        private void SetupEventListeners()
        {
            // Character selection buttons
            for (int i = 0; i < characterButtons.Length; i++)
            {
                int index = i; // Capture for closure
                characterButtons[i].button.onClick.AddListener(() => SelectCharacter((CharacterType)index));
            }
            
            // Settings sliders
            if (masterVolumeSlider != null)
                masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
            if (musicVolumeSlider != null)
                musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
            if (sfxVolumeSlider != null)
                sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);
            if (voiceVolumeSlider != null)
                voiceVolumeSlider.onValueChanged.AddListener(SetVoiceVolume);
            
            // Settings toggles
            if (voiceLinesToggle != null)
                voiceLinesToggle.onValueChanged.AddListener(SetVoiceLinesEnabled);
            if (subtitlesToggle != null)
                subtitlesToggle.onValueChanged.AddListener(SetSubtitlesEnabled);
            
            // Game Manager events
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
                GameManager.Instance.OnScoreChanged += UpdateScore;
            }
            
            // Audio Manager events
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.OnVoiceLineStarted += ShowSubtitle;
                AudioManager.Instance.OnVoiceLineEnded += HideSubtitle;
            }
        }
        
        private void HandleInput()
        {
            // Pause menu toggle
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (GameManager.Instance != null)
                {
                    if (GameManager.Instance.currentGameState == GameState.Playing)
                    {
                        ShowPauseMenu();
                    }
                    else if (GameManager.Instance.currentGameState == GameState.Paused)
                    {
                        HidePauseMenu();
                    }
                }
            }
        }
        
        private void UpdateHUD()
        {
            if (!hudPanel.activeInHierarchy) return;
            
            // Update health and Nen bars
            BaseCharacter player = FindObjectOfType<BaseCharacter>();
            if (player != null && player.IsPlayerControlled())
            {
                if (healthBar != null)
                {
                    healthBar.value = player.currentHealth / player.maxHealth;
                }
                
                if (nenBar != null)
                {
                    nenBar.value = player.currentNen / player.maxNenCapacity;
                }
                
                // Update Nen state indicator
                UpdateNenStateIndicator(player.currentNenState);
                
                // Update combo display
                CombatController combat = player.GetComponent<CombatController>();
                if (combat != null && comboText != null)
                {
                    int comboCount = combat.GetComboCount();
                    comboText.text = comboCount > 1 ? $"COMBO x{comboCount}" : "";
                    comboText.gameObject.SetActive(comboCount > 1);
                }
            }
        }
        
        private void UpdateNenStateIndicator(NenState nenState)
        {
            if (nenStateIndicator == null) return;
            
            // Update color and text based on Nen state
            Image indicator = nenStateIndicator.GetComponent<Image>();
            TextMeshProUGUI stateText = nenStateIndicator.GetComponentInChildren<TextMeshProUGUI>();
            
            Color stateColor = Color.white;
            string stateName = "";
            
            switch (nenState)
            {
                case NenState.Ten:
                    stateColor = Color.blue;
                    stateName = "TEN";
                    break;
                case NenState.Zetsu:
                    stateColor = Color.gray;
                    stateName = "ZETSU";
                    break;
                case NenState.Ren:
                    stateColor = Color.red;
                    stateName = "REN";
                    break;
                case NenState.Hatsu:
                    stateColor = Color.yellow;
                    stateName = "HATSU";
                    break;
                default:
                    stateColor = Color.white;
                    stateName = "";
                    break;
            }
            
            if (indicator != null)
                indicator.color = stateColor;
            
            if (stateText != null)
                stateText.text = stateName;
            
            nenStateIndicator.SetActive(!string.IsNullOrEmpty(stateName));
        }
        
        // Menu Navigation
        public void ShowMainMenu()
        {
            HideAllPanels();
            if (mainMenuPanel != null)
            {
                mainMenuPanel.SetActive(true);
                isMenuActive = true;
            }
            
            // Play menu music
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayContextualMusic(GameState.Menu);
            }
        }
        
        public void ShowCharacterSelect()
        {
            HideAllPanels();
            if (characterSelectPanel != null)
            {
                characterSelectPanel.SetActive(true);
                isMenuActive = true;
                
                // Initialize character selection
                SelectCharacter(selectedCharacter);
                
                // Play character select music
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayMusic(AudioManager.Instance.characterSelectMusic);
                }
            }
        }
        
        public void ShowSettings()
        {
            HideAllPanels();
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(true);
                isMenuActive = true;
                RefreshSettingsUI();
            }
        }
        
        public void ShowPauseMenu()
        {
            if (pauseMenuPanel != null)
            {
                pauseMenuPanel.SetActive(true);
                GameManager.Instance?.PauseGame();
            }
        }
        
        public void HidePauseMenu()
        {
            if (pauseMenuPanel != null)
            {
                pauseMenuPanel.SetActive(false);
                GameManager.Instance?.ResumeGame();
            }
        }
        
        public void ShowGameOver(int finalScore, int highScore)
        {
            HideAllPanels();
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
                
                if (finalScoreText != null)
                    finalScoreText.text = $"Score: {finalScore:N0}";
                
                if (highScoreText != null)
                    highScoreText.text = $"High Score: {highScore:N0}";
            }
            
            // Play game over music
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayMusic(AudioManager.Instance.gameOverMusic, false);
            }
        }
        
        public void ShowHUD()
        {
            HideAllPanels();
            if (hudPanel != null)
            {
                hudPanel.SetActive(true);
                isMenuActive = false;
                
                // Update character portrait
                UpdateCharacterPortrait();
            }
        }
        
        private void HideAllPanels()
        {
            if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
            if (characterSelectPanel != null) characterSelectPanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false);
            if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
            if (hudPanel != null) hudPanel.SetActive(false);
        }
        
        // Character Selection
        public void SelectCharacter(CharacterType character)
        {
            selectedCharacter = character;
            
            // Update UI
            UpdateCharacterSelectionUI();
            
            // Play character voice line
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayVoiceLine(character, VoiceLineType.Greeting, true);
            }
            
            // Notify listeners
            OnCharacterSelected?.Invoke(character);
            
            // Update GameManager
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SelectCharacter(character);
            }
        }
        
        private void UpdateCharacterSelectionUI()
        {
            CharacterUIData data = GetCharacterData(selectedCharacter);
            if (data == null) return;
            
            // Update character info
            if (characterNameText != null)
                characterNameText.text = data.characterName;
            
            if (characterDescriptionText != null)
                characterDescriptionText.text = data.description;
            
            if (characterPreviewImage != null)
                characterPreviewImage.sprite = data.portraitSprite;
            
            // Update button states
            for (int i = 0; i < characterButtons.Length; i++)
            {
                bool isSelected = (CharacterType)i == selectedCharacter;
                characterButtons[i].SetSelected(isSelected);
            }
        }
        
        private void UpdateCharacterPortrait()
        {
            if (characterPortrait == null) return;
            
            CharacterUIData data = GetCharacterData(selectedCharacter);
            if (data != null)
            {
                characterPortrait.sprite = data.portraitSprite;
            }
        }
        
        private CharacterUIData GetCharacterData(CharacterType character)
        {
            if (characterData == null) return null;
            
            foreach (CharacterUIData data in characterData)
            {
                if (data.characterType == character)
                    return data;
            }
            
            return null;
        }
        
        // Settings
        private void RefreshSettingsUI()
        {
            if (AudioManager.Instance == null) return;
            
            if (masterVolumeSlider != null)
                masterVolumeSlider.value = AudioManager.Instance.masterVolume;
            
            if (musicVolumeSlider != null)
                musicVolumeSlider.value = AudioManager.Instance.musicVolume;
            
            if (sfxVolumeSlider != null)
                sfxVolumeSlider.value = AudioManager.Instance.sfxVolume;
            
            if (voiceVolumeSlider != null)
                voiceVolumeSlider.value = AudioManager.Instance.voiceVolume;
            
            if (voiceLinesToggle != null)
                voiceLinesToggle.isOn = AudioManager.Instance.enableVoiceLines;
            
            if (subtitlesToggle != null)
                subtitlesToggle.isOn = AudioManager.Instance.enableSubtitles;
            
            if (languageDropdown != null)
                languageDropdown.value = (int)AudioManager.Instance.currentLanguage;
        }
        
        private void SetMasterVolume(float value)
        {
            AudioManager.Instance?.SetMasterVolume(value);
        }
        
        private void SetMusicVolume(float value)
        {
            AudioManager.Instance?.SetMusicVolume(value);
        }
        
        private void SetSFXVolume(float value)
        {
            AudioManager.Instance?.SetSFXVolume(value);
        }
        
        private void SetVoiceVolume(float value)
        {
            AudioManager.Instance?.SetVoiceVolume(value);
        }
        
        private void SetVoiceLinesEnabled(bool enabled)
        {
            AudioManager.Instance?.SetVoiceLinesEnabled(enabled);
        }
        
        private void SetSubtitlesEnabled(bool enabled)
        {
            AudioManager.Instance?.SetSubtitlesEnabled(enabled);
        }
        
        private void LoadSettings()
        {
            RefreshSettingsUI();
        }
        
        public void SaveSettings()
        {
            AudioManager.Instance?.SaveAudioSettings();
        }
        
        // Score Display
        public void UpdateScore(int score)
        {
            if (scoreText != null)
            {
                scoreText.text = $"Score: {score:N0}";
            }
        }
        
        // Subtitles
        public void ShowSubtitle(VoiceLine voiceLine)
        {
            if (!AudioManager.Instance.enableSubtitles) return;
            
            if (subtitlePanel != null && subtitleText != null)
            {
                subtitlePanel.SetActive(true);
                subtitleText.text = voiceLine.subtitleText;
                
                // Auto-hide after voice line duration
                if (subtitleCoroutine != null)
                    StopCoroutine(subtitleCoroutine);
                
                subtitleCoroutine = StartCoroutine(HideSubtitleAfterDelay(voiceLine.audioClip.length));
            }
        }
        
        public void HideSubtitle(VoiceLine voiceLine)
        {
            if (subtitlePanel != null)
            {
                subtitlePanel.SetActive(false);
            }
        }
        
        private IEnumerator HideSubtitleAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (subtitlePanel != null)
            {
                subtitlePanel.SetActive(false);
            }
        }
        
        // Button Events
        public void OnStartGameClicked()
        {
            OnGameStartRequested?.Invoke();
            
            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartGame(GameMode.EndlessRun); // Default to endless run
            }
        }
        
        public void OnRetryClicked()
        {
            OnRetryRequested?.Invoke();
            
            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartGame(GameManager.Instance.currentGameMode);
            }
        }
        
        public void OnMenuClicked()
        {
            OnMenuRequested?.Invoke();
            
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ReturnToMenu();
            }
        }
        
        public void OnQuitClicked()
        {
            Application.Quit();
        }
        
        // Event Handlers
        private void OnGameStateChanged(GameState newState)
        {
            switch (newState)
            {
                case GameState.Menu:
                    ShowMainMenu();
                    break;
                case GameState.Playing:
                    ShowHUD();
                    break;
                case GameState.GameOver:
                    ShowGameOver(GameManager.Instance.currentScore, GameManager.Instance.highScore);
                    break;
            }
        }
        
        void OnDestroy()
        {
            // Clean up event listeners
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged -= OnGameStateChanged;
                GameManager.Instance.OnScoreChanged -= UpdateScore;
            }
            
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.OnVoiceLineStarted -= ShowSubtitle;
                AudioManager.Instance.OnVoiceLineEnded -= HideSubtitle;
            }
        }
    }
    
    [System.Serializable]
    public class CharacterSelectButton
    {
        public Button button;
        public Image characterIcon;
        public GameObject selectedIndicator;
        
        public void SetSelected(bool selected)
        {
            if (selectedIndicator != null)
                selectedIndicator.SetActive(selected);
            
            if (button != null)
            {
                ColorBlock colors = button.colors;
                colors.normalColor = selected ? Color.yellow : Color.white;
                button.colors = colors;
            }
        }
    }
    
    [System.Serializable]
    public class CharacterUIData
    {
        public CharacterType characterType;
        public string characterName;
        public string description;
        public Sprite portraitSprite;
        public Sprite iconSprite;
    }
}