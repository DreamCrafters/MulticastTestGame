using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WordPuzzle.Core.Architecture;
using WordPuzzle.Data.Models;

namespace WordPuzzle.Gameplay.Word
{
    /// <summary>
    /// Управляющий компонент для игрового поля
    /// Отвечает за создание и управление 4 слотами для слов
    /// </summary>
    public class GameField : MonoBehaviour
    {
        [Header("Field Configuration")]
        [SerializeField] private int _wordsCount = 4;
        [SerializeField] private int _lettersPerWord = 6;
        
        [Header("Word Slots")]
        [SerializeField] private WordSlot _wordSlotPrefab;
        [SerializeField] private Transform _slotsContainer;
        [SerializeField] private float _slotSpacing = 20f;
        
        [Header("Layout Settings")]
        [SerializeField] private bool _useVerticalLayout = true;
        [SerializeField] private bool _autoSizeSlots = true;
        
        private readonly List<WordSlot> _wordSlots = new List<WordSlot>();
        private VerticalLayoutGroup _verticalLayoutGroup;
        private bool _isInitialized = false;
        
        /// <summary>
        /// Все слоты для слов
        /// </summary>
        public IReadOnlyList<WordSlot> WordSlots => _wordSlots;
        
        /// <summary>
        /// Количество слов на поле
        /// </summary>
        public int WordsCount => _wordsCount;
        
        /// <summary>
        /// Количество букв в слове
        /// </summary>
        public int LettersPerWord => _lettersPerWord;
        
        /// <summary>
        /// Инициализовано ли игровое поле
        /// </summary>
        public bool IsInitialized => _isInitialized;
        
        /// <summary>
        /// Инициализация игрового поля
        /// </summary>
        private void Awake()
        {
            InitializeGameField();
        }
        
        /// <summary>
        /// Инициализация компонента
        /// </summary>
        private void InitializeGameField()
        {
            if (_isInitialized) return;
            
            GameLogger.LogInfo("GameField", "Initializing game field...");
            
            // Настраиваем контейнер
            SetupContainer();
            
            // Настраиваем layout
            if (_useVerticalLayout)
            {
                SetupVerticalLayout();
            }
            
            // Создаем слоты для слов
            CreateWordSlots();
            
            _isInitialized = true;
            GameLogger.LogInfo("GameField", $"Game field initialized with {_wordSlots.Count} word slots");
        }
        
        /// <summary>
        /// Настройка контейнера для слотов
        /// </summary>
        private void SetupContainer()
        {
            if (_slotsContainer == null)
            {
                _slotsContainer = transform;
                GameLogger.LogWarning("GameField", "Slots container not assigned, using self transform");
            }
        }
        
        /// <summary>
        /// Настройка вертикального layout
        /// </summary>
        private void SetupVerticalLayout()
        {
            _verticalLayoutGroup = _slotsContainer.GetComponent<VerticalLayoutGroup>();
            
            if (_verticalLayoutGroup == null)
            {
                _verticalLayoutGroup = _slotsContainer.gameObject.AddComponent<VerticalLayoutGroup>();
            }
            
            _verticalLayoutGroup.spacing = _slotSpacing;
            _verticalLayoutGroup.childAlignment = TextAnchor.MiddleCenter;
            _verticalLayoutGroup.childControlWidth = _autoSizeSlots;
            _verticalLayoutGroup.childControlHeight = false;
            _verticalLayoutGroup.childForceExpandWidth = _autoSizeSlots;
            _verticalLayoutGroup.childForceExpandHeight = false;
            
            // Добавляем Content Size Fitter если нужно
            var contentSizeFitter = _slotsContainer.GetComponent<ContentSizeFitter>();
            if (contentSizeFitter == null)
            {
                contentSizeFitter = _slotsContainer.gameObject.AddComponent<ContentSizeFitter>();
            }
            
            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            
            GameLogger.LogInfo("GameField", "Vertical layout configured");
        }
        
