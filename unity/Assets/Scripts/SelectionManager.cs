using UnityEngine;

public class SelectionManager : MonoBehaviour
{
    [SerializeField] private string selectableTag = "Selectable";
    

    private ISelectionResponse _selectionResponse;

    private Transform _selection;

    private void Awake()
    {
        _selectionResponse = GetComponent<ISelectionResponse>();
    }

    // Update is called once per frame
    private void Update()
    {
        // selection and deselection response
        if (_selection != null)
        {

            _selectionResponse.OnDeselect(_selection);

        }

        #region MyRegion
        //creates the ray 
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //selection determination
        _selection = null;
        if (Physics.Raycast(ray, out var hit))
        {
            var selection = hit.transform;
            if (selection.CompareTag(selectableTag))
            {
                _selection = selection;
            }
        } 
        #endregion

        // selection and deselection response
        if(_selection != null)
        {
            _selectionResponse.OnSelect(_selection);
        }
    }

    
}