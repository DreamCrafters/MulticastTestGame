using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WordPuzzle.Core.Architecture;
using WordPuzzle.Data.Models;

namespace WordPuzzle.Gameplay.Cluster
{
    /// <summary>
    /// Компонент для панели с кластерами букв
    /// Обеспечивает горизонтальную прокрутку и управление кластерами
    /// </summary>
    public class ClusterPanel : MonoBehaviour
    {
        [Header("Panel Configuration")]
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private Transform _clustersContainer;
        [SerializeField] private ClusterView _clusterViewPrefab;
        
        [Header("Layout Settings")]
        [SerializeField] private float _clusterSpacing = 15f;
        [SerializeField] private bool _useHorizontalLayout = true;
        [SerializeField] private RectOffset _contentPadding;
        
        [Header("Scrolling Settings")]
        [SerializeField] private bool _enableHorizontalScrolling = true;
        [SerializeField] private bool _enableVerticalScrolling = false;
        [SerializeField] private float _scrollSensitivity = 1f;
        [SerializeField] private bool _enableInertia = true;
        [SerializeField] private float _decelerationRate = 0.135f;
        
        [Header("Visual Settings")]
        [SerializeField] private bool _showPanelBackground = true;
        [SerializeField] private Color _panelBackgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
        [SerializeField] private Vector2 _panelPadding = new Vector2(10, 10);
        
        private readonly List<ClusterView> _clusterViews = new List<ClusterView>();
        private HorizontalLayoutGroup _horizontalLayoutGroup;
        private ContentSizeFitter _contentSizeFitter;
        private Image _backgroundImage;
        private bool _isInitialized = false;
        
        /// <summary>
        /// Все представления кластеров
        /// </summary>
        public IReadOnlyList<ClusterView> ClusterViews => _clusterViews;
        
        /// <summary>
        /// Инициализирована ли панель
        /// </summary>
        public bool IsInitialized => _isInitialized;
        
        /// <summary>
        /// Инициализация панели кластеров
        /// </summary>
        private void Awake()
        {
            InitializePanel();
        }
        
        /// <summary>
        /// Инициализация компонента
        /// </summary>
        private void InitializePanel()
        {
            if (_isInitialized) return;
            
            GameLogger.LogInfo("ClusterPanel", "Initializing cluster panel...");
            
            // Настраиваем ScrollRect
            SetupScrollRect();
            
            // Настраиваем контейнер для кластеров
            SetupClustersContainer();
            
            // Создаем фон панели
            if (_showPanelBackground)
            {
                CreatePanelBackground();
            }
            
            // Настраиваем layout
            if (_useHorizontalLayout)
            {
                SetupHorizontalLayout();
            }
            
            _isInitialized = true;
            GameLogger.LogInfo("ClusterPanel", "Cluster panel initialized");
        }
        
        /// <summary>
        /// Настройка ScrollRect
        /// </summary>
        private void SetupScrollRect()
        {
            if (_scrollRect == null)
            {
                _scrollRect = GetComponent<ScrollRect>();
                
                if (_scrollRect == null)
                {
                    _scrollRect = gameObject.AddComponent<ScrollRect>();
                    GameLogger.LogInfo("ClusterPanel", "ScrollRect component added");
                }
            }
            
            // Настраиваем свойства прокрутки
            _scrollRect.horizontal = _enableHorizontalScrolling;
            _scrollRect.vertical = _enableVerticalScrolling;
            _scrollRect.scrollSensitivity = _scrollSensitivity;
            _scrollRect.inertia = _enableInertia;
            _scrollRect.decelerationRate = _decelerationRate;
            
            GameLogger.LogInfo("ClusterPanel", "ScrollRect configured");
        }
        
        /// <summary>
        /// Настройка контейнера для кластеров
        /// </summary>
        private void SetupClustersContainer()
        {
            if (_clustersContainer == null)
            {
                // Создаем контейнер для содержимого
                var contentObject = new GameObject("ClustersContent");
                contentObject.transform.SetParent(transform, false);
                
                var contentRect = contentObject.AddComponent<RectTransform>();
                contentRect.anchorMin = new Vector2(0, 0.5f);
                contentRect.anchorMax = new Vector2(0, 0.5f);
                contentRect.pivot = new Vector2(0, 0.5f);
                
                _clustersContainer = contentObject.transform;
                
                GameLogger.LogInfo("ClusterPanel", "Clusters container created");
            }
            
            // Привязываем контейнер к ScrollRect
            if (_scrollRect != null)
            {
                _scrollRect.content = _clustersContainer.GetComponent<RectTransform>();
            }
        }
        
