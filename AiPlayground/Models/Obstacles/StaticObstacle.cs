using AiPlayground.Game;

namespace AiPlayground.Models.Obstacles
{
    /// <summary>
    /// 静态障碍物 - 蛇碰到会死亡
    /// </summary>
    public class StaticObstacle : Obstacle
    {
        public StaticObstacle(Point position) : base(position, ObstacleType.Static)
        {
        }

        public override ObstacleInteractionResult Interact(GameState state)
        {
            return ObstacleInteractionResult.Deadly();
        }

        public override string ToString()
        {
            return $"StaticObstacle at ({Position.X}, {Position.Y})";
        }
    }
}
