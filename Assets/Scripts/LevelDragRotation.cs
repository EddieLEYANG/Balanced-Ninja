using UnityEngine;

public class LevelDragRotation : MonoBehaviour
{
    public Rigidbody2D levelRb;   // 拖到 Inspector

    private bool isDragging = false;
    private Vector3 lastMousePos;
    private Vector2 pivotScreenPos;

    void Start()
    {
        if (levelRb == null)
            levelRb = GetComponent<Rigidbody2D>();

        pivotScreenPos = Camera.main.WorldToScreenPoint(levelRb.position);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            lastMousePos = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }
    }

    void FixedUpdate()
    {
        if (!isDragging) return;

        Vector3 currentMousePos = Input.mousePosition;
        if (currentMousePos == lastMousePos) return;

        // 计算角度差
        Vector2 prevDir = (Vector2)lastMousePos - pivotScreenPos;
        Vector2 currDir = (Vector2)currentMousePos - pivotScreenPos;
        float angleDiff = Vector2.SignedAngle(prevDir, currDir);

        // 用物理接口旋转
        float newAngle = levelRb.rotation + angleDiff;
        levelRb.MoveRotation(newAngle);

        lastMousePos = currentMousePos;
    }
}
