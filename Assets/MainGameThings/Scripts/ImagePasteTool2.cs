using UnityEngine;

public class ImagePasteTool2 : MonoBehaviour
{
    public GameObject decalPrefab;
    public float range = 50f;

    public Color color1 = Color.red;
    public Color color2 = Color.blue;

    public string allowedTag = "Paintable";

    private bool useFirstColor = true;

    private GameObject currentDecal; // stores pasted image

    void Update()
    {
        // Left click = paste image (only once)
        if (Input.GetMouseButtonDown(0))
        {
            Shoot();
        }

        // Press C to swap color
        if (Input.GetKeyDown(KeyCode.C))
        {
            useFirstColor = !useFirstColor;
            Debug.Log("Swapped Color!");
        }

        // Press R to remove image
        if (Input.GetKeyDown(KeyCode.R))
        {
            RemoveDecal();
        }
    }

    void Shoot()
    {
        // do not paste if one already exists
        if (currentDecal != null)
            return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, range))
        {
            // check tag
            if (!hit.collider.CompareTag(allowedTag))
                return;

            // create decal
            currentDecal = Instantiate(
                decalPrefab,
                hit.point + hit.normal * 0.01f,
                Quaternion.LookRotation(hit.normal)
            );

            // attach to object hit
            currentDecal.transform.SetParent(hit.collider.transform);

            // set color
            Renderer r = currentDecal.GetComponent<Renderer>();
            if (r != null)
            {
                r.material.color = useFirstColor ? color1 : color2;
            }
        }
    }

    void RemoveDecal()
    {
        if (currentDecal != null)
        {
            Destroy(currentDecal);
            currentDecal = null;
            Debug.Log("Image Removed");
        }
    }
}
