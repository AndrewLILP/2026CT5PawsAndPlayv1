using UnityEngine;
using TMPro;

/// <summary>
/// Handles player interaction input and dispatches to appropriate systems.
///
/// Responsibilities:
///   - Detect E key presses (interact)
///   - Check distance to dog
///   - Determine which interaction to trigger (feed, pet)
///   - Dispatch left-click throw to FetchController
///   - Display context-sensitive prompts via TMP Pro
///
/// Attach to: Player GameObject
/// Assign in Inspector: dogStateMachine, fetchController, inventorySystem,
///                      promptText (TMP Pro), dogTransform
/// </summary>
public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private float interactionRadius = 3f;
    [SerializeField] private float petRadius         = 2.5f;

    [Header("References")]
    [SerializeField] private DogStateMachine  dogStateMachine;
    [SerializeField] private FetchController  fetchController;
    [SerializeField] private InventorySystem  inventorySystem;
    [SerializeField] private ObjectiveManager objectiveManager;
    [SerializeField] private Transform        dogTransform;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI promptText;    // World-space or screen-space TMP

    // ── Private state ─────────────────────────────────────────────────────────
    private float promptTimer = 0f;

    // ── Unity lifecycle ───────────────────────────────────────────────────────
    private void Update()
    {
        HandleInteractInput();
        HandleThrowInput();
        UpdatePromptTimer();
        UpdateContextPrompt(); // Show/hide context hint near dog
    }

    // ── Input handlers ────────────────────────────────────────────────────────

    /// <summary> E key — determine and trigger correct interaction. </summary>
    private void HandleInteractInput()
    {
        if (!Input.GetKeyDown(KeyCode.E)) return;

        float distance = Vector3.Distance(transform.position, dogTransform.position);

        // Check: is player close enough?
        if (distance > interactionRadius)
        {
            if (distance < interactionRadius * 2f) // Nearby but not close enough
                ShowPrompt("Get closer!");
            return;
        }

        // Priority 1: Feed (if player has food)
        if (inventorySystem.HasFood())
        {
            inventorySystem.UseFood();
            dogStateMachine.Feed();
            objectiveManager.IncrementFeedCount();
            return;
        }

        // Priority 2: Pet (if close enough and no ball)
        if (distance <= petRadius)
        {
            dogStateMachine.TriggerPet();
            return;
        }

        // No valid interaction
        ShowPrompt("Find some food first!");
    }

    /// <summary> Left-click — throw ball if player has one. </summary>
    private void HandleThrowInput()
    {
        if (!Input.GetMouseButtonDown(0)) return;
        if (!inventorySystem.HasBall()) return;
        if (dogStateMachine.CurrentState == DogStateMachine.DogState.Fetching) return;

        // Raycast to find throw target point
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 50f))
        {
            inventorySystem.UseBall();
            fetchController.ThrowBall(hit.point);
        }
    }

    // ── Prompt system ─────────────────────────────────────────────────────────

    private void ShowPrompt(string message, float duration = 3f)
    {
        if (promptText == null) return;
        promptText.text    = message;
        promptText.enabled = true;
        promptTimer        = duration;
    }

    private void UpdatePromptTimer()
    {
        if (promptTimer > 0f)
        {
            promptTimer -= Time.deltaTime;
            if (promptTimer <= 0f && promptText != null)
                promptText.enabled = false;
        }
    }

    /// <summary>
    /// Shows a context hint near the dog when player is within range.
    /// Guides player without a formal tutorial.
    /// </summary>
    private void UpdateContextPrompt()
    {
        if (promptText == null) return;
        if (promptTimer > 0f) return; // Don't override timed prompts

        float distance = Vector3.Distance(transform.position, dogTransform.position);

        if (distance < interactionRadius)
        {
            if (inventorySystem.HasFood())
                promptText.text = "[E] Feed";
            else if (distance < petRadius)
                promptText.text = "[E] Pet";
            else
                promptText.text = "[LMB] Throw  |  Find food to feed";

            promptText.enabled = true;
        }
        else
        {
            promptText.enabled = false;
        }
    }
}
