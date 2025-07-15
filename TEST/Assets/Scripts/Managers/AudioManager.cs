using UnityEngine;
using System.Collections;

/// <summary>
/// �Ľ�����Ƶ������ - �޸��汾��������ֲ����ŵ�����
/// ��ǿ�˴�����͵��Թ���
/// </summary>
public class AudioManager : MonoBehaviour
{
    [Header("�����ļ�")]
    [SerializeField] private AudioConfiguration audioConfig;
    
    [Header("��ƵԴ����")]
    [SerializeField] private AudioSource backgroundMusicSource;
    [SerializeField] private AudioSource combatMusicSource;
    [SerializeField] private AudioSource sfxSource;
    
    [Header("�������")]
    [SerializeField] private CombatStateDetector combatDetector;
    
    [Header("�ֶ��������ã���ѡ��")]
    [SerializeField] private AudioClip manualNormalMusic;
    [SerializeField] private AudioClip manualCombatMusic;
    [SerializeField] private AudioClip manualBossMusic;
    
    [Header("��������")]
    [SerializeField] private bool forceEnableDebugLogs = true;
    [SerializeField] private bool disableSmoothTransition = false;
    
    // ����ģʽ
    public static AudioManager Instance { get; private set; }
    
    // ��ǰ״̬
    private MusicState currentMusicState = MusicState.Normal;
    private bool isInitialized = false;
    private bool hasPlayedInitialMusic = false; // �����������Ƿ񲥷��˳�ʼ����
    
    // Э������
    private Coroutine fadeCoroutine;
    
    // ����״̬ö��
    public enum MusicState
    {
        Normal,     // ����̽������
        Combat,     // ս������
        Boss        // Bossս����
    }
    
    void Awake()
    {
        // ����ģʽ����
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            DebugLog("AudioManager Awake - Instance created");
        }
        else
        {
            DebugLog("AudioManager Awake - Duplicate instance destroyed");
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        DebugLog("AudioManager Start - Beginning initialization");
        StartCoroutine(DelayedInitialization());
    }
    
    private IEnumerator DelayedInitialization()
    {
        // �ȴ�һ֡ȷ�������������׼����
        yield return null;
        
        InitializeAudioManager();
        SetupCombatDetection();
        
        // �ӳٲ������֣�ȷ���������ö������
        yield return new WaitForSeconds(0.1f);
        
        DebugLog("Starting initial normal music...");
        ForcePlayInitialMusic();
    }
    
    private void ForcePlayInitialMusic()
    {
        DebugLog("ForcePlayInitialMusic called");
        
        // ǿ�Ʋ��ų�ʼ���֣��ƹ�״̬���
        AudioClip initialClip = GetMusicClip(MusicState.Normal);
        
        if (initialClip == null)
        {
            Debug.LogError("No initial music clip available!");
            return;
        }
        
        DebugLog($"Force playing initial music: {initialClip.name}");
        
        // ֱ�Ӳ��ţ�������SwitchMusic��״̬���
        PlayMusicDirectly(initialClip);
        hasPlayedInitialMusic = true;
        
        DebugLog("Initial music playback completed");
    }
    
    private void PlayMusicDirectly(AudioClip clip)
    {
        if (backgroundMusicSource == null || clip == null) return;
        
        DebugLog($"PlayMusicDirectly: {clip.name}");
        
        backgroundMusicSource.Stop();
        backgroundMusicSource.clip = clip;
        backgroundMusicSource.volume = GetMusicVolume();
        backgroundMusicSource.Play();
        
        DebugLog($"Direct play - Source playing: {backgroundMusicSource.isPlaying}, Volume: {backgroundMusicSource.volume}");
        
        // ��֤����״̬
        StartCoroutine(VerifyPlayback(backgroundMusicSource, clip.name));
    }
    
    private void InitializeAudioManager()
    {
        DebugLog("InitializeAudioManager started");
        
        // ��֤�򴴽���ƵԴ
        SetupAudioSources();
        
        // ��������
        LoadConfiguration();
        
        // Ӧ�ó�ʼ����
        ApplyVolumeSettings();
        
        isInitialized = true;
        DebugLog("AudioManager initialized successfully");
        
        // ������֤����
        ValidateSetup();
    }
    
