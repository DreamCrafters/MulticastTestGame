using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace WordPuzzle.UI.Components
{
    /// <summary>
    /// Компонент всплывающего сообщения
    /// </summary>
    public class MessagePopup : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI _messageText;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private CanvasGroup _canvasGroup;

        [Header("Animation Settings")]
        [SerializeField] private float _showDuration = 0.3f;
        [SerializeField] private float _hideDuration = 0.3f;
        [SerializeField] private Ease _showEase = Ease.OutBack;
        [SerializeField] private Ease _hideEase = Ease.InQuad;

        private Action _onHideComplete;

        /// <summary>
        /// Инициализация компонента
        /// </summary>
        private void Awake()
        {
            SetupComponents();
        }

        /// <summary>
        /// Настройка компонентов
        /// </summary>
        private void SetupComponents()
        {
            if (_canvasGroup == null)
                _canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();

            CreateBasicElements();

            // Начальное состояние
            _canvasGroup.alpha = 0f;
            transform.localScale = Vector3.zero;
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Создание базовых элементов
        /// </summary>
        private void CreateBasicElements()
        {
            if (_backgroundImage == null)
            {
                _backgroundImage = gameObject.AddComponent<Image>();
                _backgroundImage.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
            }

            if (_messageText == null)
            {
                var textObject = new GameObject("MessageText");
                textObject.transform.SetParent(transform, false);

                _messageText = textObject.AddComponent<TextMeshProUGUI>();
                _messageText.fontSize = 24;
                _messageText.color = Color.white;
                _messageText.alignment = TextAlignmentOptions.Center;
                _messageText.margin = new Vector4(20, 10, 20, 10);

                var textRect = textObject.GetComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = Vector2.zero;
                textRect.offsetMax = Vector2.zero;
            }
        }

        /// <summary>
        /// Показать сообщение
        /// </summary>
        public void Show(string message, float duration, Action onComplete = null)
        {
            _onHideComplete = onComplete;

            gameObject.SetActive(true);
            _messageText.text = message;

            var sequence = DOTween.Sequence();
            sequence.Append(_canvasGroup.DOFade(1f, _showDuration).SetEase(_showEase));
            sequence.Join(transform.DOScale(Vector3.one, _showDuration).SetEase(_showEase));
            sequence.AppendInterval(duration);
            sequence.AppendCallback(Hide);
        }

        /// <summary>
        /// Скрыть сообщение
        /// </summary>
        public void Hide()
        {
            var sequence = DOTween.Sequence();
            sequence.Append(_canvasGroup.DOFade(0f, _hideDuration).SetEase(_hideEase));
            sequence.Join(transform.DOScale(Vector3.zero, _hideDuration).SetEase(_hideEase));
            sequence.OnComplete(() =>
            {
                gameObject.SetActive(false);
                _onHideComplete?.Invoke();
                Destroy(gameObject);
            });
        }

        private void OnDestroy()
        {
            DOTween.Kill(this);
        }
    }
}