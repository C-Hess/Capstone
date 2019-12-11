using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveSprite : MonoBehaviour
{
    public Vector2 EndPosition;
    public float Speed = 0.1f;

    public Vector2 StartPosition { get; set; }
    public Vector2 CurrentGoal { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        StartPosition = transform.position;
        CurrentGoal = EndPosition;
    }

    // Update for physics system
    void FixedUpdate()
    {

        UpdateDesition();

        var direction = (CurrentGoal - (Vector2)this.transform.position).normalized;

        transform.position += (Vector3)(direction * Speed * Time.fixedDeltaTime);
    }
    /**
        * This method updates the sprite so that it moves on the dfa
        * 
        * @param 
        */
    public void UpdateDesition()
    {
        if (Vector2.Distance(transform.position, CurrentGoal) <= 1.0)
        {
            if (CurrentGoal == EndPosition)
            {
                CurrentGoal = StartPosition;
            }
            else
            {
                CurrentGoal = EndPosition;
            }
        }
    }
}
