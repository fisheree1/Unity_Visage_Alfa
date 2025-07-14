using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverUI : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Button respawnButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button quitButton;
    
    [Header("Text Components")]
    [SerializeField] private TextMeshProUGUI gameOverText;
    [SerializeField] private TextMeshProUGUI respawnButtonText;
    [SerializeField] private TextMeshProUGUI restartButtonText;
    [SerializeField] private TextMeshProUGUI mainMenuButtonText;
    [SerializeField] private TextMeshProUGUI quitButtonText;
    
    [Header("Animation Settings")]
    [SerializeField] private float fadeInDuration = 1.0f;
    [SerializeField] private float scaleAnimationDuration = 0.5f;
    [SerializeField] private AnimationCurve scaleAnimationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip gameOverSound;
    [SerializeField] private AudioClip buttonClickSound;
    
    [Header("Scene Settings")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private int mainMenuSceneIndex = 0;
    [SerializeField] private bool useSceneIndex = false;
    
    // Components
    private CanvasGroup canvasGroup;
    private HeroLife heroLife;
    
    // State
    private bool isShowing = false;
    
    void Awake()
    {
        // 获取或添加CanvasGroup组件用于淡入淡出效果
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        // 初始化UI状态
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
            
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }
    
    void Start()
    {
        SetupButtons();
        SetupText();
        FindHeroLife();
    }
    
    void Update()
    {
        // 检测键盘输入
        if (isShowing)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                OnRestartButtonClicked();
            }
            else if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
            {
                OnRespawnButtonClicked();
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                OnMainMenuButtonClicked();
            }
        }
    }
    
    private void SetupButtons()
    {
        // 设置按钮点击事件
        if (respawnButton != null)
        {
            respawnButton.onClick.AddListener(OnRespawnButtonClicked);
        }
        
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(OnRestartButtonClicked);
        }
        
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(OnMainMenuButtonClicked);
        }
        
        if (quitButton != null)
        {
            quitButton.onClick.AddListener(OnQuitButtonClicked);
        }
    }
    
    private void SetupText()
    {
        // 设置默认文本（如果没有在Inspector中设置）
        if (gameOverText != null && string.IsNullOrEmpty(gameOverText.text))
        {
            gameOverText.text = "游戏结束";
        }
        
        if (respawnButtonText != null && string.IsNullOrEmpty(respawnButtonText.text))
        {
            respawnButtonText.text = "复活 (空格键)";
        }
        
        if (restartButtonText != null && string.IsNullOrEmpty(restartButtonText.text))
        {
            restartButtonText.text = "重新开始 (R键)";
        }
        
        if (mainMenuButtonText != null && string.IsNullOrEmpty(mainMenuButtonText.text))
        {
            mainMenuButtonText.text = "主菜单 (ESC键)";
        }
        
        if (quitButtonText != null && string.IsNullOrEmpty(quitButtonText.text))
        {
            quitButtonText.text = "退出游戏";
        }
    }
    
    private void FindHeroLife()
    {
        if (heroLife == null)
        {
            // 寻找HeroLife组件
            heroLife = FindObjectOfType<HeroLife>();
            
            if (heroLife == null)
            {
                Debug.LogWarning("GameOverUI: 无法找到HeroLife组件！");
            }
        }
    }
    
    public void ShowGameOverUI()
    {
        if (isShowing) return;
        
        isShowing = true;
        
        // 播放游戏结束音效
        PlayGameOverSound();
        
        // 显示面板
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
            
        // 根据英雄状态决定显示哪些按钮
        UpdateButtonVisibility();
        
        // 开始动画
        StartCoroutine(ShowAnimation());
    }
    
    public void HideGameOverUI()
    {
        if (!isShowing) return;
        
        StartCoroutine(HideAnimation());
    }
    
    private void UpdateButtonVisibility()
    {
        // 检查是否有有效的复活点
        bool hasRespawnPoint = heroLife != null && heroLife.HasValidRespawnPoint();
        
        // 复活按钮只在有复活点时显示
        if (respawnButton != null)
        {
            respawnButton.gameObject.SetActive(hasRespawnPoint);
        }
        
        // 重启按钮总是显示
        if (restartButton != null)
        {
            restartButton.gameObject.SetActive(true);
        }
        
        // 主菜单和退出按钮总是显示
        if (mainMenuButton != null)
        {
            mainMenuButton.gameObject.SetActive(true);
        }
        
        if (quitButton != null)
        {
            quitButton.gameObject.SetActive(true);
        }
    }
    
    private IEnumerator ShowAnimation()
    {
        // 确保UI可以交互
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        
        // 淡入效果
        float elapsedTime = 0f;
        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeInDuration);
            yield return null;
        }
        
        canvasGroup.alpha = 1f;
        
        // 缩放动画（可选）
        if (gameOverPanel != null)
        {
            Vector3 originalScale = gameOverPanel.transform.localScale;
            gameOverPanel.transform.localScale = Vector3.zero;
            
            elapsedTime = 0f;
            while (elapsedTime < scaleAnimationDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float progress = elapsedTime / scaleAnimationDuration;
                float scaleFactor = scaleAnimationCurve.Evaluate(progress);
                gameOverPanel.transform.localScale = originalScale * scaleFactor;
                yield return null;
            }
            
            gameOverPanel.transform.localScale = originalScale;
        }
    }
    
    private IEnumerator HideAnimation()
    {
        // 淡出效果
        float elapsedTime = 0f;
        while (elapsedTime < fadeInDuration * 0.5f)
        {
            elapsedTime += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / (fadeInDuration * 0.5f));
            yield return null;
        }
        
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
            
        isShowing = false;
    }
    
    private void PlayGameOverSound()
    {
        if (audioSource != null && gameOverSound != null)
        {
            audioSource.PlayOneShot(gameOverSound);
        }
    }
    
    private void PlayButtonClickSound()
    {
        if (audioSource != null && buttonClickSound != null)
        {
            audioSource.PlayOneShot(buttonClickSound);
        }
    }
    
    #region Button Event Handlers
    
    public void OnRespawnButtonClicked()
    {
        PlayButtonClickSound();
        
        if (heroLife != null && heroLife.HasValidRespawnPoint())
        {
            heroLife.Respawn();
            HideGameOverUI();
        }
        else
        {
            Debug.LogWarning("GameOverUI: 无法复活，没有有效的复活点！");
            // 如果没有复活点，则重启游戏
            OnRestartButtonClicked();
        }
    }
    
    public void OnRestartButtonClicked()
    {
        PlayButtonClickSound();
        
        // 恢复时间流速（以防被暂停）
        Time.timeScale = 1f;
        
        if (heroLife != null)
        {
            heroLife.RestartGame();
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
    
    public void OnMainMenuButtonClicked()
    {
        PlayButtonClickSound();
        
        // 恢复时间流速
        Time.timeScale = 1f;
        
        // 加载主菜单场景
        if (useSceneIndex)
        {
            SceneManager.LoadScene(mainMenuSceneIndex);
        }
        else
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }
    
    public void OnQuitButtonClicked()
    {
        PlayButtonClickSound();
        
        // 退出游戏
        Application.Quit();
        
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
    
    #endregion
    
    #region Public Methods
    
    /// <summary>
    /// 从外部调用显示游戏结束UI
    /// </summary>
    public void Show()
    {
        ShowGameOverUI();
    }
    
    /// <summary>
    /// 从外部调用隐藏游戏结束UI
    /// </summary>
    public void Hide()
    {
        HideGameOverUI();
    }
    
    /// <summary>
    /// 检查GameOverUI是否正在显示
    /// </summary>
    public bool IsShowing => isShowing;
    
    /// <summary>
    /// 设置游戏结束文本
    /// </summary>
    public void SetGameOverText(string text)
    {
        if (gameOverText != null)
        {
            gameOverText.text = text;
        }
    }
    
    #endregion
}
