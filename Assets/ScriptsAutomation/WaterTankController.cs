using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WaterTankController : MonoBehaviour
{
    [Header("Settings")]
    public float drainRate     = 0.02f;
    public float fillRate      = 0.05f;
    public float lowThreshold  = 0.2f;
    public float highThreshold = 0.9f;

    [Header("References")]
    public Slider           levelSlider;
    public ParticleSystem   pumpParticles;
    public TextMeshProUGUI  pumpStatusLabel;

    private float tankLevel = 0.8f;
    private bool  pumpOn    = false;

    void Update()
    {
        // INPUT — natural drain
        tankLevel -= drainRate * Time.deltaTime;

        // INPUT — pump fill
        if (pumpOn)
            tankLevel += fillRate * Time.deltaTime;

        // Clamp
        tankLevel = Mathf.Clamp01(tankLevel);

        // PROCESS — hysteresis thresholds
        if (tankLevel < lowThreshold && !pumpOn)
            pumpOn = true;

        if (tankLevel > highThreshold && pumpOn)
            pumpOn = false;

        // OUTPUT — slider
        levelSlider.value = tankLevel;

        // OUTPUT — particles (only call Play/Stop on state change)
        if (pumpOn)
        {
            if (!pumpParticles.isPlaying)
                pumpParticles.Play();
        }
        else
        {
            if (pumpParticles.isPlaying)
                pumpParticles.Stop();
        }

        // OUTPUT — label
        pumpStatusLabel.text = pumpOn ? "PUMP ON" : "PUMP OFF";
    }
}