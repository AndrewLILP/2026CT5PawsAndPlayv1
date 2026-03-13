using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Controls the dog's behaviour using a finite state machine.
/// 
/// States:
///   Idle      — dog waits until player approaches
///   Following — dog walks toward player position each frame
///   Fetching  — dog moves to ball, picks it up, returns to player
///   Eating    — dog plays eat animation; trust increases
///   BeingPet  — dog plays sit/nuzzle animation
///
/// Attach to: Dog GameObject
/// Requires:  NavMeshAgent, Animator (on same GameObject)
/// Assign in Inspector: player Transform
/// 
/// Animator parameters required (all Triggers):
///   "Idle", "Following", "Fetching", "Eating", "BeingPet"
/// </summary>
public class DogStateMachine : MonoBehaviour
{
    // ── State enum ────────────────────────────────────────────────────────────
    public enum DogState { Idle, Following, Fetching, Eating, BeingPet }

    // ── Inspector-editable settings ───────────────────────────────────────────
    [Header("Behaviour Radii")]
    [SerializeField] private float followRadius     = 5f;   // Distance to start following
    [SerializeField] private float stopFollowRadius = 8f;   // Distance to stop following (hysteresis)
    [SerializeField] private float interactionRadius = 3f;  // Distance for feed/pet
    [SerializeField] private float pickupRadius      = 1.2f; // Distance to "pick up" ball
    

    [Header("Trust")]
    [SerializeField] private float feedTrustAmount = 10f;   // Trust gained per feed

    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Animator  animator;

    // ── Private fields ────────────────────────────────────────────────────────

    private Vector3 lastPlayerPosition;
    private NavMeshAgent agent;
    private DogState     currentState = DogState.Idle;
    private float        trustLevel   = 0f;
    private Transform    fetchTarget;             // Set by FetchController

    // ── Public read-only properties ───────────────────────────────────────────
    public float    TrustLevel   => trustLevel;
    public DogState CurrentState => currentState;

    // ── Unity lifecycle ───────────────────────────────────────────────────────
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        EvaluateState();
    }

    // ── State evaluation (called every frame) ─────────────────────────────────
    private void EvaluateState()
    {
        float distance = Vector3.Distance(transform.position, player.position);

        switch (currentState)
        {
            case DogState.Idle:
                float playerSpeedIdle = Vector3.Distance(player.position, lastPlayerPosition) 
                                        / Time.deltaTime;
                if (distance < followRadius && playerSpeedIdle > 0.1f)
                    SetState(DogState.Following);
                break;

            case DogState.Following:
                agent.SetDestination(player.position);
                float playerSpeed = Vector3.Distance(player.position, lastPlayerPosition) 
                                    / Time.deltaTime;
                if (distance > stopFollowRadius)
                    SetState(DogState.Idle);
                else if (playerSpeed < 0.1f && distance < 2f)
                    SetState(DogState.Idle);
                break;
            // Fetching, Eating, BeingPet are managed by coroutines —
            // no per-frame logic needed here.
        }

        lastPlayerPosition = player.position;
    }

    // ── Public interaction methods (called by PlayerInteraction) ──────────────

    /// <summary>
    /// Feed the dog. Increases trust, plays eat animation.
    /// Call only when player has food and is within interactionRadius.
    /// </summary>
    public void Feed()
    {
        if (currentState == DogState.Eating || currentState == DogState.Fetching) return;

        trustLevel = Mathf.Clamp(trustLevel + feedTrustAmount, 0f, 100f);
        StartCoroutine(EatRoutine());
    }

    /// <summary>
    /// Begin fetch sequence. Called by FetchController after ball is thrown.
    /// </summary>
    public void StartFetch(Transform ball)
    {
        fetchTarget = ball;
        SetState(DogState.Fetching);
        StartCoroutine(FetchMoveRoutine());
    }

    /// <summary>
    /// Called by FetchController once dog has returned to player.
    /// </summary>
    public void ReturnFromFetch()
    {
        fetchTarget = null;
        SetState(DogState.Following);
    }

    /// <summary>
    /// Pet the dog. Plays sit/nuzzle animation, then returns to Following.
    /// </summary>
    public void TriggerPet()
    {
        if (currentState == DogState.Eating || currentState == DogState.Fetching) return;
        StartCoroutine(PetRoutine());
    }

    // ── Coroutines ────────────────────────────────────────────────────────────

    private System.Collections.IEnumerator EatRoutine()
    {
        SetState(DogState.Eating);
        // Wait for the eat animation clip to complete
        yield return new WaitForSeconds(1.5f); // Adjust to match your animation length
        SetState(DogState.Following);
    }

    private System.Collections.IEnumerator FetchMoveRoutine()
    {
        // Phase 1: Move to ball
        while (fetchTarget != null &&
               Vector3.Distance(transform.position, fetchTarget.position) > pickupRadius)
        {
            agent.SetDestination(fetchTarget.position);
            yield return null; // Wait one frame
        }

        // Phase 2: Play pick-up animation
        animator.SetTrigger("PickUp");
        yield return new WaitForSeconds(0.8f);

        // Phase 3: Return to player
        while (Vector3.Distance(transform.position, player.position) > 2f)
        {
            agent.SetDestination(player.position);
            yield return null;
        }

        // Fetch complete — FetchController.FetchRoutine() will call ReturnFromFetch()
    }

    private System.Collections.IEnumerator PetRoutine()
    {
        SetState(DogState.BeingPet);
        yield return new WaitForSeconds(2f);
        SetState(DogState.Following);
    }

    // ── State transition ──────────────────────────────────────────────────────
    private void SetState(DogState newState)
    {
        currentState = newState;
        animator.SetTrigger(newState.ToString()); // Matches Animator parameter names
        agent.isStopped = (newState == DogState.Eating || newState == DogState.BeingPet);
    }
}
