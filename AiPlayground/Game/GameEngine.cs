using System;
using System.Drawing;
using AiPlayground.Models;
using AiPlayground.Models.Obstacles;
using AiPlayground.Services.Abstractions;
using AiPlayground.Services;

namespace AiPlayground.Game;

/// <summary>
/// 游戏引擎 - 负责游戏核心逻辑
/// </summary>
public class GameEngine
{
    private readonly IRandomProvider _randomProvider;
    private readonly GameState _state;
    private List<Obstacle> _obstacles = new();
    private long _lastObstacleUpdateTime;

    public GameEngine(GameState state, IRandomProvider? randomProvider = null)
    {
        _state = state;
        _randomProvider = randomProvider ?? new Services.DefaultRandomProvider();
    }

    /// <summary>
    /// 设置当前关卡
    /// </summary>
    public void SetLevel(Level level)
    {
        _state.CurrentLevel = level;
        _obstacles = new List<Obstacle>(level.Obstacles);
        _state.FoodCollected = 0;
        _state.TotalFoodSpawned = 0;
        _state.IsLevelCompleted = false;
        _state.LevelTime = 0;
    }

    /// <summary>
    /// 初始化游戏
    /// </summary>
    public void Initialize()
    {
        // 如果有关卡设置，使用关卡的配置
        int startX, startY, initialLength;
        Point initialDirection;

        if (_state.CurrentLevel != null)
        {
            startX = _state.CurrentLevel.Settings.SnakeStartPosition.X;
            startY = _state.CurrentLevel.Settings.SnakeStartPosition.Y;
            initialLength = _state.CurrentLevel.Settings.InitialSnakeLength;
            initialDirection = _state.CurrentLevel.Settings.InitialDirection;
            _state.SpeedLevel = _state.CurrentLevel.Settings.InitialSpeedLevel;
            _state.Difficulty = _state.CurrentLevel.Settings.DefaultDifficulty;

            // 重新复制障碍物列表（重置状态）
            _obstacles = new List<Obstacle>(_state.CurrentLevel.Obstacles);
            _state.FoodCollected = 0;
            _state.TotalFoodSpawned = 0;
            _state.IsLevelCompleted = false;
            _state.LevelTime = 0;
            _lastObstacleUpdateTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
        else
        {
            startX = GameConfig.GridSize / 2;
            startY = GameConfig.GridSize / 2;
            initialLength = 3;
            initialDirection = GameConfig.DirectionRight;
        }

        _state.Snake.Clear();
        // 蛇头在 First，蛇身向相反方向延伸
        // 例如：如果方向向右(1,0)，蛇头在(startX, startY)，身体向左延伸
        for (int i = 0; i < initialLength; i++)
        {
            _state.Snake.AddLast(new Point(startX - i * initialDirection.X, startY - i * initialDirection.Y));
        }

        _state.Foods.Clear();
        SpawnFood();

        _state.Direction = initialDirection;
        _state.Score = 0;
        _state.IsGameOver = false;
        _state.IsPaused = false;
        _state.IsWaitingToStart = true;
        _state.IsNewHighScore = false;
    }

    /// <summary>
    /// 移动蛇
    /// </summary>
    public void MoveSnake()
    {
        Point head = _state.Snake.First!.Value;
        Point newHead = new Point(head.X + _state.Direction.X, head.Y + _state.Direction.Y);

        // 先检查下一步是否会碰撞（在移动之前检查）
        if (WillCollide(newHead))
        {
            // 会碰撞，不移动蛇，直接游戏结束
            _state.IsGameOver = true;
            return;
        }

        // 不会碰撞，才移动蛇
        _state.Snake.AddFirst(newHead);

        bool ateFood = false;
        for (int i = _state.Foods.Count - 1; i >= 0; i--)
        {
            if (newHead == _state.Foods[i])
            {
                int points = GetBasePoints() + _state.SpeedLevel;
                _state.Score += points;
                _state.Foods.RemoveAt(i);
                _state.FoodCollected++;
                SpawnFood();
                ateFood = true;
                break;
            }
        }

        // 如果没吃到食物，移除尾部节点
        if (!ateFood)
        {
            _state.Snake.RemoveLast();
        }
    }

    /// <summary>
    /// 检查碰撞
    /// </summary>
    public void CheckCollisions()
    {
        Point head = _state.Snake.First!.Value;

        // 获取当前网格大小
        int gridWidth = _state.CurrentLevel?.GridWidth ?? GameConfig.GridSize;
        int gridHeight = _state.CurrentLevel?.GridHeight ?? GameConfig.GridSize;

        // 检查墙壁碰撞（理论上不应该走到这里，因为MoveSnake已经检查过了）
        if (head.X < 0 || head.X >= gridWidth || head.Y < 0 || head.Y >= gridHeight)
        {
            _state.IsGameOver = true;
            return;
        }

        // 检查自身碰撞（从第二个节点开始遍历）
        var current = _state.Snake.First!.Next;
        while (current != null)
        {
            if (head == current.Value)
            {
                _state.IsGameOver = true;
                return;
            }
            current = current.Next;
        }

        // 检查障碍物碰撞
        CheckObstacleCollisions();
    }

    /// <summary>
    /// 检查新头部位置是否会碰撞（用于移动前预测）
    /// </summary>
    private bool WillCollide(Point newHead)
    {
        int gridWidth = _state.CurrentLevel?.GridWidth ?? GameConfig.GridSize;
        int gridHeight = _state.CurrentLevel?.GridHeight ?? GameConfig.GridSize;

        // 检查墙壁碰撞
        if (newHead.X < 0 || newHead.X >= gridWidth || newHead.Y < 0 || newHead.Y >= gridHeight)
        {
            return true;
        }

        // 检查自身碰撞
        var current = _state.Snake.First;
        while (current != null)
        {
            if (newHead == current.Value)
            {
                return true;
            }
            current = current.Next;
        }

        // 检查障碍物碰撞
        foreach (var obstacle in _obstacles)
        {
            if (obstacle.Position == newHead)
            {
                // 检查是否是致命障碍物
                var result = obstacle.Interact(_state);
                if (result.IsDeadly)
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// 检查障碍物碰撞
    /// </summary>
    private void CheckObstacleCollisions()
    {
        Point head = _state.Snake.First!.Value;
        var obstaclesToRemove = new List<Obstacle>();

        foreach (var obstacle in _obstacles)
        {
            if (obstacle.Position == head)
            {
                var result = obstacle.Interact(_state);

                if (result.IsDeadly)
                {
                    _state.IsGameOver = true;
                    return;
                }

                if (result.ShouldRemove)
                {
                    obstaclesToRemove.Add(obstacle);
                }

                // 处理特殊效果
                if (result.SpeedChange != 0)
                {
                    _state.SpeedLevel = Math.Max(1, Math.Min(10, _state.SpeedLevel + result.SpeedChange));
                }

                if (result.ScoreMultiplier > 1 && _state.Foods.Count > 0)
                {
                    // 给最近吃到的食物加分（简化处理，加固定分数）
                    _state.Score += GetBasePoints() * (result.ScoreMultiplier - 1);
                }

                if (result.TeleportTarget.HasValue)
                {
                    TeleportSnake(result.TeleportTarget.Value);
                }
            }
        }

        // 移除已破坏的障碍物
        foreach (var obstacle in obstaclesToRemove)
        {
            _obstacles.Remove(obstacle);
        }
    }

    /// <summary>
    /// 传送蛇到指定位置
    /// </summary>
    private void TeleportSnake(Point targetPosition)
    {
        Point head = _state.Snake.First!.Value;
        Point offset = new Point(targetPosition.X - head.X, targetPosition.Y - head.Y);

        // 移动整个蛇身
        var newSnake = new Models.Collections.LinkedList<Point>();
        var current = _state.Snake.First;
        while (current != null)
        {
            Point newSegment = new Point(
                current.Value.X + offset.X,
                current.Value.Y + offset.Y
            );
            newSnake.AddLast(newSegment);
            current = current.Next;
        }

        _state.Snake.Clear();
        current = newSnake.First;
        while (current != null)
        {
            _state.Snake.AddLast(current.Value);
            current = current.Next;
        }
    }

    /// <summary>
    /// 尝试改变方向
    /// </summary>
    public bool TryChangeDirection(Point newDirection)
    {
        if (_state.IsWaitingToStart || _state.IsGameOver)
            return false;

        // 防止反向移动
        if (newDirection.X != 0 && _state.Direction.X == -newDirection.X)
            return false;
        if (newDirection.Y != 0 && _state.Direction.Y == -newDirection.Y)
            return false;

        _state.Direction = newDirection;
        return true;
    }

    /// <summary>
    /// 切换暂停状态
    /// </summary>
    public void TogglePause()
    {
        if (_state.IsGameOver || _state.IsWaitingToStart)
            return;

        _state.IsPaused = !_state.IsPaused;
    }

    /// <summary>
    /// 开始游戏
    /// </summary>
    public void StartGame()
    {
        if (_state.IsWaitingToStart || _state.IsGameOver)
        {
            if (_state.IsGameOver)
            {
                Initialize();
            }
            _state.IsWaitingToStart = false;
        }
    }

    /// <summary>
    /// 获取定时器间隔时间
    /// </summary>
    public int GetTimerInterval()
    {
        int baseInterval = _state.Difficulty == Difficulty.Hard
            ? GameConfig.HardBaseInterval
            : GameConfig.NormalBaseInterval;
        int interval = baseInterval - (_state.SpeedLevel - 1) * GameConfig.SpeedIntervalReduction;
        return Math.Max(GameConfig.MinInterval, interval);
    }

    /// <summary>
    /// 生成食物
    /// </summary>
    private void SpawnFood()
    {
        int gridWidth = _state.CurrentLevel?.GridWidth ?? GameConfig.GridSize;
        int gridHeight = _state.CurrentLevel?.GridHeight ?? GameConfig.GridSize;

        int foodCount = _state.CurrentLevel?.Settings.FoodCount ??
            (_state.Difficulty == Difficulty.Easy ? GameConfig.EasyFoodCount : GameConfig.NormalFoodCount);

        // 如果有固定食物位置，使用固定位置（优先级最高）
        if (_state.CurrentLevel?.FixedFoodPositions != null && _state.CurrentLevel.FixedFoodPositions.Count > 0)
        {
            foreach (var foodPos in _state.CurrentLevel.FixedFoodPositions)
            {
                if (!_state.Foods.Contains(foodPos))
                {
                    _state.Foods.Add(foodPos);
                    _state.TotalFoodSpawned++;
                }
            }
            return;
        }

        // 检查是否有关卡模式的"收集所有食物"通关条件
        bool mustCollectAllFood = _state.CurrentLevel?.VictoryCondition.Type == VictoryConditionType.CollectAllFood
            || (_state.CurrentLevel?.VictoryCondition.Type == VictoryConditionType.Combined
                && _state.CurrentLevel.VictoryCondition.MustCollectAllFood);

        // 检查是否有限制食物生成数量
        int? foodSpawnCount = _state.CurrentLevel?.VictoryCondition.FoodSpawnCount;

        // 如果不要求收集所有食物且有限制，正常检查限制
        if (!mustCollectAllFood && foodSpawnCount.HasValue)
        {
            int maxFood = foodSpawnCount.Value;
            if (_state.TotalFoodSpawned >= maxFood)
            {
                return; // 已达到最大食物生成数量
            }

            // 调整要生成的食物数量
            foodCount = Math.Min(foodCount, maxFood - _state.TotalFoodSpawned);
        }

        // 随机生成食物
        int maxTotalAttempts = foodCount * 200; // 为每个食物分配更多尝试次数
        int totalAttempts = 0;

        while (_state.Foods.Count < foodCount && totalAttempts < maxTotalAttempts)
        {
            Point newFood = new Point(
                _randomProvider.Next(gridWidth),
                _randomProvider.Next(gridHeight)
            );
            totalAttempts++;

            if (!IsOccupied(newFood))
            {
                _state.Foods.Add(newFood);
                _state.TotalFoodSpawned++;
            }
        }
    }

    /// <summary>
    /// 检查位置是否被占用
    /// </summary>
    private bool IsOccupied(Point position)
    {
        // 检查蛇身
        if (_state.Snake.Contains(position))
            return true;

        // 检查现有食物
        if (_state.Foods.Contains(position))
            return true;

        // 检查障碍物
        foreach (var obstacle in _obstacles)
        {
            if (obstacle.Position == position)
                return true;
        }

        return false;
    }

    /// <summary>
    /// 获取基础得分
    /// </summary>
    private int GetBasePoints()
    {
        return _state.Difficulty switch
        {
            Difficulty.Easy => GameConfig.EasyBasePoints,
            Difficulty.Medium => GameConfig.MediumBasePoints,
            Difficulty.Hard => GameConfig.HardBasePoints,
            _ => GameConfig.MediumBasePoints
        };
    }

    /// <summary>
    /// 更新动态障碍物
    /// </summary>
    public void UpdateDynamicObstacles()
    {
        int gridWidth = _state.CurrentLevel?.GridWidth ?? GameConfig.GridSize;
        int gridHeight = _state.CurrentLevel?.GridHeight ?? GameConfig.GridSize;

        foreach (var obstacle in _obstacles)
        {
            obstacle.Update(gridWidth, gridHeight);
        }
    }

    /// <summary>
    /// 获取当前关卡的所有障碍物
    /// </summary>
    public IReadOnlyList<Obstacle> GetObstacles()
    {
        return _obstacles;
    }

    /// <summary>
    /// 更新关卡时间（每秒调用一次）
    /// </summary>
    public void UpdateLevelTime()
    {
        if (_state.CurrentLevel != null && !_state.IsPaused && !_state.IsWaitingToStart && !_state.IsGameOver)
        {
            _state.LevelTime++;
        }
    }
}