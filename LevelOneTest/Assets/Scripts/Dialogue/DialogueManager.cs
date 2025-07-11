using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI speakerNameText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Image speakerPortrait;
    [SerializeField] private Button nextButton;
    [SerializeField] private GameObject choicesPanel;
    [SerializeField] private Button[] choiceButtons;
    [SerializeField] private TextMeshProUGUI[] choiceTexts;
    
    [Header("Typing Settings")]
    [SerializeField] private float typingSpeed = 0.05f;
    [SerializeField] private bool allowSkipTyping = true;
    
    [Header("Audio Settings")]
    [SerializeField] private AudioSource typingAudioSource;
    [SerializeField] private AudioClip typingSound;
    
    // State
    private DialogueData currentDialogue;
    private int currentLineIndex = 0;
    private bool isTyping = false;
    private bool isDialogueActive = false;
    private Coroutine typingCoroutine;
    private System.Action onDialogueComplete;
    private PlayerChoice selectedChoice;
    
    // Singleton pattern
    public static DialogueManager Instance { get; private set; }
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        // Initialize UI
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
            
        if (choicesPanel != null)
            choicesPanel.SetActive(false);
            
        // Setup choice button events automatically
        SetupChoiceButtons();
    }
    
    private void SetupChoiceButtons()
    {
        // Setup choice button events
        if (choiceButtons != null)
        {
            for (int i = 0; i < choiceButtons.Length; i++)
            {
                int index = i; // Avoid closure issues
                if (choiceButtons[i] != null)
                {
                    choiceButtons[i].onClick.RemoveAllListeners();
                    choiceButtons[i].onClick.AddListener(() => SelectChoice(index));
                }
            }
        }
    }
    
    void Update()
    {
        if (isDialogueActive)
        {
            // Press E key or Spacebar to continue dialogue
            if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space))
            {
                if (isTyping && allowSkipTyping)
                {
                    // Skip typing animation
                    StopTyping();
                    CompleteCurrentLine();
                }
                else if (!isTyping)
                {
                    // Continue to next line
                    NextLine();
                }
            }
            
            // Press ESC key to close dialogue
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                EndDialogue();
            }
        }
    }
    
    public void StartDialogue(DialogueData dialogue, System.Action onComplete = null)
    {
        currentDialogue = dialogue;
        currentLineIndex = 0;
        onDialogueComplete = onComplete;
        isDialogueActive = true;
        
        // Enable dialogue panel
        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);
            
        // Disable player movement
        DisablePlayerMovement();
        
        // Display dialogue
        DisplayCurrentLine();
    }
    
    public void NextLine()
    {
        if (isTyping) return;
        
        if (currentDialogue.type == DialogueType.WithChoices && currentLineIndex >= currentDialogue.dialogueLines.Length - 1)
        {
            // Show choices
            ShowChoices();
            return;
        }
        
        currentLineIndex++;
        
        if (currentLineIndex < currentDialogue.dialogueLines.Length)
        {
            DisplayCurrentLine();
        }
        else
        {
            // Dialogue ended
            EndDialogue();
        }
    }
    
    private void DisplayCurrentLine()
    {
        if (currentDialogue == null || currentLineIndex >= currentDialogue.dialogueLines.Length)
            return;
            
        // Set speaker information
        if (speakerNameText != null)
            speakerNameText.text = currentDialogue.speakerName;
            
        if (speakerPortrait != null && currentDialogue.speakerPortrait != null)
        {
            speakerPortrait.sprite = currentDialogue.speakerPortrait;
            speakerPortrait.gameObject.SetActive(true);
        }
        else if (speakerPortrait != null)
        {
            speakerPortrait.gameObject.SetActive(false);
        }
        
        // Start typing animation
        string currentLine = currentDialogue.dialogueLines[currentLineIndex];
        StartTyping(currentLine);
        
        // Hide choices panel
        if (choicesPanel != null)
            choicesPanel.SetActive(false);
    }
    
    private void StartTyping(string text)
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);
            
        isTyping = true;
        typingCoroutine = StartCoroutine(TypeText(text));
    }
    
    private void StopTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
        isTyping = false;
    }
    
    private void CompleteCurrentLine()
    {
        if (currentDialogue != null && currentLineIndex < currentDialogue.dialogueLines.Length)
        {
            dialogueText.text = currentDialogue.dialogueLines[currentLineIndex];
        }
        isTyping = false;
    }
    
    private IEnumerator TypeText(string text)
    {
        dialogueText.text = "";
        
        foreach (char c in text)
        {
            dialogueText.text += c;
            
            // Play typing sound effect
            if (typingAudioSource != null && typingSound != null)
            {
                typingAudioSource.PlayOneShot(typingSound);
            }
            
            yield return new WaitForSeconds(typingSpeed);
        }
        
        isTyping = false;
    }
    
    private void ShowChoices()
    {
        if (currentDialogue.playerChoices == null || currentDialogue.playerChoices.Length == 0)
        {
            EndDialogue();
            return;
        }
        
        if (choicesPanel != null)
            choicesPanel.SetActive(true);
        
        // Setup choice buttons
        for (int i = 0; i < choiceButtons.Length; i++)
        {
            if (i < currentDialogue.playerChoices.Length)
            {
                choiceButtons[i].gameObject.SetActive(true);
                if (choiceTexts[i] != null)
                    choiceTexts[i].text = currentDialogue.playerChoices[i].choiceText;
            }
            else
            {
                choiceButtons[i].gameObject.SetActive(false);
            }
        }
    }
    
    private void SelectChoice(int choiceIndex)
    {
        if (currentDialogue.playerChoices == null || 
            choiceIndex >= currentDialogue.playerChoices.Length)
            return;
            
        selectedChoice = currentDialogue.playerChoices[choiceIndex];
        
        // Hide choices panel
        if (choicesPanel != null)
            choicesPanel.SetActive(false);
        
        // Show selected response
        if (selectedChoice.responseDialogue != null)
        {
            StartDialogue(selectedChoice.responseDialogue, onDialogueComplete);
        }
        else
        {
            EndDialogue();
        }
    }
    
    private void EndDialogue()
    {
        isDialogueActive = false;
        
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
            
        if (choicesPanel != null)
            choicesPanel.SetActive(false);
        
        // 恢复玩家移动
        EnablePlayerMovement();
        
        // 调用完成回调
        onDialogueComplete?.Invoke();
        
        // 清理
        currentDialogue = null;
        currentLineIndex = 0;
        selectedChoice = null;
    }
    
    private void DisablePlayerMovement()
    {
        HeroMovement playerMovement = FindObjectOfType<HeroMovement>();
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }
        
        // 停止玩家刚体的运动
        Rigidbody2D playerRb = FindObjectOfType<HeroMovement>()?.GetComponent<Rigidbody2D>();
        if (playerRb != null)
        {
            playerRb.velocity = Vector2.zero;
        }
    }
    
    private void EnablePlayerMovement()
    {
        HeroMovement playerMovement = FindObjectOfType<HeroMovement>();
        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }
    }
    
    // 公共属性
    public bool IsDialogueActive => isDialogueActive;
}
