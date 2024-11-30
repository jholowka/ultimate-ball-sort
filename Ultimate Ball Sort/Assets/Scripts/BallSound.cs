using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallSound : MonoBehaviour
{
    [SerializeField] private AudioSource dropSound;
    [SerializeField] private AudioSource clinkSound;
    [SerializeField] private AudioSource rollSound;
    [SerializeField] private Rigidbody body;
    [SerializeField] private BallDrag ballDrag;

    private void Update()
    {
        if (!ballDrag.isDragging)
        {
            Vector3 horizontalVelocity = new Vector3(body.velocity.x, 0, body.velocity.z);
            if (horizontalVelocity.sqrMagnitude > 0.5f)
            {
                if (!rollSound.isPlaying)
                {
                    rollSound.Play();
                }
            }
            else
            {
                rollSound.Stop();
            }
        }
    }

    public void PlayClinkSound(){
        clinkSound.Play();
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Ball"))
        {
            if (gameObject.GetInstanceID() > other.gameObject.GetInstanceID())
            {
                rollSound.Stop();
                if (!clinkSound.isPlaying)
                {
                    clinkSound.Play();
                }
            }
        }
        else if (other.gameObject.CompareTag("Board") || other.gameObject.CompareTag("Walls"))
        {
            if (!dropSound.isPlaying)
            {
                dropSound.Play();
            }
        }
    }
}
