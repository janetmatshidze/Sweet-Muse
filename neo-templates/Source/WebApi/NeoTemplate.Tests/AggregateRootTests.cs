namespace NeoTemplate.Tests
{
  using System;
  using System.Linq;
  using NeoTemplate.Models;
  using Xunit;

  public class AggregateRootTests
  {
    [Fact]
    public void CheckValid()
    {
      AggregateRoot entity = new AggregateRoot()
      {
        AggregateRootId = 4,
        AggregateRootName = "Example 4",
        ExampleDate = DateTime.UtcNow
      };

      var result = UnitTestHelper.ModelValidator.ValidateObject(entity);

      Assert.True(result.IsValid);
      Assert.Empty(result.Errors);
    }

    [Fact]
    public void CheckInValid()
    {
      AggregateRoot entity = new AggregateRoot()
      {
        AggregateRootId = 4,
        AggregateRootName = "Example 4" + new string('x', 100),
        ExampleDate = DateTime.UtcNow
      };

      var result = UnitTestHelper.ModelValidator.ValidateObject(entity);

      Assert.False(result.IsValid);
      Assert.NotNull(result.Errors.FirstOrDefault(
        validationResult => validationResult.MemberNames.Contains("AggregateRootName")
          && validationResult.ErrorMessage != null
          && validationResult.ErrorMessage.Contains("maximum length", StringComparison.InvariantCulture)));
    }
  }
}
