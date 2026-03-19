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

    // 🔥 Track which object has which decal
    private Dictionary<GameObject, GameObject> objectToDecal = new Dictionary<GameObject, GameObject>();

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Shoot();
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            SwapColor();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            RemoveLookedAtDecal();
        }
    }

    void SwapColor()
    {
        currentColorIndex++;
        if (currentColorIndex > 2)
            currentColorIndex = 0;

        Debug.Log("Switched to color index: " + currentColorIndex);
    }

    Color GetCurrentColor()
    {
        switch (currentColorIndex)
        {
            case 0: return color1;
            case 1: return color2;
            case 2: return color3;
            default: return color1;
        }
    }

    void Shoot()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, range))
        {
            if (!hit.collider.CompareTag(allowedTag))
                return;

            GameObject targetObject = hit.collider.gameObject;

            // ❌ Already has a stamp → stop
            if (objectToDecal.ContainsKey(targetObject))
            {
                Debug.Log("This object already has a stamp!");
                return;
            }

            GameObject newDecal = Instantiate(
                decalPrefab,
                hit.point + hit.normal * 0.01f,
                Quaternion.LookRotation(hit.normal)
            );

            newDecal.transform.SetParent(targetObject.transform);

            Renderer r = newDecal.GetComponent<Renderer>();
            if (r != null)
            {
                r.material.color = GetCurrentColor();
            }

            //  Save mapping
            objectToDecal.Add(targetObject, newDecal);

        
        }
    }

    void RemoveLookedAtDecal()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, range))
        {
            GameObject hitObject = hit.collider.gameObject;

            // Loop through dictionary to find matching decal
            GameObject objectToRemove = null;

            foreach (var pair in objectToDecal)
            {
                if (pair.Value == hitObject)
                {
                    objectToRemove = pair.Key;
                    Destroy(pair.Value);
                    break;
                }
            }

            if (objectToRemove != null)
            {
                objectToDecal.Remove(objectToRemove);
                Debug.Log("Removed looked-at decal");
            }
        }
    }
}