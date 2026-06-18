using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using IcePlant.Infrastructure.Persistence;

var configuration = new ConfigurationBuilder()
    .SetBasePath(System.IO.Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.Development.json")
    .Build();

var services = new ServiceCollection();
services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

var provider = services.BuildServiceProvider();
using var scope = provider.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

var count = db.TransactionHistories.Count();
Console.WriteLine($"Total Transaction Histories: {count}");

var latestSales = db.Sales.OrderByDescending(s => s.Id).Take(5).ToList();
Console.WriteLine("Latest Sales:");
foreach (var sale in latestSales)
{
    Console.WriteLine($"SaleId: {sale.Id}, Blocks: {sale.BlocksSold}, Time: {sale.SaleTime}");
}
