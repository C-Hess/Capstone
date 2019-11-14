using UnityEngine;

internal class HighlightSelectionResponse : MonoBehaviour, ISelectionResponse
{
    [SerializeField] public Material highlightMaterial = null;
    [SerializeField] private Material defaultMaterial;

    public void OnSelect(Transform selection)
    {
        var selectionRenderer = selection.GetComponent<Renderer>();
        if(selectionRenderer != null)
        {
            defaultMaterial = selectionRenderer.material;
            selectionRenderer.material = this.highlightMaterial;
        }
    }

    public void OnDeselect(Transform selection)
    {
        var selectionRenderer = selection.GetComponent<Renderer>();
        if(selectionRenderer != null)
        {
            selectionRenderer.material = this.defaultMaterial;
        }
    }
}