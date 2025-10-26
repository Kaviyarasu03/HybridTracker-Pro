# HybridTracker Pro - Enhanced Application Tracking System

A comprehensive **Hybrid Application Tracking System** built with **ASP.NET Core**, supporting both **automated tracking for technical roles** and **manual tracking for non-technical roles**. This system features secure authentication, complete application lifecycle tracking, role-based access, and dashboard analytics.

---

## 🚀 Features

- 🔐 **JWT Authentication & Role-Based Authorization** — Roles: *Applicant, BotMimic, Admin*
- 🤖 **Hybrid Tracking System**
  - *Automated* tracking for Technical roles
  - *Manual* tracking for Non-Technical roles
- 📊 **Full Traceability** — Complete audit trail via `ApplicationHistory`
- 💼 **Job Postings Management** — Admin can create/manage job positions
- 📈 **Dashboard Analytics**
- 🛡️ **Security First** — BCrypt hashing, validation & access control
- 📌 **Swagger UI API Documentation**

---

## 🛠️ Technologies Used

- ASP.NET Core 8.0 Web API
- Entity Framework Core
- SQL Server / LocalDB
- JWT Bearer Authentication
- BCrypt.Net (password hashing)
- Swagger / OpenAPI

---

## ⚙️ Quick Start

### ✅ Prerequisites
- .NET 8.0 SDK
- SQL Server Express / LocalDB
- Visual Studio 2022 or VS Code

### 📌 Installation

```bash
git clone https://github.com/Kaviyarasu03/HybridTracker-Pro.git
cd HybridTracker-Pro
```

### 🗄️ Configure Database — `appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=HybridTrackerProDb;Trusted_Connection=True;MultipleActiveResultSets=true"
  },
  "Jwt": {
    "Key": "YourVeryLongSecretKeyThatIsAtLeast32CharactersLong12345!",
    "Issuer": "HybridTrackerPro",
    "Audience": "HybridTrackerUsers",
    "ExpiryInMinutes": "60"
  }
}
```

### ▶️ Run the Application

```bash
dotnet run
```

Swagger UI: **http://localhost:5029/swagger**

---

## 👥 Default Users & Roles

| Email              | Password     | Role      | Description |
|-------------------|-------------|-----------|-------------|
| admin@test.com     | password123 | Admin     | Manages non-technical workflows & job postings |
| applicant@test.com | password123 | Applicant | Creates and tracks own applications |
| bot@test.com       | password123 | BotMimic  | Automated updates for technical roles |

---

## 📡 API Endpoints (Summary)

### 🔑 Authentication
| Method | Endpoint | Description |
|---------|----------|-------------|
| POST | `/api/Auth/register` | Register |
| POST | `/api/Auth/login` | Login & JWT |
| GET | `/api/Auth/profile` | Get profile |

### 📌 Applications
| Method | Endpoint | Description |
|---------|----------|-------------|
| POST | `/api/Applications` | Create application |
| GET | `/api/Applications` | Get all (Admin/Bot only) |
| GET | `/api/Applications/user/{userId}` | User applications |
| GET | `/api/Applications/{id}` | Get by ID |
| PUT | `/api/Applications/{id}` | Update |

### 🛠️ Admin
```
/api/Admin/dashboard
/api/Admin/stats
/api/Admin/applications
/api/Admin/applications/{id}
/api/Admin/jobpostings
/api/Admin/application-history/{applicationId}
```

### 🤖 Bot Mimic
```
/api/BotMimic/update-all
/api/BotMimic/update/{id}
/api/BotMimic/status
```

---

## 🔄 Application Workflow

**Technical (Automated)**
```
Applied → Reviewed → Interview → Offer
```

**Non-Technical (Manual)**
```
Applied → Phone Screen → Interview → HR Review → Offer
```

---

## 💾 Database Schema (Core Tables)
- Users
- Roles
- Applications
- ApplicationHistory
- JobPostings

---

## 🧪 Sample Test Data

```json
{
  "candidateName": "John Developer",
  "roleApplied": "Technical",
  "status": "Submitted",
  "comments": "Backend .NET Developer",
  "userId": 1
}
```

```json
{
  "candidateName": "Jane Manager",
  "roleApplied": "Non-Technical",
  "status": "Applied",
  "comments": "HR Manager position",
  "userId": 1
}
```

---

## 📁 Project Structure

```
HybridTracker-Pro/
├── Controllers/
│   ├── AdminController.cs (10 endpoints)
│   ├── ApplicationsController.cs (6 endpoints)
│   ├── AuthController.cs (3 endpoints)
│   └── BotMimicController.cs (5 endpoints)
├── Models/
│   ├── User.cs
│   ├── Role.cs
│   ├── Application.cs
│   ├── ApplicationHistory.cs
│   ├── JobPosting.cs
│   └── Request Models
├── Data/
│   └── AppDbContext.cs
├── Services/
│   └── HistoryService.cs
├── Program.cs
└── appsettings.json
```

---

## 🚀 Deployment

```bash
dotnet publish -c Release
```

---

## 📄 License
Licensed under the **MIT License**.

---

## ⭐ Support
- Open an issue in GitHub
- Use Swagger for API testing

> If you like the project, **star ⭐ the repository!**

---

**HybridTracker Pro — Streamlining your hiring process with intelligent automation 🚀**
