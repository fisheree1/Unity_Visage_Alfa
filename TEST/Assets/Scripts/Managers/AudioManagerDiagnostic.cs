using UnityEngine;

/// <summary>
/// AudioManager��Ϲ��� - ר����Գ�ʼ���ֲ����������ǿ�汾
/// </summary>
public class AudioManagerDiagnostic : MonoBehaviour
{
    [Header("��Ͽ���")]
    [SerializeField] private bool autoRunDiagnostic = true;
    [SerializeField] private KeyCode diagnosticKey = KeyCode.F2;
    
    [Header("���Կ���")]
    [SerializeField] private KeyCode testNormalMusicKey = KeyCode.F3;
    [SerializeField] private KeyCode testCombatMusicKey = KeyCode.F4;
    [SerializeField] private KeyCode testBossMusicKey = KeyCode.F5;
    [SerializeField] private KeyCode forcePlayCurrentKey = KeyCode.F6; // ����
    
    void Start()
    {
        if (autoRunDiagnostic)
        {
            // �ӳ�2���������ϣ�ȷ��AudioManager��ȫ��ʼ��
            Invoke(nameof(RunFullDiagnostic), 2f);
        }
        
        Debug.Log("=== AudioManager��Ϲ��� (��ǿ��) ===");
        Debug.Log($"�� {diagnosticKey} ���������");
        Debug.Log($"�� {testNormalMusicKey} ��������������");
        Debug.Log($"�� {testCombatMusicKey} ������ս������");
        Debug.Log($"�� {testBossMusicKey} ������Boss����");
        Debug.Log($"�� {forcePlayCurrentKey} ��ǿ�Ʋ��ŵ�ǰ����");
        Debug.Log("====================================");
    }
    
    void Update()
    {
        if (Input.GetKeyDown(diagnosticKey))
        {
            RunFullDiagnostic();
        }
        
        if (Input.GetKeyDown(testNormalMusicKey))
        {
            TestNormalMusic();
        }
        
        if (Input.GetKeyDown(testCombatMusicKey))
        {
            TestCombatMusic();
        }
        
        if (Input.GetKeyDown(testBossMusicKey))
        {
            TestBossMusic();
        }
        
        if (Input.GetKeyDown(forcePlayCurrentKey))
        {
            ForcePlayCurrentMusic();
        }
    }
    
    [ContextMenu("�����������")]
    public void RunFullDiagnostic()
    {
        Debug.Log("========== AudioManager ��ʼ����������� ==========");
        
        CheckAudioManagerInstance();
        CheckInitializationStatus();
        CheckAudioConfiguration();
        CheckAudioSources();
        CheckAudioClips();
        CheckVolumeSettings();
        CheckCurrentState();
        CheckInitialMusicPlayback();
        
        Debug.Log("========== ������ ==========");
    }
    
    private void CheckAudioManagerInstance()
    {
        Debug.Log("--- ���AudioManagerʵ�� ---");
        
        if (AudioManager.Instance == null)
        {
            Debug.LogError("? AudioManager.Instance Ϊ��!");
            Debug.LogError("�������: ȷ����������AudioManager���");
            return;
        }
        
        Debug.Log("? AudioManagerʵ������");
        
        // ���GameObject״̬
        if (!AudioManager.Instance.gameObject.activeInHierarchy)
        {
            Debug.LogError("? AudioManager GameObjectδ����!");
            return;
        }
        
        Debug.Log("? AudioManager GameObject�Ѽ���");
    }
    
    private void CheckInitializationStatus()
    {
        Debug.Log("--- ����ʼ��״̬ ---");
        
        if (AudioManager.Instance == null) return;
        
        bool isInitialized = AudioManager.Instance.IsInitialized;
        bool hasPlayedInitialMusic = AudioManager.Instance.HasPlayedInitialMusic;
        
        Debug.Log($"AudioManager�ѳ�ʼ��: {(isInitialized ? "?" : "?")} {isInitialized}");
        Debug.Log($"�Ѳ��ų�ʼ����: {(hasPlayedInitialMusic ? "?" : "?")} {hasPlayedInitialMusic}");
        
        if (!isInitialized)
        {
            Debug.LogWarning("?? AudioManager��δ��ȫ��ʼ������ȴ�");
        }
        
        if (!hasPlayedInitialMusic)
        {
            Debug.LogError("? ��ʼ������δ���� - ������������!");
        }
    }
    
    private void CheckAudioConfiguration()
    {
        Debug.Log("--- �����Ƶ���� ---");
        
        if (AudioManager.Instance == null) return;
        
        var config = AudioManager.Instance.Config;
        if (config == null)
        {
            Debug.LogError("? AudioConfiguration δ����!");
            Debug.LogError("�������: ��AudioManager��Inspector�з���AudioConfiguration");
            return;
        }
        
        Debug.Log("? AudioConfiguration�ѷ���");
        
        // ���������Ч��
        if (!config.IsValid())
        {
            Debug.LogWarning("?? AudioConfiguration������Ч");
        }
        else
        {
            Debug.Log("? AudioConfiguration������Ч");
        }
    }
    
