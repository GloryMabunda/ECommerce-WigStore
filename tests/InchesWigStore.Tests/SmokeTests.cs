namespace InchesWigStore.Tests;

public class SmokeTests
{
    [Fact]
    public void Solution_references_web_project()
    {
        Assert.Equal("InchesWigStore.Web", typeof(Program).Assembly.GetName().Name);
    }
}
