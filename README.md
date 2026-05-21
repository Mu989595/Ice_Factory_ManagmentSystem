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

## 📁 Project Structure
IceFactoryManagmentSystem/ │ ├── src/ │   ├── Modules/ │   │   ├── HR/ │   │   │   ├── Domain/ │   │   │   │   ├── DailyAttendance.cs          # Aggregate root │   │   │   │   └── Worker.cs │   │   │   ├── Application/ │   │   │   │   ├── AttendanceService.cs        # Business logic │   │   │   │   └── DTOs/ │   │   │   └── Infrastructure/ │   │   │       ├── WorkerRepository.cs │   │   │       └── AttendanceRepository.cs │   │   │ │   │   ├── Finance/ │   │   │   ├── Domain/ │   │   │   │   ├── LedgerDay.cs                # Aggregate root │   │   │   │   ├── Sale.cs │   │   │   │   ├── Expense.cs │   │   │   │   └── ExpenseCategory.cs │   │   │   ├── Application/ │   │   │   │   ├── SaleService.cs │   │   │   │   ├── ExpenseService.cs │   │   │   │   └── DTOs/ │   │   │   └── Infrastructure/ │   │   │       ├── LedgerDayRepository.cs │   │   │       ├── SaleRepository.cs │   │   │       └── ExpenseRepository.cs │   │   │ │   │   ├── Inventory/ │   │   │   ├── Domain/ │   │   │   │   ├── BasinAggregate.cs           # Aggregate root (singleton) │   │   │   │   └── ProductionCycle.cs │   │   │   ├── Application/ │   │   │   │   └── DTOs/ │   │   │   └── Infrastructure/ │   │   │       ├── BasinRepository.cs │   │   │       └── ProductionCycleRepository.cs │   │   │ │   │   ├── Reports/ │   │   │   ├── Application/ │   │   │   │   ├── ReportService.cs            # Monthly, Inventory, HR reports │   │   │   │   └── DTOs/ │   │   │   └── Controllers/ │   │   │       └── ReportsController.cs │   │   │ │   │   └── Monthly/ │   │       ├── Domain/ │   │       │   ├── MonthlySummary.cs           # Aggregate root │   │       │   └── ProfitSplit.cs │   │       ├── Infrastructure/ │   │       │   └── MonthlySummaryRepository.cs │   │       └── Controllers/ │   │           └── MonthlyController.cs │   │ │   ├── Shared/ │   │   ├── Domain/ │   │   │   ├── Common/ │   │   │   │   ├── Entity.cs │   │   │   │   ├── AggregateRoot.cs │   │   │   │   ├── Result.cs                   # Railway-oriented programming │   │   │   │   ├── IDomainEvent.cs │   │   │   │   └── DomainException.cs │   │   │   ├── Events/ │   │   │   │   ├── SaleRecordedEvent.cs │   │   │   │   ├── StockDeductedEvent.cs │   │   │   │   ├── StockReplenishedEvent.cs │   │   │   │   └── ... (6 events total) │   │   │   ├── Enums/ │   │   │   │   ├── WorkerRole.cs │   │   │   │   ├── ExpenseCategoryType.cs │   │   │   │   └── ReplenishmentTrigger.cs │   │   │   ├── Interfaces/ │   │   │   │   ├── IUnitOfWork.cs │   │   │   │   ├── IEventDispatcher.cs │   │   │   │   ├── IRepository.cs │   │   │   │   └── IReplenishmentService.cs │   │   │   └── ValueObjects/ │   │   │       ├── Money.cs │   │   │       └── SplitPercentage.cs │   │   │ │   │   └── Infrastructure/ │   │       ├── Persistence/ │   │       │   ├── AppDbContext.cs │   │       │   ├── DbSeeder.cs │   │       │   ├── Configurations/             # 9 EF Core entity configs │   │       │   └── Migrations/ │   │       ├── BackgroundJobs/ │   │       │   ├── DayRolloverBackgroundService.cs │   │       │   └── ReplenishmentBackgroundService.cs │   │       ├── Events/ │   │       │   ├── EventDispatcher.cs │   │       │   └── EventHandlers/ │   │       ├── UnitOfWork/ │   │       │   └── UnitOfWork.cs │   │       ├── Interceptors/ │   │       │   └── AuditSaveChangesInterceptor.cs │   │       └── JsonConverters/ │   │           └── DateOnlyJsonConverter.cs │   │ │   ├── Controllers/ │   │   ├── WorkersController.cs │   │   ├── AttendanceController.cs │   │   ├── SalesController.cs │   │   ├── ExpensesController.cs │   │   ├── BasinController.cs │   │   ├── MonthlyController.cs │   │   └── ReportsController.cs │   │ │   └── Program.cs                              # ASP.NET Core entry point │ ├── frontend/                                   # React + TypeScript frontend │   ├── src/ │   │   ├── pages/                              # Route pages │   │   │   ├── Attendance.tsx │   │   │   ├── Dashboard.tsx │   │   │   ├── Sales.tsx │   │   │   ├── Expenses.tsx │   │   │   └── Reports.tsx │   │   ├── components/                         # Reusable UI components │   │   │   ├── ui/                             # Primitive components (Button, Card, Badge) │   │   │   └── features/                       # Domain-specific components │   │   ├── api/                                # API service modules │   │   │   ├── attendance.ts │   │   │   ├── workers.ts │   │   │   ├── sales.ts │   │   │   └── expenses.ts │   │   ├── lib/                                # Utility functions │   │   │   └── utils.ts │   │   ├── App.tsx │   │   └── main.tsx │   ├── vite.config.ts │   ├── tsconfig.json │   ├── tailwind.config.js │   ├── eslint.config.js │   ├── package.json │   └── README.md │ ├── .git/                                       # Git repository ├── .gitignore └── README.md                                   # This file
