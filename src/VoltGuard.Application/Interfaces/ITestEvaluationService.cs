using VoltGuard.Domain.Entities;

namespace VoltGuard.Application.Interfaces;

public interface ITestEvaluationService
{
    string EvaluateMeasurement(Measurement measurement);

    string EvaluateTestResult(IEnumerable<Measurement> measurements);

    string EvaluateAndApply(TestResult testResult);
}
