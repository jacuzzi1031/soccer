

# Unity 2D 足球游戏

基于 Unity 开发的2D 足球游戏，支持双人实时对战，采用确定性帧同步（Deterministic Lockstep）同步方案，保证多客户端演算一致。

SimulationWorld 作为核心驱动，以固定 Tick 推进 MatchSystem、PlayerSystem、PlayerSim、BallSim 等演算系统。

模拟层采用 Fixed Point 定点数学、自研物理系统以及 CommandBuffer/EventBus 状态管理机制，避免浮点误差和逻辑耦合，保证多端演算一致。

 Unity 表现层根据模拟状态进行插值渲染和动画更新，实现逻辑模拟与视觉表现分离。

网络层使用 TCP 处理连接与房间管理，UDP 同步帧输入，并结合 Checksum、Snapshot、Rollback 机制处理状态校验与异常恢复。


## 技术栈

Unity / C# / TCP UDP Socket / Protobuf / Fixed Point / Deterministic Lockstep


## 游戏操作

默认键位如下：

| 按键          | 功能                       |
| ------------- | -------------------------- |
| W / A / S / D | 移动                       |
| Q             | 直塞                       |
| E             | 短传                       |
| 4             | 长传 / 铲球                |
| X             | 切换球员                   |
| 空格          | 射门 / 大力射门 / 凌空射门 |


## 素材来源

部分足球素材来源：

https://github.com/nicolasbize/soccer-course-assets

Copyright (c) 2025 Nicolas Bize

Licensed under the MIT License.     
