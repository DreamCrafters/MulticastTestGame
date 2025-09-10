using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WordPuzzle.Core.Architecture;
using WordPuzzle.Data.Models;

namespace WordPuzzle.Gameplay.Cluster
{
    /// <summary>
    /// Менеджер кластеров для управления всеми кластерами в игровой сцене
    /// Обеспечивает централизованное управление состоянием кластеров
    /// </summary>
    public class ClusterManager : MonoBehaviour
    {
        [Header("Cluster Management")]
        [SerializeField] private ClusterPanel _clusterPanel;
        [SerializeField] private bool _autoInitializeWithPanel = true;
        
        [Header("Settings")]
        [SerializeField] private bool _enableClusterValidation = true;
        [SerializeField] private bool _allowDuplicateClusters = false;
        [SerializeField] private int _maxClustersPerLevel = 20;
        
        [Header("Debug")]
        [SerializeField] private bool _enableDebugLogging = true;

        // События для системы валидации и взаимодействия
        public event Action<ClusterData> OnClusterCreated;
        public event Action<ClusterData> OnClusterPlaced;
        public event Action<ClusterData> OnClusterRemoved;
        public event Action<ClusterData> OnClusterSelected;
        public event Action OnAllClustersValidated;

        // Управление кластерами
        private readonly Dictionary<int, ClusterData> _allClusters = new Dictionary<int, ClusterData>();
        private readonly Dictionary<int, ClusterView> _clusterViews = new Dictionary<int, ClusterView>();
        private readonly List<ClusterData> _placedClusters = new List<ClusterData>();
        private readonly List<ClusterData> _availableClusters = new List<ClusterData>();

        private LevelData _currentLevelData;
        private ClusterData _selectedCluster;
        private bool _isInitialized = false;

        /// <summary>
        /// Все кластеры уровня
        /// </summary>
        public IReadOnlyDictionary<int, ClusterData> AllClusters => _allClusters;

        /// <summary>
        /// Размещенные кластеры
        /// </summary>
        public IReadOnlyList<ClusterData> PlacedClusters => _placedClusters;

        /// <summary>
        /// Доступные кластеры
        /// </summary>
        public IReadOnlyList<ClusterData> AvailableClusters => _availableClusters;

        /// <summary>
        /// Выбранный кластер
        /// </summary>
        public ClusterData SelectedCluster => _selectedCluster;

        /// <summary>
        /// Данные текущего уровня
        /// </summary>
        public LevelData CurrentLevelData => _currentLevelData;

        /// <summary>
        /// Инициализирован ли менеджер
        /// </summary>
        public bool IsInitialized => _isInitialized;

        /// <summary>
        /// Инициализация менеджера кластеров
        /// </summary>
        private void Awake()
        {
            InitializeClusterManager();
        }

        /// <summary>
        /// Инициализация компонента
        /// </summary>
        private void InitializeClusterManager()
        {
            if (_isInitialized) return;

            GameLogger.LogInfo("ClusterManager", "Initializing ClusterManager...");

            // Поиск ClusterPanel если не назначен
            if (_clusterPanel == null && _autoInitializeWithPanel)
            {
                _clusterPanel = FindFirstObjectByType<ClusterPanel>();
                if (_clusterPanel == null)
                {
                    GameLogger.LogWarning("ClusterManager", "ClusterPanel not found in scene");
                }
                else
                {
                    GameLogger.LogInfo("ClusterManager", "ClusterPanel found and assigned");
                }
            }

            _isInitialized = true;
            GameLogger.LogInfo("ClusterManager", "ClusterManager initialized successfully");
        }

        /// <summary>
        /// Загрузка уровня и создание кластеров
        /// </summary>
        /// <param name="levelData">Данные уровня</param>
        public void LoadLevel(LevelData levelData)
        {
            if (levelData == null)
            {
                GameLogger.LogError("ClusterManager", "Cannot load null level data");
                return;
            }

            if (!_isInitialized)
            {
                InitializeClusterManager();
            }

            GameLogger.LogInfo("ClusterManager", $"Loading level {levelData.LevelId} with {levelData.AvailableClusters.Length} clusters");

            // Очищаем предыдущие данные
            ClearAllClusters();

            _currentLevelData = levelData;

            // Создаем кластеры из данных уровня
            var clusters = ClusterFactory.CreateClustersFromLevel(levelData);
            
            if (clusters.Count == 0)
            {
                GameLogger.LogError("ClusterManager", "No clusters were created from level data");
                return;
            }

            // Проверяем лимит кластеров
            if (clusters.Count > _maxClustersPerLevel)
            {
                GameLogger.LogWarning("ClusterManager", 
                    $"Level has {clusters.Count} clusters, exceeding limit of {_maxClustersPerLevel}");
            }

            // Добавляем кластеры в менеджер
            AddClusters(clusters);

            // Создаем визуальные представления через ClusterPanel
            CreateClusterViews();

            // Валидация если включена
            if (_enableClusterValidation)
            {
                ValidateAllClusters();
            }

            GameLogger.LogInfo("ClusterManager", $"Level {levelData.LevelId} loaded successfully");
        }

