using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallSpawner : MonoBehaviour
{
    public void SpawnBall(GameObject newBall){
        GameObject ballGO = Instantiate(newBall, transform.position, Quaternion.identity);
        ballGO.transform.position = transform.position;
        ballGO.transform.SetParent(transform);
    }
}
