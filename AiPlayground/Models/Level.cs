using AiPlayground.Models.Obstacles;
using System.Text.Json.Serialization;

namespace AiPlayground.Models
{
    /// <summary>
    /// 关卡主数据模型
    /// </summary>
    public class Level
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int LevelNumber { get; set; }
        public bool IsUnlocked { get; set; }
        public bool IsCustom { get; set; }
        public VictoryCondition VictoryCondition { get; set; } = new();
        public List<Obstacle> Obstacles { get; set; } = new();
        public LevelSettings Settings { get; set; } = new();
        public List<Point> FixedFoodPositions { get; set; } = new();

        [JsonIgnore]
        public int GridWidth => Settings.GridWidth;

        [JsonIgnore]
        public int GridHeight => Settings.GridHeight;

        public Level()
        {
            Id = Guid.NewGuid().ToString();
            IsUnlocked = false;
            IsCustom = false;
        }

        /// <summary>
        /// 创建预设关卡
        /// </summary>
        public static Level CreatePresetLevel(int number, string name, string description)
        {
            return new Level
            {
                Id = $"preset_level_{number}",
                Name = name,
                Description = description,
                LevelNumber = number,
                IsUnlocked = number == 1, // 第一关默认解锁
                IsCustom = false
            };
        }

        /// <summary>
        /// 创建自定义关卡
        /// </summary>
        public static Level CreateCustomLevel(string name)
        {
            return new Level
            {
                Id = $"custom_{Guid.NewGuid()}",
                Name = name,
                Description = "自定义关卡",
                LevelNumber = 0,
                IsUnlocked = true,
                IsCustom = true
            };
        }
    }
}
