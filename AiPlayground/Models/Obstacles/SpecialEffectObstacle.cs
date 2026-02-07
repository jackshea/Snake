using AiPlayground.Game;

namespace AiPlayground.Models.Obstacles
{
    /// <summary>
    /// 特殊效果障碍物 - 提供各种增益或特殊效果
    /// </summary>
    public class SpecialEffectObstacle : Obstacle
    {
        public int SpeedChangeAmount { get; set; }
        public int ScoreMultiplierValue { get; set; }
        public Point? TeleportDestination { get; set; }
        public bool IsPermanent { get; set; } = false; // 是否永久存在（一次性道具会消失）

        public SpecialEffectObstacle(Point position, ObstacleType effectType) : base(position, effectType)
        {
            SpeedChangeAmount = 0;
            ScoreMultiplierValue = 1;
            TeleportDestination = null;
        }

        public override ObstacleInteractionResult Interact(GameState state)
        {
            return Type switch
            {
                ObstacleType.SpeedUp => ObstacleInteractionResult.SpeedBoost(SpeedChangeAmount > 0 ? SpeedChangeAmount : 2),
                ObstacleType.SpeedDown => ObstacleInteractionResult.SpeedBoost(SpeedChangeAmount < 0 ? SpeedChangeAmount : -2),
                ObstacleType.ScoreMultiplier => ObstacleInteractionResult.MultiplyScore(ScoreMultiplierValue),
                ObstacleType.Teleport when TeleportDestination.HasValue => ObstacleInteractionResult.Teleport(TeleportDestination.Value),
                _ => ObstacleInteractionResult.PassThrough()
            };
        }

        /// <summary>
        /// 创建加速道具
        /// </summary>
        public static SpecialEffectObstacle CreateSpeedUp(Point position, int amount = 2)
        {
            return new SpecialEffectObstacle(position, ObstacleType.SpeedUp)
            {
                SpeedChangeAmount = amount
            };
        }

        /// <summary>
        /// 创建减速道具
        /// </summary>
        public static SpecialEffectObstacle CreateSpeedDown(Point position, int amount = -2)
        {
            return new SpecialEffectObstacle(position, ObstacleType.SpeedDown)
            {
                SpeedChangeAmount = amount
            };
        }

        /// <summary>
        /// 创建分数倍增道具
        /// </summary>
        public static SpecialEffectObstacle CreateScoreMultiplier(Point position, int multiplier = 2)
        {
            return new SpecialEffectObstacle(position, ObstacleType.ScoreMultiplier)
            {
                ScoreMultiplierValue = multiplier
            };
        }

        /// <summary>
        /// 创建传送门
        /// </summary>
        public static SpecialEffectObstacle CreateTeleport(Point position, Point destination)
        {
            return new SpecialEffectObstacle(position, ObstacleType.Teleport)
            {
                TeleportDestination = destination
            };
        }

        public override string ToString()
        {
            return $"SpecialEffectObstacle ({Type}) at ({Position.X}, {Position.Y})";
        }
    }
}
