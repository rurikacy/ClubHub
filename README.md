<div align="center">
  <img src="assets/clubhub.svg" alt="ClubHub 图标" width="96" height="96" />
  <h1>ClubHub — 校园社团管理系统</h1>
  <p>社团、成员与活动管理 · 实时讨论 · 问卷调查</p>
  <p><img alt="ASP.NET Core" src="https://img.shields.io/badge/ASP.NET_Core-8.0-512BD4" /> <img alt="SQL Server" src="https://img.shields.io/badge/Database-SQL_Server-CC2927" /> <img alt="License" src="https://img.shields.io/badge/License-GPL--3.0-blue" /></p>
  <p><a href="README_EN.md">English</a> · <a href="#-快速开始">快速开始</a> · <a href="SUPPORT.md">使用支持</a></p>
</div>

---

## 📖 简介

**ClubHub** 是基于 ASP.NET Core MVC 的校园社团管理项目，提供社团、成员、活动和消息管理，并通过 SignalR 支持社团内实时讨论。界面主要使用简体中文。项目适合学习和二次开发；现有实现需要安全加固后才适合公开部署。

## ✨ 功能特性

| 功能 | 说明 |
| --- | --- |
| 用户与权限 | 超级管理员、社团管理员、成员及游客入口。 |
| 社团与成员 | 社团信息、部门、成员及入社申请管理。 |
| 活动 | 活动发布、报名与签到；后台检查人数不足的活动并自动取消。 |
| 讨论与通知 | SignalR 社团讨论、消息及公告。 |
| 问卷 | 问卷、题目、作答与统计。 |

## 🚀 快速开始

需要 .NET 8 SDK 和 SQL Server（数据库导出来自 SQL Server 2022）。类库仍以 `netcoreapp3.1` 为目标，依赖较旧。以下步骤适用于隔离的本地学习环境。

1. 获取源码，进入项目根目录。
2. 在 SQL Server 创建一个空数据库 `club_hub`，使用支持 `GO` 的工具（如 SSMS 或 sqlcmd）执行 [database/schema.sql](database/schema.sql)。**脚本会删除同名表，切勿在已有数据库上执行。** 脚本保留菜单数据，不包含原始业务数据或登录账号。
3. 配置本地数据库连接并运行：

   ```powershell
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Data Source=.;Initial Catalog=club_hub;Integrated Security=True" --project ClubHub/ClubHub.csproj
   dotnet restore ClubHub.sln
   dotnet run --project ClubHub/ClubHub.csproj
   ```

4. 打开启动输出中的地址（项目启动配置为 `https://localhost:5001`）。可通过游客入口浏览；账号登录需要自行在数据库中配置 `T_Admin` 或 `T_Member` 记录，参照表结构及 `AuthRepository`。该初始化流程尚未提供自动化工具。

## 🗂️ 项目结构

```text
ClubHub/          ASP.NET Core MVC 应用、视图和静态资源
App/              会话辅助代码
Repository/       实体、仓储接口及实现
Infrastructure/   数据访问基础设施
database/         不含业务记录的 SQL Server 结构及菜单数据
assets/           项目图标
```

## ⚠️ 安全与限制

- 当前认证直接比较和保存明文密码，部分管理界面也会显示密码；不要使用真实密码或真实个人数据。公开部署前须重新设计密码存储、认证和授权并进行安全审计。
- 原数据库导出包含账号及业务记录，未纳入公开文件。结构脚本来自该导出，尚未在全新 SQL Server 上验证；部分代码引用的表未出现在导出中，相关功能可能不可用。
- 旧依赖存在已知安全问题；Dockerfile 使用 .NET Core 3.1 镜像，与 Web 项目的 .NET 8 目标不匹配，当前不作为可用部署方案。
- 本项目包含第三方前端资源和依赖，它们各自的许可证仍然适用。

## 🤝 参与与反馈

请阅读 [贡献指南](CONTRIBUTING.md)、[行为准则](CODE_OF_CONDUCT.md) 和 [安全说明](SECURITY.md)。提交问题时请删除密码、个人信息和数据库记录。

## 📄 许可证

现有 [LICENSE](LICENSE) 为 GNU GPL v3.0。第三方资源及依赖保留各自的许可条款；发布前仍需核对其来源与分发要求。

原 README 署名为 QinT6o，并链接至 [GitHub](https://github.com/Qin3Yu) 和 [Gitee](https://gitee.com/qint6o)。
