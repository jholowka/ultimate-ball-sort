using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    public enum BallColour { Blue, Green, Red, Yellow, Purple }
    [field: SerializeField] public BallColour ballColour { get; private set; }
    [SerializeField] private BallDrag ballDrag;
    [SerializeField] private Rigidbody body;

    public bool GetIsMoving()
    {
        if (body.linearVelocity.sqrMagnitude > 0.02f)
        {
            return true;
        }

        return false;
    }

    public void SetVictory()
    {
        ballDrag.allowDragging = false;
    }

    public void DisableDrag(){
        ballDrag.enabled = false;
        body.isKinematic = false;
    }
}
