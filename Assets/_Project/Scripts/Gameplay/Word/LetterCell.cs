using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using WordPuzzle.Core.Architecture;

namespace WordPuzzle.Gameplay.Word
{
    /// <summary>
    /// Компонент для отображения одной ячейки буквы
    /// Поддерживает визуализацию состояний: пустая, заполненная, подсвеченная
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class LetterCell : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private TextMeshProUGUI _letterText;
        [SerializeField] private Image _borderImage;
        
        [Header("Visual Settings")]
        [SerializeField] private Color _emptyBackgroundColor = new Color(1f, 1f, 1f, 0.1f);
        [SerializeField] private Color _filledBackgroundColor = Color.white;
        [SerializeField] private Color _emptyBorderColor = Color.gray;
        [SerializeField] private Color _filledBorderColor = Color.black;
        [SerializeField] private Color _highlightColor = Color.yellow;
        [SerializeField] private Color _letterColor = Color.black;
        
        [Header("Animation Settings")]
        [SerializeField] private bool _enableAnimations = true;
        [SerializeField] private float _letterAppearDuration = 0.3f;
        [SerializeField] private float _letterDisappearDuration = 0.2f;
        [SerializeField] private Ease _letterAppearEase = Ease.OutBack;
        [SerializeField] private Ease _letterDisappearEase = Ease.InQuad;
        
        [Header("Cell Settings")]
        [SerializeField] private float _borderWidth = 2f;
        
        private RectTransform _rectTransform;
        private string _currentLetter = "";
        private bool _isHighlighted = false;
        private bool _isInitialized = false;
        
        // Позиция ячейки в игровом поле
        private int _wordIndex = -1;
        private int _cellIndex = -1;
        
        /// <summary>
        /// Текущая буква в ячейке
        /// </summary>
        public string CurrentLetter => _currentLetter;
        
        /// <summary>
        /// Пуста ли ячейка
        /// </summary>
        public bool IsEmpty => string.IsNullOrEmpty(_currentLetter);
        
        /// <summary>
        /// Подсвечена ли ячейка
        /// </summary>
        public bool IsHighlighted => _isHighlighted;
        
        /// <summary>
        /// Индекс слова в игровом поле
        /// </summary>
        public int WordIndex => _wordIndex;
        
        /// <summary>
        /// Индекс ячейки в слове
        /// </summary>
        public int CellIndex => _cellIndex;
        
        /// <summary>
        /// Инициализация ячейки
        /// </summary>
        private void Awake()
        {
            InitializeCell();
        }
        
        /// <summary>
        /// Инициализация компонентов ячейки
        /// </summary>
        private void InitializeCell()
        {
            if (_isInitialized) return;
            
            _rectTransform = GetComponent<RectTransform>();
            
            // Создаем компоненты если не назначены
            CreateComponents();
            
            // Устанавливаем начальное состояние
            SetEmptyState();
            
            _isInitialized = true;
            GameLogger.LogInfo("LetterCell", $"LetterCell initialized at position ({_wordIndex}, {_cellIndex})");
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
                
                GameLogger.LogInfo("LetterCell", "Background image created");
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
                
                GameLogger.LogInfo("LetterCell", "Border image created");
            }
            
