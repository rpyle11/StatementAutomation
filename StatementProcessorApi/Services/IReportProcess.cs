using StatementProcessorModels;

namespace StatementProcessorApi.Services;

public interface IReportProcess
{
    Task<bool> GenerateReport(ReportProcessParameters parameters);
}