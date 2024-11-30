using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallDrag : MonoBehaviour
{
    public bool isDragging { get; private set; } // To track if the object is being dragged
    private Plane dragPlane;         // The plane on which the object will move
    private Vector3 offset;          // Offset between the object and the mouse position
    private Camera cam;              // Main camera
    [SerializeField] private Rigidbody body;
    [SerializeField] private float horizontalThres = 38.7f;
    [SerializeField] private float leftBounds = -2.1f;
    [SerializeField] private float rightBounds = 9.4f;
    [SerializeField] private float topBounds = 40f;
    [SerializeField] private float bottomBounds = 30.5f;
    [SerializeField] private float lowerVerticalBounds = -0.22f;
    [SerializeField] private float upperVerticalBounds = 1.05f;
    [SerializeField] private BallSound ballSound;
    private float defaultBottomBounds;
    public bool allowDragging { get; set; } = false;
    private bool illegalMove;
    public static Action<int> MouseOverBall;

    void Start()
    {
        cam = Camera.main; // Cache the main camera
        defaultBottomBounds = bottomBounds;
    }

    private void Update()
    {
        float clampedX = Mathf.Clamp(transform.position.x, leftBounds, rightBounds);
        float clampedY = Mathf.Clamp(transform.position.y, lowerVerticalBounds, upperVerticalBounds);
        float clampedZ = Mathf.Clamp(transform.position.z, bottomBounds, topBounds);
        Vector3 clampedPos = transform.position;
        clampedPos.x = clampedX;
        clampedPos.y = clampedY;
        clampedPos.z = clampedZ;
        transform.position = clampedPos;

        if (!isDragging && Physics.Raycast(transform.position, -Vector3.forward, 0.5f, LayerMask.GetMask("Ball")) && !body.isKinematic)
        {
            ballSound.PlayClinkSound();
            StopAllMotion();
        }

        if (Physics.Raycast(transform.position, Vector3.down, 0.6f, LayerMask.GetMask("Ball"))){
            // This is for anti-cheat. If at any point a ball is detected beneath this ball. Teleport this ball to a random spawner
            TeleportToSpawner();
        }
    }

    void OnMouseDown()
    {
        // Define the drag plane (aligned with the board's surface)
        if (!allowDragging) return;
        if (!CanDrag()) return;

        dragPlane = new Plane(Vector3.up, transform.position);

        // Calculate the offset
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (dragPlane.Raycast(ray, out float enter))
        {
            offset = transform.position - ray.GetPoint(enter);
        }

        isDragging = true;
        bottomBounds = defaultBottomBounds;
    }

    private bool CanDrag()
    {
        bool hitForward = Physics.Raycast(transform.position, Vector3.forward, 2f, LayerMask.GetMask("Ball"));
        bool hitLeft = Physics.Raycast(transform.position, Vector3.left, 0.6f, LayerMask.GetMask("Ball"));
        bool hitRight = Physics.Raycast(transform.position, Vector3.right, 0.6f, LayerMask.GetMask("Ball"));

        bool hit = false;
        if (hitForward || hitLeft || hitRight){
            hit = true;
        }
        return !hit;
    }

    void OnMouseDrag()
    {
        if (!illegalMove && isDragging && CanDrag() && allowDragging)
        {
            body.isKinematic = false;
            // Cast a ray from the camera to the mouse position
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            // Move the ball along the drag plane
            if (dragPlane.Raycast(ray, out float enter))
            {
                Vector3 pos = ray.GetPoint(enter) + offset;
                pos.y = transform.position.y;
                if (transform.position.z < horizontalThres){
                    // Only move left/right if above a certain point
                    pos.x = transform.position.x;
                }
                transform.position = pos;
            }
        }
    }

    void OnMouseOver()
    {
        MouseOverBall.Invoke(gameObject.GetInstanceID());
    }

    private void OnMouseEnter()
    {
        illegalMove = false;
    }

    private void HoveredMouseOverBall(int ballId){
        if (!isDragging) return;
        if (ballId == gameObject.GetInstanceID()) return;
        CancelDrag();
        illegalMove = true;
    }

    private void CancelDrag()
    {
        body.velocity = Vector2.zero;
        body.angularVelocity = Vector2.zero;
        isDragging = false;
    }

    private void TeleportToSpawner()
    {
        CancelDrag();
        BallSpawner[] spawners = FindObjectsOfType<BallSpawner>();
        int random = UnityEngine.Random.Range(0, spawners.Length);
        transform.position = spawners[random].transform.position;
        body.isKinematic = false;
    }

    void OnMouseUp()
    {
        // Stop dragging when the mouse button is released
        isDragging = false;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Walls"))
        {
            StopAllMotion();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isDragging) return;
        if (other.gameObject.CompareTag("Walls"))
        {
            StopAllMotion();
        }
    }

    private void StopAllMotion()
    {
        if (body.isKinematic) return;
        body.velocity = Vector2.zero;
        body.angularVelocity = Vector2.zero;
        body.isKinematic = true;
        bottomBounds = transform.position.z;
        isDragging = false;
    }

    private void OnCollisionStay(Collision other)
    {
        if (other.gameObject.CompareTag("Ball")){
            bottomBounds = transform.position.z;
        }
    }

    private void OnCollisionExit(Collision other)
    {
        if (other.gameObject.CompareTag("Ball")){
            bottomBounds = defaultBottomBounds;
        }
    }

    private void OnGameStateChange(GameManager.GameState state)
    {
        switch (state)
        {
            case GameManager.GameState.Active:
                allowDragging = true;
                break;

            default:
                allowDragging = false;
                break;
        }
    }

    private void OnEnable()
    {
        GameManager.OnGameStateChange += OnGameStateChange;
        MouseOverBall += HoveredMouseOverBall;
    }

    private void OnDisable()
    {
        GameManager.OnGameStateChange -= OnGameStateChange;
        MouseOverBall -= HoveredMouseOverBall;
    }
}
