using System.Collections.Generic;
using UnityEngine;
using WordPuzzle.Core.Architecture;

namespace WordPuzzle.Gameplay.Word
{
    /// <summary>
    /// Компонент для отображения одного слова на игровом поле
    /// Состоит из 6 ячеек для букв, расположенных горизонтально
    /// </summary>
    public class WordSlot : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private int _maxLetters = 6;
        
        [Header("Letter Cells")]
        [SerializeField] private LetterCell _letterCellPrefab;
        [SerializeField] private Transform _cellsContainer;
        
        private readonly List<LetterCell> _letterCells = new List<LetterCell>();
        private int _wordIndex = -1; // Индекс слова в игровом поле
        private bool _isInitialized = false;
        
        /// <summary>
        /// Индекс слова в игровом поле (0-3)
        /// </summary>
        public int WordIndex
        {
            get => _wordIndex;
            set
            {
                _wordIndex = value;
                gameObject.name = $"WordSlot_{_wordIndex}";
            }
        }
        
        /// <summary>
        /// Количество ячеек в слове
        /// </summary>
        public int CellCount => _letterCells.Count;
        
        /// <summary>
        /// Все ячейки слова
        /// </summary>
        public IReadOnlyList<LetterCell> LetterCells => _letterCells;
        
        /// <summary>
        /// Инициализация слота для слова
        /// </summary>
        private void Awake()
        {
            InitializeWordSlot();
        }
        
        /// <summary>
        /// Инициализация компонента
        /// </summary>
        private void InitializeWordSlot()
        {
            if (_isInitialized) return;
            
            GameLogger.LogInfo("WordSlot", $"Initializing WordSlot for word {_wordIndex}...");
            
            // Проверяем контейнер для ячеек
            if (_cellsContainer == null)
            {
                _cellsContainer = transform;
                GameLogger.LogWarning("WordSlot", "Cells container not assigned, using self transform");
            }
            
            // Создаем ячейки для букв
            CreateLetterCells();
            
            _isInitialized = true;
            GameLogger.LogInfo("WordSlot", $"WordSlot {_wordIndex} initialized with {_letterCells.Count} cells");
        }
        
        /// <summary>
        /// Создание ячеек для букв
        /// </summary>
        private void CreateLetterCells()
        {
            // Очищаем существующие ячейки
            ClearExistingCells();
            
            for (int i = 0; i < _maxLetters; i++)
            {
                LetterCell cell = CreateSingleLetterCell(i);
                if (cell != null)
                {
                    _letterCells.Add(cell);
                }
            }
            
            GameLogger.LogInfo("WordSlot", $"Created {_letterCells.Count} letter cells");
        }
        
        /// <summary>
        /// Создание одной ячейки для буквы
        /// </summary>
        private LetterCell CreateSingleLetterCell(int cellIndex)
        {
            GameObject cellObject;
            
            if (_letterCellPrefab != null)
            {
                // Создаем из префаба
                cellObject = Instantiate(_letterCellPrefab.gameObject, _cellsContainer);
            }
            else
            {
                // Создаем динамически
                cellObject = CreateDynamicLetterCell();
            }
            
            cellObject.name = $"LetterCell_{_wordIndex}_{cellIndex}";
            
            var letterCell = cellObject.GetComponent<LetterCell>();
            if (letterCell == null)
            {
                letterCell = cellObject.AddComponent<LetterCell>();
            }
            
            // Настраиваем ячейку
            letterCell.SetCellIndices(_wordIndex, cellIndex);
            letterCell.ClearCell();
            
            return letterCell;
        }
        
        /// <summary>
        /// Создание простой ячейки если нет префаба
        /// </summary>
        private GameObject CreateDynamicLetterCell()
        {
            var cellObject = new GameObject("LetterCell");
            cellObject.transform.SetParent(_cellsContainer, false);
            
            // Добавляем RectTransform для UI
            var rectTransform = cellObject.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(60, 60); // Размер ячейки
            
            return cellObject;
        }
        
        /// <summary>
        /// Очистка существующих ячеек
        /// </summary>
        private void ClearExistingCells()
        {
            foreach (var cell in _letterCells)
            {
                if (cell != null && cell.gameObject != null)
                {
                    if (Application.isPlaying)
                    {
                        Destroy(cell.gameObject);
                    }
                    else
                    {
                        DestroyImmediate(cell.gameObject);
                    }
                }
            }
            _letterCells.Clear();
        }
        
