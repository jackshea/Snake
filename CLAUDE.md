# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 项目概述

这是一个基于 .NET 8.0 的 Windows 窗体贪吃蛇游戏项目，使用 WinForms 作为渲染框架，不依赖 Unity 或其他游戏引擎。

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

### 游戏核心组件

- **SnakeGame.cs** - 主游戏类，包含游戏循环、状态管理和渲染逻辑
- **Program.cs** - 应用程序入口点

### 游戏逻辑设计

- **网格系统**: 游戏区域划分为固定大小的网格（例如 20x20 像素的单元格）
- **蛇的表示**: 使用 `List<Point>` 存储蛇身各段的坐标
- **食物系统**: 随机生成食物位置，确保不与蛇身重叠
- **游戏循环**: 使用 `System.Windows.Forms.Timer` 控制游戏速度（默认 100ms 间隔）
- **碰撞检测**: 检测蛇头与墙壁、蛇身、食物的碰撞

### 渲染方式

- 使用 `Paint` 事件进行双缓冲绘制
- 通过 `Graphics` 对象绘制网格、蛇身和食物
- 背景色: 黑色，蛇头: 绿色，蛇身: 深绿色，食物: 红色

### 输入处理

- 监听 `KeyDown` 事件处理方向键输入
- 防止蛇反向移动（如向右移动时不能直接向左）

### 游戏状态

- **Running**: 游戏进行中
- **GameOver**: 游戏结束（碰撞发生）
- 分数统计：每吃一个食物加 10 分
