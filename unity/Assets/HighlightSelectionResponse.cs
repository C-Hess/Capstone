using UnityEngine;

internal interface HighlightSelectionResponse
{
    void OnDeselect(Transform selection);
    void OnSelect(Transform selection);
}