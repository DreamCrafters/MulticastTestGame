using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using WordPuzzle.Core.Architecture;
using WordPuzzle.Data.Models;

namespace WordPuzzle.Gameplay.Cluster
{
    /// <summary>
    /// Компонент для отображения кластера букв
    /// Отображает кластер с рамкой и поддерживает различные визуальные состояния
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class ClusterView : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Image _borderImage;
        [SerializeField] private TextMeshProUGUI _clusterText;
        
        [Header("Visual Settings")]
        [SerializeField] private Color _normalBackgroundColor = Color.white;
        [SerializeField] private Color _normalBorderColor = Color.black;
        [SerializeField] private Color _normalTextColor = Color.black;
        
        [Header("State Colors")]
        [SerializeField] private Color _hoveredBackgroundColor = new Color(0.9f, 0.9f, 1f, 1f);
        [SerializeField] private Color _selectedBackgroundColor = new Color(0.8f, 0.8f, 1f, 1f);
        [SerializeField] private Color _placedBackgroundColor = Color.gray;
        [SerializeField] private Color _placedTextColor = Color.white;
        
        [Header("Animation Settings")]
        [SerializeField] private bool _enableHoverAnimation = true;
        [SerializeField] private bool _enableSelectAnimation = true;
        [SerializeField] private float _hoverScale = 1.05f;
        [SerializeField] private float _selectScale = 0.95f;
        [SerializeField] private float _animationDuration = 0.2f;
        
        [Header("Layout Settings")]
        [SerializeField] private Vector2 _minSize = new Vector2(60, 50);
        [SerializeField] private Vector2 _padding = new Vector2(20, 10);
        [SerializeField] private float _borderWidth = 2f;
        [SerializeField] private int _fontSize = 20;
        
        private RectTransform _rectTransform;
        private ClusterData _clusterData;
        private ClusterState _currentState = ClusterState.Normal;
        private bool _isInitialized = false;
        
        /// <summary>
        /// Данные кластера
        /// </summary>
        public ClusterData ClusterData => _clusterData;
        
        /// <summary>
        /// Текст кластера
        /// </summary>
        public string ClusterText => _clusterData?.Text ?? "";
        
        /// <summary>
        /// Текущее состояние кластера
        /// </summary>
        public ClusterState CurrentState => _currentState;
        
        /// <summary>
        /// Размещен ли кластер на игровом поле
        /// </summary>
        public bool IsPlaced => _clusterData?.IsPlaced == true;
        
        /// <summary>
        /// Инициализация компонента
        /// </summary>
        private void Awake()
        {
            InitializeClusterView();
        }
        
        /// <summary>
        /// Инициализация компонентов
        /// </summary>
        private void InitializeClusterView()
        {
            if (_isInitialized) return;
            
            _rectTransform = GetComponent<RectTransform>();
            
            // Создаем компоненты если не назначены
            CreateComponents();
            
            // Устанавливаем начальное состояние
            SetState(ClusterState.Normal);
            
            _isInitialized = true;
            GameLogger.LogInfo("ClusterView", "ClusterView initialized");
        }
        
        /// <summary>
        /// Создание UI компонентов
        /// </summary>
        private void CreateComponents()
        {
            // Background Image
            if (_backgroundImage == null)
            {
                var backgroundObject = new GameObject("Background");
                backgroundObject.transform.SetParent(transform, false);
                
                _backgroundImage = backgroundObject.AddComponent<Image>();
                var backgroundRect = backgroundObject.GetComponent<RectTransform>();
                backgroundRect.anchorMin = Vector2.zero;
                backgroundRect.anchorMax = Vector2.one;
                backgroundRect.offsetMin = Vector2.zero;
                backgroundRect.offsetMax = Vector2.zero;
                
                GameLogger.LogInfo("ClusterView", "Background image created");
            }
            
            // Border Image
            if (_borderImage == null)
            {
                var borderObject = new GameObject("Border");
                borderObject.transform.SetParent(transform, false);
                
                _borderImage = borderObject.AddComponent<Image>();
                var borderRect = borderObject.GetComponent<RectTransform>();
                borderRect.anchorMin = Vector2.zero;
                borderRect.anchorMax = Vector2.one;
                borderRect.offsetMin = Vector2.zero;
                borderRect.offsetMax = Vector2.zero;
                
                // Создаем простую рамку
                _borderImage.sprite = CreateBorderSprite();
                _borderImage.type = Image.Type.Sliced;
                
                GameLogger.LogInfo("ClusterView", "Border image created");
            }
            
            // Cluster Text
            if (_clusterText == null)
            {
                var textObject = new GameObject("ClusterText");
                textObject.transform.SetParent(transform, false);
                
                _clusterText = textObject.AddComponent<TextMeshProUGUI>();
                _clusterText.fontSize = _fontSize;
                _clusterText.color = _normalTextColor;
                _clusterText.alignment = TextAlignmentOptions.Center;
                _clusterText.fontStyle = FontStyles.Bold;
                
                var textRect = textObject.GetComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = _padding * 0.5f;
                textRect.offsetMax = -_padding * 0.5f;
                
                GameLogger.LogInfo("ClusterView", "Cluster text created");
            }
        }
        
