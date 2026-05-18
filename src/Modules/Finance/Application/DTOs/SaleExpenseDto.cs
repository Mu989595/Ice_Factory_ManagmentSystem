namespace IcePlant.Application.DTOs;

// ── Sale DTOs ────────────────────────────────────────────────────────────────

/// <summary>
/// Data the cashier sends to record a new sale.
/// </summary>
public sealed record RecordSaleDto(
    int      LedgerDayId,
    int      BlocksSold,
    decimal  UnitPrice,
    string?  CustomerName = null,
    string?  Notes        = null);

/// <summary>
/// What we send back after a sale is recorded.
/// </summary>
public sealed record SaleResultDto(
    int      SaleId,
    int      LedgerDayId,
    DateTime SaleTime,
    int      BlocksSold,
    decimal  UnitPrice,
    decimal  TotalAmount,
    string?  CustomerName,
    string?  Notes);

// ── Expense DTOs ─────────────────────────────────────────────────────────────

/// <summary>
/// Data sent to record a new expense.
/// </summary>
public sealed record RecordExpenseDto(
    int      LedgerDayId,
    int      CategoryId,
    decimal  Amount,
    string?  Supplier   = null,
    string?  InvoiceRef = null,
    string?  Notes      = null);

/// <summary>
/// What we send back after an expense is recorded.
/// </summary>
public sealed record ExpenseResultDto(
    int      ExpenseId,
    int      LedgerDayId,
    int      CategoryId,
    string   CategoryName,
    string   CategoryType,
    decimal  Amount,
    DateTime ExpenseTime,
    string?  Supplier,
    string?  InvoiceRef,
    string?  Notes);

/// <summary>
/// A summary of an expense category (used in GET category lists).
/// </summary>
public sealed record ExpenseCategoryDto(
    int    Id,
    string Name,
    string CategoryType,
    string? UtilityType,
    bool   IsActive);