    private void CheckAudioSources()
    {
        Debug.Log("--- �����ƵԴ ---");
        
        if (AudioManager.Instance == null) return;
        
        // ����Ƿ����Ӷ���AudioSource
        var audioSources = AudioManager.Instance.GetComponentsInChildren<AudioSource>();
        
        Debug.Log($"�ҵ� {audioSources.Length} ��AudioSource���");
        
        foreach (var source in audioSources)
        {
            CheckAudioSource(source.name, source);
        }
        
        if (audioSources.Length == 0)
        {
            Debug.LogError("? δ�ҵ��κ�AudioSource���!");
        }
    }
    
    private void CheckAudioSource(string name, AudioSource source)
    {
        if (source == null)
        {
            Debug.LogError($"? {name} Ϊ��");
            return;
        }
        
        string status = source.isPlaying ? "?? ������" : "?? δ����";
        Debug.Log($"{status} {name}:");
        Debug.Log($"   - ����: {source.volume:F2}");
        Debug.Log($"   - ����: {(source.mute ? "?" : "?")} {source.mute}");
        Debug.Log($"   - ����: {(source.enabled ? "?" : "?")} {source.enabled}");
        Debug.Log($"   - ѭ��: {(source.loop ? "?" : "?")} {source.loop}");
        Debug.Log($"   - ��ǰ����: {(source.clip != null ? source.clip.name : "? ��")}");
        Debug.Log($"   - ���ڲ���: {(source.isPlaying ? "?" : "?")} {source.isPlaying}");
        
        // ��鳣������
        if (!source.enabled)
        {
            Debug.LogError($"   ?? �޸�����: ���� {name} AudioSource");
        }
        
        if (source.mute)
        {
            Debug.LogError($"   ?? �޸�����: ȡ�� {name} �ľ���");
        }
        
        if (source.volume <= 0.01f)
        {
            Debug.LogError($"   ?? �޸�����: ���� {name} ������");
        }
        
        if (source.clip == null)
        {
            Debug.LogError($"   ?? �޸�����: Ϊ {name} ������Ƶ����");
        }
    }
    
    private void CheckAudioClips()
    {
        Debug.Log("--- �����Ƶ���� ---");
        
        if (AudioManager.Instance?.Config == null) return;
        
        var config = AudioManager.Instance.Config;
        
        CheckAudioClip("������������", config.normalBackgroundMusic);
        CheckAudioClip("ս����������", config.combatBackgroundMusic);
        CheckAudioClip("Boss��������", config.bossBackgroundMusic);
    }
    
