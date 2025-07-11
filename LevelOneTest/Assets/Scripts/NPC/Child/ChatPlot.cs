using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChatPlot : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private float interactionRange = 3f;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    
    [Header("UI References")]
    [SerializeField] private GameObject interactionUI;
    [SerializeField] private TextMeshProUGUI interactionText;
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI speakerNameText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Button nextButton;
    [SerializeField] private GameObject choicesPanel;
    [SerializeField] private Button[] choiceButtons;
    [SerializeField] private TextMeshProUGUI[] choiceTexts;
    
    [Header("Dialogue Settings")]
    [SerializeField] private float typingSpeed = 0.05f;
    [SerializeField] private string speakerName = "小孩";
    
    // State
    private bool playerInRange = false;
    private bool isDialogueActive = false;
    private bool isTyping = false;
    private int currentDialogueIndex = 0;
    private int currentLineIndex = 0;
    private Transform player;
    private HeroMovement playerMovement;
    
    // 对话内容
    private List<DialogueSection> dialogueSections;
    private Coroutine typingCoroutine;
    
    [System.Serializable]
    public class DialogueSection
    {
        public string[] lines;
        public bool hasChoice;
        public string[] choices;
        public int[] choiceTargets; // 指向下一个对话部分的索引
    }
    
    void Start()
    {
        InitializeDialogue();
        
        // 找到玩家
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
            playerMovement = playerObject.GetComponent<HeroMovement>();
        }
        
        // 设置UI
        if (interactionUI != null)
            interactionUI.SetActive(false);
            
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
            
        if (choicesPanel != null)
            choicesPanel.SetActive(false);
            
        if (interactionText != null)
            interactionText.text = $"按 [{interactKey}] 与小孩对话";
            
        if (speakerNameText != null)
            speakerNameText.text = speakerName;
            
        // 设置按钮事件
        if (nextButton != null)
            nextButton.onClick.AddListener(NextLine);
            
        for (int i = 0; i < choiceButtons.Length; i++)
        {
            int index = i;
            if (choiceButtons[i] != null)
                choiceButtons[i].onClick.AddListener(() => SelectChoice(index));
        }
    }
    
    void Update()
    {
        CheckPlayerInRange();
        
        if (playerInRange && Input.GetKeyDown(interactKey) && !isDialogueActive)
        {
            StartDialogue();
        }
        
        if (isDialogueActive)
        {
            if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space))
            {
                if (isTyping)
                {
                    // 跳过打字动画
                    StopTyping();
                    CompleteCurrentLine();
                }
                else
                {
                    NextLine();
                }
            }
            
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                EndDialogue();
            }
        }
    }
    
    private void CheckPlayerInRange()
    {
        if (player == null) return;
        
        float distance = Vector2.Distance(transform.position, player.position);
        bool wasInRange = playerInRange;
        playerInRange = distance <= interactionRange;
        
        if (playerInRange && !wasInRange)
        {
            ShowInteractionUI();
        }
        else if (!playerInRange && wasInRange)
        {
            HideInteractionUI();
        }
    }
    
    private void ShowInteractionUI()
    {
        if (interactionUI != null && !isDialogueActive)
            interactionUI.SetActive(true);
    }
    
    private void HideInteractionUI()
    {
        if (interactionUI != null)
            interactionUI.SetActive(false);
    }
    
    private void StartDialogue()
    {
        isDialogueActive = true;
        currentDialogueIndex = 0;
        currentLineIndex = 0;
        
        HideInteractionUI();
        
        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);
            
        // 禁用玩家移动
        if (playerMovement != null)
            playerMovement.enabled = false;
            
        DisplayCurrentLine();
    }
    
    private void DisplayCurrentLine()
    {
        if (currentDialogueIndex >= dialogueSections.Count)
        {
            EndDialogue();
            return;
        }
        
        DialogueSection currentSection = dialogueSections[currentDialogueIndex];
        
        if (currentLineIndex >= currentSection.lines.Length)
        {
            // 检查是否有选择
            if (currentSection.hasChoice)
            {
                ShowChoices();
            }
            else
            {
                EndDialogue();
            }
            return;
        }
        
        string currentLine = currentSection.lines[currentLineIndex];
        StartTyping(currentLine);
        
        // 隐藏选择面板
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
        if (currentDialogueIndex < dialogueSections.Count && 
            currentLineIndex < dialogueSections[currentDialogueIndex].lines.Length)
        {
            dialogueText.text = dialogueSections[currentDialogueIndex].lines[currentLineIndex];
        }
        isTyping = false;
    }
    
    private IEnumerator TypeText(string text)
    {
        dialogueText.text = "";
        
        foreach (char c in text)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
        
        isTyping = false;
    }
    
    private void NextLine()
    {
        if (isTyping) return;
        
        currentLineIndex++;
        DisplayCurrentLine();
    }
    
    private void ShowChoices()
    {
        DialogueSection currentSection = dialogueSections[currentDialogueIndex];
        
        if (choicesPanel != null)
            choicesPanel.SetActive(true);
        
        for (int i = 0; i < choiceButtons.Length; i++)
        {
            if (i < currentSection.choices.Length)
            {
                choiceButtons[i].gameObject.SetActive(true);
                if (choiceTexts[i] != null)
                    choiceTexts[i].text = currentSection.choices[i];
            }
            else
            {
                choiceButtons[i].gameObject.SetActive(false);
            }
        }
    }
    
    private void SelectChoice(int choiceIndex)
    {
        DialogueSection currentSection = dialogueSections[currentDialogueIndex];
        
        if (choiceIndex < currentSection.choiceTargets.Length)
        {
            currentDialogueIndex = currentSection.choiceTargets[choiceIndex];
            currentLineIndex = 0;
            
            if (choicesPanel != null)
                choicesPanel.SetActive(false);
                
            DisplayCurrentLine();
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
        if (playerMovement != null)
            playerMovement.enabled = true;
    }
    
    private void InitializeDialogue()
    {
        dialogueSections = new List<DialogueSection>();
        
        // 第一部分：小孩的开场白
        DialogueSection opening = new DialogueSection();
        opening.lines = new string[]
        {
            "喂(#`O′)！——",
            "你好呀姐姐！我还是第一次知道这个洞穴还有其他\"人\"住呢！",
            "你家乡应该在外地吧，和我们这儿的\"人\"长得都不一样。",
            "妈妈跟我说洞穴里住着一个疯子。他十几年前受了刺激，打伤了好多\"人\"，还说什么自己分不清。",
            "后来我们村里人把他安置在这个洞穴里，十几年了也没人去看过他，他也不出来。",
            "我还没有见过疯子呢...",
            "话说回来，最近大祭司给了我一本书。书上说我们村子曾经非常兴盛，一个好心\"人\"帮我们建造村落、发展科技，还教我们兵器和魔法。",
            "那个\"人\"也是金色的头发呢！"
        };
        opening.hasChoice = true;
        opening.choices = new string[] { "后来呢？" };
        opening.choiceTargets = new int[] { 1 };
        
        // 第二部分：小孩的回应
        DialogueSection response = new DialogueSection();
        response.lines = new string[]
        {
            "后来...后来还没看呢...",
            "说起来，姐姐你和书上那个人长的真的很像欸！",
            "你有来过我们村子吗，我们村子真的很好看的！"
        };
        response.hasChoice = true;
        response.choices = new string[] { "还没有，那就劳烦小导游带路了。" };
        response.choiceTargets = new int[] { 2 };
        
        // 第三部分：结束对话
        DialogueSection ending = new DialogueSection();
        ending.lines = new string[]
        {
            "好耶！跟我来吧！我带你去村子里最好看的地方！"
        };
        ending.hasChoice = false;
        
        dialogueSections.Add(opening);
        dialogueSections.Add(response);
        dialogueSections.Add(ending);
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}
