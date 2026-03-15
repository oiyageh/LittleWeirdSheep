using UnityEngine;

public class HitImageColor : MonoBehaviour
{
    public GameObject imageObject;   // image that appears
    public Renderer objectRenderer;  // object to change color
    public Color firstColor = Color.red;
    public Color secondColor = Color.blue;

    private bool imageShown = false;
    private bool allowColorChange = false;
    private bool colorToggle = false;

    void Update()
    {
        // Press C to allow color change
        if (Input.GetKeyDown(KeyCode.C))
        {
            allowColorChange = true;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Paintable"))
        {
            // FIRST HIT → show image
            if (!imageShown)
            {
                imageObject.SetActive(true);
                imageShown = true;
                return;
            }

            // NEXT HIT AFTER PRESSING C → change color
            if (imageShown && allowColorChange)
            {
                colorToggle = !colorToggle;

                if (colorToggle)
                    objectRenderer.material.color = firstColor;
                else
                    objectRenderer.material.color = secondColor;

                allowColorChange = false;
            }
        }
    }
}