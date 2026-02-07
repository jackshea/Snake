using System.Drawing;
using AiPlayground.Models;
using AiPlayground.Models.Obstacles;

namespace AiPlayground.Tests.TestHelpers;

/// <summary>
/// 关卡测试数据构建器
/// </summary>
public class LevelBuilder
{
    private readonly Level _level;

    public LevelBuilder()
    {
        _level = Level.CreatePresetLevel(1, "Test Level", "Test Description");
    }

    public LevelBuilder WithId(string id)
    {
        _level.Id = id;
        return this;
    }

    public LevelBuilder WithNumber(int number)
    {
        _level.LevelNumber = number;
        return this;
    }

    public LevelBuilder Unlocked()
    {
        _level.IsUnlocked = true;
        return this;
    }

    public LevelBuilder Locked()
    {
        _level.IsUnlocked = false;
        return this;
    }

    public LevelBuilder AsCustom()
    {
        _level.IsCustom = true;
        return this;
    }

    public LevelBuilder WithObstacle(Obstacle obstacle)
    {
        _level.Obstacles.Add(obstacle);
        return this;
    }

    public LevelBuilder WithStaticObstacle(int x, int y)
    {
        _level.Obstacles.Add(new StaticObstacle(new Point(x, y)));
        return this;
    }

    public LevelBuilder WithDestructibleObstacle(int x, int y, int maxPasses = 1)
    {
        _level.Obstacles.Add(new DestructibleObstacle(new Point(x, y), maxPasses));
        return this;
    }

    public LevelBuilder WithDynamicObstacle(int x, int y, params Point[] path)
    {
        _level.Obstacles.Add(new DynamicObstacle(new Point(x, y), path.ToList()));
        return this;
    }

    public LevelBuilder WithVictoryCondition(VictoryConditionType type, int target = 0)
    {
        _level.VictoryCondition.Type = type;
        switch (type)
        {
            case VictoryConditionType.TargetScore:
                _level.VictoryCondition.TargetScore = target;
                break;
            case VictoryConditionType.TargetLength:
                _level.VictoryCondition.TargetLength = target;
                break;
            case VictoryConditionType.CollectAllFood:
                _level.VictoryCondition.MustCollectAllFood = true;
                break;
            case VictoryConditionType.Combined:
                _level.VictoryCondition.TargetScore = target;
                break;
        }
        return this;
    }

    public LevelBuilder WithGridSize(int width, int height)
    {
        _level.Settings.GridWidth = width;
        _level.Settings.GridHeight = height;
        return this;
    }

    public LevelBuilder WithFoodCount(int count)
    {
        _level.Settings.FoodCount = count;
        return this;
    }

    public LevelBuilder WithFixedFoodPosition(params Point[] positions)
    {
        foreach (var pos in positions)
        {
            _level.FixedFoodPositions.Add(pos);
        }
        return this;
    }

    public LevelBuilder WithSnakeStart(int x, int y)
    {
        _level.Settings.SnakeStartPosition = new Point(x, y);
        return this;
    }

    public LevelBuilder WithInitialDirection(Point direction)
    {
        _level.Settings.InitialDirection = direction;
        return this;
    }

    public LevelBuilder WithInitialSnakeLength(int length)
    {
        _level.Settings.InitialSnakeLength = length;
        return this;
    }

    public LevelBuilder WithInitialSpeed(int speed)
    {
        _level.Settings.InitialSpeedLevel = speed;
        return this;
    }

    public LevelBuilder WithDifficulty(Difficulty difficulty)
    {
        _level.Settings.DefaultDifficulty = difficulty;
        return this;
    }

    public Level Build() => _level;
}
