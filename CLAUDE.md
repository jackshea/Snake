# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 项目概述

这是一个基于 .NET 8.0 的 Windows 窗体贪吃蛇游戏项目，使用 WinForms 作为渲染框架，不依赖 Unity 或其他游戏引擎。

项目采用**模块化分层架构**，实现了完整的关卡系统、障碍物系统、难度设置等功能。

## 构建和运行

```bash
# 构建项目
dotnet build

# 运行项目
dotnet run

# 发布为可执行文件
dotnet publish -c Release -r win-x64 --self-contained
```

## 项目架构

### 分层结构

```
AiPlayground/
├── Models/           # 数据模型层
├── Game/             # 游戏逻辑层
├── Services/         # 服务层（文件存储等）
├── Forms/            # UI 窗体层
├── Controls/         # 自定义控件
├── PresetLevels/     # 预设关卡配置（JSON）
└── GameConfig.cs     # 游戏配置常量
```

### 核心组件

#### Models/ （数据模型层）

- **GameState.cs** - 游戏状态
  - 蛇身数据：使用 `Models.Collections.LinkedList<Point>` 自定义双端链表
  - 食物列表、分数、速度、难度等运行时状态
  - 当前关卡引用、通关状态、关卡用时

- **Level.cs** - 关卡数据模型
  - 关卡 ID、名称、描述、序号
  - 解锁状态、是否自定义
  - 通关条件、障碍物列表、关卡设置、固定食物位置

- **VictoryCondition.cs** - 通关条件
  - `Type`: 目标分数、目标长度、收集所有食物、组合条件
  - `TargetScore`, `TargetLength`, `MustCollectAllFood`, `FoodSpawnCount`

- **LevelSettings.cs** - 关卡配置
  - 默认难度、初始速度、网格尺寸
  - 蛇的初始位置、方向、长度、食物数量

- **LevelProgression.cs** - 关卡进度
  - 已完成关卡记录（最高分、最佳时间、完成次数）
  - 最高解锁关卡序号、最后游玩时间