    private void ValidateSetup()
    {
        DebugLog("=== AudioManager Setup Validation ===");
        
        if (backgroundMusicSource == null)
        {
            Debug.LogError("Background music source is null!");
        }
        else
        {
            DebugLog($"Background music source: {backgroundMusicSource.name}, volume: {backgroundMusicSource.volume}");
        }
        
        if (audioConfig == null)
        {
            Debug.LogWarning("Audio configuration is null!");
        }
        else
        {
            DebugLog($"Audio config assigned: {audioConfig.name}");
            DebugLog($"Normal music: {(audioConfig.normalBackgroundMusic != null ? audioConfig.normalBackgroundMusic.name : "null")}");
            DebugLog($"Combat music: {(audioConfig.combatBackgroundMusic != null ? audioConfig.combatBackgroundMusic.name : "null")}");
            DebugLog($"Boss music: {(audioConfig.bossBackgroundMusic != null ? audioConfig.bossBackgroundMusic.name : "null")}");
        }
        
        DebugLog("=== Validation Complete ===");
    }
    
    private void SetupAudioSources()
    {
        DebugLog("Setting up audio sources...");
        
        // ��������Դ
        if (backgroundMusicSource == null)
        {
            GameObject bgObj = new GameObject("BackgroundMusic");
            bgObj.transform.SetParent(transform);
            backgroundMusicSource = bgObj.AddComponent<AudioSource>();
            DebugLog("Created background music source");
        }
        ConfigureAudioSource(backgroundMusicSource, true, "Background");
        
        // ս������Դ
        if (combatMusicSource == null)
        {
            GameObject combatObj = new GameObject("CombatMusic");
            combatObj.transform.SetParent(transform);
            combatMusicSource = combatObj.AddComponent<AudioSource>();
            DebugLog("Created combat music source");
        }
        ConfigureAudioSource(combatMusicSource, true, "Combat");
        
        // ��ЧԴ
        if (sfxSource == null)
        {
            GameObject sfxObj = new GameObject("SFXAudio");
            sfxObj.transform.SetParent(transform);
            sfxSource = sfxObj.AddComponent<AudioSource>();
            DebugLog("Created SFX source");
        }
        ConfigureAudioSource(sfxSource, false, "SFX");
    }
    
    private void ConfigureAudioSource(AudioSource source, bool isMusic, string name)
    {
        if (source == null) return;
        
        source.loop = isMusic;
        source.playOnAwake = false;
        source.spatialBlend = 0f; // 2D��Ƶ
        source.enabled = true;
        source.mute = false;
        
        DebugLog($"Configured {name} audio source - Loop: {source.loop}, Enabled: {source.enabled}");
    }
    
    private void LoadConfiguration()
    {
        if (audioConfig != null)
        {
            if (!audioConfig.IsValid())
            {
                Debug.LogWarning("AudioManager: Audio configuration is not valid!");
            }
            DebugLog("Audio configuration loaded successfully");
        }
        else
        {
            Debug.LogWarning("AudioManager: No audio configuration assigned, using manual settings");
        }
    }
    
    private void SetupCombatDetection()
    {
        // ���һ򴴽�ս��״̬�����
        if (combatDetector == null)
        {
            combatDetector = FindObjectOfType<CombatStateDetector>();
        }
        
        if (combatDetector == null)
        {
            GameObject detectorObj = new GameObject("CombatStateDetector");
            detectorObj.transform.SetParent(transform);
            combatDetector = detectorObj.AddComponent<CombatStateDetector>();
            DebugLog("Created CombatStateDetector automatically");
        }
        
        // ����ս��״̬�¼�
        combatDetector.OnCombatStart += OnCombatStart;
        combatDetector.OnCombatEnd += OnCombatEnd;
        combatDetector.OnBossBattleStart += OnBossBattleStart;
        combatDetector.OnBossBattleEnd += OnBossBattleEnd;
        
        DebugLog("Combat detection events subscribed");
    }
    
    #region ս��״̬�¼�����
    
    private void OnCombatStart()
    {
        DebugLog("Combat started - switching to combat music");
        SwitchMusic(MusicState.Combat);
    }
    
    private void OnCombatEnd()
    {
        DebugLog("Combat ended - switching to normal music");
        SwitchMusic(MusicState.Normal);
    }
    
    private void OnBossBattleStart()
    {
        DebugLog("Boss battle started - switching to boss music");
        SwitchMusic(MusicState.Boss);
    }
    
    private void OnBossBattleEnd()
    {
        DebugLog("Boss battle ended");
        // ���������ͨս���У�����ս�����֣������л�����������
        if (combatDetector != null && combatDetector.IsInCombat && !combatDetector.IsInBossBattle)
        {
            SwitchMusic(MusicState.Combat);
        }
        else
        {
            SwitchMusic(MusicState.Normal);
        }
    }
    
