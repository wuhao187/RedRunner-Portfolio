# RedRunner 项目结构说明

## 项目定位

RedRunner 是一个 Unity 2D 平台跑酷游戏。玩家控制红色角色在平台之间移动、跳跃、收集金币，并通过 GameManager 管理游戏状态、分数和重置流程。

本仓库基于开源项目 RedRunner 进行学习型二次开发，目标是将其整理为游戏客户端应届生作品集项目。当前改造重点包括：阅读项目结构、修复运行时问题、新增 Dash 冲刺、二段跳、跳跃手感优化、动态难度、本地保存、数据 UI、成就提示和操作引导，并持续补充开发文档。

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
- 处理角色跳跃和跳跃手感容错
- 处理角色死亡
- 处理新增 Dash 冲刺
- 响应 GameManager 的重置事件

关键逻辑：

- `Update()`：每帧检测游戏状态和玩家输入
- `Move()`：通过修改 `Rigidbody2D.linearVelocity.x` 实现左右移动
- `Jump()`：记录跳跃输入，并通过 Coyote Time / Jump Buffer 提升跳跃手感
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


## 已新增玩法：Double Jump 二段跳

### 功能说明

角色现在支持二段跳。玩家在地面起跳后，可以在空中额外跳跃一次；空中跳用完后，必须重新落地才能恢复次数。

这个功能比单纯调高跳跃高度更适合作品集展示，因为它涉及角色状态管理、输入判断、落地刷新、动作反馈复用等客户端开发常见问题。

### 技术实现

二段跳功能添加在 `RedCharacter.cs` 中，核心逻辑包括：

- 使用 `m_MaxAirJumps` 控制最多允许的空中跳次数
- 使用 `m_AirJumpStrength` 控制二段跳力度
- 使用 `m_RemainingAirJumps` 记录当前剩余空中跳次数
- 每帧通过 `RefreshAirJumpCount()` 判断角色是否落地，落地后恢复空中跳次数
- `Jump()` 中先判断普通落地跳，再判断是否允许空中跳
- 使用 `PerformJump(float jumpStrength)` 复用跳跃速度、动画、粒子和音效逻辑
- `Reset()` 和 `GroundCheck_OnGrounded()` 中都会恢复空中跳次数，避免死亡重置或落地后状态不一致

### 当前参数

| 参数 | 当前值 | 作用 |
| --- | ---: | --- |
| `m_MaxAirJumps` | `1` | 每次离地后允许额外跳跃 1 次 |
| `m_AirJumpStrength` | `9` | 二段跳的向上速度 |

### 当前实现流程

```text
玩家按下跳跃键
→ 判断角色是否死亡
→ 如果角色在地面：执行普通跳跃
→ 如果角色在空中：检查剩余空中跳次数
→ 如果还有次数：消耗 1 次空中跳并执行二段跳
→ 如果次数为 0：不再跳跃
→ 角色落地后恢复空中跳次数
```

### 作品集价值

面试时可以这样解释：我没有直接做“无限跳”，而是给角色控制器增加了可配置的空中跳次数。这个设计让策划或开发者可以在 Inspector 里调整跳跃次数和力度，同时保持普通跳跃、二段跳、动画、粒子、音效的逻辑复用。

## 已新增手感优化：Coyote Time 与 Jump Buffer

### 功能说明

为了让平台跳跃更顺手，角色现在加入了两个常见的平台游戏手感优化：

- `Coyote Time`：角色刚离开平台后的极短时间内，仍然允许玩家起跳。
- `Jump Buffer`：玩家快落地前提前按下跳跃，落地后会自动触发跳跃。

这两个机制不会改变游戏规则本身，但会明显降低“我明明按了跳却没跳起来”的挫败感，属于游戏客户端里很典型的输入体验优化。

### 技术实现

手感优化添加在 `RedCharacter.cs` 中，核心逻辑包括：

- 使用 `m_CoyoteTime` 控制离地后仍可起跳的宽容时间
- 使用 `m_JumpBufferTime` 控制提前按跳的缓存时间
- 使用 `m_LastGroundedTime` 记录最近一次站在地面的时间
- 使用 `m_LastJumpPressedTime` 记录最近一次按下跳跃的时间
- 使用 `m_ConsumedCoyoteJump` 防止一次离地期间重复使用地面跳
- `Update()` 中先记录跳跃输入，再调用 `TryConsumeBufferedJump()` 判断是否能真正起跳

### 当前参数

| 参数 | 当前值 | 作用 |
| --- | ---: | --- |
| `m_CoyoteTime` | `0.12` | 离开平台后仍可跳跃的时间窗口 |
| `m_JumpBufferTime` | `0.12` | 落地前提前按跳的缓存时间窗口 |

### 当前实现流程

```text
玩家按下 Space
→ 记录最近跳跃输入时间
→ 每帧判断是否存在未过期的跳跃输入
→ 如果当前在地面：执行普通跳
→ 如果刚离开地面且仍在 Coyote Time 内：执行普通跳
→ 如果已经在空中且还有二段跳次数：执行二段跳
→ 如果都不满足：保留短时间输入缓存，等待落地
```

