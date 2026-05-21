# Ice Factory Management System

## 📋 Project Overview and Purpose

The **Ice Factory Management System** is an enterprise-level full-stack application designed to manage ice manufacturing operations with integrated HR management. The system streamlines three core business areas:

- **Inventory Management** – Track ice basin stock levels with automated replenishment triggers
- **Financial Operations** – Record sales transactions and operational expenses with detailed categorization  
- **HR & Attendance** – Manage worker attendance tracking with automated daily wage calculations

The application provides real-time dashboards, comprehensive reporting, and background job automation for seamless operations management.

---

## 🛠️ Full Technology Stack

### Backend

| Component | Technology |
|-----------|-----------|
| **Runtime** | .NET 9 with ASP.NET Core |
| **Architecture Pattern** | Domain-Driven Design (DDD) |
| **Database** | SQL Server with Entity Framework Core |
| **API Style** | RESTful Web API |
| **Authentication** | JWT Bearer Tokens + ASP.NET Identity |
| **Event System** | Domain Events with Pub/Sub Event Dispatcher |
| **Background Jobs** | Hosted Services (DayRollover, Replenishment) |
| **Interceptors** | EF Core Save Changes Interceptor (Audit Trail) |

### Frontend

| Component | Technology |
|-----------|-----------|
| **Framework** | React 19.2 with TypeScript 6.0 |
| **Build Tool** | Vite 8.0 (HMR enabled) |
| **HTTP Client** | Axios |
| **State Management** | TanStack React Query v5 (server state) |
| **Routing** | React Router v7 |
| **Forms & Validation** | React Hook Form + Zod schema validation |
| **Styling** | Tailwind CSS 3.4 + PostCSS |
| **UI Components** | Custom components + Lucide React icons |
| **Charts** | Recharts v3 for data visualization |
| **Code Quality** | ESLint with TypeScript support |

### Infrastructure & DevOps

| Tool | Purpose |
|------|---------|
| **Git** | Version control |
| **GitHub** | Remote repository |
| **Visual Studio 2022** | Primary IDE for backend |
| **npm/Node.js** | Frontend package management |

---


