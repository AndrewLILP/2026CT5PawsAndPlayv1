using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Singleton that tracks optional objective completion.
///
/// Objectives:
///   Index 0: Feed the dog 3 times
///   Index 1: Complete a fetch
///   Index 2: Explore all three zones
///
/// Subscribe to OnObjectiveComplete to receive completion events in GameUIManager.
/// </summary>
public class ObjectiveManager : MonoBehaviour
{
    // ── Singleton ─────────────────────────────────────────────────────────────
    public static ObjectiveManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // ── Data ──────────────────────────────────────────────────────────────────
    private bool[] objectiveComplete = new bool[3]; // false by default
    private int    feedCount         = 0;
    private int    zonesVisited      = 0;

    [Header("Settings")]
    [SerializeField] private int feedsRequired = 3;
    [SerializeField] private int zonesRequired = 3;

    // ── Event ─────────────────────────────────────────────────────────────────
    // GameUIManager subscribes to this to update the checklist
    public System.Action<int> OnObjectiveComplete;

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Increment the feed counter. Marks objective 0 complete when threshold reached.
    /// Called by PlayerInteraction after each successful feed.
    /// </summary>
    public void IncrementFeedCount()
    {
        feedCount++;
        if (feedCount >= feedsRequired && !objectiveComplete[0])
            MarkComplete(0);
    }

    /// <summary>
    /// Called by zone trigger colliders (ZoneTrigger.cs) when player enters.
    /// </summary>
    public void RegisterZoneVisit()
    {
        zonesVisited = Mathf.Min(zonesVisited + 1, zonesRequired);
        if (zonesVisited >= zonesRequired && !objectiveComplete[2])
            MarkComplete(2);
    }

    /// <summary>
    /// Mark an objective as complete by index and fire the completion event.
    /// </summary>
    public void MarkComplete(int index)
    {
        if (index < 0 || index >= objectiveComplete.Length) return;
        if (objectiveComplete[index]) return; // Already complete — don't double-trigger

        objectiveComplete[index] = true;
        OnObjectiveComplete?.Invoke(index); // Notify GameUIManager
    }

    /// <summary> Check if a specific objective is complete. </summary>
    public bool IsComplete(int index) =>
        (index >= 0 && index < objectiveComplete.Length) && objectiveComplete[index];

    /// <summary> Check if all objectives are complete. </summary>
    public bool AllComplete()
    {
        foreach (bool b in objectiveComplete)
            if (!b) return false;
        return true;
    }
}