        /// <summary>
        /// Добавление кластеров в менеджер
        /// </summary>
        /// <param name="clusters">Список кластеров</param>
        private void AddClusters(List<ClusterData> clusters)
        {
            foreach (var cluster in clusters)
            {
                if (cluster == null)
                {
                    GameLogger.LogWarning("ClusterManager", "Skipping null cluster");
                    continue;
                }

                // Проверка дубликатов
                if (_allClusters.ContainsKey(cluster.ClusterId))
                {
                    if (_allowDuplicateClusters)
                    {
                        // Генерируем новый ID
                        int newId = GenerateUniqueClusterId();
                        cluster.ClusterId = newId;
                        GameLogger.LogInfo("ClusterManager", $"Assigned new ID {newId} to duplicate cluster '{cluster.Text}'");
                    }
                    else
                    {
                        GameLogger.LogWarning("ClusterManager", $"Skipping duplicate cluster ID {cluster.ClusterId}");
                        continue;
                    }
                }

                _allClusters[cluster.ClusterId] = cluster;
                _availableClusters.Add(cluster);

                OnClusterCreated?.Invoke(cluster);

                if (_enableDebugLogging)
                {
                    GameLogger.LogInfo("ClusterManager", $"Added cluster '{cluster.Text}' (ID: {cluster.ClusterId})");
                }
            }
        }

        /// <summary>
        /// Создание визуальных представлений кластеров
        /// </summary>
        private void CreateClusterViews()
        {
            if (_clusterPanel == null)
            {
                GameLogger.LogWarning("ClusterManager", "Cannot create cluster views - ClusterPanel is null");
                return;
            }

            GameLogger.LogInfo("ClusterManager", "Creating cluster views...");

            // Создаем кластеры в панели
            _clusterPanel.CreateClusters(_currentLevelData);

            // Связываем ClusterViews с данными
            LinkClusterViewsWithData();

            GameLogger.LogInfo("ClusterManager", $"Created {_clusterViews.Count} cluster views");
        }

        /// <summary>
        /// Связывание ClusterView с ClusterData
        /// </summary>
        private void LinkClusterViewsWithData()
        {
            if (_clusterPanel == null || _clusterPanel.ClusterViews == null)
            {
                return;
            }

            _clusterViews.Clear();

            foreach (var clusterView in _clusterPanel.ClusterViews)
            {
                if (clusterView?.ClusterData == null) continue;

                int clusterId = clusterView.ClusterData.ClusterId;
                if (_allClusters.ContainsKey(clusterId))
                {
                    _clusterViews[clusterId] = clusterView;
                    
                    if (_enableDebugLogging)
                    {
                        GameLogger.LogInfo("ClusterManager", $"Linked ClusterView for '{clusterView.ClusterText}' (ID: {clusterId})");
                    }
                }
            }
        }

        /// <summary>
        /// Размещение кластера на игровом поле
        /// </summary>
        /// <param name="clusterId">ID кластера</param>
        /// <param name="wordIndex">Индекс слова</param>
        /// <param name="startCellIndex">Начальная ячейка</param>
        /// <returns>true если размещение успешно</returns>
        public bool PlaceCluster(int clusterId, int wordIndex, int startCellIndex)
        {
            if (!_allClusters.TryGetValue(clusterId, out var cluster))
            {
                GameLogger.LogError("ClusterManager", $"Cluster with ID {clusterId} not found");
                return false;
            }

            if (cluster.IsPlaced)
            {
                GameLogger.LogWarning("ClusterManager", $"Cluster {clusterId} is already placed");
                return false;
            }

            // Размещаем кластер
            cluster.PlaceAt(wordIndex, startCellIndex);

            // Обновляем списки
            _availableClusters.Remove(cluster);
            _placedClusters.Add(cluster);

            // Обновляем визуальное представление
            if (_clusterViews.TryGetValue(clusterId, out var clusterView))
            {
                clusterView.SetState(ClusterState.Placed);
            }

            OnClusterPlaced?.Invoke(cluster);

            GameLogger.LogInfo("ClusterManager", $"Placed cluster '{cluster.Text}' (ID: {clusterId}) at word {wordIndex}, cell {startCellIndex}");
            return true;
        }

