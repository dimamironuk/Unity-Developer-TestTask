using System;
using DG.Tweening;
using UnityEngine;
using Zenject;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody))]
public class EnemyController : MonoBehaviour
{
    private enum State
    {
        Idle,
        Chasing,
        Attacking,
        Dead
    }

    [Header("References")]
    [SerializeField] private Animator _animator;
    [SerializeField] private Health _health;
    [SerializeField] private Collider _collider;

    [Header("Movement")]
    [SerializeField] private float _runSpeed = 9f;
    [SerializeField] private float _animationRunSpeed = 6f;
    [SerializeField] private float _turnSpeed = 12f;
    [SerializeField] private float _wakeDistance = 25f;
    [SerializeField] private float _attackDistance = 2.2f;
    [SerializeField] private float _attackExitDistance = 3.2f;
    [SerializeField] private float _despawnBehindDistance = 15f;
    [SerializeField] private float _speedDampTime = 0.1f;

    [Header("Combat")]
    [SerializeField] private float _attackDamage = 10f;
    [SerializeField] private float _attackCooldown = 1.2f;
    [SerializeField] private float _damageDelay = 0.25f;

    [Header("Knockback")]
    [SerializeField] private float _hitKnockback = 2f;
    [SerializeField] private float _deathKnockback = 5f;
    [SerializeField] private float _knockbackDecay = 14f;

    [Header("Death")]
    [SerializeField] private float _deathFallAngle = 85f;
    [SerializeField] private float _deathFallDuration = 0.35f;
    [SerializeField] private float _deathDespawnDelay = 1f;
    [SerializeField] private float _deathShrinkDuration = 0.3f;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int AttackStateHash = Animator.StringToHash("Zombie Attack");
    private CarController _car;
    private IDamageable _carDamageable;
    private Pool _pool;
    private State _state;
    private float _attackTimer;
    private Vector3 _baseScale;
    private Vector3 _knockback;
    private Sequence _deathSequence;
    private Tween _damageTween;

    public event Action Died;

    [Inject]
    public void Construct(CarController car, [Inject(Id = GameInstaller.CarHealthId)] Health carHealth)
    {
        _car = car;
        _carDamageable = carHealth;
    }

    private void Awake()
    {
        _baseScale = transform.localScale;
        _animator.applyRootMotion = false;

        Rigidbody body = GetComponent<Rigidbody>();
        body.isKinematic = true;
        body.useGravity = false;
    }

    private void OnEnable()
    {
        _state = State.Idle;
        _knockback = Vector3.zero;
        transform.localScale = _baseScale;
        _collider.enabled = true;

        _health.OnDied += HandleDeath;
        _health.OnDamaged += HandleDamaged;
        _health.ResetHealth();

        _animator.speed = 1f;
        _animator.Rebind();
        _animator.Update(0f);
    }

    private void OnDisable()
    {
        _health.OnDied -= HandleDeath;
        _health.OnDamaged -= HandleDamaged;
        _deathSequence?.Kill();
        _damageTween?.Kill();
    }

    private void Init(Pool pool, Vector3 position, Quaternion rotation)
    {
        _pool = pool;
        transform.SetPositionAndRotation(position, rotation);
    }

    private void Update()
    {
        ApplyKnockback();

        if (_state == State.Dead || !_car.IsDriving) return;

        Vector3 toCar = _car.transform.position - transform.position;
        toCar.y = 0f;

        switch (_state)
        {
            case State.Idle:
                if (toCar.sqrMagnitude <= _wakeDistance * _wakeDistance) _state = State.Chasing;
                break;
            case State.Chasing:
                Chase(toCar);
                break;
            case State.Attacking:
                Attack(toCar);
                break;
        }

        if (transform.position.z < _car.transform.position.z - _despawnBehindDistance)
        {
            Despawn();
        }
    }

    private void ApplyKnockback()
    {
        if (_knockback.sqrMagnitude < 0.0001f) return;

        transform.position += _knockback * Time.deltaTime;
        _knockback = Vector3.MoveTowards(_knockback, Vector3.zero, _knockbackDecay * Time.deltaTime);
    }

    private void Chase(Vector3 toCar)
    {
        float distance = toCar.magnitude;

        if (distance <= _attackDistance)
        {
            _state = State.Attacking;
            _attackTimer = _attackCooldown;
            return;
        }

        Face(toCar);
        transform.position += toCar / distance * (_runSpeed * Time.deltaTime);

        _animator.speed = _runSpeed / _animationRunSpeed;
        _animator.SetFloat(SpeedHash, 1f, _speedDampTime, Time.deltaTime);
    }

    private void Attack(Vector3 toCar)
    {
        if (toCar.magnitude > _attackExitDistance)
        {
            _state = State.Chasing;
            return;
        }

        _animator.speed = Mathf.Max(1f, _car.Velocity.z / _animationRunSpeed);
        _animator.SetFloat(SpeedHash, 1f, _speedDampTime, Time.deltaTime);

        transform.position += _car.Velocity * Time.deltaTime;
        Face(toCar);

        _attackTimer += Time.deltaTime;
        if (_attackTimer < _attackCooldown) return;

        _attackTimer = 0f;
        _animator.CrossFadeInFixedTime(AttackStateHash, 0.05f);

        _damageTween?.Kill();
        _damageTween = DOVirtual.DelayedCall(_damageDelay, DealDamage).SetLink(gameObject);
    }

    private void DealDamage()
    {
        if (_state == State.Attacking)
        {
            _carDamageable.TakeDamage(_attackDamage);
        }
    }

    private void Face(Vector3 toCar)
    {
        if (toCar.sqrMagnitude < 0.001f) return;

        Quaternion target = Quaternion.LookRotation(toCar, Vector3.up);
        float t = 1f - Mathf.Exp(-_turnSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, target, t);
    }

    private void HandleDamaged(float damage)
    {
        if (_state == State.Dead) return;

        _knockback += Vector3.forward * _hitKnockback;
    }

    private void HandleDeath()
    {
        _state = State.Dead;
        _collider.enabled = false;
        _animator.speed = 0f;
        _damageTween?.Kill();

        _knockback += Vector3.forward * _deathKnockback;

        Quaternion fallRotation = Quaternion.AngleAxis(-_deathFallAngle, transform.right) * transform.rotation;

        _deathSequence = DOTween.Sequence()
            .Append(transform.DORotateQuaternion(fallRotation, _deathFallDuration).SetEase(Ease.OutCubic))
            .AppendInterval(_deathDespawnDelay)
            .Append(transform.DOScale(Vector3.zero, _deathShrinkDuration).SetEase(Ease.InQuad))
            .AppendCallback(Despawn)
            .SetLink(gameObject);

        Died?.Invoke();
    }

    private void Despawn() => _pool.Despawn(this);

    public class Pool : MonoMemoryPool<Vector3, Quaternion, EnemyController>
    {
        protected override void Reinitialize(Vector3 position, Quaternion rotation, EnemyController enemy)
        {
            enemy.Init(this, position, rotation);
        }
    }
}