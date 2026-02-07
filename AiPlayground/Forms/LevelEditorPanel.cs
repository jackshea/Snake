using System;
using System.Drawing;
using System.Windows.Forms;
using AiPlayground.Controls;
using AiPlayground.Models;
using AiPlayground.Models.Obstacles;

namespace AiPlayground.Forms;

/// <summary>
/// 关卡编辑器绘图面板
/// </summary>
public class LevelEditorPanel : DoubleBufferPanel
{
    private Level _level;
    private ObstacleType _selectedTool = ObstacleType.Static;
    private object? _selectedObject;
    private Point _hoverPosition = Point.Empty;
    private float _zoom = 1.0f;

    public event Action<object?>? SelectionChanged;
    public event Action? ObstacleAdded;
    public event Action? ObstacleRemoved;

    public ObstacleType SelectedTool
    {
        get => _selectedTool;
        set => _selectedTool = value;
    }

    public float Zoom
    {
        get => _zoom;
        set => _zoom = Math.Max(0.5f, Math.Min(3.0f, value));
    }

    public LevelEditorPanel(Level level)
    {
        _level = level;
        BackColor = Color.Black;
        DoubleBuffered = true;
    }

    public void SetLevel(Level level)
    {
        _level = level;
        _selectedObject = null;
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        Graphics g = e.Graphics;
        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
        g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
        g.ScaleTransform(_zoom, _zoom);

        DrawGrid(g);
        DrawObstacles(g);
        DrawStartAndFood(g);
        DrawHoverPreview(g);
    }

    private void DrawGrid(Graphics g)
    {
        using var gridPen = new Pen(Color.FromArgb(30, Color.Gray), 1);
        int cellSize = 20;

        for (int x = 0; x <= _level.GridWidth; x++)
        {
            g.DrawLine(gridPen, x * cellSize, 0, x * cellSize, _level.GridHeight * cellSize);
        }

        for (int y = 0; y <= _level.GridHeight; y++)
        {
            g.DrawLine(gridPen, 0, y * cellSize, _level.GridWidth * cellSize, y * cellSize);
        }
    }

    private void DrawObstacles(Graphics g)
    {
        int cellSize = 20;

        foreach (var obstacle in _level.Obstacles)
        {
            int x = obstacle.Position.X * cellSize;
            int y = obstacle.Position.Y * cellSize;
            bool isSelected = ReferenceEquals(obstacle, _selectedObject);

            Color color = obstacle.Type switch
            {
                ObstacleType.Static => Color.FromArgb(100, 100, 100),
                ObstacleType.Destructible => Color.FromArgb(139, 69, 19),
                ObstacleType.Dynamic => Color.FromArgb(128, 0, 128),
                ObstacleType.SpeedUp => Color.FromArgb(0, 200, 255),
                ObstacleType.SpeedDown => Color.FromArgb(100, 100, 255),
                ObstacleType.ScoreMultiplier => Color.FromArgb(255, 215, 0),
                ObstacleType.Teleport => Color.FromArgb(148, 0, 211),
                _ => Color.Gray
            };

            using var brush = new SolidBrush(color);
            g.FillRectangle(brush, x + 1, y + 1, cellSize - 2, cellSize - 2);

            using var pen = new Pen(isSelected ? Color.Yellow : Color.White, isSelected ? 3 : 1);
            g.DrawRectangle(pen, x + 1, y + 1, cellSize - 2, cellSize - 2);
        }
    }

