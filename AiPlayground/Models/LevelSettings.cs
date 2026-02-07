namespace AiPlayground.Models
{
    /// <summary>
    /// 关卡配置
    /// </summary>
    public class LevelSettings
    {
        public Difficulty DefaultDifficulty { get; set; }
        public int InitialSpeedLevel { get; set; }
        public int GridWidth { get; set; }
        public int GridHeight { get; set; }
        public Point SnakeStartPosition { get; set; }
        public Point InitialDirection { get; set; }
        public int InitialSnakeLength { get; set; }
        public int FoodCount { get; set; }
        public bool EnableDynamicObstacles { get; set; }

        public LevelSettings()
        {
            DefaultDifficulty = Difficulty.Easy;
            InitialSpeedLevel = 5;
            GridWidth = 30;
            GridHeight = 20;
            SnakeStartPosition = new Point(5, 5);
            InitialDirection = new Point(1, 0); // 向右
            InitialSnakeLength = 3;
            FoodCount = 1;
            EnableDynamicObstacles = false;
        }
    }
}
