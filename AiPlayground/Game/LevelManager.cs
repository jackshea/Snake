using AiPlayground.Models;
using AiPlayground.Services;

namespace AiPlayground.Game;

/// <summary>
/// 关卡管理器 - 管理关卡列表、解锁状态和通关条件
/// </summary>
public class LevelManager
{
    private readonly LevelStorageService _storageService;
    private List<Level> _presetLevels = new();
    private List<Level> _customLevels = new();
    private LevelProgression _progression = new();
    private Level? _currentLevel;

    /// <summary>
    /// 关卡完成事件
    /// </summary>
    public event Action<Level>? LevelCompleted;

    /// <summary>
    /// 关卡变更事件
    /// </summary>
    public event Action<Level>? LevelChanged;

    /// <summary>
    /// 当前关卡
    /// </summary>
    public Level? CurrentLevel => _currentLevel;

    /// <summary>
    /// 所有已解锁的预设关卡
    /// </summary>
    public IReadOnlyList<Level> UnlockedPresetLevels
    {
        get
        {
            LoadPresetLevels();
            var unlocked = new List<Level>();
            foreach (var level in _presetLevels)
            {
                if (_progression.IsLevelUnlocked(level.LevelNumber))
                {
                    level.IsUnlocked = true;
                    unlocked.Add(level);
                }
                else
                {
                    level.IsUnlocked = false;
                }
            }
            return unlocked;
        }
    }

    /// <summary>
    /// 所有自定义关卡
    /// </summary>
    public IReadOnlyList<Level> CustomLevels => _customLevels;

    public LevelManager(LevelStorageService storageService)
    {
        _storageService = storageService;
        LoadData();
    }

    /// <summary>
    /// 加载所有数据
    /// </summary>
    private void LoadData()
    {
        _presetLevels = _storageService.LoadPresetLevels();
        _customLevels = _storageService.LoadCustomLevels();
        _progression = _storageService.LoadProgression();
    }

    /// <summary>
    /// 重新加载关卡数据
    /// </summary>
    public void ReloadLevels()
    {
        LoadData();
    }

    /// <summary>
    /// 根据序号获取关卡
    /// </summary>
    public Level? GetLevelByNumber(int levelNumber)
    {
        LoadPresetLevels();
        var level = _presetLevels.FirstOrDefault(l => l.LevelNumber == levelNumber);
        if (level != null)
        {
            // 更新解锁状态
            level.IsUnlocked = _progression.IsLevelUnlocked(levelNumber);
        }
        return level;
    }

    /// <summary>
    /// 根据 ID 获取关卡
    /// </summary>
    public Level? GetLevelById(string levelId)
    {
        LoadPresetLevels();
        var preset = _presetLevels.FirstOrDefault(l => l.Id == levelId);
        if (preset != null) return preset;

        LoadCustomLevels();
        return _customLevels.FirstOrDefault(l => l.Id == levelId);
    }

    /// <summary>
    /// 尝试加载关卡
    /// </summary>
    public bool TryLoadLevel(string levelId)
    {
        var level = GetLevelById(levelId);
        if (level == null) return false;

        // 检查是否解锁
        if (!level.IsCustom && !_progression.IsLevelUnlocked(level.LevelNumber))
        {
            return false;
        }

        _currentLevel = level;
        LevelChanged?.Invoke(level);
        return true;
    }

    /// <summary>
    /// 检查通关条件
    /// </summary>
    public bool CheckVictoryCondition(GameState state)
    {
        if (_currentLevel == null) return false;

        return _currentLevel.VictoryCondition.CheckCondition(
            state,
            state.FoodCollected,
            state.TotalFoodSpawned
        );
    }

