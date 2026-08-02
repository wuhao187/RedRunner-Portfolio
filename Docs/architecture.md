# RedRunner 项目结构说明

## 项目定位

RedRunner 是一个 Unity 2D 平台跑酷游戏。玩家控制红色角色在平台之间移动、跳跃、收集金币，并通过 GameManager 管理游戏状态、分数和重置流程。

本仓库基于开源项目 RedRunner 进行学习型二次开发，目标是将其整理为游戏客户端应届生作品集项目。当前改造重点包括：阅读项目结构、修复运行时问题、新增 Dash 冲刺玩法，并持续补充开发文档。

## 核心模块

### GameManager

路径：`Assets/Scripts/RedRunner/GameManager.cs`

职责：

- 管理游戏开始、暂停、继续、结束
- 维护当前分数、最高分、上一局分数
- 维护金币数量
- 通过事件通知其他模块重置或刷新 UI

关键方法：

- `StartGame()`：开始游戏，并恢复运行状态
- `StopGame()`：暂停游戏，设置 `Time.timeScale = 0`
- `ResumeGame()`：继续游戏，设置 `Time.timeScale = 1`
- `EndGame()`：结束游戏，并停止运行
- `Reset()`：重置分数，并触发 `OnReset` 事件
- `RespawnCharacter()`：根据地形生成器提供的位置复活角色

理解说明：

`GameManager` 是项目里的全局管理器。其他脚本可以通过 `GameManager.Singleton` 访问它，从而获取游戏是否开始、是否运行、金币数量和分数等状态。

### RedCharacter

路径：`Assets/Scripts/RedRunner/Characters/RedCharacter.cs`

职责：

- 处理角色移动
- 处理角色跳跃
- 处理角色死亡
- 处理新增 Dash 冲刺
- 响应 GameManager 的重置事件

关键逻辑：

- `Update()`：每帧检测游戏状态和玩家输入
- `Move()`：通过修改 `Rigidbody2D.linearVelocity.x` 实现左右移动
- `Jump()`：通过修改 `Rigidbody2D.linearVelocity.y` 实现跳跃
- `Dash()`：根据当前面向方向触发短距离快速冲刺
- `Die()`：角色死亡时切换死亡状态并触发相关表现
- `Reset()`：重置角色状态、速度、死亡状态和骨骼显示

理解说明：

这个项目的角色控制是 Rigidbody2D 速度驱动型。也就是说，角色不是直接修改坐标瞬移，而是通过修改刚体速度来完成移动、跳跃和冲刺。

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

修复前问题：

```text
角色碰到金币
→ Coin.Collect()
→ Coin.ReturnToPool()
→ m_objectPool 为空
→ NullReferenceException
```

修复后逻辑：

```text
如果存在 ObjectPool
→ 回收到对象池

如果 ObjectPool 为空
→ 等待粒子效果播放后销毁金币对象
```

### TerrainGenerator

职责：

- 生成跑酷平台
- 提供角色复活位置
- 配合 GameManager 的 `RespawnCharacter()` 完成角色重生

理解说明：

`TerrainGenerator` 负责生成关卡地形。角色死亡或重置时，GameManager 可以通过它找到合适的平台位置，让角色重新生成在平台上方。

## 已新增玩法：Dash 冲刺

### 功能说明

角色现在可以通过左 Shift 触发 Dash 冲刺，朝当前面向方向进行短距离快速位移。这个能力可以用于跨越平台间隙、快速调整站位，也为后续扩展冷却 UI、特效和音效留下空间。

### 技术实现

Dash 功能添加在 `RedCharacter.cs` 中，核心逻辑包括：

- 使用 `Input.GetKeyDown(KeyCode.LeftShift)` 检测冲刺输入
- 使用 `m_DashSpeed` 控制冲刺速度
- 使用 `m_DashDuration` 控制冲刺持续时间
- 使用 `m_DashCooldown` 控制冲刺冷却
- 使用 `m_IsDashing` 防止冲刺期间被普通移动逻辑覆盖
- 通过 `Rigidbody2D.linearVelocity.x` 实现横向冲刺
- 使用协程 `DashRoutine()` 控制冲刺开始和结束

### 当前参数

| 参数 | 当前值 | 作用 |
| --- | ---: | --- |
| `m_DashSpeed` | `14` | 冲刺时的横向速度 |
| `m_DashDuration` | `0.15` | 冲刺持续时间 |
| `m_DashCooldown` | `1` | 冲刺冷却时间 |

### 当前实现流程

```text
玩家按下左 Shift
→ 判断角色是否死亡
→ 判断是否正在冲刺
→ 判断冷却是否结束
→ 根据角色当前朝向计算冲刺方向
→ 设置 Rigidbody2D.linearVelocity.x
→ 等待 Dash Duration
→ 结束冲刺状态
```

### 后续优化

- 增加冲刺特效
- 增加冷却 UI 提示
- 增加冲刺音效
- 调整冲刺手感和关卡适配
- 支持手柄或移动端按钮输入

## 当前项目流程理解

这个项目的核心流程是：

```text
玩家输入
→ RedCharacter 处理移动、跳跃、冲刺
→ Coin 检测收集并更新金币
→ GameManager 管理游戏状态、分数、重置
→ UI 根据 GameManager 的数据进行显示
→ TerrainGenerator 提供地图和复活位置
```

## 已完成改造记录

### 1. 跑通原版项目

完成项目 Fork、Clone、本地导入 Unity，并使用 Unity 6000.2.6f2 成功运行原版游戏。

### 2. 阅读核心代码

重点阅读并理解以下脚本：

- `RedCharacter.cs`
- `Coin.cs`
- `GameManager.cs`
- `TerrainGenerator` 相关逻辑

### 3. 修复金币收集空引用问题

修复 `Coin.ReturnToPool()` 中对象池引用为空导致的 `NullReferenceException`。

### 4. 新增 Dash 冲刺玩法

在 `RedCharacter.cs` 中新增 Dash 冲刺能力，包括输入检测、冲刺速度、持续时间、冷却控制和移动覆盖保护。

### 5. 编写项目结构文档

整理 `Docs/architecture.md`，用于说明项目核心模块职责、已完成改造和后续优化方向。

## 后续计划

- 为 Dash 增加视觉反馈
- 为 Dash 增加冷却 UI
- 增加本地最高分保存
- 优化暂停和结算流程
- 整理 README，加入运行截图、操作说明和作品集说明
- 构建 WebGL 或 Windows 可运行版本