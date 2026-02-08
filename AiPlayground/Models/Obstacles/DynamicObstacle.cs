using AiPlayground.Game;
using AiPlayground.Services.Abstractions;

namespace AiPlayground.Models.Obstacles
{
    /// <summary>
    /// 动态障碍物 - 按路径移动
    /// </summary>
    public class DynamicObstacle : Obstacle
    {
        private readonly ITimeProvider _timeProvider;
        private List<Point> _path;
        private int _currentPathIndex;
        private long _lastMoveTime;

        public List<Point> Path
        {
            get => _path;
            set => _path = value ?? new List<Point>();
        }

        public int MoveIntervalMs { get; set; } = 500; // 移动间隔（毫秒）
        public bool LoopPath { get; set; } = true;

        public DynamicObstacle(Point position, List<Point>? path = null, ITimeProvider? timeProvider = null)
            : base(position, ObstacleType.Dynamic)
        {
            _timeProvider = timeProvider ?? new Services.DefaultTimeProvider();
            Position = position;
            _path = path ?? new List<Point> { position };
            _currentPathIndex = 0;
            _lastMoveTime = _timeProvider.GetCurrentUnixTimeMilliseconds();
        }

        public override ObstacleInteractionResult Interact(GameState state)
        {
            return ObstacleInteractionResult.Deadly();
        }

        public override void Update(int gridWidth, int gridHeight)
        {
            long currentTime = _timeProvider.GetCurrentUnixTimeMilliseconds();

            if (currentTime - _lastMoveTime < MoveIntervalMs)
            {
                return;
            }

            _lastMoveTime = currentTime;

            if (_path.Count == 0) return;

            // 移动到路径中的下一个点
            _currentPathIndex++;

            if (_currentPathIndex >= _path.Count)
            {
                if (LoopPath)
                {
                    _currentPathIndex = 0;
                }
                else
                {
                    _currentPathIndex = _path.Count - 1;
                    return;
                }
            }

            Point nextPosition = _path[_currentPathIndex];

            // 确保新位置在网格内
            if (nextPosition.X >= 0 && nextPosition.X < gridWidth &&
                nextPosition.Y >= 0 && nextPosition.Y < gridHeight)
            {
                Position = nextPosition;
            }
        }

        public override string ToString()
        {
            return $"DynamicObstacle at ({Position.X}, {Position.Y}), path index: {_currentPathIndex}/{_path.Count}";
        }
    }
}
