using TechTalk.SpecFlow;

namespace Qa.BddTests.Support;

/// <summary>Hooks run around every scenario. Here: stop any virtual service the scenario started.</summary>
[Binding]
public class Hooks
{
    private readonly ApiContext _context;

    public Hooks(ApiContext context)
    {
        _context = context;
    }

    [AfterScenario]
    public void CleanUp() => _context.Dispose();
}
