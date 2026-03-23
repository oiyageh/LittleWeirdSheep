using UnityEngine;

public class SceneTrigger : MonoBehaviour
{
    public GameObject promptUI; // Assign your UI panel here
    private bool playerInZone = false;

    void Start()
    {
        promptUI.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInZone = true;
            promptUI.SetActive(true);
            Time.timeScale = 0f; // Pause game
            AudioListener.pause = true;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            AudioListener.pause = false;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        
            playerInZone = false;
            promptUI.SetActive(false);
            Time.timeScale = 1f; // Resume game
        }
    }
}