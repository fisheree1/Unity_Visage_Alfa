using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SimpleNPCDialogue : MonoBehaviour
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
    [SerializeField] private Button continueButton;
    
    [Header("Dialogue Settings")]
    [SerializeField] private float typingSpeed = 0.05f;
    
    // State
    private bool playerInRange = false;
    private bool isDialogueActive = false;
    private bool isTyping = false;
    private int currentLineIndex = 0;
    private Transform player;
    private HeroMovement playerMovement;
    
    // Simple dialogue content
    private DialogueLine[] dialogueLines;
    private Coroutine typingCoroutine;
    
    [System.Serializable]
    public class DialogueLine
    {
        public string speaker;
        public string text;
        public bool isPlayer;
    }
    
    void Start()
    {
        InitializeDialogue();
        
        // Find player
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
            playerMovement = playerObject.GetComponent<HeroMovement>();
        }
        
        // Setup UI
        if (interactionUI != null)
            interactionUI.SetActive(false);
            
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
            
        if (interactionText != null)
            interactionText.text = $"Press [{interactKey}] to talk";
            
        // Setup continue button
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(ContinueDialogue);
            
            // Set button text to English
            TextMeshProUGUI buttonText = continueButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = "Continue";
            }
        }
        
        Debug.Log("Simple NPC Dialogue initialized with " + dialogueLines.Length + " lines");
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
                    // Skip typing animation
                    StopTyping();
                    CompleteCurrentLine();
                }
                else
                {
                    ContinueDialogue();
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
        currentLineIndex = 0;
        
        HideInteractionUI();
        
        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);
            
        // Disable player movement
        if (playerMovement != null)
            playerMovement.enabled = false;
            
        DisplayCurrentLine();
        
        Debug.Log("Started dialogue with child");
    }
    
    private void DisplayCurrentLine()
    {
        if (currentLineIndex >= dialogueLines.Length)
        {
            EndDialogue();
            return;
        }
        
        DialogueLine currentLine = dialogueLines[currentLineIndex];
        
        // Set speaker name with appropriate styling
        if (speakerNameText != null)
        {
            speakerNameText.text = currentLine.speaker;
            
            // Change color based on speaker
            if (currentLine.isPlayer)
            {
                speakerNameText.color = Color.cyan; // Player text in cyan
            }
            else if (currentLine.speaker == "Child's Mother")
            {
                speakerNameText.color = Color.red; // Mother text in red (angry)
            }
            else
            {
                speakerNameText.color = Color.white; // Default white
            }
        }
        
        // Start typing animation
        StartTyping(currentLine.text);
        
        Debug.Log($"Displaying line {currentLineIndex}: {currentLine.speaker} - {currentLine.text}");
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
        if (currentLineIndex < dialogueLines.Length)
        {
            dialogueText.text = dialogueLines[currentLineIndex].text;
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
    
    private void ContinueDialogue()
    {
        if (isTyping) return;
        
        currentLineIndex++;
        DisplayCurrentLine();
    }
    
    private void EndDialogue()
    {
        isDialogueActive = false;
        
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
            
        // Restore player movement
        if (playerMovement != null)
            playerMovement.enabled = true;
            
        Debug.Log("Dialogue ended");
    }
    
    private void InitializeDialogue()
    {
        dialogueLines = new DialogueLine[]
        {
            // Child's opening
            new DialogueLine { speaker = "Child", text = "Hey (#`O′)! —", isPlayer = false },
            new DialogueLine { speaker = "Child", text = "Hello there, sister! This is the first time I know there are other \"people\" living in this cave!", isPlayer = false },
            new DialogueLine { speaker = "Child", text = "Your hometown must be from somewhere else, right? You look different from us \"people\" here.", isPlayer = false },
            new DialogueLine { speaker = "Child", text = "Mom told me there's a crazy person living in the cave. He got traumatized more than ten years ago, hurt many \"people\", and said something about not being able to tell the difference.", isPlayer = false },
            new DialogueLine { speaker = "Child", text = "Later, our villagers placed him in this cave. No one has visited him for more than ten years, and he doesn't come out either.", isPlayer = false },
            new DialogueLine { speaker = "Child", text = "I've never seen a crazy person before...", isPlayer = false },
            new DialogueLine { speaker = "Child", text = "Speaking of which, the high priest recently gave me a book. The book says our village was once very prosperous, and a kind \"person\" helped us build the village, develop technology, and taught us weapons and magic.", isPlayer = false },
            new DialogueLine { speaker = "Child", text = "That \"person\" also had golden hair!", isPlayer = false },
            
            // Player response
            new DialogueLine { speaker = "You", text = "What happened next?", isPlayer = true },
            
            // Child continues
            new DialogueLine { speaker = "Child", text = "Next... I haven't read it yet...", isPlayer = false },
            new DialogueLine { speaker = "Child", text = "Come to think of it, sister, you really look like the person in the book!", isPlayer = false },
            new DialogueLine { speaker = "Child", text = "Have you been to our village before? Our village is really beautiful!", isPlayer = false },
            
            // Player response
            new DialogueLine { speaker = "You", text = "Not yet, then I'll trouble you, little guide, to show me the way. :)", isPlayer = true },
            
            // Child calls mother
            new DialogueLine { speaker = "Child", text = "Mom, I found someone who looks just like the person in the book! Come and see, they really look alike!", isPlayer = false },
            
            // Mother appears - dramatic turn
            new DialogueLine { speaker = "Child's Mother", text = "!!!", isPlayer = false },
            new DialogueLine { speaker = "Child's Mother", text = "Get over here quickly!", isPlayer = false },
            new DialogueLine { speaker = "Child's Mother", text = "......", isPlayer = false },
            new DialogueLine { speaker = "Child's Mother", text = "Isn't it enough that you helped the tyrant kill so many of our people? Now you've found us and want to wipe us out completely?", isPlayer = false },
            new DialogueLine { speaker = "Child's Mother", text = "Yuanyuan, go find your uncle, tell him they've found us.", isPlayer = false },
            
            // Child's confusion
            new DialogueLine { speaker = "Child (Yuanyuan)", text = "Huh? But sister seems like a good person...", isPlayer = false },
            
            // Mother's final words
            new DialogueLine { speaker = "Child's Mother", text = "You don't understand anything, just go quickly!", isPlayer = false },
        };
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}
