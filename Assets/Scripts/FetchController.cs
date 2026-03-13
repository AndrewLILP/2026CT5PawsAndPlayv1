using System.Collections;
using UnityEngine;

/// <summary>
/// Manages ball throwing (physics) and the dog fetch retrieval coroutine.
///
/// How it works:
///   1. ThrowBall() instantiates a ball prefab and applies a physics impulse.
///   2. After 1s (physics settle), the dog's fetch sequence is started.
///   3. FetchRoutine() monitors distance; when dog is close to ball, waits,
///      then sends dog back to player.
///   4. On return, ball is destroyed and ObjectiveManager is notified.
///
/// Attach to: Dog GameObject (alongside DogStateMachine)
/// Assign in Inspector: ballPrefab, throwOrigin (player hand transform)
/// </summary>
public class FetchController : MonoBehaviour
{
    [Header("Throw Settings")]
    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private Transform  throwOrigin;    // Empty child of player (hand position)
    [SerializeField] private float      throwForce = 14f;
    [SerializeField] private float      throwUpAngle = 0.3f; // Adds upward arc (0 = flat, 1 = straight up)

    [Header("Fetch Settings")]
    [SerializeField] private float pickupRadius = 1.2f;

    // ── References ────────────────────────────────────────────────────────────
    private DogStateMachine dogSM;
    private GameObject      activeBall;

    private void Awake() => dogSM = GetComponent<DogStateMachine>();

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Instantiates and throws the ball; begins fetch coroutine.
    /// Called by PlayerInteraction on left-click.
    /// </summary>
    /// <param name="targetPoint">World-space point the player clicked on.</param>
    public void ThrowBall(Vector3 targetPoint)
    {
        if (dogSM.CurrentState == DogStateMachine.DogState.Fetching) return;
        if (activeBall != null) Destroy(activeBall); // Safety: clean up previous ball

        // Spawn ball at player hand position
        activeBall = Instantiate(ballPrefab, throwOrigin.position, Quaternion.identity);
        Rigidbody rb = activeBall.GetComponent<Rigidbody>();

        if (rb == null)
        {
            Debug.LogError("[FetchController] Ball prefab is missing a Rigidbody component.");
            return;
        }

        // Calculate throw direction with upward arc
        Vector3 direction = (targetPoint - throwOrigin.position).normalized;
        direction.y += throwUpAngle;
        direction.Normalize();

        rb.AddForce(direction * throwForce, ForceMode.Impulse);

        StartCoroutine(FetchRoutine());
    }

    // ── Coroutine ─────────────────────────────────────────────────────────────

    private IEnumerator FetchRoutine()
    {
        // Wait for ball to land (let physics settle before sending dog)
        yield return new WaitForSeconds(1f);

        if (activeBall == null) yield break; // Ball fell off map — abort

        // Tell dog to fetch; DogStateMachine handles movement
        dogSM.StartFetch(activeBall.transform);

        // Wait until dog is close to ball
        while (activeBall != null &&
               Vector3.Distance(transform.position, activeBall.transform.position) > pickupRadius)
        {
            yield return null;
        }

        // "Pick up" ball — destroy it (dog carries it implicitly)
        Destroy(activeBall);
        activeBall = null;

        // Wait for dog to return to player (managed inside DogStateMachine.FetchMoveRoutine)
        // Give DogStateMachine a moment to detect return arrival
        yield return new WaitForSeconds(2.5f);

        // Notify state machine and objective system
        dogSM.ReturnFromFetch();
        ObjectiveManager.Instance.MarkComplete(1); // Objective index 1 = "Complete a fetch"
    }
}
