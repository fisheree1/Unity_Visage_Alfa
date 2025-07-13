using UnityEngine;

/// <summary>
/// AudioManagerʹ��ʾ�� - չʾ�������Ϸ�м�����Ƶϵͳ
/// </summary>
public class AudioManagerExample : MonoBehaviour
{
    [Header("ʾ����Ч")]
    [SerializeField] private AudioClip buttonClickSound;
    [SerializeField] private AudioClip itemPickupSound;
    [SerializeField] private AudioClip playerHurtSound;
    
    [Header("��������")]
    [SerializeField] private KeyCode testNormalMusicKey = KeyCode.Alpha1;
    [SerializeField] private KeyCode testCombatMusicKey = KeyCode.Alpha2;
    [SerializeField] private KeyCode testBossMusicKey = KeyCode.Alpha3;
    [SerializeField] private KeyCode testSFXKey = KeyCode.Space;
    
    void Update()
    {
        HandleTestInput();
    }
    
    private void HandleTestInput()
    {
        // ���������л�
        if (Input.GetKeyDown(testNormalMusicKey))
        {
            PlayNormalMusic();
        }
        
        if (Input.GetKeyDown(testCombatMusicKey))
        {
            PlayCombatMusic();
        }
        
        if (Input.GetKeyDown(testBossMusicKey))
        {
            PlayBossMusic();
        }
        
        // ������Ч����
        if (Input.GetKeyDown(testSFXKey))
        {
            PlayTestSFX();
        }
        
        // �������ڲ���
        if (Input.GetKey(KeyCode.LeftShift))
        {
            if (Input.GetKeyDown(KeyCode.Equals))
            {
                AdjustMasterVolume(0.1f);
            }
            else if (Input.GetKeyDown(KeyCode.Minus))
            {
                AdjustMasterVolume(-0.1f);
            }
        }
    }
    
    #region ���ֿ���ʾ��
    
