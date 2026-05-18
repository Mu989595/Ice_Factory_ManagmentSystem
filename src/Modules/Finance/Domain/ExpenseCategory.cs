using IcePlant.Domain.Common;
using IcePlant.Domain.Enums;

namespace IcePlant.Domain.Aggregates.Finance;

/// <summary>
/// Lookup entity for expense categories.
/// Seeded at startup — Water, Electricity, Ammonia, Salt, Wages, Maintenance, Petty Cash, Other.
/// </summary>
public sealed class ExpenseCategory : Entity
{
    public string              Name         { get; private set; } = string.Empty;
    public ExpenseCategoryType CategoryType { get; private set; }
    public UtilityBillType?    UtilityType  { get; private set; } // only set if CategoryType == UtilityBill
    public bool                IsActive     { get; private set; }

    private ExpenseCategory() { }

    public static ExpenseCategory CreateUtilityBill(string name, UtilityBillType utilityType)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Category name cannot be empty.");

        return new ExpenseCategory
        {
            Name         = name.Trim(),
            CategoryType = ExpenseCategoryType.UtilityBill,
            UtilityType  = utilityType,
            IsActive     = true
        };
    }

    public static ExpenseCategory CreateGeneralExpense(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Category name cannot be empty.");

        return new ExpenseCategory
        {
            Name         = name.Trim(),
            CategoryType = ExpenseCategoryType.GeneralExpense,
            UtilityType  = null,
            IsActive     = true
        };
    }

    public bool IsUtilityBill    => CategoryType == ExpenseCategoryType.UtilityBill;
    public bool IsGeneralExpense => CategoryType == ExpenseCategoryType.GeneralExpense;

    public void Deactivate() => IsActive = false;
    public void Activate()   => IsActive = true;
}
