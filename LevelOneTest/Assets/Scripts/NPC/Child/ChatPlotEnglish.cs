using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChatPlotEnglish : MonoBehaviour
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
    [SerializeField] private string speakerName = "Child";
    
    // State
    private bool playerInRange = false;
    private bool isDialogueActive = false;
    private bool isTyping = false;
    private int currentDialogueIndex = 0;
    private int currentLineIndex = 0;
    private Transform player;
    private HeroMovement playerMovement;
    
    // Dialogue content
    private List<DialogueSection> dialogueSections;
    private Coroutine typingCoroutine;
    
    [System.Serializable]
    public class DialogueSection
    {
        public string[] lines;
        public bool hasChoice;
        public string[] choices;
        public int[] choiceTargets; // Points to next dialogue section index
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
            
        if (choicesPanel != null)
            choicesPanel.SetActive(false);
            
        if (interactionText != null)
            interactionText.text = $"Press [{interactKey}] to talk to the child";
            
        if (speakerNameText != null)
            speakerNameText.text = speakerName;
            
        // Setup button events
        if (nextButton != null)
            nextButton.onClick.AddListener(NextLine);
            
        // Setup choice button events with debug
        SetupChoiceButtons();
        
        // Check UI state for debugging
        CheckUIState();
        
        // Delayed button setup to ensure UI is fully initialized
        StartCoroutine(DelayedButtonSetup());
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
                    NextLine();
                }
            }
            
            // ESC to close dialogue
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                EndDialogue();
            }
            
            // Number keys for quick choice selection (debugging)
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                Debug.Log("Keyboard shortcut: Choice 1");
                SelectChoice(0);
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                Debug.Log("Keyboard shortcut: Choice 2");
                SelectChoice(1);
            }
            
            // T key to test UI state
            if (Input.GetKeyDown(KeyCode.T))
            {
                CheckUIState();
            }
            
            // Y key to test all buttons
            if (Input.GetKeyDown(KeyCode.Y))
            {
                TestAllButtons();
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
            
        // Disable player movement
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
            // Check if there are choices
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
        
        Debug.Log($"Showing choices for section {currentDialogueIndex}");
        
        if (choicesPanel != null)
        {
            choicesPanel.SetActive(true);
            Debug.Log("Choices panel activated");
            
            // Force canvas update
            Canvas.ForceUpdateCanvases();
        }
        else
        {
            Debug.LogError("Choices panel is null!");
            return;
        }
        
        // Setup choice buttons
        for (int i = 0; i < choiceButtons.Length; i++)
        {
            if (i < currentSection.choices.Length)
            {
                // Activate button
                choiceButtons[i].gameObject.SetActive(true);
                choiceButtons[i].interactable = true;
                
                // Set text
                if (choiceTexts[i] != null)
                {
                    choiceTexts[i].text = currentSection.choices[i];
                    Debug.Log($"Button {i} text set to: '{currentSection.choices[i]}'");
                    
                    // Force English text to avoid Chinese display issues
                    if (choiceTexts[i].text.Contains("什么") || choiceTexts[i].text.Contains("接下来"))
                    {
                        choiceTexts[i].text = "What happened next?";
                        Debug.Log($"Button {i} text converted to English: 'What happened next?'");
                    }
                    else if (choiceTexts[i].text.Contains("带路") || choiceTexts[i].text.Contains("指路"))
                    {
                        choiceTexts[i].text = "Show me the way, guide";
                        Debug.Log($"Button {i} text converted to English: 'Show me the way, guide'");
                    }
                }
                else
                {
                    Debug.LogWarning($"Choice text {i} is null!");
                }
                
                Debug.Log($"Button {i} activated - Active: {choiceButtons[i].gameObject.activeInHierarchy}, Interactable: {choiceButtons[i].interactable}");
            }
            else
            {
                choiceButtons[i].gameObject.SetActive(false);
                Debug.Log($"Button {i} deactivated");
            }
        }
        
        // Additional debug info
        Debug.Log($"Total choices: {currentSection.choices.Length}");
        Debug.Log($"Available buttons: {choiceButtons.Length}");
    }
    
    private void SelectChoice(int choiceIndex)
    {
        Debug.Log($"SelectChoice called with index: {choiceIndex}");
        
        if (currentDialogueIndex >= dialogueSections.Count)
        {
            Debug.LogError($"Invalid dialogue index: {currentDialogueIndex}");
            return;
        }
        
        DialogueSection currentSection = dialogueSections[currentDialogueIndex];
        
        if (choiceIndex >= currentSection.choiceTargets.Length)
        {
            Debug.LogError($"Choice index {choiceIndex} out of range. Available targets: {currentSection.choiceTargets.Length}");
            return;
        }
        
        // Get target dialogue section
        int targetIndex = currentSection.choiceTargets[choiceIndex];
        Debug.Log($"Moving from section {currentDialogueIndex} to section {targetIndex}");
        
        // Update indices
        currentDialogueIndex = targetIndex;
        currentLineIndex = 0;
        
        // Hide choices panel
        if (choicesPanel != null)
        {
            choicesPanel.SetActive(false);
            Debug.Log("Choices panel hidden");
        }
        
        // Continue with next dialogue
        DisplayCurrentLine();
    }
    
    private void EndDialogue()
    {
        isDialogueActive = false;
        
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
            
        if (choicesPanel != null)
            choicesPanel.SetActive(false);
            
        // Restore player movement
        if (playerMovement != null)
            playerMovement.enabled = true;
    }
    
    private void InitializeDialogue()
    {
        dialogueSections = new List<DialogueSection>();
        
        // Part 1: Child's opening dialogue
        DialogueSection opening = new DialogueSection();
        opening.lines = new string[]
        {
            "Hey (#`O′)! —",
            "Hello there, sister! This is the first time I know there are other \"people\" living in this cave!",
            "Your hometown must be from somewhere else, right? You look different from us \"people\" here.",
            "Mom told me there's a crazy person living in the cave. He got traumatized more than ten years ago, hurt many \"people\", and said something about not being able to tell the difference.",
            "Later, our villagers placed him in this cave. No one has visited him for more than ten years, and he doesn't come out either.",
            "I've never seen a crazy person before...",
            "Speaking of which, the high priest recently gave me a book. The book says our village was once very prosperous, and a kind \"person\" helped us build the village, develop technology, and taught us weapons and magic.",
            "That \"person\" also had golden hair!"
        };
        opening.hasChoice = true;
        opening.choices = new string[] { "What happened next?" };
        opening.choiceTargets = new int[] { 1 };
        
        // Part 2: Child's response
        DialogueSection response = new DialogueSection();
        response.lines = new string[]
        {
            "Next... I haven't read it yet...",
            "Come to think of it, sister, you really look like the person in the book!",
            "Have you been to our village before? Our village is really beautiful!"
        };
        response.hasChoice = true;
        response.choices = new string[] { "Not yet, then I'll trouble you, little guide, to show me the way." };
        response.choiceTargets = new int[] { 2 };
        
        // Part 3: Ending dialogue
        DialogueSection ending = new DialogueSection();
        ending.lines = new string[]
        {
            "Great! Come with me! I'll take you to the most beautiful place in our village!"
        };
        ending.hasChoice = false;
        
        dialogueSections.Add(opening);
        dialogueSections.Add(response);
        dialogueSections.Add(ending);
    }
    
    private void SetupChoiceButtons()
    {
        Debug.Log("Setting up choice buttons...");
        
        if (choiceButtons == null)
        {
            Debug.LogError("choiceButtons array is null!");
            return;
        }
        
        // Clear existing listeners
        for (int i = 0; i < choiceButtons.Length; i++)
        {
            if (choiceButtons[i] != null)
            {
                choiceButtons[i].onClick.RemoveAllListeners();
                Debug.Log($"Cleared listeners for button {i}");
            }
        }
        
        // Setup new listeners
        for (int i = 0; i < choiceButtons.Length; i++)
        {
            int index = i; // Capture for closure
            if (choiceButtons[i] != null)
            {
                // Ensure button is interactable
                choiceButtons[i].interactable = true;
                
                // Add click listener with debug
                choiceButtons[i].onClick.AddListener(() => {
                    Debug.Log($"Choice button {index} clicked!");
                    SelectChoice(index);
                });
                
                Debug.Log($"Setup button {i} - Interactable: {choiceButtons[i].interactable}");
            }
            else
            {
                Debug.LogWarning($"Choice button {i} is null!");
            }
        }
    }
    
    private IEnumerator DelayedButtonSetup()
    {
        yield return new WaitForSeconds(0.1f);
        
        Debug.Log("Executing delayed button setup...");
        
        // Force rebind all buttons
        ForceRebindButtons();
    }
    
    private void ForceRebindButtons()
    {
        Debug.Log("Force rebinding all buttons...");
        
        if (choiceButtons != null)
        {
            for (int i = 0; i < choiceButtons.Length; i++)
            {
                if (choiceButtons[i] != null)
                {
                    // Clear all listeners
                    choiceButtons[i].onClick.RemoveAllListeners();
                    
                    // Rebind with debug
                    int index = i;
                    choiceButtons[i].onClick.AddListener(() => {
                        Debug.Log($"🔥 Button {index} clicked via FORCED rebind!");
                        SelectChoice(index);
                    });
                    
                    // Ensure button is interactable
                    choiceButtons[i].interactable = true;
                    
                    // Check if button has text component
                    TextMeshProUGUI buttonText = choiceButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                    if (buttonText != null)
                    {
                        Debug.Log($"Button {i} text component found: {buttonText.text}");
                        
                        // Fix Chinese text display by setting to English
                        if (i == 0)
                            buttonText.text = "What happened next?";
                        else if (i == 1)
                            buttonText.text = "Show me the way";
                    }
                    else
                    {
                        Debug.LogWarning($"Button {i} has no text component!");
                    }
                    
                    Debug.Log($"✅ Force rebound button {i} - Interactable: {choiceButtons[i].interactable}");
                }
                else
                {
                    Debug.LogError($"❌ Button {i} is null!");
                }
            }
        }
        else
        {
            Debug.LogError("❌ choiceButtons array is null!");
        }
    }
    
    // Debug method to check UI state
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    private void CheckUIState()
    {
        Debug.Log("=== UI State Check ===");
        
        // Check EventSystem
        UnityEngine.EventSystems.EventSystem eventSystem = UnityEngine.EventSystems.EventSystem.current;
        Debug.Log($"EventSystem exists: {eventSystem != null}");
        
        // Check Canvas
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            Debug.Log($"Canvas found - Render Mode: {canvas.renderMode}");
            Debug.Log($"Canvas Sort Order: {canvas.sortingOrder}");
        }
        else
        {
            Debug.LogWarning("No Canvas found in parent hierarchy!");
        }
        
        // Check choice buttons
        if (choiceButtons != null)
        {
            Debug.Log($"Choice buttons array length: {choiceButtons.Length}");
            for (int i = 0; i < choiceButtons.Length; i++)
            {
                if (choiceButtons[i] != null)
                {
                    Button btn = choiceButtons[i];
                    Debug.Log($"Button {i}: Active={btn.gameObject.activeInHierarchy}, Interactable={btn.interactable}, HasListeners={btn.onClick.GetPersistentEventCount() > 0}");
                }
                else
                {
                    Debug.LogWarning($"Button {i} is null!");
                }
            }
        }
        else
        {
            Debug.LogError("Choice buttons array is null!");
        }
        
        Debug.Log("=== End UI State Check ===");
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
    
    private void TestAllButtons()
    {
        Debug.Log("🧪 Testing all buttons programmatically...");
        
        if (choiceButtons != null)
        {
            for (int i = 0; i < choiceButtons.Length; i++)
            {
                if (choiceButtons[i] != null && choiceButtons[i].gameObject.activeInHierarchy)
                {
                    Debug.Log($"🎯 Testing button {i} - Active: {choiceButtons[i].gameObject.activeInHierarchy}, Interactable: {choiceButtons[i].interactable}");
                    
                    // Get button's position for debugging
                    RectTransform rectTransform = choiceButtons[i].GetComponent<RectTransform>();
                    if (rectTransform != null)
                    {
                        Debug.Log($"Button {i} position: {rectTransform.anchoredPosition}");
                    }
                    
                    // Check if button has listeners
                    int listenerCount = choiceButtons[i].onClick.GetPersistentEventCount();
                    Debug.Log($"Button {i} has {listenerCount} persistent listeners");
                    
                    // Programmatically invoke button click
                    choiceButtons[i].onClick.Invoke();
                    
                    Debug.Log($"✅ Button {i} invoked programmatically");
                }
                else
                {
                    Debug.LogWarning($"⚠️ Button {i} is null or inactive");
                }
            }
        }
        else
        {
            Debug.LogError("❌ choiceButtons array is null!");
        }
    }
}