        /// <summary>
        /// Создание слотов для слов
        /// </summary>
        private void CreateWordSlots()
        {
            // Очищаем существующие слоты
            ClearExistingSlots();
            
            for (int i = 0; i < _wordsCount; i++)
            {
                WordSlot wordSlot = CreateSingleWordSlot(i);
                if (wordSlot != null)
                {
                    _wordSlots.Add(wordSlot);
                }
            }
            
            GameLogger.LogInfo("GameField", $"Created {_wordSlots.Count} word slots");
        }
        
        /// <summary>
        /// Создание одного слота для слова
        /// </summary>
        private WordSlot CreateSingleWordSlot(int wordIndex)
        {
            GameObject slotObject;
            
            if (_wordSlotPrefab != null)
            {
                // Создаем из префаба
                slotObject = Instantiate(_wordSlotPrefab.gameObject, _slotsContainer);
            }
            else
            {
                // Создаем динамически
                slotObject = CreateDynamicWordSlot();
            }
            
            slotObject.name = $"WordSlot_{wordIndex}";
            
            var wordSlot = slotObject.GetComponent<WordSlot>();
            if (wordSlot == null)
            {
                wordSlot = slotObject.AddComponent<WordSlot>();
            }
            
            // Настраиваем слот
            wordSlot.WordIndex = wordIndex;
            
            return wordSlot;
        }
        
        /// <summary>
        /// Создание простого слота если нет префаба
        /// </summary>
        private GameObject CreateDynamicWordSlot()
        {
            var slotObject = new GameObject("WordSlot");
            slotObject.transform.SetParent(_slotsContainer, false);
            
            // Добавляем RectTransform для UI
            var rectTransform = slotObject.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(400, 70); // Размер слота для 6 букв
            
            // Добавляем горизонтальный layout для ячеек букв
            var horizontalLayout = slotObject.AddComponent<HorizontalLayoutGroup>();
            horizontalLayout.spacing = 10f;
            horizontalLayout.childAlignment = TextAnchor.MiddleCenter;
            horizontalLayout.childControlWidth = false;
            horizontalLayout.childControlHeight = false;
            horizontalLayout.childForceExpandWidth = false;
            horizontalLayout.childForceExpandHeight = false;
            
            return slotObject;
        }
        
        /// <summary>
        /// Очистка существующих слотов
        /// </summary>
        private void ClearExistingSlots()
        {
            foreach (var slot in _wordSlots)
            {
                if (slot != null && slot.gameObject != null)
                {
                    if (Application.isPlaying)
                    {
                        Destroy(slot.gameObject);
                    }
                    else
                    {
                        DestroyImmediate(slot.gameObject);
                    }
                }
            }
            _wordSlots.Clear();
        }
        
        /// <summary>
        /// Получение слота по индексу
        /// </summary>
        public WordSlot GetWordSlot(int wordIndex)
        {
            if (wordIndex < 0 || wordIndex >= _wordSlots.Count)
            {
                GameLogger.LogWarning("GameField", $"Invalid word index {wordIndex}");
                return null;
            }
            
            return _wordSlots[wordIndex];
        }
        
        /// <summary>
        /// Получение ячейки по координатам
        /// </summary>
        public LetterCell GetLetterCell(int wordIndex, int cellIndex)
        {
            var wordSlot = GetWordSlot(wordIndex);
            return wordSlot?.GetLetterCell(cellIndex);
        }
        
        /// <summary>
        /// Установка буквы в ячейку
        /// </summary>
        public bool SetLetter(int wordIndex, int cellIndex, string letter)
        {
            var wordSlot = GetWordSlot(wordIndex);
            if (wordSlot == null) return false;
            
            return wordSlot.SetLetter(cellIndex, letter);
        }
        
        /// <summary>
        /// Очистка буквы в ячейке
        /// </summary>
        public bool ClearLetter(int wordIndex, int cellIndex)
        {
            var wordSlot = GetWordSlot(wordIndex);
            if (wordSlot == null) return false;
            
            return wordSlot.ClearLetter(cellIndex);
        }
        
