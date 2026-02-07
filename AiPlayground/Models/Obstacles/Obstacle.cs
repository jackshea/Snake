using AiPlayground.Game;

namespace AiPlayground.Models.Obstacles
{
    /// <summary>
    /// 障碍物基类
    /// </summary>
    public abstract class Obstacle
    {
        public Point Position { get; set; }
        public ObstacleType Type { get; set; }

        protected Obstacle(Point position, ObstacleType type)
        {
            Position = position;
            Type = type;
        }

        /// <summary>
        /// 与蛇交互时的行为
        /// </summary>
        /// <returns>交互结果</returns>
        public abstract ObstacleInteractionResult Interact(GameState state);

        /// <summary>
        /// 更新动态障碍物状态
        /// </summary>
        public virtual void Update(int gridWidth, int gridHeight) { }
    }

    /// <summary>
    /// 障碍物类型
    /// </summary>
    public enum ObstacleType
    {
        Static,              // 静态障碍物（致命）
        Destructible,        // 可破坏障碍物（可穿过指定次数）
        Dynamic,             // 动态障碍物（移动）
        SpeedUp,            // 加速
        SpeedDown,          // 减速
        ScoreMultiplier,    // 分数倍增
        Teleport            // 传送
    }

    /// <summary>
    /// 障碍物交互结果
    /// </summary>
    public class ObstacleInteractionResult
    {
        public bool IsDeadly { get; set; }          // 是否致命
        public bool ShouldRemove { get; set; }       // 是否应该移除
        public bool CanPassThrough { get; set; }     // 是否可以穿过
        public int SpeedChange { get; set; }         // 速度变化（0表示不变）
        public int ScoreMultiplier { get; set; }     // 分数倍数（1表示不变）
        public Point? TeleportTarget { get; set; }   // 传送目标位置
        public string Message { get; set; } = string.Empty; // 显示消息

        public static ObstacleInteractionResult Deadly() =>
            new() { IsDeadly = true };

        public static ObstacleInteractionResult PassThrough(int maxPasses = 1) =>
            new() { CanPassThrough = true };

        public static ObstacleInteractionResult RemoveAfterPass() =>
            new() { CanPassThrough = true, ShouldRemove = true };

        public static ObstacleInteractionResult SpeedBoost(int amount) =>
            new() { CanPassThrough = true, SpeedChange = amount, Message = amount > 0 ? "加速!" : "减速!" };

        public static ObstacleInteractionResult MultiplyScore(int multiplier) =>
            new() { CanPassThrough = true, ScoreMultiplier = multiplier, Message = $"{multiplier}x 分数!" };

        public static ObstacleInteractionResult Teleport(Point target) =>
            new() { CanPassThrough = true, TeleportTarget = target, Message = "传送!" };
    }
}
