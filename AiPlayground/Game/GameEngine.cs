using System;
using System.Drawing;
using AiPlayground.Models;

namespace AiPlayground.Game;

/// <summary>
/// 游戏引擎 - 负责游戏核心逻辑
/// </summary>
public class GameEngine
{
    private readonly Random _random = new();
    private readonly GameState _state;

    public GameEngine(GameState state)
    {
        _state = state;
    }

    /// <summary>
    /// 初始化游戏
    /// </summary>
    public void Initialize()
    {
        int startX = GameConfig.GridSize / 2;
        int startY = GameConfig.GridSize / 2;

        _state.Snake.Clear();
        _state.Snake.AddLast(new Point(startX, startY));
        _state.Snake.AddLast(new Point(startX - 1, startY));
        _state.Snake.AddLast(new Point(startX - 2, startY));

        _state.Foods.Clear();
        SpawnFood();

        _state.Direction = GameConfig.DirectionRight;
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

        // 在头部添加新节点
        _state.Snake.AddFirst(newHead);

        bool ateFood = false;
        for (int i = _state.Foods.Count - 1; i >= 0; i--)
        {
            if (newHead == _state.Foods[i])
            {
                int points = GetBasePoints() + _state.SpeedLevel;
                _state.Score += points;
                _state.Foods.RemoveAt(i);
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

        // 检查墙壁碰撞
        if (head.X < 0 || head.X >= GameConfig.GridSize || head.Y < 0 || head.Y >= GameConfig.GridSize)
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
        int foodCount = _state.Difficulty == Difficulty.Easy
            ? GameConfig.EasyFoodCount
            : GameConfig.NormalFoodCount;

        while (_state.Foods.Count < foodCount)
        {
            Point newFood;
            do
            {
                newFood = new Point(
                    _random.Next(GameConfig.GridSize),
                    _random.Next(GameConfig.GridSize)
                );
            } while (_state.Snake.Contains(newFood) || _state.Foods.Contains(newFood));

            _state.Foods.Add(newFood);
        }
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
}