        /// <summary>
        /// Создание простого спрайта для рамки
        /// </summary>
        private Sprite CreateBorderSprite()
        {
            var texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            
            return Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
        }
        
        /// <summary>
        /// Настройка кластера с данными
        /// </summary>
        public void SetupCluster(ClusterData clusterData)
        {
            if (clusterData == null)
            {
                GameLogger.LogWarning("ClusterView", "Cannot setup cluster with null data");
                return;
            }
            
            _clusterData = clusterData;
            gameObject.name = $"ClusterView_{_clusterData.ClusterId}_{_clusterData.Text}";
            
            // Обновляем текст
            UpdateClusterText();
            
            // Обновляем размер
            UpdateClusterSize();
            
            // Устанавливаем состояние
            var initialState = _clusterData.IsPlaced ? ClusterState.Placed : ClusterState.Normal;
            SetState(initialState);
            
            GameLogger.LogInfo("ClusterView", $"Cluster setup: '{_clusterData.Text}' (ID: {_clusterData.ClusterId})");
        }
        
        /// <summary>
        /// Обновление текста кластера
        /// </summary>
        private void UpdateClusterText()
        {
            if (_clusterText != null && _clusterData != null)
            {
                _clusterText.text = _clusterData.Text;
            }
        }
        
        /// <summary>
        /// Обновление размера кластера в зависимости от текста
        /// </summary>
        private void UpdateClusterSize()
        {
            if (_clusterText == null || _clusterData == null) return;
            
            // Принудительное обновление размера текста
            _clusterText.ForceMeshUpdate();
            
            // Получаем размер текста
            var textBounds = _clusterText.textBounds.size;
            
            // Вычисляем размер кластера с учетом padding
            var clusterSize = new Vector2(
                Mathf.Max(_minSize.x, textBounds.x + _padding.x),
                Mathf.Max(_minSize.y, textBounds.y + _padding.y)
            );
            
            // Применяем размер
            _rectTransform.sizeDelta = clusterSize;
            
            GameLogger.LogInfo("ClusterView", $"Cluster size updated: {clusterSize} for text '{_clusterData.Text}'");
        }
        
        /// <summary>
        /// Установка состояния кластера
        /// </summary>
        public void SetState(ClusterState state)
        {
            if (_currentState == state) return;
            
            _currentState = state;
            
            // Обновляем визуал в зависимости от состояния
            switch (state)
            {
                case ClusterState.Normal:
                    SetNormalAppearance();
                    break;
                    
                case ClusterState.Hovered:
                    SetHoveredAppearance();
                    break;
                    
                case ClusterState.Selected:
                    SetSelectedAppearance();
                    break;
                    
                case ClusterState.Placed:
                    SetPlacedAppearance();
                    break;
            }
            
            // Анимация при изменении состояния
            PlayStateChangeAnimation();
            
            GameLogger.LogInfo("ClusterView", $"Cluster '{ClusterText}' state changed to: {state}");
        }
        
        /// <summary>
        /// Установка обычного вида
        /// </summary>
        private void SetNormalAppearance()
        {
            if (_backgroundImage != null)
            {
                _backgroundImage.color = _normalBackgroundColor;
            }
            
            if (_borderImage != null)
            {
                _borderImage.color = _normalBorderColor;
            }
            
            if (_clusterText != null)
            {
                _clusterText.color = _normalTextColor;
            }
        }
        
        /// <summary>
        /// Установка вида при наведении
        /// </summary>
        private void SetHoveredAppearance()
        {
            if (_backgroundImage != null)
            {
                _backgroundImage.color = _hoveredBackgroundColor;
            }
            
            if (_borderImage != null)
            {
                _borderImage.color = _normalBorderColor;
            }
            
            if (_clusterText != null)
            {
                _clusterText.color = _normalTextColor;
            }
        }
        
