using System;
using System.Linq;
using Newtonsoft.Json;

namespace WordPuzzle.Data.Models
{
    /// <summary>
    /// Модель данных для слова и его кластеров
    /// </summary>
    [Serializable]
    public class WordData
    {
        [JsonProperty("word")]
        public string Word { get; set; } = string.Empty;
        
        [JsonProperty("clusters")]
        public string[] Clusters { get; set; } = new string[0];
        
        /// <summary>
        /// Длина слова в символах
        /// </summary>
        [JsonIgnore]
        public int WordLength => Word?.Length ?? 0;
        
        /// <summary>
        /// Количество кластеров для составления слова
        /// </summary>
        [JsonIgnore]
        public int ClustersCount => Clusters?.Length ?? 0;
        
        /// <summary>
        /// Валидация данных слова
        /// </summary>
        public bool IsValid()
        {
            if (string.IsNullOrEmpty(Word)) return false;
            if (Clusters == null || Clusters.Length == 0) return false;
            
            // Проверяем что кластеры действительно составляют слово
            string reconstructedWord = string.Join("", Clusters);
            if (reconstructedWord != Word)
            {
                return false;
            }
            
            // Проверяем что каждый кластер не пустой
            return Clusters.All(cluster => !string.IsNullOrEmpty(cluster));
        }
        
        /// <summary>
        /// Проверка принадлежности кластера к данному слову
        /// </summary>
        public bool ContainsCluster(string cluster)
        {
            return Clusters != null && Clusters.Contains(cluster);
        }
        
        /// <summary>
        /// Получение индекса кластера в слове
        /// </summary>
        public int GetClusterIndex(string cluster)
        {
            if (Clusters == null) return -1;
            
            for (int i = 0; i < Clusters.Length; i++)
            {
                if (Clusters[i] == cluster) return i;
            }
            
            return -1;
        }
    }
}