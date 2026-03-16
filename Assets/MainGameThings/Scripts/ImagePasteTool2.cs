using UnityEngine;

public class ImagePasteTool2 : MonoBehaviour
{
    [Header("Decal Settings")]
    public GameObject decalPrefab;
    public float range = 50f;

    public Color color1 = Color.red;   // First color (e.g., Hostile)
    public Color color2 = Color.blue;  // Second color
    public Color color3 = Color.green; // Third color

    public string allowedTag = "Paintable";

    private bool useFirstColor = true;
    private GameObject currentDecal; // Stores pasted decal

    void Update()
    {
        // Left click = paste image
        if (Input.GetMouseButtonDown(0))
        {
            Shoot();
        }

        // Press C = swap color
        if (Input.GetKeyDown(KeyCode.C))
        {
            SwapColor();
        }

        // Press R = remove decal
        if (Input.GetKeyDown(KeyCode.R))
        {
            RemoveDecal();
        }
    }

    void SwapColor()
    {
        useFirstColor = !useFirstColor;
        Debug.Log("Swapped Color!");
    }

    void Shoot()
    {
        // Only one decal at a time
        if (currentDecal != null)
            return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, range))
        {
            // Only allow specific tag
            if (!hit.collider.CompareTag(allowedTag))
                return;

            // Create decal
            currentDecal = Instantiate(
                decalPrefab,
                hit.point + hit.normal * 0.01f,
                Quaternion.LookRotation(hit.normal)
            );

            // Attach to hit object
            currentDecal.transform.SetParent(hit.collider.transform);

            // Set color
            Renderer r = currentDecal.GetComponent<Renderer>();
            if (r != null)
            {
                if (useFirstColor)
                    r.material.color = color1;
                else
                    r.material.color = color2;
            }

            // -------------------------
            // Save enemy to EnemyManager
            // -------------------------
            // Make sure the hit object has EnemyDataComponent
            SheepDataComponent enemyComp = hit.collider.GetComponent<SheepDataComponent>();
            if (enemyComp != null && enemyComp.enemyData != null)
            {
                SheepManager.Instance.AddEnemy(enemyComp.enemyData);
                Debug.Log("Stamped and saved: " + enemyComp.enemyData.SheepName);
            }
        }
    }

    void RemoveDecal()
    {
        if (currentDecal != null)
        {
            Destroy(currentDecal);
            currentDecal = null;
            Debug.Log("Decal Removed");
        }
    }
}