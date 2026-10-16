using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private CanvasGroup _startPanel;
    [SerializeField] private CanvasGroup _hudPanel;
    [SerializeField] private CanvasGroup _winPanel;
    [SerializeField] private CanvasGroup _losePanel;
    [SerializeField] private CanvasGroup _pausePanel;

    [Header("Start Screen")]
    [SerializeField] private RectTransform _startHint;
    [SerializeField] private RectTransform _clickImage;

    [Header("HUD")]
    [SerializeField] private TextMeshProUGUI _levelText;
    [FormerlySerializedAs("_killsText")]
    [SerializeField] private TextMeshProUGUI _coinsText;
    [SerializeField] private Slider _progressBar;

    [Header("Cards")]
    [SerializeField] private RectTransform _winCard;
    [SerializeField] private RectTransform _loseCard;
    [SerializeField] private RectTransform _pauseCard;

    [Header("Buttons")]
    [SerializeField] private Button _pauseButton;
    [SerializeField] private Button _resumeButton;
    [SerializeField] private Button _nextLevelButton;
    [SerializeField] private Button[] _restartButtons;

    [Header("Damage Overlay")]
    [SerializeField] private CanvasGroup _damageOverlay;
    [SerializeField, Range(0f, 1f)] private float _flashAlpha = 0.45f;
    [SerializeField] private float _flashDuration = 0.4f;

    [Header("Animation")]
    [SerializeField] private float _fadeDuration = 0.25f;
    [SerializeField] private float _cardDuration = 0.45f;
    [SerializeField] private float _cardStartScale = 0.6f;
    [SerializeField] private float _pulseDuration = 0.7f;
    [SerializeField] private float _coinPunch = 0.2f;

    public event Action PauseClicked;
    public event Action ResumeClicked;
    public event Action RestartClicked;
    public event Action NextLevelClicked; 

    private void Awake()
    {
        _pauseButton.onClick.AddListener(() => PauseClicked?.Invoke());
        _resumeButton.onClick.AddListener(() => ResumeClicked?.Invoke());

        if (_nextLevelButton != null)
        {
            _nextLevelButton.onClick.AddListener(() => NextLevelClicked?.Invoke());
        }

        foreach (Button button in _restartButtons)
        {
            button.onClick.AddListener(() => RestartClicked?.Invoke());
        }
    }

    public void ShowStart(int level, int coins)
    {
        _levelText.text = $"{level}\nLevel";
        _coinsText.text = coins.ToString();
        UpdateProgress(0f);

        Hide(_winPanel, true);
        Hide(_losePanel, true);
        Hide(_pausePanel, true);
        Show(_hudPanel);
        Show(_startPanel);
        PlayStartPulse();
    }

    public void ShowGameplay()
    {
        _startHint.DOKill();
        _clickImage.DOKill();
        Hide(_startPanel);
    }

    public void SetPaused(bool isPaused)
    {
        if (isPaused)
        {
            Show(_pausePanel, _pauseCard);
        }
        else
        {
            Hide(_pausePanel);
        }
    }

    public void ShowResult(bool isWin)
    {
        Hide(_hudPanel);
        Show(isWin ? _winPanel : _losePanel, isWin ? _winCard : _loseCard);
    }

    public void UpdateCoins(int coins)
    {
        _coinsText.text = coins.ToString();

        _coinsText.rectTransform.DOKill(true);
        _coinsText.rectTransform
            .DOPunchScale(Vector3.one * _coinPunch, 0.25f, 6, 0.7f)
            .SetLink(_coinsText.gameObject);
    }

    public void UpdateProgress(float normalized) => _progressBar.value = normalized;

    public void FlashDamage(float intensity)
    {
        _damageOverlay.DOKill();
        _damageOverlay.alpha = Mathf.Clamp01(_flashAlpha * intensity);
        _damageOverlay
            .DOFade(0f, _flashDuration)
            .SetEase(Ease.OutQuad)
            .SetLink(gameObject);
    }

    private void PlayStartPulse()
    {
        _startHint.DOKill();
        _clickImage.DOKill();
        _startHint.localScale = Vector3.one;
        _clickImage.localScale = Vector3.one;

        _startHint
            .DOScale(1.08f, _pulseDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .SetLink(_startHint.gameObject);

        _clickImage
            .DOScale(0.85f, _pulseDuration * 0.7f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .SetLink(_clickImage.gameObject);
    }

    private void Show(CanvasGroup panel, RectTransform card = null)
    {
        panel.DOKill();
        panel.gameObject.SetActive(true);
        panel.alpha = 0f;
        panel.DOFade(1f, _fadeDuration).SetUpdate(true).SetLink(panel.gameObject);

        if (card == null) return;

        card.DOKill();
        card.localScale = Vector3.one * _cardStartScale;
        card.DOScale(1f, _cardDuration)
            .SetEase(Ease.OutBack)
            .SetUpdate(true)
            .SetLink(card.gameObject);
    }

    private void Hide(CanvasGroup panel, bool instant = false)
    {
        panel.DOKill();

        if (instant || !panel.gameObject.activeSelf)
        {
            panel.gameObject.SetActive(false);
            return;
        }

        panel.DOFade(0f, _fadeDuration)
            .SetUpdate(true)
            .OnComplete(() => panel.gameObject.SetActive(false))
            .SetLink(panel.gameObject);
    }
}