using UnityEngine;

public class ExhibitActivator : MonoBehaviour
{
    public GameObject spotlight;
    public GameObject infoPanel;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            spotlight.SetActive(true);
            infoPanel.SetActive(true);
        }
    }

    // EXTENSION → Grade D
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            spotlight.SetActive(false);
            infoPanel.SetActive(false);
        }
    }
}