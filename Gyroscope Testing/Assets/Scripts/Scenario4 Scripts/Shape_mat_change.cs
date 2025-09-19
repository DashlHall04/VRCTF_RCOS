using UnityEngine;

public class MaterialChanger : MonoBehaviour
{
    [Header("Materials to switch between")]
    public Material[] materials;

    [Header("Renderer of the item")]
    public MeshRenderer meshRenderer;

    private int currentIndex = 0;

    void Start()
    {
        if (meshRenderer == null)
        {
            meshRenderer = GetComponent<MeshRenderer>();
        }

        if (materials.Length > 0 && meshRenderer != null)
        {
            meshRenderer.material = materials[0];
        }
    }

    // Change to a specific material by index
    public void SetMaterial(int index)
    {
        if (meshRenderer == null || materials.Length == 0) return;
        if (index < 0 || index >= materials.Length) return;

        currentIndex = index;
        meshRenderer.material = materials[currentIndex];
    }

    // Cycle to the next material
    public void NextMaterial()
    {
        if (meshRenderer == null || materials.Length == 0) return;

        currentIndex = (currentIndex + 1) % materials.Length;
        meshRenderer.material = materials[currentIndex];
    }
}