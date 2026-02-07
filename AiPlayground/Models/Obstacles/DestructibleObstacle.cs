using AiPlayground.Game;

namespace AiPlayground.Models.Obstacles
{
    /// <summary>
    /// 可破坏障碍物 - 蛇可以穿过指定次数后消失
    /// </summary>
    public class DestructibleObstacle : Obstacle
    {
        private int _remainingPasses;

        public int MaxPasses { get; set; }
        public int RemainingPasses
        {
            get => _remainingPasses;
            set => _remainingPasses = Math.Max(0, value);
        }

        public DestructibleObstacle(Point position, int maxPasses = 1) : base(position, ObstacleType.Destructible)
        {
            MaxPasses = maxPasses;
            RemainingPasses = maxPasses;
        }

        public override ObstacleInteractionResult Interact(GameState state)
        {
            RemainingPasses--;

            if (RemainingPasses <= 0)
            {
                // 最后一次通过，障碍物消失
                return ObstacleInteractionResult.RemoveAfterPass();
            }

            // 还可以穿过 RemainingPasses 次
            return ObstacleInteractionResult.PassThrough();
        }

        public override string ToString()
        {
            return $"DestructibleObstacle at ({Position.X}, {Position.Y}), remaining: {RemainingPasses}/{MaxPasses}";
        }
    }
}