- **Obstacles/** - 障碍物系统
  - `Obstacle.cs` - 基类，包含 `Interact(GameState)` 交互方法
  - `StaticObstacle.cs` - 静态障碍物（蛇碰到死亡）
  - `DynamicObstacle.cs` - 动态障碍物（按路径移动）
  - `DestructibleObstacle.cs` - 可破坏障碍物（可穿过指定次数后消失）
  - `SpecialEffectObstacle.cs` - 特殊效果（加速、减速、分数倍增、传送等）

#### Game/ （游戏逻辑层）

- **GameEngine.cs** - 游戏引擎核心
  - `MoveSnake()` - 移动蛇（**先检查碰撞再移动**，避免蛇头进入墙内）
  - `WillCollide(newHead)` - 预测下一步是否碰撞
  - `CheckCollisions()` - 当前位置的碰撞检测
  - `CheckObstacleCollisions()` - 障碍物交互处理
  - `SpawnFood()` - 生成食物（支持固定位置模式）
  - `UpdateDynamicObstacles()` - 更新动态障碍物位置

- **LevelManager.cs** - 关卡管理器
  - 加载/保存关卡
  - 检查通关条件
  - 处理关卡完成（自动解锁下一关）
  - 重置进度

#### Services/ （服务层）

- **HighScoreService.cs** - 最高分存储（JSON 文件）
- **LevelStorageService.cs** - 关卡数据存储
  - 加载预设关卡（`PresetLevels/*.json`）
  - 加载/保存自定义关卡（`%AppData%\AiPlayground\Levels\Custom\`）
  - 加载/保存关卡进度

#### Forms/ （UI 层）

- **MainForm.cs** - 主窗体
  - 负责 UI 组装和事件委托
  - **重要**：不要订阅 `_levelManager.LevelCompleted` 事件（已在 OnGameTick 中手动调用）
  - 游戏循环：`OnGameTick()` → 移动蛇 → 检查碰撞 → 检查通关条件

- **GamePanel.cs** - 游戏渲染面板（继承 DoubleBufferPanel）
  - `DrawGrid()` - 绘制网格
  - `DrawSnake()` - 绘制蛇（绿色头，深绿身）
  - `DrawFoods()` - 绘制食物（根据难度不同颜色）
  - `DrawObstacles()` - 绘制障碍物（根据类型不同样式）
  - `DrawGameOver()`, `DrawPaused()` - 状态覆盖层

- **InfoPanel.cs** - 信息面板
  - 显示分数、难度、速度、最高分
  - 关卡模式下：显示关卡名称、通关目标、进度、用时

- **LevelSelectionForm.cs** - 关卡选择窗体
- **LevelCompleteForm.cs** - 关卡完成弹窗

#### Controls/ （自定义控件）

- **DoubleBufferPanel.cs** - 双缓冲面板基类（防止闪烁）

### 游戏配置（GameConfig.cs）

```csharp
GridSize = 30          // 默认网格 30x30
CellSize = 20          // 每个单元格 20x20 像素
InfoPanelWidth = 200   // 右侧信息面板宽度

MinSpeed = 1, MaxSpeed = 10
NormalBaseInterval = 150  // 基础定时器间隔（毫秒）
HardBaseInterval = 100
SpeedIntervalReduction = 10  // 每级速度减少的间隔

EasyBasePoints = 5
MediumBasePoints = 10
HardBasePoints = 15
```

## 关键实现细节

### 蛇的移动逻辑

使用自定义双端链表 `LinkedList<Point>` 存储蛇身：
- `First` 节点 = 蛇头
- 移动时 `AddFirst(newHead)` 添加新头
- 未吃到食物时 `RemoveLast()` 移除尾部

### 碰撞检测顺序（重要）

**正确实现**（GameEngine.cs:92-103）：
1. 计算新头部位置 `newHead`
2. **先调用 `WillCollide(newHead)` 检查是否碰撞**
3. 如果碰撞 → 设置 `IsGameOver = true`，**不移动蛇**
4. 如果不碰撞 → 执行移动

这样可以避免蛇头渲染到墙内的问题。

### 关卡完成事件处理（重要）

**避免重复弹出窗口的关键**（MainForm.cs:58-59）：
```csharp
// 注意：不订阅 LevelCompleted 事件，因为我们在 OnGameTick 中手动调用 OnLevelCompleted
// 如果订阅，会在 CompleteLevelAsync 中触发事件，导致重复调用
```

使用标志位防止重复触发：
```csharp
private bool _isShowingLevelComplete;

if (_gameState.CurrentLevel != null && !_gameState.IsLevelCompleted && !_isShowingLevelComplete)
{
    if (_levelManager.CheckVictoryCondition(_gameState))
    {
        _gameTimer.Stop();
        _levelTimeTimer.Stop();
        _gameState.IsLevelCompleted = true;
        _isShowingLevelComplete = true; // 防止重复调用
        OnLevelCompleted(_gameState.CurrentLevel);
        return;
    }
}
```

在 `finally` 块中重置标志。

### 食物生成逻辑

- **固定位置模式**：优先使用 `Level.FixedFoodPositions`（用于测试或精确设计）
- **随机模式**：确保不与蛇身、现有食物、障碍物重叠
- **限制数量模式**：支持 `VictoryCondition.FoodSpawnCount` 限制总生成量

### 输入处理

- 方向键：`OnKeyDown` → `HandleDirectionKey` → `GameEngine.TryChangeDirection`
- 防止反向移动（如向右时不能直接向左）
- 数字键 1-0：直接设置速度等级
- 空格键：开始游戏 / 暂停
- F1：帮助，F2：新游戏

## 预设关卡

预设关卡存储在 `PresetLevels/*.json`：

| 关卡 | 名称 | 特点 |
|------|------|------|
| Level1.json | 初出茅庐 | 无障碍物，目标分数 100 |
| Level2.json | 障碍初现 | 静态障碍物，目标长度 15 |
| Level3.json | 移动威胁 | 动态障碍物，组合条件 |
| Level4.json | 可破防线 | 可破坏障碍物，收集所有食物 |
| Level5.json | 终极挑战 | 混合障碍物，高难度 |

## 测试配置

Level1.json 底部包含注释的测试配置（方便调试）：
- 目标分数改为 10（吃 1 个食物就胜利）
- 只生成 1 个食物
- 固定食物位置在蛇前方

## 开发注意事项

### 添加新功能时

1. **数据模型**：先在 `Models/` 中定义或修改
2. **游戏逻辑**：在 `GameEngine.cs` 或 `LevelManager.cs` 中实现
3. **渲染**：在 `GamePanel.cs` 中添加绘制逻辑
4. **UI 更新**：在 `InfoPanel.cs` 或 `MainForm.cs` 中更新显示
5. **持久化**：如需保存，在对应的 `Service` 类中实现

### 修改游戏循环时

- 游戏循环在 `MainForm.OnGameTick()` 中
- 使用 `System.Windows.Forms.Timer` 定时触发
- 动态障碍物每帧更新
- 关卡计时器单独使用 1 秒间隔的 Timer

### 调试关卡问题

- 使用 Level1.json 底部的测试配置
- 修改后重新构建项目
- 检查 `GameEngine.SpawnFood()` 中的固定食物逻辑

### 已知问题和解决方案

| 问题 | 原因 | 解决方案 |
|------|------|----------|
| 蛇头进入墙内 | 碰撞检测在移动后执行 | 使用 `WillCollide()` 在移动前检查 |
| 重复弹出关卡完成窗口 | 事件订阅导致循环调用 | 不订阅 `LevelCompleted` 事件，使用标志位 |
| 固定食物不显示 | `foodCount=0` 导致提前退出循环 | 将固定位置检查移到循环前 |

## 扩展方向

- 实现关卡编辑器（`Forms/LevelEditorForm.cs` 等文件已创建）
- 添加更多特殊效果障碍物
- 在线排行榜
- 关卡分享功能
- 主题系统（更换颜色方案）
