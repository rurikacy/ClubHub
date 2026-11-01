<div align="center">
  <img src="assets/clubhub.svg" alt="ClubHub icon" width="96" height="96" />
  <h1>ClubHub — Campus Club Management</h1>
  <p>Clubs, members and activities · Live discussions · Surveys</p>
  <p><img alt="ASP.NET Core" src="https://img.shields.io/badge/ASP.NET_Core-8.0-512BD4" /> <img alt="SQL Server" src="https://img.shields.io/badge/Database-SQL_Server-CC2927" /> <img alt="License" src="https://img.shields.io/badge/License-GPL--3.0-blue" /></p>
  <p><a href="README.md">简体中文</a> · <a href="#-quick-start">Quick start</a> · <a href="SUPPORT.md">Support</a></p>
</div>

---

## 📖 Introduction

**ClubHub** is an ASP.NET Core MVC campus club management project for clubs, members, activities and announcements, with live club discussions using SignalR. The application interface is primarily in Simplified Chinese. It is intended for learning and further development and requires security work before public deployment.

## ✨ Features

| Feature | Description |
| --- | --- |
| Users and roles | Super administrators, club administrators, members and a guest entry point. |
| Clubs and members | Club information, departments, members and membership applications. |
| Activities | Publish activities, register and check in; a background service cancels activities with insufficient participants. |
| Discussions and notices | SignalR club discussions, messages and announcements. |
| Surveys | Questionnaires, questions, answers and statistics. |

## 🚀 Quick Start

Requires the .NET 8 SDK and SQL Server (the export was made with SQL Server 2022). Libraries still target `netcoreapp3.1` and use older dependencies. Use an isolated local learning environment.

1. Get the source and enter the project root.
2. Create an empty SQL Server database named `club_hub`. Run [database/schema.sql](database/schema.sql) using a tool that supports `GO`, such as SSMS or sqlcmd. **The script drops existing tables of the same names; never run it on an existing database.** It includes menu data but no original business records or login accounts.
3. Set a local connection string and run the project:

   ```powershell
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Data Source=.;Initial Catalog=club_hub;Integrated Security=True" --project ClubHub/ClubHub.csproj
   dotnet restore ClubHub.sln
   dotnet run --project ClubHub/ClubHub.csproj
   ```

4. Open the address printed at startup (the launch profile specifies `https://localhost:5001`). Guest access is available; account login requires manually creating a `T_Admin` or `T_Member` record using the schema and `AuthRepository` as references. Account initialization is not automated.

## 🗂️ Project Structure

```text
ClubHub/          ASP.NET Core MVC application, views and static assets
App/              Session helper
Repository/       Entities and repository interfaces and implementations
Infrastructure/   Data-access infrastructure
database/         SQL Server schema and menu data without business records
assets/           Project icon
```

## ⚠️ Security and Limitations

- Authentication currently stores and compares plaintext passwords, and some management screens display them. Do not use real passwords or personal data. Redesign password storage, authentication and authorization and perform a security audit before public deployment.
- The original database export contains accounts and business records and is excluded from public files. The schema script derived from it has not been verified against a fresh SQL Server; some tables referenced by code are absent from the export, so related features may be unavailable.
- Old dependencies have known security issues. The Dockerfile uses a .NET Core 3.1 image that does not match the web project's .NET 8 target and is not a validated deployment option.
- Bundled frontend resources and dependencies retain their own licenses.

## 🤝 Contributing and Feedback

See [Contributing](CONTRIBUTING.md), the [Code of Conduct](CODE_OF_CONDUCT.md) and [Security](SECURITY.md). Remove passwords, personal information and database records from reports.

## 📄 License

The existing [LICENSE](LICENSE) is GNU GPL v3.0. Third-party resources and dependencies retain their respective terms; their sources and distribution requirements still need review before release.
