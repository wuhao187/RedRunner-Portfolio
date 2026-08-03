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

角色现在可以通过左 Shift 触发 Dash 冲刺，朝当前面向方向进行短距离快速位移。这个能力可以用于跨越平台间隙、快速调整站位。当前版本已经加入冲刺时的角色拉伸、冷却 UI、粒子特效和音效反馈，让玩家能更明显地感受到冲刺动作。

### 技术实现

Dash 功能添加在 `RedCharacter.cs` 中，核心逻辑包括：

- 使用 `Input.GetKeyDown(KeyCode.LeftShift)` 检测冲刺输入
- 使用 `m_DashSpeed` 控制冲刺速度
- 使用 `m_DashDuration` 控制冲刺持续时间
- 使用 `m_DashCooldown` 控制冲刺冷却
- 使用 `m_IsDashing` 防止冲刺期间被普通移动逻辑覆盖
- 通过 `Rigidbody2D.linearVelocity.x` 实现横向冲刺
- 使用协程 `DashRoutine()` 控制冲刺开始和结束
- 使用 `m_DashStretchX` 和 `m_DashStretchY` 控制冲刺时的角色拉伸反馈

### 当前参数

| 参数 | 当前值 | 作用 |
| --- | ---: | --- |
| `m_DashSpeed` | `14` | 冲刺时的横向速度 |
| `m_DashDuration` | `0.15` | 冲刺持续时间 |
| `m_DashCooldown` | `1` | 冲刺冷却时间 |
| `m_DashStretchX` | `1.25` | 冲刺时角色横向拉伸比例 |
| `m_DashStretchY` | `0.85` | 冲刺时角色纵向压缩比例 |

### 当前实现流程

```text
玩家按下左 Shift
→ 判断角色是否死亡
→ 判断是否正在冲刺
→ 判断冷却是否结束
→ 根据角色当前朝向计算冲刺方向
→ 设置角色临时拉伸比例
→ 播放 Dash 粒子特效和音效
→ 设置 Rigidbody2D.linearVelocity.x
→ 等待 Dash Duration
→ 恢复角色原始缩放
→ 结束冲刺状态
```

### 已完成反馈

- 已增加冲刺时的角色横向拉伸和纵向压缩反馈
- 冲刺结束后会恢复角色原始缩放
- 已新增 UIDashCooldownText.cs，用于在游戏界面显示 Dash 是否可用或剩余冷却时间
- 已新增 Dash Particle System，用于在冲刺瞬间提供视觉反馈
- 已新增 Dash.wav，并通过 AudioManager 播放冲刺音效

### 冷却 UI 说明

UIDashCooldownText.cs 负责读取 RedCharacter 的 Dash 状态，并在游戏中的 In-Game Screen 显示提示：

- Dash 可用时显示 Dash Ready
- Dash 冷却中显示剩余秒数，例如 Dash 0.7s
- Dash UI 会根据状态切换颜色：可用时显示绿色，冷却中显示黄色
- UI 不直接修改角色移动逻辑，只读取角色暴露出来的状态

这样做的好处是角色控制和界面显示分离：RedCharacter 负责玩法，UIDashCooldownText 负责展示。

### 后续优化

- 后续可继续增加粒子特效
- 增加冲刺音效
- 调整冲刺手感和关卡适配
- 支持手柄或移动端按钮输入


## 已新增系统：本地数据保存

### 功能说明

项目现在使用 Unity 内置的 `PlayerPrefs` 保存轻量级本地数据，让玩家退出游戏后仍然能保留基础进度和设置。

当前保存内容包括：

- 金币数量：收集金币后立即保存
- 声音开关：切换音量后立即保存
- 上一局分数：角色死亡或退出游戏时保存
- 最高分：角色死亡或退出游戏时比较并保存

### 技术实现

本地保存逻辑集中在 `GameManager.cs` 中：

- `LoadLocalData()`：游戏启动时从本地读取金币、声音开关、上一局分数和最高分
- `SaveLocalData()`：将当前数据写入 `PlayerPrefs` 并调用 `PlayerPrefs.Save()` 持久化
- `SetAudioEnabled()`：切换声音状态后保存设置
- `DeathCrt()`：角色死亡后保存本局分数和最高分
- `OnApplicationQuit()`：退出游戏时兜底保存当前数据

金币收集逻辑在 `Coin.cs` 中调用 `GameManager.Singleton.SaveLocalData()`，保证金币变化后及时保存。

### 作品集价值

这个功能展示了游戏客户端常见的数据持久化能力：不是只会做角色移动，还能处理玩家进度、设置保存和 UI 数据刷新。对于应届生作品集来说，这是一个容易讲清楚、也比较贴近真实项目的小系统。

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

### 6. 完善 Dash 反馈系统

为 Dash 增加冷却 UI、颜色状态、粒子特效和自定义音效，让技能从“能用”变成“有手感、有反馈、能展示”的作品集功能。
## 后续计划

- 已为 Dash 增加角色拉伸视觉反馈
- 已为 Dash 增加冷却 UI 和颜色状态反馈
- 已为 Dash 增加粒子特效和自定义冲刺音效
- 已增加本地最高分、金币和声音设置保存
- 优化暂停和结算流程
- 整理 README，加入运行截图、操作说明和作品集说明
- 构建 WebGL 或 Windows 可运行版本
