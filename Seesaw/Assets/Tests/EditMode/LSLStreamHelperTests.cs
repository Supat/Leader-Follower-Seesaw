using NUnit.Framework;
using SeesawHelper;

public class LSLStreamHelperTests
{
    [Test]
    public void ResolveStreamSuffix_PlayerOne_ReturnsServer()
    {
        Assert.AreEqual("Server", LSLStreamHelper.ResolveStreamSuffix(1));
    }

    [Test]
    public void ResolveStreamSuffix_PlayerNegativeOne_ReturnsClient()
    {
        Assert.AreEqual("Client", LSLStreamHelper.ResolveStreamSuffix(-1));
    }

    [Test]
    public void ResolveStreamSuffix_PlayerZero_ReturnsEmpty()
    {
        Assert.AreEqual("", LSLStreamHelper.ResolveStreamSuffix(0));
    }

    [TestCase(2)]
    [TestCase(-2)]
    [TestCase(100)]
    [TestCase(int.MinValue)]
    [TestCase(int.MaxValue)]
    public void ResolveStreamSuffix_UnexpectedPlayerID_ReturnsEmpty(int playerID)
    {
        Assert.AreEqual("", LSLStreamHelper.ResolveStreamSuffix(playerID));
    }
}
