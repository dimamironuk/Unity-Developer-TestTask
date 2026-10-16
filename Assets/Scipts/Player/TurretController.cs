using UnityEngine;
using Zenject;

public class TurretController : MonoBehaviour
{
    [Header("Rotation")]
    [SerializeField] private Transform _turretHead;
    [SerializeField] private float _sensitivity = 0.5f;
    [SerializeField] private float _smoothing = 18f;
    [SerializeField] private float _maxAngle = 85f;

    [Header("Shooting")]
    [SerializeField] private Transform _firePoint;
    [SerializeField] private float _fireRate = 0.1f;

    [Header("Aim Line")]
    [SerializeField] private LineRenderer _aimLine;
    [SerializeField] private float _lineLength = 40f;

    private IInputService _input;
    private Bullet.Pool _bulletPool;
    private float _targetAngle;
    private float _currentAngle;
    private float _fireTimer;
    private bool _canShoot;

    [Inject]
    public void Construct(IInputService input, Bullet.Pool bulletPool)
    {
        _input = input;
        _bulletPool = bulletPool;
    }

    private void Start()
    {
        _aimLine.useWorldSpace = false;
        _aimLine.positionCount = 2;
        _aimLine.SetPosition(0, Vector3.zero);
        _aimLine.SetPosition(1, Vector3.forward * _lineLength);
        _aimLine.enabled = _canShoot;
    }

    public void SetCanShoot(bool canShoot)
    {
        _canShoot = canShoot;
        _fireTimer = 0f;

        if (_aimLine != null)
        {
            _aimLine.enabled = canShoot;
        }
    }

    private void Update()
    {
        if (!_canShoot) return;

        Rotate();
        Shoot();
    }

    private void Rotate()
    {
        if (_input.IsPressed)
        {
            _targetAngle = Mathf.Clamp(_targetAngle + _input.Delta.x * _sensitivity, -_maxAngle, _maxAngle);
        }

        float t = 1f - Mathf.Exp(-_smoothing * Time.deltaTime);
        _currentAngle = Mathf.Lerp(_currentAngle, _targetAngle, t);
        _turretHead.localRotation = Quaternion.Euler(0f, _currentAngle, 0f);
    }

    private void Shoot()
    {
        _fireTimer += Time.deltaTime;
        if (_fireTimer < _fireRate) return;

        _fireTimer -= _fireRate;

        Vector3 direction = Vector3.ProjectOnPlane(_firePoint.forward, Vector3.up).normalized;
        _bulletPool.Spawn(_firePoint.position, Quaternion.LookRotation(direction));
    }
}