        /// <summary>
        /// Создание фона панели
        /// </summary>
        private void CreatePanelBackground()
        {
            var backgroundObject = new GameObject("PanelBackground");
            backgroundObject.transform.SetParent(transform, false);
            backgroundObject.transform.SetAsFirstSibling();
            
            _backgroundImage = backgroundObject.AddComponent<Image>();
            _backgroundImage.color = _panelBackgroundColor;
            
            var backgroundRect = backgroundObject.GetComponent<RectTransform>();
            backgroundRect.anchorMin = Vector2.zero;
            backgroundRect.anchorMax = Vector2.one;
            backgroundRect.offsetMin = -_panelPadding;
            backgroundRect.offsetMax = _panelPadding;
            
            GameLogger.LogInfo("ClusterPanel", "Panel background created");
        }
        
        /// <summary>
        /// Настройка горизонтального layout
        /// </summary>
        private void SetupHorizontalLayout()
        {
            _horizontalLayoutGroup = _clustersContainer.GetComponent<HorizontalLayoutGroup>();
            
            if (_horizontalLayoutGroup == null)
            {
                _horizontalLayoutGroup = _clustersContainer.gameObject.AddComponent<HorizontalLayoutGroup>();
            }
            
            _horizontalLayoutGroup.spacing = _clusterSpacing;
            _horizontalLayoutGroup.childControlWidth = false;
            _horizontalLayoutGroup.childControlHeight = false;
            _horizontalLayoutGroup.childForceExpandWidth = false;
            _horizontalLayoutGroup.childForceExpandHeight = false;
            _horizontalLayoutGroup.padding = _contentPadding;
            
            // Добавляем Content Size Fitter для автоматического размера
            _contentSizeFitter = _clustersContainer.GetComponent<ContentSizeFitter>();
            if (_contentSizeFitter == null)
            {
                _contentSizeFitter = _clustersContainer.gameObject.AddComponent<ContentSizeFitter>();
            }
            
            _contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            _contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            
            GameLogger.LogInfo("ClusterPanel", "Horizontal layout configured");
        }
        
        /// <summary>
        /// Создание кластеров из данных уровня
        /// </summary>
        public void CreateClusters(LevelData levelData)
        {
            if (levelData == null)
            {
                GameLogger.LogWarning("ClusterPanel", "Cannot create clusters from null level data");
                return;
            }
            
            // Очищаем существующие кластеры
            ClearAllClusters();
            
            // Создаем кластеры из данных уровня
            for (int i = 0; i < levelData.AvailableClusters.Length; i++)
            {
                var clusterText = levelData.AvailableClusters[i];
                CreateSingleCluster(clusterText, i);
            }
            
            GameLogger.LogInfo("ClusterPanel", $"Created {_clusterViews.Count} clusters from level {levelData.LevelId}");
        }
        
        /// <summary>
        /// Создание кластеров из массива строк (для тестирования)
        /// </summary>
        public void CreateClusters(string[] clusterTexts)
        {
            if (clusterTexts == null || clusterTexts.Length == 0)
            {
                GameLogger.LogWarning("ClusterPanel", "Cannot create clusters from empty array");
                return;
            }
            
            // Очищаем существующие кластеры
            ClearAllClusters();
            
            // Создаем кластеры
            for (int i = 0; i < clusterTexts.Length; i++)
            {
                CreateSingleCluster(clusterTexts[i], i);
            }
            
            GameLogger.LogInfo("ClusterPanel", $"Created {_clusterViews.Count} test clusters");
        }
        
        /// <summary>
        /// Создание одного кластера
        /// </summary>
        private void CreateSingleCluster(string clusterText, int clusterId)
        {
            if (string.IsNullOrEmpty(clusterText))
            {
                GameLogger.LogWarning("ClusterPanel", $"Skipping empty cluster at index {clusterId}");
                return;
            }
            
            GameObject clusterObject;
            
            if (_clusterViewPrefab != null)
            {
                // Создаем из префаба
                clusterObject = Instantiate(_clusterViewPrefab.gameObject, _clustersContainer);
            }
            else
            {
                // Создаем динамически
                clusterObject = CreateDynamicClusterView();
            }
            
            clusterObject.name = $"Cluster_{clusterId}_{clusterText}";
            
            var clusterView = clusterObject.GetComponent<ClusterView>();
            if (clusterView == null)
            {
                clusterView = clusterObject.AddComponent<ClusterView>();
            }
            
            // Настраиваем кластер
            var clusterData = ClusterData.FromString(clusterText, clusterId);
            clusterView.SetupCluster(clusterData);
            
            _clusterViews.Add(clusterView);
        }
        
        /// <summary>
        /// Создание простого представления кластера
        /// </summary>
        private GameObject CreateDynamicClusterView()
        {
            var clusterObject = new GameObject("ClusterView");
            clusterObject.transform.SetParent(_clustersContainer, false);
            
            // Добавляем RectTransform
            var rectTransform = clusterObject.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(80, 50); // Базовый размер
            
            return clusterObject;
        }
        