        /// <summary>
        /// Удаление кластера с игрового поля
        /// </summary>
        /// <param name="clusterId">ID кластера</param>
        /// <returns>true если удаление успешно</returns>
        public bool RemoveCluster(int clusterId)
        {
            if (!_allClusters.TryGetValue(clusterId, out var cluster))
            {
                GameLogger.LogError("ClusterManager", $"Cluster with ID {clusterId} not found");
                return false;
            }

            if (!cluster.IsPlaced)
            {
                GameLogger.LogWarning("ClusterManager", $"Cluster {clusterId} is not placed");
                return false;
            }

            // Убираем кластер
            cluster.RemoveFromField();

            // Обновляем списки
            _placedClusters.Remove(cluster);
            _availableClusters.Add(cluster);

            // Обновляем визуальное представление
            if (_clusterViews.TryGetValue(clusterId, out var clusterView))
            {
                clusterView.SetState(ClusterState.Normal);
            }

            OnClusterRemoved?.Invoke(cluster);

            GameLogger.LogInfo("ClusterManager", $"Removed cluster '{cluster.Text}' (ID: {clusterId}) from game field");
            return true;
        }

        /// <summary>
        /// Выбор кластера
        /// </summary>
        /// <param name="clusterId">ID кластера</param>
        /// <returns>true если выбор успешен</returns>
        public bool SelectCluster(int clusterId)
        {
            if (!_allClusters.TryGetValue(clusterId, out var cluster))
            {
                GameLogger.LogError("ClusterManager", $"Cluster with ID {clusterId} not found");
                return false;
            }

            if (cluster.IsPlaced)
            {
                GameLogger.LogWarning("ClusterManager", $"Cannot select placed cluster {clusterId}");
                return false;
            }

            // Снимаем выделение с предыдущего кластера
            if (_selectedCluster != null)
            {
                if (_clusterViews.TryGetValue(_selectedCluster.ClusterId, out var previousView))
                {
                    previousView.SetState(ClusterState.Normal);
                }
            }

            _selectedCluster = cluster;

            // Обновляем визуальное представление
            if (_clusterViews.TryGetValue(clusterId, out var clusterView))
            {
                clusterView.SetState(ClusterState.Selected);
            }

            OnClusterSelected?.Invoke(cluster);

            GameLogger.LogInfo("ClusterManager", $"Selected cluster '{cluster.Text}' (ID: {clusterId})");
            return true;
        }

        /// <summary>
        /// Снятие выделения с кластера
        /// </summary>
        public void DeselectCluster()
        {
            if (_selectedCluster == null) return;

            // Обновляем визуальное представление
            if (_clusterViews.TryGetValue(_selectedCluster.ClusterId, out var clusterView))
            {
                clusterView.SetState(ClusterState.Normal);
            }

            GameLogger.LogInfo("ClusterManager", $"Deselected cluster '{_selectedCluster.Text}' (ID: {_selectedCluster.ClusterId})");
            _selectedCluster = null;
        }

        /// <summary>
        /// Получение кластера по ID
        /// </summary>
        /// <param name="clusterId">ID кластера</param>
        /// <returns>Кластер или null</returns>
        public ClusterData GetCluster(int clusterId)
        {
            return _allClusters.TryGetValue(clusterId, out var cluster) ? cluster : null;
        }

        /// <summary>
        /// Получение представления кластера по ID
        /// </summary>
        /// <param name="clusterId">ID кластера</param>
        /// <returns>ClusterView или null</returns>
        public ClusterView GetClusterView(int clusterId)
        {
            return _clusterViews.TryGetValue(clusterId, out var view) ? view : null;
        }

        /// <summary>
        /// Поиск кластеров по тексту
        /// </summary>
        /// <param name="text">Текст кластера</param>
        /// <returns>Список найденных кластеров</returns>
        public List<ClusterData> FindClustersByText(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return new List<ClusterData>();
            }

            return _allClusters.Values.Where(c => c.Text == text.ToUpper()).ToList();
        }