        /// <summary>
        /// Очистка всего игрового поля
        /// </summary>
        public void ClearAllLetters()
        {
            foreach (var wordSlot in _wordSlots)
            {
                wordSlot?.ClearAllLetters();
            }
            
            GameLogger.LogInfo("GameField", "All letters cleared from game field");
        }
        
        /// <summary>
        /// Получение всех слов как строкового массива
        /// </summary>
        public string[] GetAllWords()
        {
            var words = new string[_wordSlots.Count];
            
            for (int i = 0; i < _wordSlots.Count; i++)
            {
                words[i] = _wordSlots[i]?.GetCurrentWord() ?? "";
            }
            
            return words;
        }
        
        /// <summary>
        /// Подсветка конкретного слота
        /// </summary>
        public void HighlightWordSlot(int wordIndex, bool highlight, Color? highlightColor = null)
        {
            var wordSlot = GetWordSlot(wordIndex);
            wordSlot?.HighlightSlot(highlight, highlightColor);
        }
        
        /// <summary>
        /// Заполнение поля тестовыми данными
        /// </summary>
        public void FillWithTestData()
        {
            // Тестовые слова
            var testWords = new string[]
            {
                "КЛАСТЕР",
                "ПРОЕКТ", 
                "ЗАДАЧА",
                "ИГРОКА"
            };
            
            for (int wordIndex = 0; wordIndex < Mathf.Min(testWords.Length, _wordSlots.Count); wordIndex++)
            {
                var word = testWords[wordIndex];
                var wordSlot = _wordSlots[wordIndex];
                
                if (wordSlot != null)
                {
                    // Очищаем слот
                    wordSlot.ClearAllLetters();
                    
                    // Заполняем буквами
                    for (int letterIndex = 0; letterIndex < Mathf.Min(word.Length, _lettersPerWord); letterIndex++)
                    {
                        wordSlot.SetLetter(letterIndex, word[letterIndex].ToString());
                    }
                }
            }
            
            GameLogger.LogInfo("GameField", "Test data filled");
        }
        
        /// <summary>
        /// Загрузка данных уровня в игровое поле
        /// Пока только очищает поле, в следующих этапах будет полная реализация
        /// </summary>
        public void LoadLevelData(LevelData levelData)
        {
            if (levelData == null)
            {
                GameLogger.LogWarning("GameField", "Cannot load null level data");
                return;
            }
            
            GameLogger.LogInfo("GameField", $"Loading level {levelData.LevelId} data (placeholder implementation)");
            
            // Пока просто очищаем поле
            // В следующих этапах здесь будет полная логика размещения кластеров
            ClearAllLetters();
            
            GameLogger.LogInfo("GameField", "Level data loaded (field cleared for now)");
        }
        
        /// <summary>
        /// Получение отладочной информации
        /// </summary>
        public string GetDebugInfo()
        {
            var words = GetAllWords();
            var wordsList = string.Join(", ", words);
            
            return $"GameField: {_wordSlots.Count} slots, Words: [{wordsList}]";
        }
        
        /// <summary>
        /// Валидация настроек в редакторе
        /// </summary>
        private void OnValidate()
        {
            if (_wordsCount <= 0) _wordsCount = 4;
            if (_lettersPerWord <= 0) _lettersPerWord = 6;
            if (_slotSpacing < 0) _slotSpacing = 0;
        }
        
        /// <summary>
        /// Очистка при уничтожении
        /// </summary>
        private void OnDestroy()
        {
            ClearExistingSlots();
        }
        
        #if UNITY_EDITOR
        [ContextMenu("Fill With Test Data")]
        private void ContextFillWithTestData()
        {
            if (!_isInitialized)
            {
                InitializeGameField();
            }
            FillWithTestData();
        }
        
        [ContextMenu("Clear All Letters")]
        private void ContextClearAllLetters()
        {
            ClearAllLetters();
        }
        
        [ContextMenu("Recreate Slots")]
        private void ContextRecreateSlots()
        {
            _isInitialized = false;
            InitializeGameField();
        }
        
        [ContextMenu("Show Debug Info")]
        private void ContextShowDebugInfo()
        {
            Debug.Log(GetDebugInfo());
        }
        #endif
    }
}