    private void DrawStartAndFood(Graphics g)
    {
        int cellSize = 20;

        // 起点
        int startX = _level.Settings.SnakeStartPosition.X * cellSize;
        int startY = _level.Settings.SnakeStartPosition.Y * cellSize;
        using (var brush = new SolidBrush(Color.Lime))
        {
            g.FillRectangle(brush, startX + 2, startY + 2, cellSize - 4, cellSize - 4);
        }
        using (var pen = new Pen(Color.White, 2))
        {
            g.DrawRectangle(pen, startX + 2, startY + 2, cellSize - 4, cellSize - 4);
        }

        // 初始方向箭头
        using (var arrowPen = new Pen(Color.White, 2))
        {
            var arrowStart = new Point(startX + cellSize / 2, startY + cellSize / 2);
            var arrowEnd = new Point(
                arrowStart.X + _level.Settings.InitialDirection.X * 8,
                arrowStart.Y + _level.Settings.InitialDirection.Y * 8
            );
            g.DrawLine(arrowPen, arrowStart, arrowEnd);
        }
    }

    private void DrawHoverPreview(Graphics g)
    {
        if (_hoverPosition.X < 0 || _hoverPosition.X >= _level.GridWidth ||
            _hoverPosition.Y < 0 || _hoverPosition.Y >= _level.GridHeight)
        {
            return;
        }

        int cellSize = 20;
        int x = _hoverPosition.X * cellSize;
        int y = _hoverPosition.Y * cellSize;

        using var brush = new SolidBrush(Color.FromArgb(100, Color.White));
        g.FillRectangle(brush, x + 1, y + 1, cellSize - 2, cellSize - 2);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        int cellSize = 20;
        int gridX = (int)(e.X / _zoom / cellSize);
        int gridY = (int)(e.Y / _zoom / cellSize);

        if (gridX != _hoverPosition.X || gridY != _hoverPosition.Y)
        {
            _hoverPosition = new Point(gridX, gridY);
            Invalidate();
        }
    }

    protected override void OnMouseClick(MouseEventArgs e)
    {
        base.OnMouseClick(e);

        int cellSize = 20;
        int gridX = (int)(e.X / _zoom / cellSize);
        int gridY = (int)(e.Y / _zoom / cellSize);

        if (gridX < 0 || gridX >= _level.GridWidth || gridY < 0 || gridY >= _level.GridHeight)
        {
            return;
        }

        var position = new Point(gridX, gridY);

        if (e.Button == MouseButtons.Left)
        {
            HandleLeftClick(position);
        }
        else if (e.Button == MouseButtons.Right)
        {
            HandleRightClick(position);
        }

        Invalidate();
    }

    private void HandleLeftClick(Point position)
    {
        // 检查是否点击了已有对象
        var existingObstacle = _level.Obstacles.FirstOrDefault(o => o.Position == position);
        if (existingObstacle != null)
        {
            _selectedObject = existingObstacle;
            SelectionChanged?.Invoke(_selectedObject);
            return;
        }

        // 添加新障碍物
        Obstacle newObstacle = _selectedTool switch
        {
            ObstacleType.Static => new StaticObstacle(position),
            ObstacleType.Destructible => new DestructibleObstacle(position, 1),
            ObstacleType.Dynamic => new DynamicObstacle(position, new List<Point> { position }),
            ObstacleType.SpeedUp => SpecialEffectObstacle.CreateSpeedUp(position),
            ObstacleType.SpeedDown => SpecialEffectObstacle.CreateSpeedDown(position),
            ObstacleType.ScoreMultiplier => SpecialEffectObstacle.CreateScoreMultiplier(position),
            _ => new StaticObstacle(position)
        };

        _level.Obstacles.Add(newObstacle);
        _selectedObject = newObstacle;
        SelectionChanged?.Invoke(_selectedObject);
        ObstacleAdded?.Invoke();
    }

    private void HandleRightClick(Point position)
    {
        var obstacle = _level.Obstacles.FirstOrDefault(o => o.Position == position);
        if (obstacle != null)
        {
            _level.Obstacles.Remove(obstacle);
            if (ReferenceEquals(_selectedObject, obstacle))
            {
                _selectedObject = null;
                SelectionChanged?.Invoke(null);
            }
            ObstacleRemoved?.Invoke();
        }
    }
}
