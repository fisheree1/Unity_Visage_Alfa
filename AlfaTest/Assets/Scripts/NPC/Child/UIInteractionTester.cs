using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class UIInteractionTester : MonoBehaviour
{
    [Header("Test Settings")]
    public Button[] testButtons;
    public bool autoFindButtons = true;
    public bool enableDebugLogs = true;
    
    void Start()
    {
        if (autoFindButtons)
        {
            testButtons = FindObjectsOfType<Button>();
        }
        
        TestUISetup();
        SetupTestButtons();
    }
    
    void Update()
    {
        // Press T to test UI state
        if (Input.GetKeyDown(KeyCode.T))
        {
            TestUISetup();
        }
        
        // Press Y to test all buttons
        if (Input.GetKeyDown(KeyCode.Y))
        {
            TestAllButtons();
        }
    }
    
    private void TestUISetup()
    {
        if (!enableDebugLogs) return;
        
        Debug.Log("=== UI Interaction Test ===");
        
        // Check EventSystem
        EventSystem eventSystem = EventSystem.current;
        if (eventSystem == null)
        {
            Debug.LogError("❌ No EventSystem found! Create one: GameObject → UI → Event System");
        }
        else
        {
            Debug.Log("✅ EventSystem found");
            
            // Check input modules
            if (eventSystem.GetComponent<StandaloneInputModule>() == null)
            {
                Debug.LogWarning("⚠️ No StandaloneInputModule found");
            }
            else
            {
                Debug.Log("✅ StandaloneInputModule found");
            }
        }
        
        // Check Canvas
        Canvas[] canvases = FindObjectsOfType<Canvas>();
        Debug.Log($"Found {canvases.Length} Canvas objects");
        
        foreach (Canvas canvas in canvases)
        {
            Debug.Log($"Canvas: {canvas.name} - RenderMode: {canvas.renderMode}, SortOrder: {canvas.sortingOrder}");
            
            if (canvas.renderMode == RenderMode.ScreenSpaceCamera && canvas.worldCamera == null)
            {
                Debug.LogWarning($"⚠️ Canvas {canvas.name} uses Screen Space - Camera but no camera assigned");
            }
        }
        
        // Check GraphicRaycaster
        GraphicRaycaster[] raycasters = FindObjectsOfType<GraphicRaycaster>();
        Debug.Log($"Found {raycasters.Length} GraphicRaycaster objects");
        
        Debug.Log("=== End UI Test ===");
    }
    
    private void SetupTestButtons()
    {
        if (testButtons == null) return;
        
        for (int i = 0; i < testButtons.Length; i++)
        {
            if (testButtons[i] != null)
            {
                int index = i;
                testButtons[i].onClick.AddListener(() => OnTestButtonClick(index));
                
                if (enableDebugLogs)
                {
                    Debug.Log($"Test listener added to button {index}: {testButtons[i].name}");
                }
            }
        }
    }
    
    private void OnTestButtonClick(int buttonIndex)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"✅ Test button {buttonIndex} clicked successfully!");
        }
        
        // Visual feedback
        if (testButtons[buttonIndex] != null)
        {
            StartCoroutine(FlashButton(testButtons[buttonIndex]));
        }
    }
    
    private void TestAllButtons()
    {
        if (testButtons == null) return;
        
        Debug.Log("Testing all buttons...");
        
        for (int i = 0; i < testButtons.Length; i++)
        {
            if (testButtons[i] != null)
            {
                Button btn = testButtons[i];
                Debug.Log($"Button {i} ({btn.name}): Active={btn.gameObject.activeInHierarchy}, Interactable={btn.interactable}, Raycast={btn.GetComponent<Image>()?.raycastTarget}");
                
                // Test click programmatically
                if (btn.interactable && btn.gameObject.activeInHierarchy)
                {
                    btn.onClick.Invoke();
                }
            }
        }
    }
    
    private System.Collections.IEnumerator FlashButton(Button button)
    {
        Color originalColor = button.image.color;
        button.image.color = Color.green;
        yield return new WaitForSeconds(0.1f);
        button.image.color = originalColor;
    }
    
    void OnGUI()
    {
        if (!enableDebugLogs) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label("UI Interaction Tester");
        GUILayout.Label("Press T - Test UI Setup");
        GUILayout.Label("Press Y - Test All Buttons");
        
        if (GUILayout.Button("Test UI Setup"))
        {
            TestUISetup();
        }
        
        if (GUILayout.Button("Test All Buttons"))
        {
            TestAllButtons();
        }
        
        GUILayout.EndArea();
    }
}
