# RedRunner 项目结构说明

## 项目定位

RedRunner 是一个 Unity 2D 平台跑酷游戏。玩家控制红色角色在平台之间移动、跳跃、收集金币，并通过 GameManager 管理游戏状态和分数。

## 核心模块

### GameManager

路径：`Assets/Scripts/RedRunner/GameManager.cs`

职责：

- 管理游戏开始、暂停、继续、结束
- 维护当前分数、最高分、上一局分数
- 维护金币数量
- 通过事件通知其他模块重置或刷新 UI

关键方法：

- `StartGame()`：开始游戏，并恢复运行
- `StopGame()`：暂停游戏，设置 `Time.timeScale = 0`
- `ResumeGame()`：继续游戏，设置 `Time.timeScale = 1`
- `EndGame()`：结束游戏
- `Reset()`：重置分数，并触发 `OnReset` 事件

### RedCharacter

路径：`Assets/Scripts/RedRunner/Characters/RedCharacter.cs`

职责：

- 处理角色移动
- 处理角色跳跃
- 处理角色死亡
- 响应 GameManager 的重置事件

关键逻辑：

- `Update()` 每帧检测游戏状态和玩家输入
- `Move()` 通过修改 `Rigidbody2D.linearVelocity.x` 实现左右移动
- `Jump()` 通过修改 `Rigidbody2D.linearVelocity.y` 实现跳跃
- 角色掉到 y 坐标 0 以下会调用死亡逻辑

### Coin

路径：`Assets/Scripts/RedRunner/Collectables/Coin.cs`

职责：

- 检测角色碰撞
- 增加金币数量
- 播放金币收集动画、粒子和音效
- 收集后回收到对象池，或在没有对象池时销毁

已修复问题：

- 修复金币收集时 `ObjectPool` 引用为空导致的 `NullReferenceException`
- 当对象池不存在时，使用 `Destroy(gameObject, duration)` 作为兜底逻辑

### TerrainGenerator

职责：

- 生成跑酷平台
- 提供角色复活位置
- 配合 GameManager 的 `RespawnCharacter()` 完成角色重生

## 当前理解

这个项目的核心流程是：

```text
玩家输入
→ RedCharacter 处理移动/跳跃
→ Coin 检测收集并更新金币
→ GameManager 管理游戏状态、分数、重置
→ UI 根据 GameManager 的数据进行显示