    public void PlayNormalMusic()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayNormalMusic();
            Debug.Log("�л���������������");
        }
    }
    
    public void PlayCombatMusic()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayCombatMusic();
            Debug.Log("�л���ս����������");
        }
    }
    
    public void PlayBossMusic()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBossMusic();
            Debug.Log("�л���Bossս��������");
        }
    }
    
    #endregion
    
    #region ��Ч����ʾ��
    
    public void PlayButtonClickSFX()
    {
        if (AudioManager.Instance != null && buttonClickSound != null)
        {
            AudioManager.Instance.PlaySFX(buttonClickSound);
        }
    }
    
    public void PlayItemPickupSFX()
    {
        if (AudioManager.Instance != null && itemPickupSound != null)
        {
            AudioManager.Instance.PlaySFX(itemPickupSound, 0.8f); // 80%����
        }
    }
    
    public void PlayPlayerHurtSFX()
    {
        if (AudioManager.Instance != null && playerHurtSound != null)
        {
            AudioManager.Instance.PlaySFX(playerHurtSound, 1.2f); // 120%����
        }
    }
    
    public void PlayTestSFX()
    {
        // �������UI��Ч
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayRandomSFX(SoundType.UI);
            Debug.Log("�������UI��Ч");
        }
    }
    
    #endregion
    
    #region ��������ʾ��
    
    public void AdjustMasterVolume(float delta)
    {
        if (AudioManager.Instance != null)
        {
            float currentVolume = AudioManager.Instance.MasterVolume;
            float newVolume = Mathf.Clamp01(currentVolume + delta);
            AudioManager.Instance.SetMasterVolume(newVolume);
            Debug.Log($"����������Ϊ: {newVolume:F2}");
        }
    }
    
    public void SetMusicVolume(float volume)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(volume);
            Debug.Log($"������������Ϊ: {volume:F2}");
        }
    }
    
    public void SetSFXVolume(float volume)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSFXVolume(volume);
            Debug.Log($"��Ч��������Ϊ: {volume:F2}");
        }
    }
    
    #endregion
    
    #region UI����ʾ��
    
    /// <summary>
    /// UI��ť����¼�ʾ��
    /// </summary>
    public void OnUIButtonClick()
    {
        PlayButtonClickSFX();
        // ������ť�߼�...
    }
    
    /// <summary>
    /// ��������¼�ʾ��
    /// </summary>
    public void OnPlayerTakeDamage()
    {
        PlayPlayerHurtSFX();
        // ���������߼�...
    }
    
    /// <summary>
    /// ����ʰȡ�¼�ʾ��
    /// </summary>
    public void OnItemPickup()
    {
        PlayItemPickupSFX();
        // ����ʰȡ�߼�...
    }
    
    #endregion
    
    #region ״̬��ѯʾ��
    
    public void LogCurrentAudioState()
    {
        if (AudioManager.Instance != null)
        {
            Debug.Log("=== ��ǰ��Ƶ״̬ ===");
            Debug.Log($"�Ƿ���ս����: {AudioManager.Instance.IsInCombat}");
            Debug.Log($"�Ƿ���Bossս��: {AudioManager.Instance.IsBossBattle}");
            Debug.Log($"��ǰ����״̬: {AudioManager.Instance.CurrentMusicState}");
            Debug.Log($"������: {AudioManager.Instance.MasterVolume:F2}");
            Debug.Log($"��������: {AudioManager.Instance.MusicVolume:F2}");
            Debug.Log($"��Ч����: {AudioManager.Instance.SFXVolume:F2}");
        }
    }
    
    #endregion
    
    #region �¼�����ʾ��
    
    void Start()
    {
        // ����ս��״̬�仯�¼�
        CombatStateDetector detector = FindObjectOfType<CombatStateDetector>();
        if (detector != null)
        {
            detector.OnCombatStart += OnCombatStarted;
            detector.OnCombatEnd += OnCombatEnded;
            detector.OnBossBattleStart += OnBossBattleStarted;
            detector.OnBossBattleEnd += OnBossBattleEnded;
        }
        
        // ��ʾ������ʾ
        ShowControlsHelp();
    }
    
    void OnDestroy()
    {
        // ȡ���¼�����
        CombatStateDetector detector = FindObjectOfType<CombatStateDetector>();
        if (detector != null)
        {
            detector.OnCombatStart -= OnCombatStarted;
            detector.OnCombatEnd -= OnCombatEnded;
            detector.OnBossBattleStart -= OnBossBattleStarted;
            detector.OnBossBattleEnd -= OnBossBattleEnded;
        }
    }
    
    private void OnCombatStarted()
    {
        Debug.Log("?? ս����ʼ! �������л���ս��ģʽ");
        // �����������������ս����ʼ���߼�����UI�仯��
    }
    
    private void OnCombatEnded()
    {
        Debug.Log("? ս������! �����ѻָ�����");
        // �����������������ս���������߼�
    }
    
    private void OnBossBattleStarted()
    {
        Debug.Log("?? Bossս��ʼ! ʷʫ������������");
        // Bossս�����߼���������UI������Ч����
    }
    
    private void OnBossBattleEnded()
    {
        Debug.Log("?? Bossս����! ʤ��������!");
        // Bossս���������߼����罱����Ч��ʤ�������
    }
    
    #endregion
    
    private void ShowControlsHelp()
    {
        Debug.Log("=== AudioManager ���Կ��� ===");
        Debug.Log($"�� {testNormalMusicKey} - ������������");
        Debug.Log($"�� {testCombatMusicKey} - ����ս������");
        Debug.Log($"�� {testBossMusicKey} - ����Boss����");
        Debug.Log($"�� {testSFXKey} - ���Ų�����Ч");
        Debug.Log("�� Shift + Plus/Minus - ����������");
        Debug.Log("============================");
    }
}