### 作品集展示价值

面试时可以这样解释：我在角色控制器中补充了 Coyote Time 和 Jump Buffer，让平台跳跃从“能用”变成“更顺手”。这个改动体现了我不只关注功能是否跑通，也关注输入响应、容错窗口和玩家手感。
## 已新增体验优化：操作提示 UI

### 功能说明

游戏现在会在进入游玩界面时短暂显示操作提示，告诉试玩者基础按键：

- `A / D`：左右移动
- `Space`：跳跃与二段跳
- `Left Shift`：Dash 冲刺

提示会在显示数秒后自动淡出，避免长期遮挡游戏画面。

### 技术实现

操作提示由运行时脚本自动创建，不需要手动修改场景层级：

- `UIControlHintBootstrap`：进入场景后查找 `In-Game Screen`，自动创建提示文本
- `UIControlHintText`：控制提示显示时间和淡出效果
- `UIScreenVisibilityFollower`：保证提示只在游戏内界面显示，不会出现在开始页或结束页

### 作品集价值

这个改动体现了基础的玩家引导意识。面试时可以说明：新增玩法后，我补充了操作提示，降低试玩成本，并通过运行时 UI 创建避免手动污染场景。

## 已新增玩法系统：动态难度速度成长

### 功能说明

游戏现在会根据玩家跑出的距离逐步提高角色速度。前期速度较稳定，方便玩家适应；跑得越远，角色基础跑速和最高跑速会逐步上升，让后期节奏更紧张。

### 技术实现

动态难度逻辑添加在 `RedCharacter.cs` 中，通过监听 `GameManager.OnScoreChanged` 获得当前距离，并使用 `Extensions.modifier` 将内部 score 转换为玩家看到的米数，再根据米数计算成长进度：

- `m_DifficultyScalingEnabled`：是否启用速度成长
- `m_DifficultyFullScore`：达到完整成长的目标距离
- `m_DifficultyRunSpeedBonus`：基础跑速最多增加多少
- `m_DifficultyMaxRunSpeedBonus`：最高跑速最多增加多少
- `m_BaseRunSpeed` / `m_BaseMaxRunSpeed`：记录初始速度，重置游戏时恢复

### 当前参数

| 参数 | 当前值 | 作用 |
| --- | ---: | --- |
| `m_DifficultyFullScore` | `120` | 跑到约 120 米时达到完整速度成长 |
| `m_DifficultyRunSpeedBonus` | `1.2` | 基础跑速最多提高 1.2 |
| `m_DifficultyMaxRunSpeedBonus` | `2` | 最高跑速最多提高 2 |

### 当前实现流程

```text
GameManager 更新分数
→ 触发 OnScoreChanged
→ RedCharacter 接收当前分数
→ 根据 当前分数 / DifficultyFullScore 计算成长进度
→ 提高 RunSpeed 和 MaxRunSpeed
→ 游戏重置时恢复到初始速度
```

### 作品集价值

这个功能体现了基础数值设计和玩法节奏控制能力。面试时可以说明：我把速度成长做成可配置参数，而不是写死数值，这样后续可以继续调整难度曲线和关卡适配。

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


## 已优化系统：开始页与结束页数据展示

### 功能说明

项目现在会在菜单流程中展示玩家关键数据，让本地保存系统不只是后台生效，也能被玩家直接看到。

当前展示内容包括：

- 开始页：显示历史最高分和当前累计金币
- 结束页：显示本局距离、历史最高分和当前累计金币
- 游戏内：显示 Dash 冷却状态和当前金币数量

### 技术实现

新增 `UIPlayerStatsText.cs`，用于统一刷新开始页和结束页的数据文本。它会读取 `GameManager` 中的分数和金币数据，并监听以下变化：

- `GameManager.OnScoreChanged`：分数、最高分、上一局分数变化时刷新
- `GameManager.m_Coin`：金币数量变化时刷新

`GameManager.cs` 新增只读属性，供 UI 层读取当前数据：

- `CurrentScore`：当前本局分数
- `HighScore`：历史最高分
- `LastScore`：上一局分数
- `CoinCount`：当前累计金币

### 设计说明

这个功能的重点不是单纯加几行文字，而是把“数据保存”和“界面展示”连起来。玩家能在开始游戏前看到历史积累，在死亡结算后看到本局表现，这更接近真实游戏客户端中的数据闭环。

作品集讲法可以概括为：

```text
我为原项目补充了本地数据持久化，并在开始页、游戏页、结束页建立 UI 展示链路。
数据由 GameManager 统一管理，UI 通过事件订阅刷新，避免每帧硬查找。
```

## 已新增系统：成就与里程碑提示

### 功能说明

项目现在新增了运行时成就提示系统。玩家在达成特定目标时，屏幕上方会弹出 `Achievement Unlocked` 提示，让游戏反馈更完整。