    #endregion
    
    #region �����л�ϵͳ
    
    public void SwitchMusic(MusicState newState)
    {
        DebugLog($"SwitchMusic called: {currentMusicState} -> {newState}");
        
        if (!isInitialized)
        {
            DebugLog("AudioManager not initialized yet, queuing music switch");
            StartCoroutine(WaitForInitializationAndPlay(newState));
            return;
        }
        
        // �޸��������ʼ���ֲ��ţ���ʹ״̬��ͬ
        bool isInitialPlay = !hasPlayedInitialMusic && newState == MusicState.Normal;
        
        if (newState == currentMusicState && !isInitialPlay) 
        {
            DebugLog("Same music state, skipping switch (unless initial play)");
            return;
        }
        
        MusicState oldState = currentMusicState;
        currentMusicState = newState;
        
        DebugLog($"Switching music from {oldState} to {newState} (Initial: {isInitialPlay})");
        
        AudioClip newClip = GetMusicClip(newState);
        
        if (newClip == null)
        {
            Debug.LogError($"No music clip assigned for state: {newState}");
            DebugLog("Available clips:");
            DebugLog($"  Manual Normal: {(manualNormalMusic != null ? manualNormalMusic.name : "null")}");
            DebugLog($"  Manual Combat: {(manualCombatMusic != null ? manualCombatMusic.name : "null")}");
            DebugLog($"  Manual Boss: {(manualBossMusic != null ? manualBossMusic.name : "null")}");
            if (audioConfig != null)
            {
                DebugLog($"  Config Normal: {(audioConfig.normalBackgroundMusic != null ? audioConfig.normalBackgroundMusic.name : "null")}");
                DebugLog($"  Config Combat: {(audioConfig.combatBackgroundMusic != null ? audioConfig.combatBackgroundMusic.name : "null")}");
                DebugLog($"  Config Boss: {(audioConfig.bossBackgroundMusic != null ? audioConfig.bossBackgroundMusic.name : "null")}");
            }
            return;
        }
        
        DebugLog($"Playing clip: {newClip.name}");
        
        // ��һ�β��Ż����ƽ������ʱֱ�Ӳ���
        bool shouldFade = GetSmoothTransitionSetting() && !disableSmoothTransition && hasPlayedInitialMusic;
        
        if (shouldFade)
        {
            DebugLog("Using smooth transition");
            StartSmoothTransition(newClip);
        }
        else
        {
            DebugLog("Playing music immediately");
            PlayMusicImmediately(newClip);
        }
        
        hasPlayedInitialMusic = true; // ����Ѳ�������
    }
    
    private IEnumerator WaitForInitializationAndPlay(MusicState state)
    {
        while (!isInitialized)
        {
            yield return null;
        }
        
        DebugLog($"Initialization complete, now playing queued music: {state}");
        SwitchMusic(state);
    }
    
    private AudioClip GetMusicClip(MusicState state)
    {
        AudioClip clip = null;
        
        // ����ʹ���ֶ����ã�Ȼ��ʹ�������ļ�
        switch (state)
        {
            case MusicState.Normal:
                clip = manualNormalMusic != null ? manualNormalMusic : 
                       (audioConfig != null ? audioConfig.normalBackgroundMusic : null);
                break;
                       
            case MusicState.Combat:
                clip = manualCombatMusic != null ? manualCombatMusic : 
                       (audioConfig != null ? audioConfig.combatBackgroundMusic : null);
                break;
                       
            case MusicState.Boss:
                clip = manualBossMusic != null ? manualBossMusic : 
                       (audioConfig != null ? audioConfig.bossBackgroundMusic : null);
                break;
        }
        
        DebugLog($"GetMusicClip({state}) = {(clip != null ? clip.name : "null")}");
        return clip;
    }
    
    private bool GetSmoothTransitionSetting()
    {
        return audioConfig != null ? audioConfig.smoothTransition : true;
    }
    
    private float GetFadeTime()
    {
        return audioConfig != null ? audioConfig.fadeTime : 2f;
    }
    
