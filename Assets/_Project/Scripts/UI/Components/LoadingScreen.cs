using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using WordPuzzle.Core.Architecture;

namespace WordPuzzle.UI.Components
{
    /// <summary>
    /// Компонент экрана загрузки
    /// </summary>
    public class LoadingScreen : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI _messageText;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Transform _spinnerTransform;
        [SerializeField] private CanvasGroup _canvasGroup;
        
        [Header("Animation Settings")]
        [SerializeField] private float _fadeInDuration = 0.3f;
        [SerializeField] private float _fadeOutDuration = 0.3f;
        [SerializeField] private float _spinnerRotationSpeed = 360f;
        
        private bool _isShown = false;
        private Tween _spinnerTween;
        
        /// <summary>
        /// Инициализация компонента при создании
        /// </summary>
        private void Awake()
        {
            SetupComponents();
        }
        
        /// <summary>
        /// Настройка компонентов если они не назначены
        /// </summary>
        private void SetupComponents()
        {
            if (_canvasGroup == null)
                _canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
            
            if (_backgroundImage == null)
                _backgroundImage = GetComponent<Image>();
            
            if (_messageText == null)
                _messageText = GetComponentInChildren<TextMeshProUGUI>();
            
            // Создаем базовые элементы если не назначены
            CreateBasicElements();
            
            // Изначально скрыт
            _canvasGroup.alpha = 0f;
            gameObject.SetActive(false);
        }
        
        /// <summary>
        /// Создание базовых UI элементов
        /// </summary>
        private void CreateBasicElements()
        {
            if (_backgroundImage == null)
            {
                _backgroundImage = gameObject.AddComponent<Image>();
                _backgroundImage.color = new Color(0, 0, 0, 0.8f);
                
                var rect = GetComponent<RectTransform>();
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
            }
            
            if (_messageText == null)
            {
                var textObject = new GameObject("LoadingText");
                textObject.transform.SetParent(transform, false);
                
                _messageText = textObject.AddComponent<TextMeshProUGUI>();
                _messageText.text = "Loading...";
                _messageText.fontSize = 32;
                _messageText.color = Color.white;
                _messageText.alignment = TextAlignmentOptions.Center;
                
                var textRect = textObject.GetComponent<RectTransform>();
                textRect.anchorMin = new Vector2(0.5f, 0.4f);
                textRect.anchorMax = new Vector2(0.5f, 0.4f);
                textRect.sizeDelta = new Vector2(600, 50);
                textRect.anchoredPosition = Vector2.zero;
            }
            
            if (_spinnerTransform == null)
            {
                CreateSpinner();
            }
        }
        
        /// <summary>
        /// Создание спиннера загрузки
        /// </summary>
        private void CreateSpinner()
        {
            var spinnerObject = new GameObject("LoadingSpinner");
            spinnerObject.transform.SetParent(transform, false);
            
            var spinnerImage = spinnerObject.AddComponent<Image>();
            // Можно использовать простую иконку или создать спиннер из кода
            spinnerImage.color = Color.white;
            
            var spinnerRect = spinnerObject.GetComponent<RectTransform>();
            spinnerRect.anchorMin = new Vector2(0.5f, 0.6f);
            spinnerRect.anchorMax = new Vector2(0.5f, 0.6f);
            spinnerRect.sizeDelta = new Vector2(50, 50);
            spinnerRect.anchoredPosition = Vector2.zero;
            
            _spinnerTransform = spinnerObject.transform;
        }
        
        /// <summary>
        /// Показать экран загрузки
        /// </summary>
        public void Show(string message)
        {
            if (_isShown) 
            {
                UpdateMessage(message);
                return;
            }
            
            gameObject.SetActive(true);
            UpdateMessage(message);
            
            _canvasGroup.DOFade(1f, _fadeInDuration).SetEase(Ease.OutQuad);
            StartSpinnerAnimation();
            
            _isShown = true;
            GameLogger.LogInfo("LoadingScreen", $"Loading screen shown: {message}");
        }
        
        /// <summary>
        /// Скрыть экран загрузки
        /// </summary>
        public void Hide(Action onComplete = null)
        {
            if (!_isShown) 
            {
                onComplete?.Invoke();
                return;
            }
            
            StopSpinnerAnimation();
            
            _canvasGroup.DOFade(0f, _fadeOutDuration)
                .SetEase(Ease.InQuad)
                .OnComplete(() =>
                {
                    gameObject.SetActive(false);
                    _isShown = false;
                    onComplete?.Invoke();
                });
            
            GameLogger.LogInfo("LoadingScreen", "Loading screen hidden");
        }
        
        /// <summary>
        /// Обновить сообщение
        /// </summary>
        public void UpdateMessage(string message)
        {
            if (_messageText != null)
            {
                _messageText.text = message ?? "Loading...";
            }
        }
        
        /// <summary>
        /// Запустить анимацию спиннера
        /// </summary>
        private void StartSpinnerAnimation()
        {
            if (_spinnerTransform != null)
            {
                _spinnerTween = _spinnerTransform.DORotate(new Vector3(0, 0, -360), 360f / _spinnerRotationSpeed, RotateMode.FastBeyond360)
                    .SetLoops(-1, LoopType.Restart)
                    .SetEase(Ease.Linear);
            }
        }
        
        /// <summary>
        /// Остановить анимацию спиннера
        /// </summary>
        private void StopSpinnerAnimation()
        {
            _spinnerTween?.Kill();
        }
        
        private void OnDestroy()
        {
            _spinnerTween?.Kill();
        }
    }
}