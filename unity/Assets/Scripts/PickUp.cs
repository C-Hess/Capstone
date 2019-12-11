using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUp : MonoBehaviour
{
    // Start is called before the first frame update
    public Transform theDestination;
     /**
     * This method checks to see if the mouse button is clicked down, if it is, it treats the object without gravity
     * you can move the game object from place to place.
     * 
     * 
     */
    void OnMouseDown()
    {
        GetComponent<Rigidbody>().useGravity = false;
        this.transform.position = theDestination.position;
        this.transform.parent = GameObject.Find("Destination").transform;

    }
    /**
 * This method checks to see if you released the mouse button, if you did, then it gives the object gravity again
 * 
 * @param wire - is the wire object that got cut
 */
    void OnMouseUp()
    {
        this.transform.parent = null;
        GetComponent<Rigidbody>().useGravity = true;

    }
}
