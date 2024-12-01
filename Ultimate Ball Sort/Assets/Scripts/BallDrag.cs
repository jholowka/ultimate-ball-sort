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
    [SerializeField] private float rollSpeed = 10f;
    [SerializeField] private float horizontalThres = 38.7f;
    [SerializeField] private float leftBounds = -2.1f;
    [SerializeField] private float rightBounds = 9.4f;
    [SerializeField] private float topBounds = 40f;
    [SerializeField] private float bottomBounds = 30.5f;
    [SerializeField] private float lowerVerticalBounds = -0.22f;
    [SerializeField] private float upperVerticalBounds = 1.05f;
    [SerializeField] private BallSound ballSound;
    [SerializeField] private bool _debug;
    private float defaultBottomBounds;
    public bool allowDragging { get; set; } = false;
    private bool illegalMove;
    private float dragTimer;
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

        if (isDragging){
            dragTimer += Time.deltaTime;
        }
        else
        {
            dragTimer = 0f;
        }

        if (isDragging && dragTimer > 0.5f)
        {
            bool hitLeft = Physics.Raycast(transform.position, Vector3.left, 0.6f, LayerMask.GetMask("Ball"));
            bool hitRight = Physics.Raycast(transform.position, Vector3.right, 0.6f, LayerMask.GetMask("Ball"));
            if (hitLeft || hitRight)
            {
                CancelDrag();
            }
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
        bool hitBehind = false;

        if (dragTimer > 0.5f){
            hitBehind = Physics.Raycast(transform.position, -Vector3.forward, 0.6f, LayerMask.GetMask("Ball"));
        }

        bool hit = false;
        if (hitForward || hitBehind){
            hit = true;
        }
        return !hit;
    }

    void OnMouseDrag()
    {
        if (_debug){
            Debug.Log ($"illegalMove? {illegalMove}. isDragging? {isDragging}. CanDrag? {CanDrag()}. allowDragging? {allowDragging}. Body is kinematic? {body.isKinematic}");
        }
        if (!illegalMove && isDragging && CanDrag() && allowDragging)
        {
            gameObject.layer = LayerMask.NameToLayer("DraggingBall");
            body.isKinematic = false;
            // Cast a ray from the camera to the mouse position
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            // Move the ball along the drag plane
            if (dragPlane.Raycast(ray, out float enter))
            {
                Vector3 targetPosition = ray.GetPoint(enter) + offset;
                targetPosition.y = transform.position.y;

                // Smoothly move left/right when below the threshold
                Vector3 smoothedPosition = Vector3.Lerp(transform.position, targetPosition, rollSpeed * Time.deltaTime);

                if (transform.position.z < horizontalThres)
                {
                    // If below the horizontal threshold only allow moving up and down
                    smoothedPosition.x = transform.position.x;
                    smoothedPosition.y = transform.position.y;
                }
                transform.position = smoothedPosition;

                // Prevent rolling down while dragging
                Vector2 fixedVelocity = body.velocity;
                fixedVelocity.y = 0;
                body.velocity = fixedVelocity;
            }
        }
    }

    void OnMouseOver()
    {
        MouseOverBall?.Invoke(gameObject.GetInstanceID());
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
        gameObject.layer = LayerMask.NameToLayer("Ball");
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
        gameObject.layer = LayerMask.NameToLayer("Ball");
    }

    private void OnTriggerStay(Collider other)
    {
        if (isDragging) return;
        if (body.isKinematic) return;
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
        gameObject.layer = LayerMask.NameToLayer("Ball");
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Ball") && dragTimer > 0.5f)
        {
            CancelDrag();
        }
    }

    private void OnCollisionStay(Collision other)
    {
        if (other.gameObject.CompareTag("Ball")){
            bottomBounds = transform.position.z;
            if (!isDragging && !body.isKinematic && Physics.Raycast(transform.position, -Vector3.forward, 0.6f, LayerMask.GetMask("Ball")))
            {
                StopAllMotion();
            }
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
