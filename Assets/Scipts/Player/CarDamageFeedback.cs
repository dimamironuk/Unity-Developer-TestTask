using DG.Tweening;
using UnityEngine;
using Zenject;

public class CarDamageFeedback : MonoBehaviour
{
    [Header("Intensity")]
    [SerializeField] private float _minIntensity = 0.6f;
    [SerializeField] private float _maxIntensity = 1.5f;
    [SerializeField] private float _damageToIntensity = 4f;

    [Header("Health Bar")]
    [SerializeField] private RectTransform _healthBar;
    [SerializeField] private float _barShakeDuration = 0.3f;
    [SerializeField] private float _barShakeStrength = 12f;

    [Header("Haptics")]
    [SerializeField] private bool _vibrate = true;
    [SerializeField] private float _vibrateCooldown = 0.4f;

    private Health _health;
    private UIManager _ui;
    private float _lastVibrateTime = -10f;

    [Inject]
    public void Construct(
        [Inject(Id = GameInstaller.CarHealthId)] Health health,
        UIManager ui)
    {
        _health = health;
        _ui = ui;
    }

    private void Start()
    {
        _health.OnDamaged += HandleDamaged;
    }

    private void OnDestroy()
    {
        if (_health != null)
        {
            _health.OnDamaged -= HandleDamaged;
        }
    }

    private void HandleDamaged(float damage)
    {
        float intensity = Mathf.Clamp(
            _minIntensity + damage / Mathf.Max(_health.MaxHealth, 1f) * _damageToIntensity,
            _minIntensity,
            _maxIntensity);

        _ui.FlashDamage(intensity);

        if (_healthBar != null)
        {
            _healthBar.DOKill(true);
            _healthBar
                .DOShakeAnchorPos(_barShakeDuration, _barShakeStrength, 25, 90f, false, true)
                .SetLink(_healthBar.gameObject);
        }

#if UNITY_ANDROID || UNITY_IOS
        if (_vibrate && Time.unscaledTime - _lastVibrateTime >= _vibrateCooldown)
        {
            _lastVibrateTime = Time.unscaledTime;
            Handheld.Vibrate();
        }
#endif
    }
}