        /// <summary>
        /// Очистка всех кластеров
        /// </summary>
        public void ClearAllClusters()
        {
            foreach (var clusterView in _clusterViews)
            {
                if (clusterView != null && clusterView.gameObject != null)
                {
                    if (Application.isPlaying)
                    {
                        Destroy(clusterView.gameObject);
                    }
                    else
                    {
                        DestroyImmediate(clusterView.gameObject);
                    }
                }
            }
            
            _clusterViews.Clear();
            GameLogger.LogInfo("ClusterPanel", "All clusters cleared");
        }
        
        /// <summary>
        /// Получение представления кластера по ID
        /// </summary>
        public ClusterView GetClusterView(int clusterId)
        {
            foreach (var clusterView in _clusterViews)
            {
                if (clusterView.ClusterData?.ClusterId == clusterId)
                {
                    return clusterView;
                }
            }
            
            GameLogger.LogWarning("ClusterPanel", $"Cluster view with ID {clusterId} not found");
            return null;
        }
        
        /// <summary>
        /// Получение представления кластера по тексту
        /// </summary>
        public ClusterView GetClusterViewByText(string clusterText)
        {
            foreach (var clusterView in _clusterViews)
            {
                if (clusterView.ClusterText == clusterText)
                {
                    return clusterView;
                }
            }
            
            GameLogger.LogWarning("ClusterPanel", $"Cluster view with text '{clusterText}' not found");
            return null;
        }
        
        /// <summary>
        /// Прокрутка к конкретному кластеру
        /// </summary>
        public void ScrollToCluster(ClusterView clusterView)
        {
            if (clusterView == null || _scrollRect == null) return;
            
            var clusterRect = clusterView.GetComponent<RectTransform>();
            if (clusterRect == null) return;
            
            // Вычисляем позицию для прокрутки
            var contentRect = _scrollRect.content;
            var viewportRect = _scrollRect.viewport;
            
            if (contentRect == null || viewportRect == null) return;
            
            // Получаем позицию кластера относительно контента
            var clusterPosition = contentRect.InverseTransformPoint(clusterRect.position);
            var normalizedPosition = Mathf.Clamp01(-clusterPosition.x / (contentRect.rect.width - viewportRect.rect.width));
            
            _scrollRect.horizontalNormalizedPosition = normalizedPosition;
            
            GameLogger.LogInfo("ClusterPanel", $"Scrolled to cluster '{clusterView.ClusterText}'");
        }
        
        /// <summary>
        /// Создание тестовых кластеров
        /// </summary>
        public void CreateTestClusters()
        {
            var testClusters = new string[]
            {
                "КЛ", "АС", "ТЕР", "ПРО", "ЕКТ", "ЗА", "ДА", "ЧА", "ИГ", "РО", "КА"
            };
            
            CreateClusters(testClusters);
        }
        
        /// <summary>
        /// Получение отладочной информации
        /// </summary>
        public string GetDebugInfo()
        {
            var clusterTexts = new List<string>();
            foreach (var clusterView in _clusterViews)
            {
                clusterTexts.Add($"'{clusterView.ClusterText}'");
            }
            
            var clustersText = string.Join(", ", clusterTexts);
            
            return $"ClusterPanel: {_clusterViews.Count} clusters [{clustersText}]";
        }
        
        /// <summary>
        /// Валидация настроек в редакторе
        /// </summary>
        private void OnValidate()
        {
            if (_clusterSpacing < 0) _clusterSpacing = 0;
            if (_scrollSensitivity <= 0) _scrollSensitivity = 1f;
            if (_decelerationRate < 0) _decelerationRate = 0;
            if (_decelerationRate > 1) _decelerationRate = 1;
        }
        
        /// <summary>
        /// Очистка при уничтожении
        /// </summary>
        private void OnDestroy()
        {
            ClearAllClusters();
        }
        
        #if UNITY_EDITOR
        [ContextMenu("Create Test Clusters")]
        private void ContextCreateTestClusters()
        {
            if (!_isInitialized)
            {
                InitializePanel();
            }
            CreateTestClusters();
        }
        
        [ContextMenu("Clear All Clusters")]
        private void ContextClearAllClusters()
        {
            ClearAllClusters();
        }
        
        [ContextMenu("Scroll To First Cluster")]
        private void ContextScrollToFirstCluster()
        {
            if (_clusterViews.Count > 0)
            {
                ScrollToCluster(_clusterViews[0]);
            }
        }
        
        [ContextMenu("Show Debug Info")]
        private void ContextShowDebugInfo()
        {
            Debug.Log(GetDebugInfo());
        }
        #endif
    }
}