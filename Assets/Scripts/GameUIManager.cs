using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Manages all HUD elements using TextMesh Pro.
///
/// Elements managed:
///   - Trust progress bar (Slider) + TMP label ("Trust: 70 / 100")
///   - Objective checklist (3 TMP text elements + tick Image icons)
///   - Congratulations panel (CanvasGroup fade-in)
///
/// Attach to: A UI Manager GameObject in the scene.
/// Subscribe to ObjectiveManager.OnObjectiveComplete in Start().
/// Assign all references in Inspector.
/// </summary>
public class GameUIManager : MonoBehaviour
{
    [Header("Trust Bar")]
    [SerializeField] private Slider          trustSlider;
    [SerializeField] private TextMeshProUGUI trustLabel;   // "Trust: 70 / 100"

    [Header("Objectives (assign 3 of each)")]
    [SerializeField] private TextMeshProUGUI[] objectiveLabels; // 3 TMP texts
    [SerializeField] private Image[]           objectiveTicks;  // 3 tick images

    [Header("Congratulations Panel")]
    [SerializeField] private CanvasGroup congratsPanel;
    [SerializeField] private float        congratsFadeDuration = 1f;

    [Header("References")]
    [SerializeField] private DogStateMachine dog;

    // ── Objective display text ────────────────────────────────────────────────
    private static readonly string[] ObjectiveDescriptions =
    {
        "Feed the dog 3 times",
        "Play fetch with the dog",
        "Explore all three zones"
    };

    // ── Unity lifecycle ───────────────────────────────────────────────────────
    private void Start()
    {
        // Set objective label text
        for (int i = 0; i < objectiveLabels.Length; i++)
        {
            if (i < ObjectiveDescriptions.Length)
                objectiveLabels[i].text = ObjectiveDescriptions[i];
        }

        // Hide all ticks initially
        foreach (Image tick in objectiveTicks)
            tick.enabled = false;

        // Hide congratulations panel
        if (congratsPanel != null)
        {
            congratsPanel.alpha          = 0f;
            congratsPanel.interactable   = false;
            congratsPanel.blocksRaycasts = false;
        }

        // Subscribe to objective completion events
        if (ObjectiveManager.Instance != null)
            ObjectiveManager.Instance.OnObjectiveComplete += OnObjectiveComplete;
    }

    private void OnDestroy()
    {
        // Always unsubscribe to prevent memory leaks
        if (ObjectiveManager.Instance != null)
            ObjectiveManager.Instance.OnObjectiveComplete -= OnObjectiveComplete;
    }

    // ── Per-frame updates ─────────────────────────────────────────────────────
    private void Update()
    {
        if (dog == null) return;

        // Smoothly animate trust bar (Lerp toward actual value)
        float targetFill = dog.TrustLevel / 100f;
        trustSlider.value = Mathf.Lerp(trustSlider.value, targetFill, Time.deltaTime * 5f);

        // Update TMP text label
        trustLabel.text = $"Trust: {Mathf.RoundToInt(dog.TrustLevel)} / 100";
    }

    // ── Event handler ─────────────────────────────────────────────────────────

    /// <summary>
    /// Called by ObjectiveManager.OnObjectiveComplete when an objective is finished.
    /// </summary>
    private void OnObjectiveComplete(int index)
    {
        if (index < 0 || index >= objectiveTicks.Length) return;

        // Show tick mark
        objectiveTicks[index].enabled = true;

        // Change text colour to green (colour + shape, not colour alone — accessibility)
        objectiveLabels[index].color = new Color(0.2f, 0.7f, 0.2f);

        // Append "✓" for screen reader / colour-blind users
        objectiveLabels[index].text = ObjectiveDescriptions[index] + "  ✓";

        // Check if all objectives are complete
        if (ObjectiveManager.Instance != null && ObjectiveManager.Instance.AllComplete())
            StartCoroutine(FadeInCongrats());
    }

    // ── Congratulations fade ──────────────────────────────────────────────────
    private IEnumerator FadeInCongrats()
    {
        congratsPanel.interactable   = true;
        congratsPanel.blocksRaycasts = true;

        float elapsed = 0f;
        while (elapsed < congratsFadeDuration)
        {
            congratsPanel.alpha = Mathf.Lerp(0f, 1f, elapsed / congratsFadeDuration);
            elapsed            += Time.deltaTime;
            yield return null;
        }
        congratsPanel.alpha = 1f;
    }
}