当前成就包括：

- `First Coin`：累计金币达到 1
- `Coin Collector x10`：累计金币达到 10
- `Treasure Keeper x50`：累计金币达到 50
- `Runner 10 m`：本局距离达到 10 米
- `Runner 50 m`：本局距离达到 50 米
- `Runner 100 m`：本局距离达到 100 米

### 技术实现

成就系统目前写在 `UIPlayerStatsText.cs` 中，核心类包括：

- `UIAchievementSystem`：监听 `GameManager.OnScoreChanged` 和 `GameManager.m_Coin`，判断是否达成成就
- `UIAchievementToast`：负责成就提示的排队、显示、停留和淡出
- `UIAchievementBootstrap`：运行时自动创建成就提示 UI，不需要手动拖拽场景对象

成就解锁状态使用 `PlayerPrefs` 保存，避免同一个成就每次启动游戏都重复弹出。点击暂停页的 `Reset Save` 会同时清除金币、分数、设置和成就记录。

### 作品集价值

这个功能展示了游戏客户端中常见的事件驱动 UI：游戏数据由 `GameManager` 管理，成就系统监听数据变化，UI 只在需要时弹出反馈。它能体现对数据流、用户反馈和本地持久化的理解。

## 已新增体验反馈：速度等级提示 UI

### 功能说明

动态难度会让角色随着奔跑距离增加逐步变快。为了让试玩者和面试官更容易感知这个变化，项目新增了速度等级提示：当玩家跑到一定距离并触发速度成长阶段时，游戏会短暂弹出类似 `Speed Up Lv.2` 的提示。

当前速度等级划分：

| 等级 | 触发距离 | 说明 |
| --- | ---: | --- |
| Lv.1 | 0 m | 初始速度 |
| Lv.2 | 30 m | 进入第一档速度成长 |
| Lv.3 | 70 m | 进入第二档速度成长 |
| Lv.4 | 120 m | 达到当前完整速度成长 |

### 技术实现

速度等级提示目前写在 `UIPlayerStatsText.cs` 中，并且与动态难度一样使用玩家看到的米数判断等级，核心类包括：

- `UISpeedLevelText`：监听 `GameManager.OnScoreChanged`，根据当前分数判断速度等级，并控制提示显示和淡出
- `UISpeedLevelBootstrap`：运行时自动在 `In-Game Screen` 下创建提示文本，不需要手动拖拽场景对象

这个 UI 只负责展示，不直接修改角色速度。实际速度成长仍由 `RedCharacter.cs` 中的动态难度逻辑负责。

### 作品集价值

这个改动体现了“玩法系统 + 玩家反馈”的完整思路：不仅实现角色速度随距离成长，还为玩家补充了可见反馈。面试时可以说明：我把动态难度拆成了角色控制层和 UI 表现层，角色负责数值变化，UI 通过事件监听展示阶段变化。

## 已新增体验优化：键盘快捷操作

### 功能说明

为了让试玩流程更顺畅，项目新增了键盘快捷操作：

- `Esc`：暂停或继续游戏
- `R`：重新开始当前局

`Esc` 原项目已经有暂停切换逻辑，本次重点补充了 `R` 键重开流程，并将操作提示 UI 同步更新为 `A/D Move    Space Jump x2    Left Shift Dash    Esc Pause    R Restart`。

### 技术实现

快捷键逻辑添加在 `UIManager.cs` 中：

- `Update()` 中检测 `Input.GetKeyDown(KeyCode.R)`
- 新增 `RestartCurrentRun()` 方法，统一执行重开流程
- 结算页 `EndScreen` 的重开按钮也改为调用 `UIManager.Singleton.RestartCurrentRun()`，避免按钮和快捷键各写一套逻辑

重开流程为：

```text
玩家按 R 或点击结算页重开按钮
→ GameManager.Reset()
→ UIManager.OpenScreen(IN_GAME_SCREEN)
→ GameManager.StartGame()
```

### 作品集价值

这个改动体现了对玩家试玩体验和代码复用的关注。面试时可以说明：我没有只给按钮加功能，而是抽出统一的重开方法，让鼠标按钮和键盘快捷键走同一条流程，减少后续维护风险。
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
- 新增暂停界面的清除本地存档按钮
- 优化开始页和结束页的最高分、金币、本局分数展示
- 新增成就与里程碑提示系统
- 优化暂停和结算流程
- 整理 README，加入运行截图、操作说明和作品集说明
- 构建 WebGL 或 Windows 可运行版本
### 7. 新增清除本地存档按钮

在暂停界面新增 `Reset Save` 按钮，点击后会调用 `GameManager.ClearLocalData()` 清除本地金币、最高分、上次分数和声音设置，并刷新相关 UI。

这个功能让本地保存系统形成完整闭环：不仅能保存和读取数据，也能让玩家主动重置本地进度。对于作品集展示来说，它体现了数据管理、UI 交互和状态刷新之间的协作。









