using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveObject : MonoBehaviour
{
    public GameObject item;
    public GameObject tempParent;
    public Transform guide;
    // Start is called before the first frame update
    void Start()
    {
        item.GetComponent<Rigidbody>().useGravity = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    /**
    * This method checks to see if the mouse is down or clicked, if it is, it sets the gravity to false, makes it kinematic so that it can move
    * it allows the object to rotate
    * 
    * @param 
    */
    void OnMouseDown()
    {
        item.GetComponent<Rigidbody>().useGravity = false;
        item.GetComponent<Rigidbody>().isKinematic = true;
        item.transform.position = guide.transform.position;
        item.transform.rotation = guide.transform.rotation;
        item.transform.parent = tempParent.transform;
    }
    /**
        * This method checks to see if the mouse is up or not clicked, if it is, it sets the gravity to true, makes the object non-kinematic, so that it does not move
        * 
        * @param 
        */
    void OnMouseUp()
    {
        item.GetComponent<Rigidbody>().useGravity = true;
        item.GetComponent<Rigidbody>().isKinematic = false;
        item.transform.parent = null;
        item.transform.position = guide.transform.position;

    }
}

