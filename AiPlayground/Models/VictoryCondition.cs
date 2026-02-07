using AiPlayground.Game;

namespace AiPlayground.Models
{
    /// <summary>
    /// 通关条件类型
    /// </summary>
    public enum VictoryConditionType
    {
        TargetScore,      // 达到目标分数
        TargetLength,     // 达到目标长度
        CollectAllFood,   // 收集所有食物
        Combined          // 组合条件
    }

    /// <summary>
    /// 关卡通通条件
    /// </summary>
    public class VictoryCondition
    {
        public VictoryConditionType Type { get; set; }
        public int TargetScore { get; set; }
        public int TargetLength { get; set; }
        public bool MustCollectAllFood { get; set; }
        public int? FoodSpawnCount { get; set; }

        public VictoryCondition()
        {
            Type = VictoryConditionType.TargetScore;
            TargetScore = 100;
            TargetLength = 10;
            MustCollectAllFood = false;
        }

        /// <summary>
        /// 检查是否满足通关条件
        /// </summary>
        public bool CheckCondition(GameState state, int foodCollected, int totalFoodSpawned)
        {
            return Type switch
            {
                VictoryConditionType.TargetScore => state.Score >= TargetScore,
                VictoryConditionType.TargetLength => state.Snake.Count >= TargetLength,
                VictoryConditionType.CollectAllFood => MustCollectAllFood && foodCollected >= totalFoodSpawned && totalFoodSpawned > 0,
                VictoryConditionType.Combined => CheckCombinedCondition(state, foodCollected, totalFoodSpawned),
                _ => false
            };
        }

        private bool CheckCombinedCondition(GameState state, int foodCollected, int totalFoodSpawned)
        {
            bool scoreMet = TargetScore <= 0 || state.Score >= TargetScore;
            bool lengthMet = TargetLength <= 0 || state.Snake.Count >= TargetLength;
            bool foodMet = !MustCollectAllFood || (foodCollected >= totalFoodSpawned && totalFoodSpawned > 0);
            return scoreMet && lengthMet && foodMet;
        }

        public string GetDescription()
        {
            return Type switch
            {
                VictoryConditionType.TargetScore => $"目标分数: {TargetScore}",
                VictoryConditionType.TargetLength => $"目标长度: {TargetLength}",
                VictoryConditionType.CollectAllFood => "收集所有食物",
                VictoryConditionType.Combined => GetCombinedDescription(),
                _ => "未知条件"
            };
        }

        private string GetCombinedDescription()
        {
            var conditions = new List<string>();
            if (TargetScore > 0) conditions.Add($"分数 {TargetScore}");
            if (TargetLength > 0) conditions.Add($"长度 {TargetLength}");
            if (MustCollectAllFood) conditions.Add("收集所有食物");
            return conditions.Count > 0 ? string.Join(", ", conditions) : "完成目标";
        }
    }
}
