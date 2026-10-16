using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Target & Offset")]
    [SerializeField] private Transform _target;
    [SerializeField] private Vector3 _offset = new Vector3(0f, 4f, -7f);

    [Header("Smooth Settings")]
    [SerializeField] private float _smoothSpeed = 8f;

    private void LateUpdate()
    {
        if (_target == null) return;

        Vector3 desiredPosition = _target.position + _offset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, _smoothSpeed * Time.deltaTime);
    }

    public void SetTarget(Transform target)
    {
        _target = target;
    }
}