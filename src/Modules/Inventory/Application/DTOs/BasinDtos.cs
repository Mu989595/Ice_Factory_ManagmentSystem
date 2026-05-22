namespace IcePlant.Application.DTOs;

public sealed record ProductionCycleDto(
    int Id,
    DateTime CycleTime,
    string Trigger,
    int BlocksAdded,
    int StockBefore,
    int StockAfter);
