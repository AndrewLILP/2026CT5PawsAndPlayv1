using UnityEngine;

public class SecurityFloodlight : MonoBehaviour
{
    public Light floodlight;
    public float holdDuration = 5f;

    private float timer = 0f;
    private bool playerInZone = false;

    void Update()
    {
        // If the player is inside the trigger, keep resetting the timer
        if (playerInZone)
        {
            timer = holdDuration;
        }
        else if (timer > 0f)
        {
            timer -= Time.deltaTime;   // Frame‑rate independent countdown
        }

        // Light stays on as long as timer is above zero
        floodlight.enabled = (timer > 0f);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInZone = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInZone = false;
        }
    }
}
