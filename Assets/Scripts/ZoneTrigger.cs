using UnityEngine;

/// <summary>
/// Marks an invisible trigger zone for the "Explore all zones" objective.
/// Place large invisible GameObjects with a Box Collider (isTrigger = true)
/// around each area of the map. Tag player as "Player".
/// </summary>
public class ZoneTrigger : MonoBehaviour
{
    private bool visited = false;

    private void OnTriggerEnter(Collider other)
    {
        if (visited) return;
        if (!other.CompareTag("Player")) return;

        visited = true;
        ObjectiveManager.Instance?.RegisterZoneVisit();

        // Optional: give visual feedback
        Debug.Log($"[ZoneTrigger] Zone '{gameObject.name}' visited.");
    }
}
