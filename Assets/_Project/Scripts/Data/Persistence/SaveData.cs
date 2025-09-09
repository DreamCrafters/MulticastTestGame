
using System;
using UnityEngine;
using Newtonsoft.Json;
using WordPuzzle.Data.Models;
using WordPuzzle.Core.Architecture;

namespace WordPuzzle.Data.Persistence
{
    /// <summary>
    /// Утилитарный класс для работы с сохранением и загрузкой данных
    /// Обеспечивает сериализацию/десериализацию данных прогресса
    /// </summary>
    public static class SaveData
    {
        /// <summary>
        /// Текущая версия формата сохранения
        /// </summary>
        public const int CurrentSaveVersion = 1;
        
        /// <summary>
        /// Сохранение прогресса игрока
        /// </summary>
        /// <param name="progress">Данные прогресса для сохранения</param>
        /// <returns>true если сохранение успешно</returns>
        public static bool SavePlayerProgress(PlayerProgress progress)
        {
            if (progress == null)
            {
                GameLogger.LogError("SaveData", "Cannot save null progress data");
                return false;
            }
            
            if (!progress.IsValid())
            {
                GameLogger.LogError("SaveData", "Cannot save invalid progress data");
                return false;
            }
            
            try
            {
                // Обновляем время сохранения
                progress.LastSaveTime = DateTime.Now;
                progress.SaveVersion = CurrentSaveVersion;
                
                // Сериализуем в JSON
                string jsonData = JsonConvert.SerializeObject(progress, Formatting.None);
                
                // Сохраняем основные данные
                PlayerPrefs.SetString(SaveKeys.Progress.PlayerProgressData, jsonData);
                
                // Сохраняем резервные данные для быстрого доступа
                PlayerPrefs.SetInt(SaveKeys.Progress.CompletedLevelsCount, progress.CompletedLevelsCount);
                PlayerPrefs.SetInt(SaveKeys.Progress.CurrentLevel, progress.GetNextLevelNumber());
                PlayerPrefs.SetString(SaveKeys.Progress.LastSaveTime, progress.LastSaveTime.ToBinary().ToString());
                PlayerPrefs.SetInt(SaveKeys.Settings.SaveVersion, CurrentSaveVersion);
                
                // Принудительное сохранение
                PlayerPrefs.Save();
                
                GameLogger.LogInfo("SaveData", $"Player progress saved successfully. Completed levels: {progress.CompletedLevelsCount}");
                return true;
            }
            catch (Exception ex)
            {
                GameLogger.LogException("SaveData", ex);
                return false;
            }
        }
        
        /// <summary>
        /// Загрузка прогресса игрока
        /// </summary>
        /// <returns>Данные прогресса или новый объект если данных нет</returns>
        public static PlayerProgress LoadPlayerProgress()
        {
            try
            {
                // Проверяем существование сохранения
                if (!PlayerPrefs.HasKey(SaveKeys.Progress.PlayerProgressData))
                {
                    GameLogger.LogInfo("SaveData", "No save data found, creating new progress");
                    return CreateNewProgress();
                }
                
                // Загружаем JSON данные
                string jsonData = PlayerPrefs.GetString(SaveKeys.Progress.PlayerProgressData);
                
                if (string.IsNullOrEmpty(jsonData))
                {
                    GameLogger.LogWarning("SaveData", "Save data is empty, creating new progress");
                    return CreateNewProgress();
                }
                
                // Десериализуем
                var progress = JsonConvert.DeserializeObject<PlayerProgress>(jsonData);
                
                if (progress == null)
                {
                    GameLogger.LogWarning("SaveData", "Failed to deserialize progress, creating new");
                    return CreateNewProgress();
                }
                
                // Проверяем версию сохранения
                if (progress.SaveVersion != CurrentSaveVersion)
                {
                    GameLogger.LogInfo("SaveData", $"Save version mismatch: {progress.SaveVersion} vs {CurrentSaveVersion}, migrating...");
                    progress = MigrateSaveData(progress);
                }
                
                // Валидация загруженных данных
                if (!progress.IsValid())
                {
                    GameLogger.LogWarning("SaveData", "Loaded progress is invalid, creating new");
                    return CreateNewProgress();
                }
                
                GameLogger.LogInfo("SaveData", $"Player progress loaded successfully. Completed levels: {progress.CompletedLevelsCount}");
                return progress;
            }
            catch (Exception ex)
            {
                GameLogger.LogException("SaveData", ex);
                GameLogger.LogWarning("SaveData", "Failed to load progress, creating new");
                return CreateNewProgress();
            }
        }
        
