using UnityEngine;
using System.Collections.Generic;

public class ImagePasteTool2 : MonoBehaviour
{
    [Header("Decal Settings")]
    public GameObject decalPrefab;
    public float range = 50f;
    public Color color1 = Color.red;
    public Color color2 = Color.blue;
    public Color color3 = Color.green;
    public string allowedTag = "Paintable";

    private int currentColorIndex = 0;

    // Tracks decals per object
    private Dictionary<GameObject, (GameObject decal, int colorIndex)> objectToDecal
        = new Dictionary<GameObject, (GameObject, int)>();

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) Shoot();
        if (Input.GetKeyDown(KeyCode.C)) SwapColor();
        if (Input.GetKeyDown(KeyCode.R)) RemoveDecalAtAim();
    }

    void SwapColor()
    {
        currentColorIndex++;
        if (currentColorIndex > 2) currentColorIndex = 0;
        Debug.Log("Switched to color index: " + currentColorIndex);
    }

    Color GetCurrentColor()
    {
        return currentColorIndex switch
        {
            0 => color1,
            1 => color2,
            2 => color3,
            _ => color1
        };
    }

    void Shoot()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, range)) return;
        if (!hit.collider.CompareTag(allowedTag)) return;

        GameObject targetObject = hit.collider.gameObject;

        // Only allow ONE decal per object
        if (objectToDecal.ContainsKey(targetObject))
        {
            Debug.Log("Object already has a decal!");
            return;
        }

        // Place decal
        GameObject newDecal = Instantiate(
            decalPrefab,
            hit.point + hit.normal * 0.01f,
            Quaternion.LookRotation(hit.normal)
        );
        newDecal.transform.SetParent(targetObject.transform);

        Renderer r = newDecal.GetComponent<Renderer>();
        if (r != null) r.material.color = GetCurrentColor();

        objectToDecal.Add(targetObject, (newDecal, currentColorIndex));

        // Update sheep if it exists
        Sheep sheep = targetObject.GetComponent<Sheep>();
        if (sheep != null) sheep.SetColor(currentColorIndex);
    }

    void RemoveDecalAtAim()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, range)) return;
        if (!hit.collider.CompareTag(allowedTag)) return;

        GameObject targetObject = hit.collider.gameObject;

        // Only remove decal if the hit object has one
        if (objectToDecal.TryGetValue(targetObject, out var decalData))
        {
            // Optional: ensure the hit point is close to the decal
            if (Vector3.Distance(hit.point, decalData.decal.transform.position) < 0.5f)
            {
                Destroy(decalData.decal);
                objectToDecal.Remove(targetObject);
                Debug.Log("Decal removed at aim!");

                // Remove color from Sheep
                Sheep sheep = targetObject.GetComponent<Sheep>();
                if (sheep != null) sheep.SetColor(-1); // -1 = no color
            }
            else
            {
                Debug.Log("Not aiming at the decal directly.");
            }
        }
        else
        {
            Debug.Log("No decal to remove at this object.");
        }
    }
}