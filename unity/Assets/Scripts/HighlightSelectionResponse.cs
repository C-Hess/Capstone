using UnityEngine;

internal class HighlightSelectionResponse : MonoBehaviour, ISelectionResponse
{
    [SerializeField] public Material highlightMaterial = null;
    [SerializeField] private Material defaultMaterial;
    /**
        * This method checks to see if the selectable object was hovered over, if it is, it turns the object yellow 
        * 
        * @param selection - the object selected
        */
    public void OnSelect(Transform selection)
    {
        var selectionRenderer = selection.GetComponent<Renderer>();
        if(selectionRenderer != null)
        {
            defaultMaterial = selectionRenderer.material;
            selectionRenderer.material = this.highlightMaterial;
        }
    }
    /**
        * This method checks to see if the selected object is no longer selected/hovered over
        * if it is not hovered over anymore the default material color is shown
        * 
        * @param selection - the selected object
        */
    public void OnDeselect(Transform selection)
    {
        var selectionRenderer = selection.GetComponent<Renderer>();
        if(selectionRenderer != null)
        {
            selectionRenderer.material = this.defaultMaterial;
        }
    }
}