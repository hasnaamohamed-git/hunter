using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace HunterRush.Audio
{
    /// <summary>
    /// Comprehensive audio management system for Hunter Rush: Nen Chronicles
    /// Handles music, sound effects, and character voice lines with anime-accurate audio
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        [Header("Audio Sources")]
        public AudioSource musicSource;
        public AudioSource sfxSource;
        public AudioSource voiceSource;
        public AudioSource ambientSource;
        
        [Header("Music Tracks")]
        public AudioClip mainMenuMusic;
        public AudioClip characterSelectMusic;
        public AudioClip[] battleMusic;
        public AudioClip[] bossMusic;
        public AudioClip victoryMusic;
        public AudioClip gameOverMusic;
        
        [Header("Sound Effects")]
        public AudioClip[] nenActivationSounds;
        public AudioClip[] impactSounds;
        public AudioClip[] jumpSounds;
        public AudioClip[] dashSounds;
        public AudioClip[] uiSounds;
        
        [Header("Character Voice Collections")]
        public CharacterVoiceCollection gonVoices;
        public CharacterVoiceCollection killuaVoices;
        public CharacterVoiceCollection kurapikaVoices;
        public CharacterVoiceCollection leorioVoices;
        
        [Header("Volume Settings")]
        [Range(0f, 1f)] public float masterVolume = 1f;
        [Range(0f, 1f)] public float musicVolume = 0.7f;
        [Range(0f, 1f)] public float sfxVolume = 1f;
        [Range(0f, 1f)] public float voiceVolume = 1f;
        [Range(0f, 1f)] public float ambientVolume = 0.5f;
        
        [Header("Audio Settings")]
        public bool enableVoiceLines = true;
        public bool enableSubtitles = true;
        public AudioLanguage currentLanguage = AudioLanguage.Japanese;
        
        // Singleton
        public static AudioManager Instance { get; private set; }
        
        // State tracking
        private AudioClip currentMusic;
        private Coroutine musicFadeCoroutine;
        private Dictionary<string, AudioClip> loadedSounds = new Dictionary<string, AudioClip>();
        private Queue<VoiceLine> voiceQueue = new Queue<VoiceLine>();
        private bool isPlayingVoice = false;
        
        // Events
        public System.Action<VoiceLine> OnVoiceLineStarted;
        public System.Action<VoiceLine> OnVoiceLineEnded;
        
        void Awake()
        {
            // Singleton pattern
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeAudioSources();
                LoadAudioSettings();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        void Start()
        {
            // Start with main menu music
            PlayMusic(mainMenuMusic, true);
        }
        
        void Update()
        {
            // Process voice queue
            ProcessVoiceQueue();
            
            // Update volume settings
            UpdateVolumeSettings();
        }
        
        private void InitializeAudioSources()
        {
            // Create audio sources if they don't exist
            if (musicSource == null)
            {
                GameObject musicGO = new GameObject("Music Source");
                musicGO.transform.SetParent(transform);
                musicSource = musicGO.AddComponent<AudioSource>();
                musicSource.loop = true;
                musicSource.playOnAwake = false;
            }
            
            if (sfxSource == null)
            {
                GameObject sfxGO = new GameObject("SFX Source");
                sfxGO.transform.SetParent(transform);
                sfxSource = sfxGO.AddComponent<AudioSource>();
                sfxSource.playOnAwake = false;
            }
            
            if (voiceSource == null)
            {
                GameObject voiceGO = new GameObject("Voice Source");
                voiceGO.transform.SetParent(transform);
                voiceSource = voiceGO.AddComponent<AudioSource>();
                voiceSource.playOnAwake = false;
            }
            
            if (ambientSource == null)
            {
                GameObject ambientGO = new GameObject("Ambient Source");
                ambientGO.transform.SetParent(transform);
                ambientSource = ambientGO.AddComponent<AudioSource>();
                ambientSource.loop = true;
                ambientSource.playOnAwake = false;
            }
        }
        
        private void LoadAudioSettings()
        {
            masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
            musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.7f);
            sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
            voiceVolume = PlayerPrefs.GetFloat("VoiceVolume", 1f);
            ambientVolume = PlayerPrefs.GetFloat("AmbientVolume", 0.5f);
            enableVoiceLines = PlayerPrefs.GetInt("EnableVoiceLines", 1) == 1;
            enableSubtitles = PlayerPrefs.GetInt("EnableSubtitles", 1) == 1;
            currentLanguage = (AudioLanguage)PlayerPrefs.GetInt("AudioLanguage", 0);
        }
        
        private void UpdateVolumeSettings()
        {
            if (musicSource != null)
                musicSource.volume = masterVolume * musicVolume;
            
            if (sfxSource != null)
                sfxSource.volume = masterVolume * sfxVolume;
            
            if (voiceSource != null)
                voiceSource.volume = masterVolume * voiceVolume;
            
            if (ambientSource != null)
                ambientSource.volume = masterVolume * ambientVolume;
        }
        
        // Music Control
        public void PlayMusic(AudioClip clip, bool loop = true, float fadeTime = 1f)
        {
            if (clip == null || clip == currentMusic) return;
            
            if (musicFadeCoroutine != null)
                StopCoroutine(musicFadeCoroutine);
            
            musicFadeCoroutine = StartCoroutine(FadeMusicCoroutine(clip, loop, fadeTime));
        }
        
        private IEnumerator FadeMusicCoroutine(AudioClip newClip, bool loop, float fadeTime)
        {
            // Fade out current music
            if (musicSource.isPlaying)
            {
                float startVolume = musicSource.volume;
                for (float t = 0; t < fadeTime * 0.5f; t += Time.deltaTime)
                {
                    musicSource.volume = Mathf.Lerp(startVolume, 0, t / (fadeTime * 0.5f));
                    yield return null;
                }
                musicSource.Stop();
            }
            
            // Play new music
            currentMusic = newClip;
            musicSource.clip = newClip;
            musicSource.loop = loop;
            musicSource.volume = 0;
            musicSource.Play();
            
            // Fade in new music
            float targetVolume = masterVolume * musicVolume;
            for (float t = 0; t < fadeTime * 0.5f; t += Time.deltaTime)
            {
                musicSource.volume = Mathf.Lerp(0, targetVolume, t / (fadeTime * 0.5f));
                yield return null;
            }
            
            musicSource.volume = targetVolume;
        }
        
        public void StopMusic(float fadeTime = 1f)
        {
            if (musicFadeCoroutine != null)
                StopCoroutine(musicFadeCoroutine);
            
            StartCoroutine(FadeOutMusicCoroutine(fadeTime));
        }
        
        private IEnumerator FadeOutMusicCoroutine(float fadeTime)
        {
            float startVolume = musicSource.volume;
            for (float t = 0; t < fadeTime; t += Time.deltaTime)
            {
                musicSource.volume = Mathf.Lerp(startVolume, 0, t / fadeTime);
                yield return null;
            }
            
            musicSource.Stop();
            musicSource.volume = masterVolume * musicVolume;
            currentMusic = null;
        }
        
        // Sound Effects
        public void PlaySFX(AudioClip clip, float volume = 1f, float pitch = 1f)
        {
            if (clip != null && sfxSource != null)
            {
                sfxSource.pitch = pitch;
                sfxSource.PlayOneShot(clip, volume);
            }
        }
        
        public void PlaySFX(string soundName, float volume = 1f, float pitch = 1f)
        {
            if (loadedSounds.ContainsKey(soundName))
            {
                PlaySFX(loadedSounds[soundName], volume, pitch);
            }
        }
        
        public void PlayRandomSFX(AudioClip[] clips, float volume = 1f, float pitch = 1f)
        {
            if (clips != null && clips.Length > 0)
            {
                AudioClip randomClip = clips[Random.Range(0, clips.Length)];
                PlaySFX(randomClip, volume, pitch);
            }
        }
        
        // Character-specific sound effects
        public void PlayNenActivation(NenState nenState)
        {
            if (nenActivationSounds.Length > 0)
            {
                int index = (int)nenState % nenActivationSounds.Length;
                PlaySFX(nenActivationSounds[index]);
            }
        }
        
        public void PlayImpactSound(float intensity = 1f)
        {
            if (impactSounds.Length > 0)
            {
                AudioClip clip = impactSounds[Random.Range(0, impactSounds.Length)];
                PlaySFX(clip, intensity);
            }
        }
        
        public void PlayJumpSound()
        {
            PlayRandomSFX(jumpSounds);
        }
        
        public void PlayDashSound()
        {
            PlayRandomSFX(dashSounds);
        }
        
        public void PlayUISound(UISound soundType)
        {
            if (uiSounds.Length > (int)soundType)
            {
                PlaySFX(uiSounds[(int)soundType]);
            }
        }
        
        // Voice Lines System
        public void PlayVoiceLine(CharacterType character, VoiceLineType lineType, bool interrupt = false)
        {
            if (!enableVoiceLines) return;
            
            CharacterVoiceCollection voices = GetCharacterVoices(character);
            if (voices == null) return;
            
            AudioClip[] availableLines = GetVoiceLines(voices, lineType);
            if (availableLines.Length == 0) return;
            
            AudioClip selectedLine = availableLines[Random.Range(0, availableLines.Length)];
            string subtitleText = GetSubtitleText(character, lineType, selectedLine);
            
            VoiceLine voiceLine = new VoiceLine
            {
                character = character,
                lineType = lineType,
                audioClip = selectedLine,
                subtitleText = subtitleText,
                interrupt = interrupt
            };
            
            if (interrupt)
            {
                // Clear queue and play immediately
                voiceQueue.Clear();
                StopVoiceLine();
                PlayVoiceLineImmediate(voiceLine);
            }
            else
            {
                // Add to queue
                voiceQueue.Enqueue(voiceLine);
            }
        }
        
        private void ProcessVoiceQueue()
        {
            if (!isPlayingVoice && voiceQueue.Count > 0)
            {
                VoiceLine nextLine = voiceQueue.Dequeue();
                PlayVoiceLineImmediate(nextLine);
            }
        }
        
        private void PlayVoiceLineImmediate(VoiceLine voiceLine)
        {
            if (voiceSource != null && voiceLine.audioClip != null)
            {
                voiceSource.clip = voiceLine.audioClip;
                voiceSource.Play();
                isPlayingVoice = true;
                
                OnVoiceLineStarted?.Invoke(voiceLine);
                
                if (enableSubtitles && !string.IsNullOrEmpty(voiceLine.subtitleText))
                {
                    ShowSubtitle(voiceLine.subtitleText, voiceLine.audioClip.length);
                }
                
                StartCoroutine(VoiceLineCoroutine(voiceLine));
            }
        }
        
        private IEnumerator VoiceLineCoroutine(VoiceLine voiceLine)
        {
            yield return new WaitForSeconds(voiceLine.audioClip.length);
            
            isPlayingVoice = false;
            OnVoiceLineEnded?.Invoke(voiceLine);
        }
        
        public void StopVoiceLine()
        {
            if (voiceSource != null && voiceSource.isPlaying)
            {
                voiceSource.Stop();
                isPlayingVoice = false;
            }
        }
        
        private CharacterVoiceCollection GetCharacterVoices(CharacterType character)
        {
            switch (character)
            {
                case CharacterType.Gon:
                    return gonVoices;
                case CharacterType.Killua:
                    return killuaVoices;
                case CharacterType.Kurapika:
                    return kurapikaVoices;
                case CharacterType.Leorio:
                    return leorioVoices;
                default:
                    return null;
            }
        }
        
        private AudioClip[] GetVoiceLines(CharacterVoiceCollection voices, VoiceLineType lineType)
        {
            switch (lineType)
            {
                case VoiceLineType.BattleCry:
                    return voices.battleCries;
                case VoiceLineType.Victory:
                    return voices.victoryLines;
                case VoiceLineType.Death:
                    return voices.deathLines;
                case VoiceLineType.SpecialAbility:
                    return voices.specialAbilityLines;
                case VoiceLineType.TakeDamage:
                    return voices.takeDamageLines;
                case VoiceLineType.Greeting:
                    return voices.greetingLines;
                default:
                    return new AudioClip[0];
            }
        }
        
        private string GetSubtitleText(CharacterType character, VoiceLineType lineType, AudioClip clip)
        {
            // In a full implementation, this would look up localized subtitle text
            // For now, return a placeholder based on character and line type
            return $"{character}: {lineType}";
        }
        
        private void ShowSubtitle(string text, float duration)
        {
            // This would interface with the UI system to display subtitles
            Debug.Log($"Subtitle: {text} (Duration: {duration}s)");
        }
        
        // Dynamic Music System
        public void PlayBattleMusic(bool isBoss = false)
        {
            AudioClip[] musicArray = isBoss ? bossMusic : battleMusic;
            if (musicArray.Length > 0)
            {
                AudioClip selectedMusic = musicArray[Random.Range(0, musicArray.Length)];
                PlayMusic(selectedMusic, true);
            }
        }
        
        public void PlayContextualMusic(GameState gameState)
        {
            switch (gameState)
            {
                case GameState.Menu:
                    PlayMusic(mainMenuMusic, true);
                    break;
                case GameState.Playing:
                    PlayBattleMusic(false);
                    break;
                case GameState.GameOver:
                    PlayMusic(gameOverMusic, false);
                    break;
            }
        }
        
        // Settings
        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat("MasterVolume", masterVolume);
        }
        
        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        }
        
        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        }
        
        public void SetVoiceVolume(float volume)
        {
            voiceVolume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat("VoiceVolume", voiceVolume);
        }
        
        public void SetVoiceLinesEnabled(bool enabled)
        {
            enableVoiceLines = enabled;
            PlayerPrefs.SetInt("EnableVoiceLines", enabled ? 1 : 0);
        }
        
        public void SetSubtitlesEnabled(bool enabled)
        {
            enableSubtitles = enabled;
            PlayerPrefs.SetInt("EnableSubtitles", enabled ? 1 : 0);
        }
        
        public void SetAudioLanguage(AudioLanguage language)
        {
            currentLanguage = language;
            PlayerPrefs.SetInt("AudioLanguage", (int)language);
            
            // Reload voice collections for new language
            ReloadVoiceCollections();
        }
        
        private void ReloadVoiceCollections()
        {
            // In a full implementation, this would load different voice collections
            // based on the selected language
        }
        
        public void SaveAudioSettings()
        {
            PlayerPrefs.Save();
        }
    }
    
    [System.Serializable]
    public class CharacterVoiceCollection
    {
        public AudioClip[] battleCries;
        public AudioClip[] victoryLines;
        public AudioClip[] deathLines;
        public AudioClip[] specialAbilityLines;
        public AudioClip[] takeDamageLines;
        public AudioClip[] greetingLines;
    }
    
    public struct VoiceLine
    {
        public CharacterType character;
        public VoiceLineType lineType;
        public AudioClip audioClip;
        public string subtitleText;
        public bool interrupt;
    }
    
    public enum VoiceLineType
    {
        BattleCry,
        Victory,
        Death,
        SpecialAbility,
        TakeDamage,
        Greeting
    }
    
    public enum AudioLanguage
    {
        Japanese,
        English,
        Spanish,
        French
    }
    
    public enum UISound
    {
        ButtonClick,
        ButtonHover,
        MenuOpen,
        MenuClose,
        Error,
        Success
    }
}