    /// <summary>
    /// 完成关卡
    /// </summary>
    public async Task CompleteLevelAsync(GameState state)
    {
        if (_currentLevel == null) return;

        // 更新进度
        _progression.UpdateCompletion(
            _currentLevel.Id,
            state.Score,
            state.LevelTime
        );

        // 解锁下一关
        if (!_currentLevel.IsCustom)
        {
            int nextLevelNumber = _currentLevel.LevelNumber + 1;
            if (nextLevelNumber > _progression.HighestUnlockedLevel)
            {
                _progression.HighestUnlockedLevel = nextLevelNumber;
            }
        }

        // 保存进度
        await _storageService.SaveProgressionAsync(_progression);

        // 触发事件
        LevelCompleted?.Invoke(_currentLevel);
    }

    /// <summary>
    /// 获取关卡完成记录
    /// </summary>
    public LevelCompletion? GetLevelCompletion(string levelId)
    {
        return _progression.GetCompletion(levelId);
    }

    /// <summary>
    /// 获取下一个未解锁的关卡序号
    /// </summary>
    public int GetNextLockedLevelNumber()
    {
        LoadPresetLevels();
        return _progression.HighestUnlockedLevel + 1;
    }

    /// <summary>
    /// 重置进度
    /// </summary>
    public bool ResetProgression()
    {
        if (_storageService.ResetProgression())
        {
            _progression = new LevelProgression();
            LoadData();
            return true;
        }
        return false;
    }

    /// <summary>
    /// 保存自定义关卡
    /// </summary>
    public async Task<bool> SaveCustomLevelAsync(Level level)
    {
        level.IsCustom = true;
        level.IsUnlocked = true;

        var success = await _storageService.SaveLevelAsync(level);
        if (success)
        {
            LoadData();
        }

        return success;
    }

    /// <summary>
    /// 删除自定义关卡
    /// </summary>
    public bool DeleteCustomLevel(string levelId)
    {
        var success = _storageService.DeleteCustomLevel(levelId);
        if (success)
        {
            LoadData();
        }

        return success;
    }

    /// <summary>
    /// 获取通关条件描述
    /// </summary>
    public string GetVictoryConditionDescription()
    {
        return _currentLevel?.VictoryCondition.GetDescription() ?? "无";
    }

    /// <summary>
    /// 获取关卡进度百分比
    /// </summary>
    public double GetLevelProgress(GameState state)
    {
        if (_currentLevel == null) return 0;

        var condition = _currentLevel.VictoryCondition;

        return condition.Type switch
        {
            VictoryConditionType.TargetScore when condition.TargetScore > 0 =>
                Math.Min(100, (double)state.Score / condition.TargetScore * 100),

            VictoryConditionType.TargetLength when condition.TargetLength > 0 =>
                Math.Min(100, (double)state.Snake.Count / condition.TargetLength * 100),

            VictoryConditionType.CollectAllFood when state.TotalFoodSpawned > 0 =>
                Math.Min(100, (double)state.FoodCollected / state.TotalFoodSpawned * 100),

            VictoryConditionType.Combined =>
                GetCombinedProgress(condition, state),

            _ => 0
        };
    }

    private double GetCombinedProgress(VictoryCondition condition, GameState state)
    {
        var progresses = new List<double>();

        if (condition.TargetScore > 0)
        {
            progresses.Add(Math.Min(100, (double)state.Score / condition.TargetScore * 100));
        }

        if (condition.TargetLength > 0)
        {
            progresses.Add(Math.Min(100, (double)state.Snake.Count / condition.TargetLength * 100));
        }

        if (condition.MustCollectAllFood && state.TotalFoodSpawned > 0)
        {
            progresses.Add(Math.Min(100, (double)state.FoodCollected / state.TotalFoodSpawned * 100));
        }

        return progresses.Count > 0 ? progresses.Average() : 0;
    }

    private void LoadPresetLevels()
    {
        if (_presetLevels.Count == 0)
        {
            _presetLevels = _storageService.LoadPresetLevels();
        }
    }

    private void LoadCustomLevels()
    {
        _customLevels = _storageService.LoadCustomLevels();
    }
}
