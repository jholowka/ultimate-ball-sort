using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinDetector : MonoBehaviour
{
    private float raycastLength = 8f;
    [SerializeField] private GameManager gameManager;
    public bool victory {get; private set;}
    public static Action OnColumnComplete;

    private void Update()
    {
        if (victory) return;
        RaycastHit[] hits = Physics.RaycastAll(transform.position, Vector3.forward, raycastLength, LayerMask.GetMask("Ball"));
        if (hits == null) return;

        Debug.DrawRay(transform.position, Vector3.forward * raycastLength, Color.red);

        if (GetVictory(hits)){
            SetColumnVictory(hits);
        }
    }

    private bool GetVictory(RaycastHit[] hits)
    {
        if (hits.Length != gameManager.winThres) return false;
        Ball.BallColour targetColour = Ball.BallColour.Blue;

        for (int i = 0; i < hits.Length; i++){
            Ball ball = hits[i].transform.gameObject.GetComponent<Ball>();
            if (ball.GetIsMoving()) return false; // Cannot detect win if any ball is moving
            if (i == 0)
            {
                targetColour = ball.ballColour;
                continue;
            }
            else
            {
                if (ball.ballColour == targetColour) continue;
                else return false;
            }
        }

        return true;
    }

    private void SetColumnVictory(RaycastHit[] hits)
    {
        victory = true;
        foreach (RaycastHit hit in hits)
        {
            hit.transform.gameObject.GetComponent<Ball>().SetVictory();
        }

        OnColumnComplete?.Invoke();
    }
}
