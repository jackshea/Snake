using System.Text.Json;
using AiPlayground.Models;
using AiPlayground.Models.Obstacles;

namespace AiPlayground.Services;

/// <summary>
/// 关卡存储服务 - 负责关卡的加载和保存
/// </summary>
public class LevelStorageService
{
    private readonly string _presetLevelsPath;
    private readonly string _customLevelsPath;
    private readonly string _progressionFilePath;
    private readonly JsonSerializerOptions _jsonOptions;

    public LevelStorageService()
    {
        var baseDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AiPlayground"
        );

        _presetLevelsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PresetLevels");
        _customLevelsPath = Path.Combine(baseDataPath, "Levels", "Custom");
        _progressionFilePath = Path.Combine(baseDataPath, "level_progression.json");

        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new ObstacleJsonConverter() }
        };
    }

    /// <summary>
    /// 加载所有预设关卡
    /// </summary>
    public List<Level> LoadPresetLevels()
    {
        var levels = new List<Level>();

        if (!Directory.Exists(_presetLevelsPath))
        {
            Directory.CreateDirectory(_presetLevelsPath);
            return levels;
        }

        try
        {
            foreach (var file in Directory.GetFiles(_presetLevelsPath, "*.json"))
            {
                try
                {
                    var json = File.ReadAllText(file);
                    var level = JsonSerializer.Deserialize<Level>(json, _jsonOptions);
                    if (level != null)
                    {
                        levels.Add(level);
                    }
                }
                catch
                {
                    // 忽略单个文件加载错误
                }
            }

            // 按关卡序号排序
            return levels.OrderBy(l => l.LevelNumber).ToList();
        }
        catch
        {
            return levels;
        }
    }

    /// <summary>
    /// 加载所有自定义关卡
    /// </summary>
    public List<Level> LoadCustomLevels()
    {
        var levels = new List<Level>();

        if (!Directory.Exists(_customLevelsPath))
        {
            Directory.CreateDirectory(_customLevelsPath);
            return levels;
        }

        try
        {
            foreach (var file in Directory.GetFiles(_customLevelsPath, "*.json"))
            {
                try
                {
                    var json = File.ReadAllText(file);
                    var level = JsonSerializer.Deserialize<Level>(json, _jsonOptions);
                    if (level != null && level.IsCustom)
                    {
                        levels.Add(level);
                    }
                }
                catch
                {
                    // 忽略单个文件加载错误
                }
            }

            return levels;
        }
        catch
        {
            return levels;
        }
    }

    /// <summary>
    /// 保存关卡（用于自定义关卡）
    /// </summary>
    public async Task<bool> SaveLevelAsync(Level level)
    {
        try
        {
            if (!Directory.Exists(_customLevelsPath))
            {
                Directory.CreateDirectory(_customLevelsPath);
            }

            var fileName = $"{level.Id}.json";
            var filePath = Path.Combine(_customLevelsPath, fileName);

            var json = JsonSerializer.Serialize(level, _jsonOptions);
            await File.WriteAllTextAsync(filePath, json);

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 删除自定义关卡
    /// </summary>
    public bool DeleteCustomLevel(string levelId)
    {
        try
        {
            var fileName = $"{levelId}.json";
            var filePath = Path.Combine(_customLevelsPath, fileName);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 加载关卡进度
    /// </summary>
    public LevelProgression LoadProgression()
    {
        try
        {
            if (File.Exists(_progressionFilePath))
            {
                var json = File.ReadAllText(_progressionFilePath);
                var progression = JsonSerializer.Deserialize<LevelProgression>(json, _jsonOptions);
                if (progression != null)
                {
                    return progression;
                }
            }
        }
        catch
        {
            // 忽略加载错误，返回默认进度
        }

        return new LevelProgression();
    }

    /// <summary>
    /// 获取 JSON 序列化选项（供外部使用）
    /// </summary>
    public JsonSerializerOptions GetJsonOptions()
    {
        return _jsonOptions;
    }

    /// <summary>
    /// 保存关卡进度
    /// </summary>
    public async Task<bool> SaveProgressionAsync(LevelProgression progression)
    {
        try
        {
            var directory = Path.GetDirectoryName(_progressionFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = JsonSerializer.Serialize(progression, _jsonOptions);
            await File.WriteAllTextAsync(_progressionFilePath, json);

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 重置关卡进度
    /// </summary>
    public bool ResetProgression()
    {
        try
        {
            if (File.Exists(_progressionFilePath))
            {
                File.Delete(_progressionFilePath);
            }
            return true;
        }
        catch
        {
            return false;
        }
    }
}

/// <summary>
/// 障碍物 JSON 转换器 - 处理多态序列化
/// </summary>
public class ObstacleJsonConverter : System.Text.Json.Serialization.JsonConverter<Obstacle>
{
    public override Obstacle? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        if (!root.TryGetProperty("type", out var typeProp))
        {
            return null;
        }

        var typeStr = typeProp.GetString();
        var obstacleType = Enum.Parse<ObstacleType>(typeStr!);

        // 解析位置
        var position = root.TryGetProperty("position", out var posProp)
            ? new Point(posProp.GetProperty("x").GetInt32(), posProp.GetProperty("y").GetInt32())
            : new Point(0, 0);

        return obstacleType switch
        {
            ObstacleType.Static => new StaticObstacle(position),
            ObstacleType.Destructible => new DestructibleObstacle(position,
                root.TryGetProperty("maxPasses", out var maxProp) ? maxProp.GetInt32() : 1),
            ObstacleType.Dynamic => new DynamicObstacle(position,
                ParsePath(root.GetProperty("path"))),
            ObstacleType.SpeedUp => SpecialEffectObstacle.CreateSpeedUp(position,
                root.TryGetProperty("speedChangeAmount", out var speedProp) ? speedProp.GetInt32() : 2),
            ObstacleType.SpeedDown => SpecialEffectObstacle.CreateSpeedDown(position,
                root.TryGetProperty("speedChangeAmount", out var slowProp) ? slowProp.GetInt32() : -2),
            ObstacleType.ScoreMultiplier => SpecialEffectObstacle.CreateScoreMultiplier(position,
                root.TryGetProperty("scoreMultiplierValue", out var multProp) ? multProp.GetInt32() : 2),
            ObstacleType.Teleport when root.TryGetProperty("teleportDestination", out var destProp) =>
                SpecialEffectObstacle.CreateTeleport(position,
                    new Point(destProp.GetProperty("x").GetInt32(), destProp.GetProperty("y").GetInt32())),
            _ => null
        };
    }

    private List<Point> ParsePath(JsonElement pathElement)
    {
        var path = new List<Point>();
        foreach (var item in pathElement.EnumerateArray())
        {
            path.Add(new Point(item.GetProperty("x").GetInt32(), item.GetProperty("y").GetInt32()));
        }
        return path;
    }

    public override void Write(Utf8JsonWriter writer, Obstacle value, JsonSerializerOptions options)
    {
        var concreteType = value.GetType();

        writer.WriteStartObject();

        // 写入通用属性
        writer.WriteString("type", value.Type.ToString());
        writer.WritePropertyName("position");
        writer.WriteStartObject();
        writer.WriteNumber("x", value.Position.X);
        writer.WriteNumber("y", value.Position.Y);
        writer.WriteEndObject();

        // 根据类型写入特定属性
        switch (value)
        {
            case DestructibleObstacle destructible:
                writer.WriteNumber("maxPasses", destructible.MaxPasses);
                writer.WriteNumber("remainingPasses", destructible.RemainingPasses);
                break;

            case DynamicObstacle dynamic:
                writer.WritePropertyName("path");
                writer.WriteStartArray();
                foreach (var point in dynamic.Path)
                {
                    writer.WriteStartObject();
                    writer.WriteNumber("x", point.X);
                    writer.WriteNumber("y", point.Y);
                    writer.WriteEndObject();
                }
                writer.WriteEndArray();
                writer.WriteNumber("moveIntervalMs", dynamic.MoveIntervalMs);
                writer.WriteBoolean("loopPath", dynamic.LoopPath);
                break;

            case SpecialEffectObstacle special:
                if (special.Type == ObstacleType.SpeedUp || special.Type == ObstacleType.SpeedDown)
                {
                    writer.WriteNumber("speedChangeAmount", special.SpeedChangeAmount);
                }
                else if (special.Type == ObstacleType.ScoreMultiplier)
                {
                    writer.WriteNumber("scoreMultiplierValue", special.ScoreMultiplierValue);
                }
                else if (special.Type == ObstacleType.Teleport && special.TeleportDestination.HasValue)
                {
                    writer.WritePropertyName("teleportDestination");
                    writer.WriteStartObject();
                    writer.WriteNumber("x", special.TeleportDestination.Value.X);
                    writer.WriteNumber("y", special.TeleportDestination.Value.Y);
                    writer.WriteEndObject();
                }
                break;
        }

        writer.WriteEndObject();
    }
}
