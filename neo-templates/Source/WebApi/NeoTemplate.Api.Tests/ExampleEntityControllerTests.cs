
namespace NeoTemplate.Api.Tests
{
  using System;
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using Microsoft.AspNetCore.Mvc;
  using NeoTemplate.Api.Controllers;
  using NeoTemplate.App.Services;
  using NeoTemplate.Models;
  using Xunit;

  public class AggregateRootControllerTests
  {
    /// <summary>
    /// Initialise the controller.
    /// </summary>
    /// <returns>The controller.</returns>
    public static async Task<AggregateRootController> InitControllerAsync()
    {
      var context = await UnitTestHelper.InitContextAsync();
      var modelService = new AggregateRootService(context);
      return new AggregateRootController(modelService);
    }

    [Fact]
    public async Task GetById()
    {
      // Arrange
      var controller = await InitControllerAsync();

      // Act
      IActionResult result = await controller.GetById(1);

      // Assert
      OkObjectResult okObjectResult = Assert.IsType<OkObjectResult>(result);
      AggregateRoot entity = Assert.IsType<AggregateRoot>(okObjectResult.Value);
      Assert.Equal("Example Entity 1", entity.AggregateRootName);
    }

    [Fact]
    public async Task GetByIdNotFound()
    {
      // Arrange
      var controller = await InitControllerAsync();

      // Act
      IActionResult result = await controller.GetById(10);

      // Assert
      Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetEntities()
    {
      // Arrange
      var controller = await InitControllerAsync();

      // Act
      IActionResult result = await controller.GetEntities();

      // Assert
      OkObjectResult okObjectResult = Assert.IsType<OkObjectResult>(result);
      List<AggregateRoot> entities = Assert.IsType<List<AggregateRoot>>(okObjectResult.Value);
      Assert.NotEmpty(entities);
      Assert.True(entities.Count == 3, "There should be 3 AggregateRoot objects in the test data");
    }

    [Fact]
    public async Task Post()
    {
      // Arrange
      var controller = await InitControllerAsync();

      // Act
      AggregateRoot postEntity = new AggregateRoot()
      {
        AggregateRootId = 4,
        AggregateRootName = "Example 4",
        ExampleDate = DateTime.UtcNow
      };
      IActionResult result = await controller.Post(postEntity);

      // Assert
      CreatedAtActionResult createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
      AggregateRoot entity = Assert.IsType<AggregateRoot>(createdAtActionResult.Value);
      Assert.True(postEntity.AggregateRootId == entity.AggregateRootId, "The posted entity key should match");
      Assert.True(postEntity.AggregateRootName == entity.AggregateRootName, "The posted entity key should match");

      // detach, re-retrieve and assert
      controller.ModelService.Context.DetachEntity(entity);

      result = await controller.GetById(postEntity.AggregateRootId);
      OkObjectResult okObjectResult = Assert.IsType<OkObjectResult>(result);
      entity = Assert.IsType<AggregateRoot>(okObjectResult.Value);
      Assert.True(postEntity.AggregateRootId == entity.AggregateRootId, "The posted entity key should match");
      Assert.True(postEntity.AggregateRootName == entity.AggregateRootName, "The posted entity key should match");
    }

    [Fact]
    public async Task Put()
    {
      // Arrange
      var controller = await InitControllerAsync();

      // Act
      IActionResult result = await controller.GetById(1);

      OkObjectResult okObjectResult = Assert.IsType<OkObjectResult>(result);
      AggregateRoot entity = Assert.IsType<AggregateRoot>(okObjectResult.Value);
      string modifiedName = entity.AggregateRootName + " modified";
      entity.AggregateRootName = modifiedName;

      result = await controller.Put(entity.AggregateRootId, entity);

      // Assert
      okObjectResult = Assert.IsType<OkObjectResult>(result);
      entity = Assert.IsType<AggregateRoot>(okObjectResult.Value);
      Assert.Equal(modifiedName, entity.AggregateRootName);

      // detach, re-retrieve and assert
      controller.ModelService.Context.DetachEntity(entity);

      okObjectResult = Assert.IsType<OkObjectResult>(result);
      entity = Assert.IsType<AggregateRoot>(okObjectResult.Value);
      Assert.True(modifiedName == entity.AggregateRootName, "The posted entity key should match");
    }

    [Fact]
    public async Task Delete()
    {
      // Arrange
      var controller = await InitControllerAsync();

      // Act
      IActionResult result = await controller.Delete(1);

      // Assert
      Assert.IsType<OkObjectResult>(result);

      // try re-retrieve and expect NotFound
      result = await controller.GetById(1);

      // Assert
      Assert.IsType<NotFoundResult>(result);
    }
  }
}