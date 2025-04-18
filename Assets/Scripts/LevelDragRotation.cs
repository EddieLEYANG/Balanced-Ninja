using UnityEngine;

public class LevelDragRotation : MonoBehaviour
{
    [Header("References")]
    public Rigidbody2D levelRb;

    [Header("Rotation Settings")]
    public float rotationSensitivity = 1.0f;
    public float maxRotationSpeed = 100f;
    public float damping = 5f;
    public float snapBackSpeed = 2f;
    public bool useRotationLimits = false;
    public float maxRotationAngle = 45f;
    public bool autoSnapBack = false;
    
    [Header("Debug")]
    public bool showDebugInfo = false;

    private bool isDragging = false;
    private Vector3 lastMousePos;
    private float targetRotation;
    private Vector2 screenCenter;

    void Start()
    {
        if (levelRb == null)
            levelRb = GetComponent<Rigidbody2D>();
            
        // Initialize to current rotation
        targetRotation = levelRb.rotation;
        
        // Get screen center for better rotation calculations
        screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
    }

    void Update()
    {
        HandleInput();
        
        if (showDebugInfo)
        {
            Debug.Log($"Rotation: {levelRb.rotation}, Target: {targetRotation}, Dragging: {isDragging}");
        }
    }

    void FixedUpdate()
    {
        ApplyRotation();
    }
    
    void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            lastMousePos = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            
            // Auto snap back to zero if enabled
            if (autoSnapBack)
            {
                targetRotation = 0f;
            }
        }
        
        if (isDragging)
        {
            Vector3 currentMousePos = Input.mousePosition;
            if (currentMousePos == lastMousePos) return;
            
            // Get vectors from screen center for better rotation calculation
            Vector2 prevVec = ((Vector2)lastMousePos - screenCenter).normalized;
            Vector2 currVec = ((Vector2)currentMousePos - screenCenter).normalized;
            
            // Calculate rotation amount based on the angle between the two vectors
            float deltaAngle = Vector2.SignedAngle(prevVec, currVec);
            
            // Apply sensitivity
            deltaAngle *= rotationSensitivity;
            
            // Clamp to max rotation speed per frame
            deltaAngle = Mathf.Clamp(deltaAngle, -maxRotationSpeed * Time.deltaTime, maxRotationSpeed * Time.deltaTime);
            
            // Update target rotation
            targetRotation += deltaAngle;
            
            // Apply rotation limits if enabled
            if (useRotationLimits)
            {
                targetRotation = Mathf.Clamp(targetRotation, -maxRotationAngle, maxRotationAngle);
            }
            
            lastMousePos = currentMousePos;
        }
    }
    
    void ApplyRotation()
    {
        // Gradually approach the target rotation with smooth damping
        if (Mathf.Abs(levelRb.rotation - targetRotation) > 0.01f)
        {
            float currentRotation = levelRb.rotation;
            float newRotation;
            
            if (isDragging || autoSnapBack)
            {
                // Smoothly interpolate toward target
                float speedFactor = isDragging ? damping : snapBackSpeed;
                newRotation = Mathf.Lerp(currentRotation, targetRotation, Time.fixedDeltaTime * speedFactor);
            }
            else
            {
                // Keep the current target when not dragging
                newRotation = targetRotation;
            }
            
            levelRb.MoveRotation(newRotation);
        }
    }
}