        /// <summary>
        /// Валидация всех кластеров
        /// </summary>
        public void ValidateAllClusters()
        {
            GameLogger.LogInfo("ClusterManager", "Validating all clusters...");

            int validCount = 0;
            int invalidCount = 0;

            foreach (var cluster in _allClusters.Values)
            {
                if (cluster.IsValid())
                {
                    validCount++;
                }
                else
                {
                    invalidCount++;
                    GameLogger.LogWarning("ClusterManager", $"Invalid cluster detected: '{cluster.Text}' (ID: {cluster.ClusterId})");
                }
            }

            GameLogger.LogInfo("ClusterManager", $"Validation complete: {validCount} valid, {invalidCount} invalid clusters");

            if (invalidCount == 0)
            {
                OnAllClustersValidated?.Invoke();
            }
        }

        /// <summary>
        /// Очистка всех кластеров
        /// </summary>
        public void ClearAllClusters()
        {
            GameLogger.LogInfo("ClusterManager", "Clearing all clusters...");

            _allClusters.Clear();
            _clusterViews.Clear();
            _placedClusters.Clear();
            _availableClusters.Clear();
            _selectedCluster = null;
            _currentLevelData = null;

            // Очищаем панель кластеров
            if (_clusterPanel != null)
            {
                _clusterPanel.ClearAllClusters();
            }

            GameLogger.LogInfo("ClusterManager", "All clusters cleared");
        }

        /// <summary>
        /// Генерация уникального ID кластера
        /// </summary>
        /// <returns>Уникальный ID</returns>
        private int GenerateUniqueClusterId()
        {
            int id = 0;
            while (_allClusters.ContainsKey(id))
            {
                id++;
            }
            return id;
        }

        /// <summary>
        /// Проверка завершения уровня
        /// </summary>
        /// <returns>true если все кластеры размещены</returns>
        public bool IsLevelComplete()
        {
            return _availableClusters.Count == 0 && _allClusters.Count > 0;
        }

        /// <summary>
        /// Получение статистики кластеров
        /// </summary>
        /// <returns>Строка со статистикой</returns>
        public string GetStatistics()
        {
            var stats = ClusterFactory.GetClustersStatistics(_allClusters.Values.ToList());
            
            var additionalInfo = $"\nManager Statistics:\n" +
                                $"Total clusters: {_allClusters.Count}\n" +
                                $"Available: {_availableClusters.Count}\n" +
                                $"Placed: {_placedClusters.Count}\n" +
                                $"Selected: {(_selectedCluster != null ? _selectedCluster.Text : "None")}\n" +
                                $"Level complete: {IsLevelComplete()}";

            return stats + additionalInfo;
        }

        /// <summary>
        /// Создание тестового уровня
        /// </summary>
        public void CreateTestLevel()
        {
            GameLogger.LogInfo("ClusterManager", "Creating test level...");

            var testClusters = ClusterFactory.CreateTestClusters();
            ClearAllClusters();
            AddClusters(testClusters);

            // Создаем представления если есть панель
            if (_clusterPanel != null)
            {
                _clusterPanel.CreateTestClusters();
                LinkClusterViewsWithData();
            }

            GameLogger.LogInfo("ClusterManager", "Test level created");
        }

        /// <summary>
        /// Очистка при уничтожении
        /// </summary>
        private void OnDestroy()
        {
            ClearAllClusters();
            
            OnClusterCreated = null;
            OnClusterPlaced = null;
            OnClusterRemoved = null;
            OnClusterSelected = null;
            OnAllClustersValidated = null;
        }

        #if UNITY_EDITOR
        [ContextMenu("Create Test Level")]
        private void ContextCreateTestLevel()
        {
            CreateTestLevel();
        }

        [ContextMenu("Clear All Clusters")]
        private void ContextClearAllClusters()
        {
            ClearAllClusters();
        }

        [ContextMenu("Validate All Clusters")]
        private void ContextValidateAllClusters()
        {
            ValidateAllClusters();
        }

        [ContextMenu("Show Statistics")]
        private void ContextShowStatistics()
        {
            Debug.Log(GetStatistics());
        }

        [ContextMenu("Test Cluster Selection")]
        private void ContextTestClusterSelection()
        {
            if (_allClusters.Count > 0)
            {
                var firstCluster = _allClusters.Values.First();
                SelectCluster(firstCluster.ClusterId);
            }
        }
        #endif
    }
}