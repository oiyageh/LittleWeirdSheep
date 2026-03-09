using UnityEngine;
using UnityEngine.UI; // Required for working with UI Image elements

public class ImageSwitcher : MonoBehaviour
{
    public Image uiImage; // Reference to the UI Image component
    public Sprite[] sprites; // Array to hold your different sprites
    private int currentIndex = 0; // Index of the currently displayed sprite

    void Start()
    {
        // Ensure an image component is assigned
        if (uiImage == null)
        {
            uiImage = GetComponent<Image>();
        }

        // Display the first image in the array on start
        if (sprites != null && sprites.Length > 0)
        {
            uiImage.sprite = sprites[currentIndex];
        }
    }

    void Update()
    {
        // Check for input
        if (Input.GetKeyDown(KeyCode.C))
        {
            ShowNextImage();
        }
        // Check for "Previous" input 
        else if (Input.GetKeyDown(KeyCode.C))
        {
            ShowPreviousImage();
        }
    }

    // Function to switch to the next image in the sequence
    public void ShowNextImage()
    {
        if (sprites.Length == 0) return;

        currentIndex++;
        if (currentIndex >= sprites.Length)
        {
            currentIndex = 0; // Loop back to the start
        }
        uiImage.sprite = sprites[currentIndex];
    }

    // Function to switch to the previous image in the sequence
    public void ShowPreviousImage()
    {
        if (sprites.Length == 0) return;

        currentIndex--;
        if (currentIndex < 0)
        {
            currentIndex = sprites.Length - 1; // Loop to the end
        }
        uiImage.sprite = sprites[currentIndex];
    }
}

