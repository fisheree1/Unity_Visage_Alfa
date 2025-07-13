using UnityEngine;

/// <summary>
/// ��Ƶ������Դ - ��������AudioManager����Ƶ��Դ������
/// </summary>
[CreateAssetMenu(fileName = "AudioConfig", menuName = "Audio/Audio Configuration")]
public class AudioConfiguration : ScriptableObject
{
    [Header("��������")]
    [Tooltip("����̽��ʱ�ı�������")]
    public AudioClip normalBackgroundMusic;
    
    [Tooltip("��ͨս��ʱ�ı�������")]
    public AudioClip combatBackgroundMusic;
    
    [Tooltip("Bossս��ʱ�ı�������")]
    public AudioClip bossBackgroundMusic;
    
    [Header("��������")]
    [Range(0f, 1f)]
    [Tooltip("������")]
    public float masterVolume = 1f;
    
    [Range(0f, 1f)]
    [Tooltip("��������")]
    public float musicVolume = 0.7f;
    
    [Range(0f, 1f)]
    [Tooltip("��Ч����")]
    public float sfxVolume = 0.8f;
    
    [Header("��������")]
    [Tooltip("���ֵ��뵭��ʱ��")]
    public float fadeTime = 2f;
    
    [Tooltip("�Ƿ�����ƽ������")]
    public bool smoothTransition = true;
    
    [Header("ս�����")]
    [Tooltip("ս����ⷶΧ")]
    public float combatDetectionRange = 15f;
    
    [Tooltip("����ս���ӳ�ʱ��")]
    public float combatExitDelay = 3f;
    
    [Header("�㼶����")]
    [Tooltip("Boss�������ڵĲ㼶")]
    public LayerMask bossLayerMask = -1;
    
    [Tooltip("��ͨ�������ڵĲ㼶")]
    public LayerMask enemyLayerMask = -1;
    
    [Header("��ǩ����")]
    [Tooltip("Boss�ı�ǩ")]
    public string bossTag = "Boss";
    
    [Tooltip("���˵ı�ǩ")]
    public string[] enemyTags = { "Enemy" };
    
    [Header("��Ч��Դ")]
    [Tooltip("UI��Ч")]
    public AudioClip[] uiSounds;
    
    [Tooltip("������Ч")]
    public AudioClip[] environmentSounds;
    
    [Tooltip("ս����Ч")]
    public AudioClip[] combatSounds;
    
    [Header("��������")]
    [Tooltip("���õ�����־")]
    public bool enableDebugLogs = true;
    
    [Tooltip("��ʾGizmos")]
    public bool showGizmos = true;
    
    /// <summary>
    /// ��֤���õ���Ч��
    /// </summary>
    public bool IsValid()
    {
        bool isValid = true;
        
        if (normalBackgroundMusic == null)
        {
            Debug.LogWarning("AudioConfiguration: Normal background music is not assigned!");
            isValid = false;
        }
        
        if (combatBackgroundMusic == null)
        {
            Debug.LogWarning("AudioConfiguration: Combat background music is not assigned!");
            isValid = false;
        }
        
        if (bossBackgroundMusic == null)
        {
            Debug.LogWarning("AudioConfiguration: Boss background music is not assigned!");
            isValid = false;
        }
        
        if (fadeTime <= 0f)
        {
            Debug.LogWarning("AudioConfiguration: Fade time should be greater than 0!");
            isValid = false;
        }
        
        if (combatDetectionRange <= 0f)
        {
            Debug.LogWarning("AudioConfiguration: Combat detection range should be greater than 0!");
            isValid = false;
        }
        
        return isValid;
    }
    
    /// <summary>
    /// Ӧ�����õ�AudioManager
    /// </summary>
    public void ApplyToAudioManager(AudioManager audioManager)
    {
        if (audioManager == null)
        {
            Debug.LogError("AudioConfiguration: AudioManager is null!");
            return;
        }
        
        audioManager.SetMasterVolume(masterVolume);
        audioManager.SetMusicVolume(musicVolume);
        audioManager.SetSFXVolume(sfxVolume);
    }
    
    /// <summary>
    /// ��ȡָ�����͵���Ч
    /// </summary>
    public AudioClip GetSoundEffect(SoundType soundType, int index = 0)
    {
        AudioClip[] targetArray = null;
        
        switch (soundType)
        {
            case SoundType.UI:
                targetArray = uiSounds;
                break;
            case SoundType.Environment:
                targetArray = environmentSounds;
                break;
            case SoundType.Combat:
                targetArray = combatSounds;
                break;
        }
        
        if (targetArray != null && index >= 0 && index < targetArray.Length)
        {
            return targetArray[index];
        }
        
        return null;
    }
    
    /// <summary>
    /// ��ȡ�����Ч
    /// </summary>
    public AudioClip GetRandomSoundEffect(SoundType soundType)
    {
        AudioClip[] targetArray = null;
        
        switch (soundType)
        {
            case SoundType.UI:
                targetArray = uiSounds;
                break;
            case SoundType.Environment:
                targetArray = environmentSounds;
                break;
            case SoundType.Combat:
                targetArray = combatSounds;
                break;
        }
        
        if (targetArray != null && targetArray.Length > 0)
        {
            int randomIndex = Random.Range(0, targetArray.Length);
            return targetArray[randomIndex];
        }
        
        return null;
    }
}

/// <summary>
/// ��Ч����ö��
/// </summary>
public enum SoundType
{
    UI,           // UI������Ч
    Environment,  // ������Ч
    Combat        // ս����Ч
}