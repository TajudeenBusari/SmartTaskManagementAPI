using Xunit.Abstractions;
using Xunit.Sdk;

namespace SmartTaskIntegrationTest.Helper;

public class PriorityOrderer: ITestCaseOrderer
{
    public IEnumerable<TTestCase> OrderTestCases<TTestCase>(IEnumerable<TTestCase> testCases) where TTestCase : ITestCase
    {
        return testCases.OrderBy(tc => tc.TestMethod.Method.GetCustomAttributes(typeof(TestPriorityAttribute))
            .FirstOrDefault()?.GetNamedArgument<int>("Priority") ?? 0);
       
    }
}

public class TestPriorityAttribute: Attribute
{
    public int Priority { get; }

    public TestPriorityAttribute(int priority) => Priority = Priority;
    
}