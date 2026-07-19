

# Unity 2D 足球游戏

基于 Unity 开发的多人在线 2D 足球游戏，通过 SimulationWorld 驱动 MatchSystem、PlayerSystem、BallSim 等 SimulationSystem 执行固定 Tick 演算，其中 PlayerSystem 管理 List<PlayerSim> 球员状态，BallSim 负责足球状态模拟。基于 Fixed Point 数学框架、自定义物理系统以及 CommandBuffer / EventBus 管理确定性状态更新；Unity 表现层通过 View 注入模拟状态进行渲染。网络层通过 TCP / UDP 同步输入，并结合 Checksum 校验、Snapshot / Rollback 恢复机制保证客户端游戏状态一致。


## 技术栈

Unity / C# / Socket TCP UDP / Protobuf / Fixed Point / Deterministic Lockstep


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
