using UnityEngine;

public class ShapeController : MonoBehaviour
{
    [SerializeField]
    private Object targetObject;
    [SerializeField]
    private float moveDistance = 1f;
    [SerializeField]
    private float rotationSpeed = 5f;

    private MeshRenderer meshRenderer;
    private float targetRotation;
    private bool isRotating;

    public Object Object => targetObject;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        targetRotation = transform.eulerAngles.y;
    }

    private void Update()
    {
        if (isRotating)
        {
            // Smoothly rotate to target
            float currentRotation = transform.eulerAngles.y;
            float newRotation = Mathf.LerpAngle(currentRotation, targetRotation, Time.deltaTime * rotationSpeed);
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, newRotation, transform.eulerAngles.z);

            // Check if we're close enough to stop rotating
            if (Mathf.Abs(Mathf.DeltaAngle(newRotation, targetRotation)) < 0.1f)
            {
                isRotating = false;
            }
        }
    }

    public void SetColor(Color color)
    {
        if (meshRenderer != null && meshRenderer.material != null)
        {
            meshRenderer.material.color = color;
        }
    }

    public void RotateTo(float degrees)
    {
        targetRotation = degrees;
        isRotating = true;
    }

    public void MoveDirection(Direction direction)
    {
        Vector3 moveVector = direction switch
        {
            Direction.Forward => Vector3.forward,
            Direction.Backward => Vector3.back,
            Direction.Left => Vector3.left,
            Direction.Right => Vector3.right,
            Direction.Up => Vector3.up,
            Direction.Down => Vector3.down,
            _ => Vector3.zero
        };

        transform.Translate(moveVector * moveDistance);
    }
}
