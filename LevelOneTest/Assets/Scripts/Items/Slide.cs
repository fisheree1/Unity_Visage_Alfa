using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SlideSkillBook : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private float interactionRadius = 2f;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private bool isCollected = false;

    [Header("Visual Effects")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color highlightColor = Color.cyan;
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobHeight = 0.5f;

    [Header("UI References")]
    [SerializeField] private GameObject interactionUI;
    [SerializeField] private TextMeshProUGUI interactionText;

    [Header("Skill Settings")]
    [SerializeField] private string skillToUnlock = "Slide";
    [SerializeField] private string skillDisplayName = "Slide";
    [SerializeField] private string skillDescription = "Allows you to dash quickly along the ground!";

    private SpriteRenderer spriteRenderer;
    private Vector3 startPosition;

    private bool playerInRange = false;
    private bool isInteracting = false;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        startPosition = transform.position;

        if (interactionText != null)
        {
            interactionText.text = $"Press <color=yellow>[{interactKey}]</color> to learn {skillDisplayName}";
        }

        if (interactionUI != null)
        {
            interactionUI.SetActive(false);
        }

        if (isCollected)
        {
            gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (isCollected || isInteracting) return;

        CheckPlayerInRange();

        if (playerInRange && Input.GetKeyDown(interactKey))
        {
            StartCoroutine(CollectSkillBook());
        }

        UpdateVisualEffects();
    }

    private void CheckPlayerInRange()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            float distance = Vector2.Distance(transform.position, player.transform.position);

            bool wasInRange = playerInRange;
            playerInRange = distance <= interactionRadius;

            if (playerInRange && !wasInRange)
                OnPlayerEnterRange();
            else if (!playerInRange && wasInRange)
                OnPlayerExitRange();
        }
        else
        {
            if (playerInRange)
                OnPlayerExitRange();
            playerInRange = false;
        }
    }

    private void OnPlayerEnterRange()
    {
        if (interactionUI != null && !isCollected)
            interactionUI.SetActive(true);
    }

    private void OnPlayerExitRange()
    {
        if (interactionUI != null)
            interactionUI.SetActive(false);
    }

    private void UpdateVisualEffects()
    {
        float bobOffset = Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = startPosition + Vector3.up * bobOffset;

        if (spriteRenderer != null)
            spriteRenderer.color = playerInRange ? highlightColor : normalColor;
    }

    private IEnumerator CollectSkillBook()
    {
        isInteracting = true;

        if (interactionUI != null)
            interactionUI.SetActive(false);

        yield return StartCoroutine(PlayCollectionEffect());

        HeroMovement heroMovement = FindObjectOfType<HeroMovement>();
        if (heroMovement != null)
        {
            heroMovement.UnlockSlide();
            Debug.Log($"Successfully learned skill: {skillDisplayName}");
            ShowSkillLearnedMessage();
        }
        else
        {
            Debug.LogError("HeroMovement not found!");
        }

        isCollected = true;
        gameObject.SetActive(false);
    }

    private IEnumerator PlayCollectionEffect()
    {
        Vector3 originalScale = transform.localScale;
        Vector3 originalPosition = transform.position;

        float duration = 1f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float progress = elapsed / duration;

            transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, progress);
            transform.position = originalPosition + Vector3.up * progress * 2f;

            if (spriteRenderer != null)
            {
                Color color = spriteRenderer.color;
                color.a = 1f - progress;
                spriteRenderer.color = color;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    private void ShowSkillLearnedMessage()
    {
        Debug.Log($"Skill Learned: {skillDisplayName}\n{skillDescription}");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }

    public bool IsCollected => isCollected;
    public string SkillName => skillToUnlock;
}