        /// <summary>
        /// Получение ячейки по индексу
        /// </summary>
        public LetterCell GetLetterCell(int cellIndex)
        {
            if (cellIndex < 0 || cellIndex >= _letterCells.Count)
            {
                GameLogger.LogWarning("WordSlot", $"Invalid cell index {cellIndex} for word {_wordIndex}");
                return null;
            }
            
            return _letterCells[cellIndex];
        }
        
        /// <summary>
        /// Установка буквы в ячейку
        /// </summary>
        public bool SetLetter(int cellIndex, string letter)
        {
            var cell = GetLetterCell(cellIndex);
            if (cell == null) return false;
            
            cell.SetLetter(letter);
            return true;
        }
        
        /// <summary>
        /// Очистка буквы в ячейке
        /// </summary>
        public bool ClearLetter(int cellIndex)
        {
            var cell = GetLetterCell(cellIndex);
            if (cell == null) return false;
            
            cell.ClearCell();
            return true;
        }
        
        /// <summary>
        /// Очистка всех ячеек слова
        /// </summary>
        public void ClearAllLetters()
        {
            foreach (var cell in _letterCells)
            {
                cell?.ClearCell();
            }
            
            GameLogger.LogInfo("WordSlot", $"Cleared all letters in word slot {_wordIndex}");
        }
        
        /// <summary>
        /// Получение текущего слова как строки
        /// </summary>
        public string GetCurrentWord()
        {
            var letters = new string[_letterCells.Count];
            
            for (int i = 0; i < _letterCells.Count; i++)
            {
                letters[i] = _letterCells[i]?.CurrentLetter ?? "";
            }
            
            return string.Join("", letters);
        }
        
        /// <summary>
        /// Проверка, заполнены ли все ячейки
        /// </summary>
        public bool IsWordComplete()
        {
            foreach (var cell in _letterCells)
            {
                if (cell == null || string.IsNullOrEmpty(cell.CurrentLetter))
                {
                    return false;
                }
            }
            
            return true;
        }
        
        /// <summary>
        /// Проверка, пуста ли вся строка
        /// </summary>
        public bool IsWordEmpty()
        {
            foreach (var cell in _letterCells)
            {
                if (cell != null && !string.IsNullOrEmpty(cell.CurrentLetter))
                {
                    return false;
                }
            }
            
            return true;
        }
        
        /// <summary>
        /// Подсветка слота (например, при валидации)
        /// </summary>
        public void HighlightSlot(bool highlight, Color? highlightColor = null)
        {
            foreach (var cell in _letterCells)
            {
                cell?.HighlightCell(highlight, highlightColor);
            }
            
            GameLogger.LogInfo("WordSlot", $"Word slot {_wordIndex} highlight: {highlight}");
        }
        
        /// <summary>
        /// Валидация настроек в редакторе
        /// </summary>
        private void OnValidate()
        {
            if (_maxLetters <= 0)
            {
                _maxLetters = 6;
            }
        }
        
        /// <summary>
        /// Получение отладочной информации
        /// </summary>
        public string GetDebugInfo()
        {
            var currentWord = GetCurrentWord();
            var completeness = IsWordComplete() ? "Complete" : (IsWordEmpty() ? "Empty" : "Partial");
            
            return $"WordSlot {_wordIndex}: '{currentWord}' ({completeness}, {_letterCells.Count} cells)";
        }
        
        /// <summary>
        /// Очистка при уничтожении
        /// </summary>
        private void OnDestroy()
        {
            ClearExistingCells();
        }
        
        #if UNITY_EDITOR
        [ContextMenu("Clear All Letters")]
        private void ContextClearAllLetters()
        {
            ClearAllLetters();
        }
        
        [ContextMenu("Show Debug Info")]
        private void ContextShowDebugInfo()
        {
            Debug.Log(GetDebugInfo());
        }
        
        [ContextMenu("Recreate Cells")]
        private void ContextRecreateCells()
        {
            _isInitialized = false;
            InitializeWordSlot();
        }
        #endif
    }
}