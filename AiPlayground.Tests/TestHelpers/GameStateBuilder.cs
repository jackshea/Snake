using System.Drawing;
using AiPlayground.Game;
using AiPlayground.Models;

namespace AiPlayground.Tests.TestHelpers;

/// <summary>
/// 测试数据构建器 - 用于构建 GameState 测试数据
/// </summary>
public class GameStateBuilder
{
    private readonly GameState _state = new();

    public GameStateBuilder WithSnakeAt(params Point[] positions)
    {
        _state.Snake.Clear();
        foreach (var pos in positions)
        {
            _state.Snake.AddLast(pos);
        }
        return this;
    }

    public GameStateBuilder WithSnakeInCenter(Point direction, int length = 3)
    {
        var startX = GameConfig.GridSize / 2;
        var startY = GameConfig.GridSize / 2;

        _state.Snake.Clear();
        for (int i = 0; i < length; i++)
        {
            _state.Snake.AddLast(new Point(
                startX - i * direction.X,
                startY - i * direction.Y
            ));
        }
        _state.Direction = direction;
        return this;
    }

    public GameStateBuilder WithFoodAt(params Point[] positions)
    {
        _state.Foods.Clear();
        foreach (var pos in positions)
        {
            _state.Foods.Add(pos);
        }
        return this;
    }

    public GameStateBuilder WithDirection(Point direction)
    {
        _state.Direction = direction;
        return this;
    }

    public GameStateBuilder WithScore(int score)
    {
        _state.Score = score;
        return this;
    }

    public GameStateBuilder WithDifficulty(Difficulty difficulty)
    {
        _state.Difficulty = difficulty;
        return this;
    }

    public GameStateBuilder WithSpeedLevel(int level)
    {
        _state.SpeedLevel = level;
        return this;
    }

    public GameStateBuilder WithLevel(Level level)
    {
        _state.CurrentLevel = level;
        return this;
    }

    public GameStateBuilder AsGameOver()
    {
        _state.IsGameOver = true;
        return this;
    }

    public GameStateBuilder AsPaused()
    {
        _state.IsPaused = true;
        return this;
    }

    public GameStateBuilder WaitingToStart()
    {
        _state.IsWaitingToStart = true;
        return this;
    }

    public GameStateBuilder Started()
    {
        _state.IsWaitingToStart = false;
        _state.IsPaused = false;
        _state.IsGameOver = false;
        return this;
    }

    public GameState Build() => _state;
}
