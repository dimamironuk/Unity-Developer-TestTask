using UnityEngine;
using Zenject;

public class CarController : MonoBehaviour
{
    [Header("Wheels")]
    [SerializeField] private Transform[] _wheels;
    [SerializeField] private float _wheelRadius = 0.4f;

    private LevelConfig _config;
    private float _currentSpeed;
    private float _targetSpeed;
    private float _startZ;

    public bool IsDriving { get; private set; }
    public Vector3 Velocity => Vector3.forward * _currentSpeed;
    public float DistanceTraveled => transform.position.z - _startZ;

    [Inject]
    public void Construct(LevelConfig config) => _config = config;

    private void Awake() => _startZ = transform.position.z;

    public void StartMoving()
    {
        IsDriving = true;
        _targetSpeed = _config.CarSpeed;
    }

    public void StopMoving()
    {
        IsDriving = false;
        _targetSpeed = 0f;
    }

    private void Update()
    {
        _currentSpeed = Mathf.MoveTowards(_currentSpeed, _targetSpeed, _config.CarAcceleration * Time.deltaTime);
        if (_currentSpeed <= 0f) return;

        float step = _currentSpeed * Time.deltaTime;
        transform.position += Vector3.forward * step;
        RotateWheels(step);
    }

    private void RotateWheels(float distance)
    {
        float angle = distance / _wheelRadius * Mathf.Rad2Deg;

        foreach (Transform wheel in _wheels)
        {
            wheel.Rotate(Vector3.right, angle, Space.Self);
        }
    }
}