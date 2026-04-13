using NUnit.Framework;
using SeesawHelper;

/// <summary>
/// Sanity checks for the gameplay tunable constants.
/// These tests act as a guard against accidental changes to game-feel values —
/// if you intentionally change a constant, update the expected value here too.
/// </summary>
public class PlayerHelperTests
{
    private const float Epsilon = 1e-6f;

    [Test]
    public void BasePlayerSpeed_HasExpectedValue()
    {
        Assert.AreEqual(0.02f, PlayerHelper.BasePlayerSpeed, Epsilon);
    }

    [Test]
    public void MaxAcceleration_HasExpectedValue()
    {
        Assert.AreEqual(2.5f, PlayerHelper.MaxAcceleration, Epsilon);
    }

    [Test]
    public void AccelerationRate_HasExpectedValue()
    {
        Assert.AreEqual(0.03f, PlayerHelper.AccelerationRate, Epsilon);
    }

    [Test]
    public void MaxAcceleration_IsGreaterThanInitialAcceleration()
    {
        // Player acceleration starts at 1.0 and ramps up — max must be greater
        // or the acceleration loop in PlayerMovementController is a no-op.
        Assert.Greater(PlayerHelper.MaxAcceleration, 1.0f);
    }

    [Test]
    public void AccelerationRate_IsPositive()
    {
        Assert.Greater(PlayerHelper.AccelerationRate, 0f);
    }

    [Test]
    public void BasePlayerSpeed_IsPositive()
    {
        Assert.Greater(PlayerHelper.BasePlayerSpeed, 0f);
    }

    [Test]
    public void RampUpFromOneToMax_TakesReasonableNumberOfSteps()
    {
        // (MaxAcceleration - 1) / AccelerationRate ≈ 50 FixedUpdate steps.
        // At default 50Hz physics, that's ~1 second to reach max speed.
        // Sanity-check the ramp isn't accidentally instantaneous or glacial.
        float steps = (PlayerHelper.MaxAcceleration - 1f) / PlayerHelper.AccelerationRate;
        Assert.Greater(steps, 10f, "Acceleration ramp is too fast");
        Assert.Less(steps, 500f, "Acceleration ramp is too slow");
    }
}
