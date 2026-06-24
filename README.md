## Unity 2D 足球游戏

基于 Unity 开发的多人在线足球游戏。玩家可选择不同国家队进行比赛，玩法参考 EA FC / FIFA Ultimate Team（UT）模式。

比赛流程：
等待开球 → 正赛阶段 → 进球庆祝 → 球员自动回位 → 等待开球

游戏实现了球员行为状态机与 AI 系统，支持移动、传球、抢断、停球、凌空射门、庆祝等比赛行为。

项目采用 TCP + UDP + Protobuf 实现网络通信，并基于确定性模拟实现实时帧同步。



素材参考：

https://github.com/nicolasbize/soccer-course-assets

Copyright (c) 2025 Nicolas Bize

Licensed under the MIT License.