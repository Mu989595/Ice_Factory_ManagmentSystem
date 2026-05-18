# IceFactoryManagementSystem — Restructure Plan

## PHASE 1: Codebase Analysis

### Current Project Structure

```
IceFactoryManagmentSystem/          ← Web API host (.NET 9, single .sln, single .csproj for API)
├── Domain/                         ← Class library (IcePlant.Domain)
│   ├── Aggregates/
│   │   ├── Basin/                  BasinAggregate, ProductionCycle
│   │   ├── Finance/                LedgerDay, Sale, Expense, ExpenseCategory
│   │   ├── HR/                     Worker, DailyAttendance
│   │   └── Monthly/               MonthlySummary, ProfitSplit
│   ├── Common/                     Entity, AggregateRoot, Result, IDomainEvent, DomainException
│   ├── Enums/                      WorkerRole, ExpenseCategoryType, UtilityBillType, ReplenishmentTrigger
│   │   └── Events/                 DomainEvents.cs (6 event records)
│   ├── Interfaces/                 IUnitOfWork, IEventDispatcher, IDomainEventHandler, IProfitSplitStrategy, IReplenishmentService
│   │   └── Repositories/          IRepositories.cs (9 interfaces in one file)
│   └── ValueObjects/              Money, SplitPercentage
│
├── Application/                    ← Class library (IcePlant.Application)
│   ├── DTOs/                       SaleExpenseDto.cs, AttendanceDto.cs
│   ├── EventHandlers/              OnSaleRecorded_DeductBasinStock, OnStockDeducted_UpdateLedgerClosingStock
│   └── Services/                   SaleService, ExpenseService, AttendanceService
│
├── IcePlant.Infrastructure/        ← Class library (IcePlant.Infrastructure)
│   ├── BackgroundJobs/             DayRolloverBackgroundService, ReplenishmentBackgroundService
│   ├── Events/                     EventDispatcher
│   ├── JsonConverters/             DateOnlyJsonConverter
│   ├── Migrations/                 EF Core migrations
│   ├── Persistence/                AppDbContext, DbSeeder, Configurations/ (9 entity configs)
│   ├── Repositories/               8 concrete repository classes + BaseRepository
│   └── UnitOfWork/                 UnitOfWork
│
├── Controllers/                    ← Inside API host project
│   ├── AttendanceController.cs
│   ├── BasinController.cs
│   ├── ExpensesController.cs
│   ├── MonthlyController.cs
│   ├── SalesController.cs
│   └── WorkersController.cs
│
├── Program.cs                      ← Entry point
├── Infrastructure/                 ← EMPTY stub project
├── WebApi/                         ← EMPTY stub project
├── IceFactory.Api/                 ← EMPTY folder
├── frontend/                       ← React/TS frontend (separate)
└── ice-factory-frontend/           ← Old/duplicate frontend
```

---

### Inventory of All Components

#### Entities (12)
| Entity | Aggregate | Layer |
|--------|-----------|-------|
| `BasinAggregate` | Basin (Singleton) | Domain |
| `ProductionCycle` | Basin | Domain |
| `LedgerDay` | Finance (AR) | Domain |
| `Sale` | Finance | Domain |
| `Expense` | Finance | Domain |
| `ExpenseCategory` | Finance (lookup) | Domain |
| `Worker` | HR (AR) | Domain |
| `DailyAttendance` | HR | Domain |
| `MonthlySummary` | Monthly (AR) | Domain |
| `ProfitSplit` | Monthly | Domain |

#### Value Objects (2)
`Money`, `SplitPercentage`

#### Domain Events (6)
`StockDeductedEvent`, `StockReplenishedEvent`, `BasinDayRolledOverEvent`, `SaleRecordedEvent`, `ExpenseRecordedEvent`, `MonthClosedEvent`

#### Repository Interfaces (9)
`IBasinRepository`, `IProductionCycleRepository`, `ILedgerDayRepository`, `ISaleRepository`, `IExpenseRepository`, `IExpenseCategoryRepository`, `IWorkerRepository`, `IAttendanceRepository`, `IMonthlySummaryRepository`

#### Application Services (3)
`SaleService`, `ExpenseService`, `AttendanceService`

