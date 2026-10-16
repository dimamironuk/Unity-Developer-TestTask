using UnityEngine;
using Zenject;

[DisallowMultipleComponent]
public class Bullet : MonoBehaviour
{
    [SerializeField] private float _speed = 40f;
    [SerializeField] private float _lifeTime = 3f;
    [SerializeField] private float _damage = 25f;
    [SerializeField] private float _radius = 0.2f;
    [SerializeField] private LayerMask _hitMask;

    private Pool _pool;
    private TrailRenderer _trail;
    private float _timer;

    private void Awake() => _trail = GetComponentInChildren<TrailRenderer>();

    private void Launch(Pool pool, Vector3 position, Quaternion rotation)
    {
        _pool = pool;
        _timer = 0f;
        transform.SetPositionAndRotation(position, rotation);

        if (_trail != null) _trail.Clear();
    }

    private void Update()
    {
        float step = _speed * Time.deltaTime;

        if (Physics.SphereCast(transform.position, _radius, transform.forward, out RaycastHit hit, step,
                _hitMask, QueryTriggerInteraction.Ignore))
        {
            hit.collider.GetComponentInParent<IDamageable>()?.TakeDamage(_damage);
            _pool.Despawn(this);
            return;
        }

        transform.position += transform.forward * step;

        _timer += Time.deltaTime;
        if (_timer >= _lifeTime)
        {
            _pool.Despawn(this);
        }
    }

    public class Pool : MonoMemoryPool<Vector3, Quaternion, Bullet>
    {
        protected override void Reinitialize(Vector3 position, Quaternion rotation, Bullet bullet)
        {
            bullet.Launch(this, position, rotation);
        }
    }
}