        /// <summary>
        /// Установка вида при выборе
        /// </summary>
        private void SetSelectedAppearance()
        {
            if (_backgroundImage != null)
            {
                _backgroundImage.color = _selectedBackgroundColor;
            }
            
            if (_borderImage != null)
            {
                _borderImage.color = _normalBorderColor;
            }
            
            if (_clusterText != null)
            {
                _clusterText.color = _normalTextColor;
            }
        }
        
        /// <summary>
        /// Установка вида размещенного кластера
        /// </summary>
        private void SetPlacedAppearance()
        {
            if (_backgroundImage != null)
            {
                _backgroundImage.color = _placedBackgroundColor;
            }
            
            if (_borderImage != null)
            {
                _borderImage.color = _normalBorderColor;
            }
            
            if (_clusterText != null)
            {
                _clusterText.color = _placedTextColor;
            }
        }
        
        /// <summary>
        /// Анимация изменения состояния
        /// </summary>
        private void PlayStateChangeAnimation()
        {
            if (!_enableHoverAnimation && !_enableSelectAnimation) return;
            
            Vector3 targetScale = Vector3.one;
            
            switch (_currentState)
            {
                case ClusterState.Hovered:
                    if (_enableHoverAnimation)
                        targetScale = Vector3.one * _hoverScale;
                    break;
                    
                case ClusterState.Selected:
                    if (_enableSelectAnimation)
                        targetScale = Vector3.one * _selectScale;
                    break;
                    
                default:
                    targetScale = Vector3.one;
                    break;
            }
            
            // Анимация масштаба
            _rectTransform.DOScale(targetScale, _animationDuration)
                .SetEase(Ease.OutQuad);
        }
        
        /// <summary>
        /// Создание тестового кластера
        /// </summary>
        public void SetupTestCluster(string text, int clusterId = 0)
        {
            var testData = ClusterData.FromString(text, clusterId);
            SetupCluster(testData);
        }
        
        /// <summary>
        /// Получение отладочной информации
        /// </summary>
        public string GetDebugInfo()
        {
            var clusterInfo = _clusterData != null ? 
                $"'{_clusterData.Text}' (ID: {_clusterData.ClusterId}, Placed: {_clusterData.IsPlaced})" : 
                "No data";
            
            return $"ClusterView: {clusterInfo}, State: {_currentState}";
        }
        
        /// <summary>
        /// Валидация настроек в редакторе
        /// </summary>
        private void OnValidate()
        {
            if (_minSize.x <= 0) _minSize.x = 60;
            if (_minSize.y <= 0) _minSize.y = 50;
            if (_padding.x < 0) _padding.x = 0;
            if (_padding.y < 0) _padding.y = 0;
            if (_borderWidth <= 0) _borderWidth = 2;
            if (_fontSize <= 0) _fontSize = 20;
            if (_hoverScale <= 0) _hoverScale = 1.05f;
            if (_selectScale <= 0) _selectScale = 0.95f;
            if (_animationDuration <= 0) _animationDuration = 0.2f;
        }
        
        /// <summary>
        /// Очистка анимаций при уничтожении
        /// </summary>
        private void OnDestroy()
        {
            DOTween.Kill(_rectTransform);
        }
        
        #if UNITY_EDITOR
        [ContextMenu("Setup Test Cluster 'КЛ'")]
        private void ContextSetupTestClusterKL()
        {
            SetupTestCluster("КЛ", 0);
        }
        
        [ContextMenu("Setup Test Cluster 'ТЕСТ'")]
        private void ContextSetupTestClusterTEST()
        {
            SetupTestCluster("ТЕСТ", 1);
        }
        
        [ContextMenu("Set State: Hovered")]
        private void ContextSetHovered()
        {
            SetState(ClusterState.Hovered);
        }
        
        [ContextMenu("Set State: Selected")]
        private void ContextSetSelected()
        {
            SetState(ClusterState.Selected);
        }
        
        [ContextMenu("Set State: Normal")]
        private void ContextSetNormal()
        {
            SetState(ClusterState.Normal);
        }
        
        [ContextMenu("Show Debug Info")]
        private void ContextShowDebugInfo()
        {
            Debug.Log(GetDebugInfo());
        }
        #endif
    }
    
    /// <summary>
    /// Состояния кластера
    /// </summary>
    public enum ClusterState
    {
        Normal,     // Обычное состояние
        Hovered,    // Наведение курсора
        Selected,   // Выбран для перетаскивания  
        Placed      // Размещен на игровом поле
    }
}