    private void StartSmoothTransition(AudioClip newClip)
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        
        fadeCoroutine = StartCoroutine(FadeTransition(newClip));
    }
    
    private IEnumerator FadeTransition(AudioClip newClip)
    {
        AudioSource currentSource = GetCurrentMusicSource();
        AudioSource targetSource = GetAlternateMusicSource();
        
        float fadeTime = GetFadeTime();
        
        DebugLog($"Starting fade transition over {fadeTime} seconds");
        
        // ����������
        targetSource.clip = newClip;
        targetSource.volume = 0f;
        targetSource.Play();
        
        DebugLog($"Target source playing: {targetSource.isPlaying}");
        
        float timer = 0f;
        float startVolume = currentSource.volume;
        float targetVolume = GetMusicVolume();
        
        // ���������֣�����������
        while (timer < fadeTime)
        {
            timer += Time.deltaTime;
            float progress = timer / fadeTime;
            
            currentSource.volume = Mathf.Lerp(startVolume, 0f, progress);
            targetSource.volume = Mathf.Lerp(0f, targetVolume, progress);
            
            yield return null;
        }
        
        // ֹͣ������
        currentSource.Stop();
        currentSource.volume = targetVolume;
        
        // ������Դ��ɫ
        SwapMusicSources();
        
        DebugLog("Fade transition completed");
        fadeCoroutine = null;
    }
    
    private void PlayMusicImmediately(AudioClip newClip)
    {
        AudioSource currentSource = GetCurrentMusicSource();
        
        DebugLog($"Playing immediately on source: {currentSource.name}");
        
        currentSource.Stop();
        currentSource.clip = newClip;
        currentSource.volume = GetMusicVolume();
        currentSource.Play();
        
        DebugLog($"Source playing: {currentSource.isPlaying}, Volume: {currentSource.volume}");
        
        // ��֤����״̬
        StartCoroutine(VerifyPlayback(currentSource, newClip.name));
    }
    
    private IEnumerator VerifyPlayback(AudioSource source, string clipName)
    {
        yield return new WaitForSeconds(0.1f);
        
        if (source.isPlaying)
        {
            DebugLog($"? Verified: {clipName} is playing successfully");
        }
        else
        {
            Debug.LogError($"? Failed: {clipName} is not playing!");
            Debug.LogError($"   Source enabled: {source.enabled}");
            Debug.LogError($"   Source muted: {source.mute}");
            Debug.LogError($"   Source volume: {source.volume}");
            Debug.LogError($"   Source clip: {(source.clip != null ? source.clip.name : "null")}");
        }
    }
    
    private AudioSource GetCurrentMusicSource()
    {
        return backgroundMusicSource;
    }
    
    private AudioSource GetAlternateMusicSource()
    {
        return combatMusicSource;
    }
    
    private void SwapMusicSources()
    {
        AudioSource temp = backgroundMusicSource;
        backgroundMusicSource = combatMusicSource;
        combatMusicSource = temp;
        
        DebugLog("Music sources swapped");
    }
    
    #endregion
    
    #region ���������ֿ��Ʒ���
    
    public void PlayNormalMusic()
    {
        DebugLog("PlayNormalMusic() called");
        SwitchMusic(MusicState.Normal);
    }
    
    public void PlayCombatMusic()
    {
        DebugLog("PlayCombatMusic() called");
        SwitchMusic(MusicState.Combat);
    }
    
    public void PlayBossMusic()
    {
        DebugLog("PlayBossMusic() called");
        SwitchMusic(MusicState.Boss);
    }
    
    // ������ǿ�����²��ŵ�ǰ״̬������
    public void ForcePlayCurrentMusic()
    {
        DebugLog("ForcePlayCurrentMusic() called");
        hasPlayedInitialMusic = false; // ���ñ�־������ǿ�Ʋ���
        SwitchMusic(currentMusicState);
    }
    
    public void PlaySFX(AudioClip clip, float volumeScale = 1f)
    {
        if (clip != null && sfxSource != null)
        {
            float volume = GetSFXVolume() * volumeScale;
            sfxSource.PlayOneShot(clip, volume);
            DebugLog($"Playing SFX: {clip.name} at volume {volume}");
        }
    }
    
    public void PlayUISFX(int index = 0)
    {
        if (audioConfig != null)
        {
            AudioClip clip = audioConfig.GetSoundEffect(SoundType.UI, index);
            PlaySFX(clip);
        }
    }
    
    public void PlayEnvironmentSFX(int index = 0)
    {
        if (audioConfig != null)
        {
            AudioClip clip = audioConfig.GetSoundEffect(SoundType.Environment, index);
            PlaySFX(clip);
        }
    }
    
    public void PlayCombatSFX(int index = 0)
    {
        if (audioConfig != null)
        {
            AudioClip clip = audioConfig.GetSoundEffect(SoundType.Combat, index);
            PlaySFX(clip);
        }
    }
    
    public void PlayRandomSFX(SoundType soundType)
    {
        if (audioConfig != null)
        {
            AudioClip clip = audioConfig.GetRandomSoundEffect(soundType);
            PlaySFX(clip);
        }
    }
    
    #endregion
    
    #region ��������
    
    public void SetMasterVolume(float volume)
    {
        if (audioConfig != null)
        {
            audioConfig.masterVolume = Mathf.Clamp01(volume);
        }
        ApplyVolumeSettings();
        DebugLog($"Master volume set to: {volume}");
    }
    
    public void SetMusicVolume(float volume)
    {
        if (audioConfig != null)
        {
            audioConfig.musicVolume = Mathf.Clamp01(volume);
        }
        ApplyVolumeSettings();
        DebugLog($"Music volume set to: {volume}");
    }
    
    public void SetSFXVolume(float volume)
    {
        if (audioConfig != null)
        {
            audioConfig.sfxVolume = Mathf.Clamp01(volume);
        }
        ApplyVolumeSettings();
        DebugLog($"SFX volume set to: {volume}");
    }
    
    private void ApplyVolumeSettings()
    {
        float musicVol = GetMusicVolume();
        float sfxVol = GetSFXVolume();
        
        if (backgroundMusicSource != null)
            backgroundMusicSource.volume = musicVol;
        
        if (combatMusicSource != null)
            combatMusicSource.volume = musicVol;
        
        if (sfxSource != null)
            sfxSource.volume = sfxVol;
        
        DebugLog($"Applied volumes - Music: {musicVol:F2}, SFX: {sfxVol:F2}");
    }
    
    private float GetMasterVolume()
    {
        return audioConfig != null ? audioConfig.masterVolume : 1f;
    }
    
    private float GetMusicVolume()
    {
        float master = GetMasterVolume();
        float music = audioConfig != null ? audioConfig.musicVolume : 0.7f;
        return master * music;
    }
    
    private float GetSFXVolume()
    {
        float master = GetMasterVolume();
        float sfx = audioConfig != null ? audioConfig.sfxVolume : 0.8f;
        return master * sfx;
    }
    
    #endregion
    
    #region ���ſ���
    
    public void PauseMusic()
    {
        if (backgroundMusicSource != null && backgroundMusicSource.isPlaying)
            backgroundMusicSource.Pause();
        
        if (combatMusicSource != null && combatMusicSource.isPlaying)
            combatMusicSource.Pause();
        
        DebugLog("Music paused");
    }
    
    public void ResumeMusic()
    {
        if (backgroundMusicSource != null && backgroundMusicSource.clip != null)
            backgroundMusicSource.UnPause();
        
        if (combatMusicSource != null && combatMusicSource.clip != null)
            combatMusicSource.UnPause();
        
        DebugLog("Music resumed");
    }
    
    public void StopAllMusic()
    {
        if (backgroundMusicSource != null)
            backgroundMusicSource.Stop();
        
        if (combatMusicSource != null)
            combatMusicSource.Stop();
        
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }
        
        DebugLog("All music stopped");
    }
    
    #endregion
    
    #region ���Ժ�����
    
    private void DebugLog(string message)
    {
        bool enableDebug = forceEnableDebugLogs || (audioConfig != null ? audioConfig.enableDebugLogs : true);
        if (enableDebug)
        {
            Debug.Log($"[AudioManager] {message}");
        }
    }
    
    void OnDestroy()
    {
        // �����¼�����
        if (combatDetector != null)
        {
            combatDetector.OnCombatStart -= OnCombatStart;
            combatDetector.OnCombatEnd -= OnCombatEnd;
            combatDetector.OnBossBattleStart -= OnBossBattleStart;
            combatDetector.OnBossBattleEnd -= OnBossBattleEnd;
        }
        
        DebugLog("AudioManager destroyed and cleaned up");
    }
    
    #endregion
    
    #region ��������
    
    public bool IsInCombat => combatDetector != null ? combatDetector.IsInCombat : false;
    public bool IsBossBattle => combatDetector != null ? combatDetector.IsInBossBattle : false;
    public MusicState CurrentMusicState => currentMusicState;
    public float MasterVolume => GetMasterVolume();
    public float MusicVolume => audioConfig != null ? audioConfig.musicVolume : 0.7f;
    public float SFXVolume => audioConfig != null ? audioConfig.sfxVolume : 0.8f;
    public AudioConfiguration Config => audioConfig;
    public bool IsInitialized => isInitialized;
    public bool HasPlayedInitialMusic => hasPlayedInitialMusic; // ������������
    
    #endregion
}