    private void CheckAudioClip(string name, AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogError($"? {name} δ����");
        }
        else
        {
            Debug.Log($"? {name}: {clip.name} (����: {clip.length:F1}��)");
        }
    }
    
    private void CheckVolumeSettings()
    {
        Debug.Log("--- ����������� ---");
        
        if (AudioManager.Instance == null) return;
        
        float masterVolume = AudioManager.Instance.MasterVolume;
        float musicVolume = AudioManager.Instance.MusicVolume;
        float sfxVolume = AudioManager.Instance.SFXVolume;
        
        Debug.Log($"������: {(masterVolume > 0.01f ? "?" : "?")} {masterVolume:F2}");
        Debug.Log($"��������: {(musicVolume > 0.01f ? "?" : "?")} {musicVolume:F2}");
        Debug.Log($"��Ч����: {(sfxVolume > 0.01f ? "?" : "?")} {sfxVolume:F2}");
        
        float finalMusicVolume = masterVolume * musicVolume;
        Debug.Log($"������������: {(finalMusicVolume > 0.01f ? "?" : "?")} {finalMusicVolume:F2}");
        
        if (finalMusicVolume <= 0.01f)
        {
            Debug.LogError("? �����������ͣ��޷���������!");
            Debug.LogError("?? �޸�����: ��������������������");
        }
    }
    
    private void CheckCurrentState()
    {
        Debug.Log("--- ��鵱ǰ״̬ ---");
        
        if (AudioManager.Instance == null) return;
        
        Debug.Log($"��ǰ����״̬: {AudioManager.Instance.CurrentMusicState}");
        Debug.Log($"�Ƿ���ս����: {AudioManager.Instance.IsInCombat}");
        Debug.Log($"�Ƿ���Bossս��: {AudioManager.Instance.IsBossBattle}");
    }
    
    private void CheckInitialMusicPlayback()
    {
        Debug.Log("--- ����ʼ���ֲ���״̬ ---");
        
        if (AudioManager.Instance == null) return;
        
        bool hasPlayed = AudioManager.Instance.HasPlayedInitialMusic;
        
        if (!hasPlayed)
        {
            Debug.LogError("? �ؼ�����: ��ʼ���ִ�δ����!");
            Debug.LogError("?? ����ԭ��:");
            Debug.LogError("   1. AudioConfigurationδ����");
            Debug.LogError("   2. Normal Background Musicδ����");
            Debug.LogError("   3. ��������Ϊ0");
            Debug.LogError("   4. AudioSourceδ��ȷ����");
            Debug.LogError($"?? �����޸�: �� {forcePlayCurrentKey} ��ǿ�Ʋ�������");
        }
        else
        {
            Debug.Log("? ��ʼ�����Ѳ���");
            
            // ��鵱ǰ�Ƿ��ڲ���
            var audioSources = AudioManager.Instance.GetComponentsInChildren<AudioSource>();
            bool anyPlaying = false;
            
            foreach (var source in audioSources)
            {
                if (source.isPlaying)
                {
                    anyPlaying = true;
                    Debug.Log($"? ��ǰ������: {source.name} - {(source.clip != null ? source.clip.name : "�޼���")}");
                }
            }
            
            if (!anyPlaying)
            {
                Debug.LogWarning("?? ��ʼ�����Ѳ��ŵ���ǰ����Ƶ�ڲ���");
                Debug.LogWarning("?? ����ԭ��: ���ֱ�����ֹͣ�����̫��");
            }
        }
    }
    
    #region ���Է���
    
    [ContextMenu("������������")]
    public void TestNormalMusic()
    {
        Debug.Log("?? ���Բ�����������...");
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayNormalMusic();
            CheckMusicPlayback("��������");
        }
    }
    
    [ContextMenu("����ս������")]
    public void TestCombatMusic()
    {
        Debug.Log("?? ���Բ���ս������...");
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayCombatMusic();
            CheckMusicPlayback("ս������");
        }
    }
    
    [ContextMenu("����Boss����")]
    public void TestBossMusic()
    {
        Debug.Log("?? ���Բ���Boss����...");
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBossMusic();
            CheckMusicPlayback("Boss����");
        }
    }
    
    [ContextMenu("ǿ�Ʋ��ŵ�ǰ����")]
    public void ForcePlayCurrentMusic()
    {
        Debug.Log("?? ǿ�Ʋ��ŵ�ǰ״̬����...");
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ForcePlayCurrentMusic();
            CheckMusicPlayback("ǿ�Ʋ���");
        }
        else
        {
            Debug.LogError("? AudioManager.Instance Ϊ�գ��޷�ǿ�Ʋ���!");
        }
    }
    
    private void CheckMusicPlayback(string musicType)
    {
        // �ӳټ�飬������һЩ����ʱ��
        Invoke(nameof(DelayedPlaybackCheck), 0.5f);
    }
    
    private void DelayedPlaybackCheck()
    {
        if (AudioManager.Instance == null) return;
        
        // �������ڲ��ŵ���ƵԴ
        var audioSources = AudioManager.Instance.GetComponentsInChildren<AudioSource>();
        bool anyPlaying = false;
        
        foreach (var source in audioSources)
        {
            if (source.isPlaying)
            {
                Debug.Log($"? ��⵽��Ƶ���ڲ���: {source.name}");
                Debug.Log($"   ����: {(source.clip != null ? source.clip.name : "��")}");
                Debug.Log($"   ����: {source.volume:F2}");
                anyPlaying = true;
            }
        }
        
        if (!anyPlaying)
        {
            Debug.LogError("? û�м�⵽���ڲ��ŵ���Ƶ!");
            Debug.LogError("?? ������:");
            Debug.LogError("   1. ��Ƶ�����Ƿ��ѷ���");
            Debug.LogError("   2. �����Ƿ����0");
            Debug.LogError("   3. AudioSource�Ƿ�����");
        }
    }
    
    #endregion
    
    #region �޸�����
    
    [ContextMenu("��ʾ��ʼ���������޸�����")]
    public void ShowInitialMusicFixSuggestions()
    {
        Debug.Log("========== ��ʼ���������޸����� ==========");
        Debug.Log("?? ����: ��ʼ״̬�����ֲ��ţ���״̬�л�������������");
        Debug.Log("");
        Debug.Log("?? �������:");
        Debug.Log("1. ���AudioConfiguration:");
        Debug.Log("   - ȷ���ѷ����AudioManager");
        Debug.Log("   - ȷ��Normal Background Music�ѷ���");
        Debug.Log("");
        Debug.Log("2. �����������:");
        Debug.Log("   - Master Volume > 0");
        Debug.Log("   - Music Volume > 0");
        Debug.Log("   - �������� = Master �� Music > 0.01");
        Debug.Log("");
        Debug.Log("3. ���AudioSource:");
        Debug.Log("   - ȷ����������δ����");
        Debug.Log("   - ȷ��ѭ��������ȷ");
        Debug.Log("");
        Debug.Log("4. ǿ�Ʋ��Ų���:");
        Debug.Log($"   - �� {forcePlayCurrentKey} ��ǿ�Ʋ��ŵ�ǰ����");
        Debug.Log($"   - �� {testNormalMusicKey} ��������������");
        Debug.Log("");
        Debug.Log("5. �����Ȼ��Ч:");
        Debug.Log("   - ��AudioManager�й�ѡ'Disable Smooth Transition'");
        Debug.Log("   - ʹ���ֶ���������ֱ�ӷ�����Ƶ����");
        Debug.Log("   - ���´���AudioManager��AudioConfiguration");
        Debug.Log("==========================================");
    }
    
    #endregion
}