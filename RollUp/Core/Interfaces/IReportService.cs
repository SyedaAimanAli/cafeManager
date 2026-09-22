using System.Threading.Tasks;
using RollUp.Application.DTOs;

namespace RollUp.Core.Interfaces;

public interface IReportService
{
    Task<SalesReportSummaryDto> GetSalesReportAsync(string timeframe = "7days");
}