#### Event Handlers (2)
`OnSaleRecorded_DeductBasinStock`, `OnStockDeducted_UpdateLedgerClosingStock`

#### Controllers (6)
`SalesController`, `ExpensesController`, `BasinController`, `WorkersController`, `AttendanceController`, `MonthlyController`

#### Background Services (2)
`ReplenishmentBackgroundService` (15-min poll), `DayRolloverBackgroundService` (midnight)

---

### Business Domains Identified

| Domain | Entities | Services | Controller | Status |
|--------|----------|----------|------------|--------|
| **Inventory (Basin)** | BasinAggregate, ProductionCycle | Replenishment BG | BasinController | ✅ Solid |
| **Sales** | Sale (child of LedgerDay) | SaleService | SalesController | ✅ Solid |
| **Finance** | LedgerDay, Expense, ExpenseCategory | ExpenseService | ExpensesController | ✅ Solid |
| **HR** | Worker, DailyAttendance | AttendanceService | AttendanceController, WorkersController | ✅ Solid |
| **Monthly** | MonthlySummary, ProfitSplit | — (inline in controller) | MonthlyController | ⚠️ Logic in controller |
| **Reports** | — | — | — | ❌ Missing |
| **Dashboard** | — | — | — | ❌ Missing |
| **Audit Trail** | — | — | — | ❌ Missing |
| **Auth/JWT** | — | — | — | ❌ Missing (referenced but not implemented) |

---

### Issues Found

> [!WARNING]
> #### Critical Issues

1. **Empty stub projects**: `Infrastructure/`, `WebApi/`, `IceFactory.Api/` are empty placeholders — dead code.
2. **Duplicate frontend**: Both `frontend/` and `ice-factory-frontend/` exist.
3. **Stray `{Persistence` directory** inside IcePlant.Infrastructure (likely accidental).
4. **`Class1.cs` files** in Infrastructure and WebApi — leftover scaffolding.
5. **`Application/Class1.cs`** — empty leftover.

> [!IMPORTANT]
> #### Architectural Issues

6. **All 9 repository interfaces crammed into one file** (`IRepositories.cs`) — violates SRP.
7. **MonthlyController contains business logic** (calculating totals, creating summary, splitting profit) — should be in a service.
8. **BasinController contains business logic** (manual replenishment with transaction management) — should be in a service.
9. **WorkersController has inline DTO** (`CreateWorkerDto` defined as nested record) — inconsistent with other DTOs.
10. **No global error handling middleware** — controllers catch errors inconsistently.
11. **No JWT/Auth** — `UseAuthorization()` is called but no auth scheme is configured.
12. **No audit trail** — entity changes are not tracked.
13. **No reporting endpoints**.
14. **No dashboard KPI endpoint**.
15. **`IProfitSplitStrategy` and `IReplenishmentService`** interfaces exist in Domain but have no implementations.

> [!NOTE]
> #### Naming Inconsistencies

16. Project folder name: `IceFactoryManagmentSystem` (typo: "Managment" → "Management") — too risky to rename, keep as-is.
17. Namespace mismatch: Controller namespace is `IceFactoryManagmentSystem.Controllers` while domain is `IcePlant.Domain`.
18. `DomainEvents.cs` is inside `Domain/Enums/Events/` — events aren't enums.
19. `SaleRepository` imports `IcePlant.Domain.Aggregates.HR` (unused).

---

## PHASE 2-4: Execution Plan

### Phase 2 — Restructure into Vertical Modules

The new `/src` folder structure will reorganize code into feature-based vertical slices.

**Key decisions:**
- All existing code logic is preserved; only namespace + file location changes
- The existing 3 class library `.csproj` files (Domain, Application, Infrastructure) stay as-is since they compile into the host project. The `/src/Modules/` is a logical folder organization within these libraries.
- EF Core DbContext, migrations, and configurations remain in shared infrastructure
- Background services remain in shared infrastructure

### Phase 3 — Add Missing Features

1. **AuditLog** entity + EF Core `SaveChanges` interceptor
2. **Reports module** with 3 report types (Monthly, Inventory, HR)
3. **Dashboard KPI** endpoint

### Phase 4 — Cleanup & Verification

1. Delete empty stubs
2. Build verification
3. Final commit