        /// <summary>
        /// Создание нового прогресса
        /// </summary>
        /// <returns>Новый объект прогресса</returns>
        private static PlayerProgress CreateNewProgress()
        {
            var newProgress = new PlayerProgress();
            
            // Отмечаем первый запуск для аналитики
            if (!PlayerPrefs.HasKey(SaveKeys.Settings.FirstLaunch))
            {
                PlayerPrefs.SetString(SaveKeys.Settings.FirstLaunch, DateTime.Now.ToBinary().ToString());
                GameLogger.LogInfo("SaveData", "First launch detected");
            }
            
            // Счетчик запусков
            int launchCount = PlayerPrefs.GetInt(SaveKeys.Debug.LaunchCount, 0) + 1;
            PlayerPrefs.SetInt(SaveKeys.Debug.LaunchCount, launchCount);
            
            return newProgress;
        }
        
        /// <summary>
        /// Миграция данных сохранения между версиями
        /// </summary>
        /// <param name="oldProgress">Старые данные прогресса</param>
        /// <returns>Мигрированные данные</returns>
        private static PlayerProgress MigrateSaveData(PlayerProgress oldProgress)
        {
            GameLogger.LogInfo("SaveData", $"Migrating save data from version {oldProgress.SaveVersion} to {CurrentSaveVersion}");
            
            // В будущем здесь будет логика миграции между версиями
            // Пока просто обновляем версию
            oldProgress.SaveVersion = CurrentSaveVersion;
            
            return oldProgress;
        }
        
        /// <summary>
        /// Полное удаление всех сохранений
        /// </summary>
        public static void DeleteAllSaveData()
        {
            GameLogger.LogInfo("SaveData", "Deleting all save data...");
            
            try
            {
                // Удаляем основные ключи
                PlayerPrefs.DeleteKey(SaveKeys.Progress.PlayerProgressData);
                PlayerPrefs.DeleteKey(SaveKeys.Progress.CompletedLevelsCount);
                PlayerPrefs.DeleteKey(SaveKeys.Progress.CurrentLevel);
                PlayerPrefs.DeleteKey(SaveKeys.Progress.LastSaveTime);
                PlayerPrefs.DeleteKey(SaveKeys.Settings.SaveVersion);
                
                // Удаляем ключи уровней (максимум 999 уровней)
                for (int i = 1; i <= 999; i++)
                {
                    PlayerPrefs.DeleteKey(SaveKeys.GetLevelKey(i));
                    PlayerPrefs.DeleteKey(SaveKeys.GetLevelTimeKey(i));
                }
                
                PlayerPrefs.Save();
                
                GameLogger.LogInfo("SaveData", "All save data deleted successfully");
            }
            catch (Exception ex)
            {
                GameLogger.LogException("SaveData", ex);
            }
        }
        
        /// <summary>
        /// Получение информации о сохранении
        /// </summary>
        /// <returns>Строка с информацией о сохранении</returns>
        public static string GetSaveInfo()
        {
            if (!PlayerPrefs.HasKey(SaveKeys.Progress.PlayerProgressData))
            {
                return "No save data found";
            }
            
            try
            {
                int completedLevels = PlayerPrefs.GetInt(SaveKeys.Progress.CompletedLevelsCount, 0);
                int currentLevel = PlayerPrefs.GetInt(SaveKeys.Progress.CurrentLevel, 1);
                int saveVersion = PlayerPrefs.GetInt(SaveKeys.Settings.SaveVersion, 0);
                
                string lastSaveTimeStr = PlayerPrefs.GetString(SaveKeys.Progress.LastSaveTime, "");
                string lastSaveInfo = "Unknown";
                
                if (!string.IsNullOrEmpty(lastSaveTimeStr) && long.TryParse(lastSaveTimeStr, out long timeBinary))
                {
                    var lastSaveTime = DateTime.FromBinary(timeBinary);
                    lastSaveInfo = lastSaveTime.ToString("yyyy-MM-dd HH:mm:ss");
                }
                
                return $"Save Version: {saveVersion}\nCompleted Levels: {completedLevels}\nCurrent Level: {currentLevel}\nLast Save: {lastSaveInfo}";
            }
            catch (Exception ex)
            {
                GameLogger.LogException("SaveData", ex);
                return "Error reading save info";
            }
        }
    }
}
