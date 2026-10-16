using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(-10)]
public class Health : MonoBehaviour, IDamageable
{
    [Header("Health")]
    [SerializeField] private float _maxHealth = 100f;
    [SerializeField] private Slider _healthSlider;

    [Header("Hit Feedback")]
    [SerializeField] private Transform _visualModel;
    [SerializeField] private Color _damageColor = Color.white;
    [SerializeField] private float _flashIntensity = 1.5f;
    [SerializeField] private float _punchStrength = 0.2f;
    [SerializeField] private float _effectDuration = 0.2f;
    [SerializeField] private int _punchVibrato = 5;
    [SerializeField] private float _punchElasticity = 0.5f;
    [SerializeField] private ParticleSystem _hitParticles;

    private static readonly int EmissionId = Shader.PropertyToID("_EmissionColor");

    private readonly List<Material> _materials = new List<Material>();

    private float _currentHealth;
    private Vector3 _originalScale = Vector3.one;
    private Tween _punchTween;
    private Tween _flashTween;

    public event Action OnDied;
    public event Action<float> OnDamaged;

    public bool IsDead => _currentHealth <= 0f;
    public float MaxHealth => _maxHealth;

    private void Awake()
    {
        if (_visualModel != null)
        {
            _originalScale = _visualModel.localScale;
        }

        CollectMaterials();
        ResetHealth();
    }

    private void OnDestroy()
    {
        _punchTween?.Kill();
        _flashTween?.Kill();

        foreach (Material material in _materials)
        {
            if (material != null) Destroy(material);
        }
    }

    public void ResetHealth()
    {
        _punchTween?.Kill();
        _flashTween?.Kill();

        if (_visualModel != null)
        {
            _visualModel.localScale = _originalScale;
        }

        ApplyFlash(0f);
        _currentHealth = _maxHealth;
        UpdateSlider();
    }

    public void TakeDamage(float damage)
    {
        if (IsDead || damage <= 0f) return;

        _currentHealth = Mathf.Max(0f, _currentHealth - damage);
        UpdateSlider();
        PlayHitFeedback();

        OnDamaged?.Invoke(damage);

        if (IsDead)
        {
            OnDied?.Invoke();
        }
    }

    private void CollectMaterials()
    {
        Transform root = _visualModel != null ? _visualModel : transform;

        foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
        {
            if (!(renderer is MeshRenderer) && !(renderer is SkinnedMeshRenderer)) continue;

            foreach (Material material in renderer.materials)
            {
                if (!material.HasProperty(EmissionId)) continue;

                material.EnableKeyword("_EMISSION");
                _materials.Add(material);
            }
        }
    }

    private void PlayHitFeedback()
    {
        if (_visualModel != null && !IsDead)
        {
            _punchTween?.Kill(true);
            _visualModel.localScale = _originalScale;
            _punchTween = _visualModel
                .DOPunchScale(_originalScale * _punchStrength, _effectDuration, _punchVibrato, _punchElasticity)
                .SetLink(gameObject);
        }

        if (_materials.Count > 0)
        {
            _flashTween?.Kill();
            ApplyFlash(1f);
            _flashTween = DOVirtual.Float(1f, 0f, _effectDuration, ApplyFlash)
                .SetEase(Ease.OutQuad)
                .SetLink(gameObject);
        }

        if (_hitParticles != null)
        {
            _hitParticles.Play();
        }
    }

    private void ApplyFlash(float t)
    {
        Color emission = _damageColor * (t * _flashIntensity);

        for (int i = 0; i < _materials.Count; i++)
        {
            _materials[i].SetColor(EmissionId, emission);
        }
    }

    private void UpdateSlider()
    {
        if (_healthSlider != null)
        {
            _healthSlider.value = _currentHealth / _maxHealth;
        }
    }
}