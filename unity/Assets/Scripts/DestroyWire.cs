using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyWire : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if(Physics.Raycast(ray, out hit))
            {
                MeshCollider mc = hit.collider as MeshCollider;
                if(mc != null)
                {
                    //Destroy(mc.gameObject);
                    mc.gameObject.SetActive(false); //instead of destroying the object, im doing this so it just deactivates it.
                    Debug.Log("I hit a wire.");
                }
            }
        }
    }
    /*private void OnMouseDown()
    {
        Destroy(gameObject);
    }*/
}