            // Letter Text
            if (_letterText == null)
            {
                var textObject = new GameObject("LetterText");
                textObject.transform.SetParent(transform, false);
                
                _letterText = textObject.AddComponent<TextMeshProUGUI>();
                _letterText.color = _letterColor;
                _letterText.alignment = TextAlignmentOptions.Center;
                _letterText.fontStyle = FontStyles.Bold;
                
                var textRect = textObject.GetComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = Vector2.zero;
                textRect.offsetMax = Vector2.zero;
                
                GameLogger.LogInfo("LetterCell", "Letter text created");
            }
        }
        
        /// <summary>
        /// Создание простого спрайта для рамки
        /// </summary>
        private Sprite CreateBorderSprite()
        {
            // Создаем простую белую текстуру для рамки
            var texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            
            return Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
        }
        
        /// <summary>
        /// Установка индексов ячейки в игровом поле
        /// </summary>
        public void SetCellIndices(int wordIndex, int cellIndex)
        {
            _wordIndex = wordIndex;
            _cellIndex = cellIndex;
            gameObject.name = $"LetterCell_{_wordIndex}_{_cellIndex}";
            
            GameLogger.LogInfo("LetterCell", $"Cell indices set: word={_wordIndex}, cell={_cellIndex}");
        }
        
        /// <summary>
        /// Установка буквы в ячейку
        /// </summary>
        public void SetLetter(string letter)
        {
            if (string.IsNullOrEmpty(letter))
            {
                ClearCell();
                return;
            }
            
            string previousLetter = _currentLetter;
            _currentLetter = letter.ToUpper();
            
            // Обновляем визуал
            UpdateLetterText();
            SetFilledState();
            
            // Анимация появления буквы
            if (_enableAnimations && string.IsNullOrEmpty(previousLetter))
            {
                AnimateLetterAppear();
            }
            
            GameLogger.LogInfo("LetterCell", $"Letter set to '{_currentLetter}' at ({_wordIndex}, {_cellIndex})");
        }
        
        /// <summary>
        /// Очистка ячейки
        /// </summary>
        public void ClearCell()
        {
            bool wasEmpty = IsEmpty;
            
            _currentLetter = "";
            
            // Обновляем визуал
            UpdateLetterText();
            SetEmptyState();
            
            // Анимация исчезновения буквы
            if (_enableAnimations && !wasEmpty)
            {
                AnimateLetterDisappear();
            }
            
            GameLogger.LogInfo("LetterCell", $"Cell cleared at ({_wordIndex}, {_cellIndex})");
        }
        
        /// <summary>
        /// Обновление текста буквы
        /// </summary>
        private void UpdateLetterText()
        {
            if (_letterText != null)
            {
                _letterText.text = _currentLetter;
            }
        }
        
        /// <summary>
        /// Установка состояния пустой ячейки
        /// </summary>
        private void SetEmptyState()
        {
            if (_backgroundImage != null)
            {
                _backgroundImage.color = _emptyBackgroundColor;
            }
            
            if (_borderImage != null)
            {
                _borderImage.color = _emptyBorderColor;
            }
        }
        
        /// <summary>
        /// Установка состояния заполненной ячейки
        /// </summary>
        private void SetFilledState()
        {
            if (_backgroundImage != null)
            {
                _backgroundImage.color = _filledBackgroundColor;
            }
            
            if (_borderImage != null)
            {
                _borderImage.color = _filledBorderColor;
            }
        }
        
        /// <summary>
        /// Подсветка ячейки
        /// </summary>
        public void HighlightCell(bool highlight, Color? highlightColor = null)
        {
            _isHighlighted = highlight;
            
            Color targetColor;
            if (highlight)
            {
                targetColor = highlightColor ?? _highlightColor;
            }
            else
            {
                targetColor = IsEmpty ? _emptyBorderColor : _filledBorderColor;
            }
            
            if (_borderImage != null)
            {
                if (_enableAnimations)
                {
                    _borderImage.DOColor(targetColor, 0.3f);
                }
                else
                {
                    _borderImage.color = targetColor;
                }
            }
            
            GameLogger.LogInfo("LetterCell", $"Cell ({_wordIndex}, {_cellIndex}) highlight: {highlight}");
        }
        
        /// <summary>
        /// Анимация появления буквы
        /// </summary>
        private void AnimateLetterAppear()
        {
            if (_letterText == null) return;
            
            _letterText.transform.localScale = Vector3.zero;
            
            _letterText.transform.DOScale(Vector3.one, _letterAppearDuration)
                .SetEase(_letterAppearEase);
        }
        
        /// <summary>
        /// Анимация исчезновения буквы
        /// </summary>
        private void AnimateLetterDisappear()
        {
            if (_letterText == null) return;
            
            _letterText.transform.DOScale(Vector3.zero, _letterDisappearDuration)
                .SetEase(_letterDisappearEase)
                .OnComplete(() => {
                    _letterText.transform.localScale = Vector3.one;
                });
        }
        
        /// <summary>
        /// Установка цвета буквы
        /// </summary>
        public void SetLetterColor(Color color)
        {
            _letterColor = color;
            if (_letterText != null)
            {
                _letterText.color = _letterColor;
            }
        }
        
        /// <summary>
        /// Получение отладочной информации
        /// </summary>
        public string GetDebugInfo()
        {
            var state = IsEmpty ? "Empty" : $"'{_currentLetter}'";
            var highlight = _isHighlighted ? " (Highlighted)" : "";
            
            return $"Cell ({_wordIndex}, {_cellIndex}): {state}{highlight}";
        }
        
        /// <summary>
        /// Валидация настроек в редакторе
        /// </summary>
        private void OnValidate()
        {
            if (_borderWidth <= 0) _borderWidth = 2;
        }
        
        /// <summary>
        /// Очистка анимаций при уничтожении
        /// </summary>
        private void OnDestroy()
        {
            DOTween.Kill(this);
            DOTween.Kill(_letterText);
            DOTween.Kill(_borderImage);
        }
        
        #if UNITY_EDITOR
        [ContextMenu("Set Test Letter")]
        private void ContextSetTestLetter()
        {
            SetLetter("A");
        }
        
        [ContextMenu("Clear Cell")]
        private void ContextClearCell()
        {
            ClearCell();
        }
        
        [ContextMenu("Toggle Highlight")]
        private void ContextToggleHighlight()
        {
            HighlightCell(!_isHighlighted);
        }
        
        [ContextMenu("Show Debug Info")]
        private void ContextShowDebugInfo()
        {
            Debug.Log(GetDebugInfo());
        }
        #endif
    }
}