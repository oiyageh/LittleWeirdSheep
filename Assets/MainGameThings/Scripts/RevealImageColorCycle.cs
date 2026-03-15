using UnityEngine;

public class RevealImageColorCycle : MonoBehaviour
{
    public Renderer imageRenderer;      // Renderer for the image
    public string requiredTag = "PaintTool"; // Tag that can trigger it

    private bool revealed = false;

    private Color[] colors = { Color.red, Color.green };
    private int selectedColor = 0;

    void Start()
    {
        if (imageRenderer != null)
        {
            imageRenderer.enabled = false; // hide image at start
        }
    }

    void Update()
    {
        // Press C to cycle colors
        if (Input.GetKeyDown(KeyCode.C))
        {
            selectedColor++;
            if (selectedColor >= colors.Length)
                selectedColor = 0;

            Debug.Log("Selected Color: " + colors[selectedColor]);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Only allow specific tagged object
        if (!other.CompareTag(requiredTag))
            Debug.Log("Wrong Tag!!");
            return;

        // First hit reveals the image
        if (!revealed)
        {
            imageRenderer.enabled = true;
            imageRenderer.material.color = colors[selectedColor];
            revealed = true;
        }
        else
        {
            // Change color on later hits
            imageRenderer.material.color = colors[selectedColor];
        }
    }
}