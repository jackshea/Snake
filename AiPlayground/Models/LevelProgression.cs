namespace AiPlayground.Models
{
    /// <summary>
    /// 关卡进度
    /// </summary>
    public class LevelProgression
    {
        public Dictionary<string, LevelCompletion> CompletedLevels { get; set; }
        public int HighestUnlockedLevel { get; set; }
        public DateTime LastPlayedTime { get; set; }

        public LevelProgression()
        {
            CompletedLevels = new Dictionary<string, LevelCompletion>();
            HighestUnlockedLevel = 1; // 第一关默认解锁
            LastPlayedTime = DateTime.Now;
        }

        /// <summary>
        /// 获取关卡完成记录
        /// </summary>
        public LevelCompletion? GetCompletion(string levelId)
        {
            return CompletedLevels.TryGetValue(levelId, out var completion) ? completion : null;
        }

        /// <summary>
        /// 更新关卡完成记录
        /// </summary>
        public void UpdateCompletion(string levelId, int score, int time)
        {
            if (!CompletedLevels.TryGetValue(levelId, out var completion))
            {
                completion = new LevelCompletion { LevelId = levelId };
                CompletedLevels[levelId] = completion;
            }

            completion.IsCompleted = true;
            completion.CompletionCount++;
            completion.LastCompletedTime = DateTime.Now;

            if (score > completion.BestScore)
            {
                completion.BestScore = score;
            }

            if (completion.BestTime == 0 || time < completion.BestTime)
            {
                completion.BestTime = time;
            }
        }

        /// <summary>
        /// 检查关卡是否已解锁
        /// </summary>
        public bool IsLevelUnlocked(int levelNumber)
        {
            return levelNumber <= HighestUnlockedLevel;
        }
    }

    /// <summary>
    /// 关卡完成记录
    /// </summary>
    public class LevelCompletion
    {
        public string LevelId { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
        public int BestScore { get; set; }
        public int BestTime { get; set; }
        public int CompletionCount { get; set; }
        public DateTime LastCompletedTime { get; set; }
    }
}
