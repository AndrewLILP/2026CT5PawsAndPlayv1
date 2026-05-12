using UnityEngine;
using TMPro;

public class TrafficLightFSM : MonoBehaviour
{
    public enum LightState { Red, Green, Amber }

    [Header("Durations")]
    public float redDuration = 5f;
    public float greenDuration = 4f;
    public float amberDuration = 2f;

    [Header("Lights")]
    public Light redLight;
    public Light greenLight;
    public Light amberLight;

    [Header("UI")]
    public TMP_Text stateLabel;

    private LightState state = LightState.Red;
    private float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;

        // Pedestrian button interrupt
        if (Input.GetKeyDown(KeyCode.E))
        {
            Transition(LightState.Red);
        }

        switch (state)
        {
            case LightState.Red:
                if (timer >= redDuration)
                    Transition(LightState.Green);
                break;

            case LightState.Green:
                if (timer >= greenDuration)
                    Transition(LightState.Amber);
                break;

            case LightState.Amber:
                if (timer >= amberDuration)
                    Transition(LightState.Red);
                break;
        }

        UpdateLights();
        UpdateLabel();
    }

    void Transition(LightState next)
    {
        state = next;
        timer = 0f;
    }

    void UpdateLights()
    {
        redLight.enabled   = (state == LightState.Red);
        greenLight.enabled = (state == LightState.Green);
        amberLight.enabled = (state == LightState.Amber);
    }

    void UpdateLabel()
    {
        if (stateLabel != null)
            stateLabel.text = state.ToString();
    }
}
