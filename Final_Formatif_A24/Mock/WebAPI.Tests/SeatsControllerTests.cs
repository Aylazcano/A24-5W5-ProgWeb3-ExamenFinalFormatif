using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using WebAPI.Controllers;
using WebAPI.Exceptions;
using WebAPI.Models;
using WebAPI.Services;

namespace WebAPI.Tests;

[TestClass]
public class SeatsControllerTests
{
    [TestMethod]
    public void ReserveSeatOK()
    {
        // Mock the SeatsService
        Mock<SeatsService> serviceMock = new Mock<SeatsService>();
        // Mock the SeatsController with the mocked service
        Mock<SeatsController> controller = new Mock<SeatsController>(serviceMock.Object) { CallBase = true };

        // Create a user and a seat
        ExamenUser eu = new ExamenUser()
        {
            Id = "11111"
        };
        Seat s = new Seat()
        {
            Id = 1,
            Number = 1
        };

        // Setup the mock to return the seat when ReserveSeat is called
        serviceMock.Setup(s => s.ReserveSeat(It.IsAny<string>(), It.IsAny<int>())).Returns(s);
        // Mock the UserId property to return the desired user ID
        controller.Setup(c => c.UserId).Returns("11111");

        // Call the ReserveSeat method of the controller
        var actionResult = controller.Object.ReserveSeat(1);

        // Verify that the result is an OkObjectResult
        var result = actionResult.Result as OkObjectResult;

        // Assertion: verify that the result is not null
        Assert.IsNotNull(result);

        // Verify that the returned seat is the expected one
        Seat? seatResult = (Seat?)result!.Value;
        Assert.AreEqual(s.Id, seatResult!.Id);
    }

    [TestMethod]
    public void ReserveSeatAlreadyTaken()
    {
        // Mock the SeatsService
        Mock<SeatsService> serviceMock = new Mock<SeatsService>();
        // Mock the SeatsController with the mocked service
        Mock<SeatsController> controller = new Mock<SeatsController>(serviceMock.Object) { CallBase = true };

        // Create two users and a seat
        ExamenUser user = new ExamenUser()
        {
            Id = "11111"
        };
        ExamenUser user2 = new ExamenUser()
        {
            Id = "22222"
        };
        Seat s = new Seat()
        {
            Id = 1,
            Number = 1,
            ExamenUser = user
        };

        // Configure the mock to throw an exception when attempting to reserve the seat
        serviceMock.Setup(s => s.ReserveSeat("22222", 1)).Throws(new SeatAlreadyTakenException());

        // Mock the UserId property to return the desired user ID
        controller.Setup(c => c.UserId).Returns("22222");

        // Call the ReserveSeat method of the controller
        var actionResult = controller.Object.ReserveSeat(s.Number);

        // Verify that the result is an UnauthorizedResult
        var result = actionResult.Result as UnauthorizedResult;

        // Assertion: verify that the result is an UnauthorizedResult
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void ReserveSeatOutOfBounds()
    {
        // Mock the SeatsService
        Mock<SeatsService> serviceMock = new Mock<SeatsService>();
        // Mock the SeatsController with the mocked service
        Mock<SeatsController> controller = new Mock<SeatsController>(serviceMock.Object) { CallBase = true };

        // Configure the mock to throw an exception when the seat number exceeds the maximum
        serviceMock.Setup(service => service.ReserveSeat(It.IsAny<string>(), It.IsAny<int>())).Throws(new SeatOutOfBoundsException());
        // Mock the UserId property to return the desired user ID
        controller.Setup(c => c.UserId).Returns("11111");

        // Call the ReserveSeat method of the controller
        var actionResult = controller.Object.ReserveSeat(101);

        // Verify that the result is a NotFoundResult
        var result = actionResult.Result as NotFoundObjectResult;

        // Assertion: verify that the result is a NotFoundResult
        Assert.IsNotNull(result);

        // Verify that the error message is correct
        Assert.AreEqual("Could not find 101", result.Value);
    }

    [TestMethod]
    public void ReserveSeatUserAlreadySeated()
    {
        // Mock the SeatsService
        Mock<SeatsService> serviceMock = new Mock<SeatsService>();
        // Mock the SeatsController with the mocked service
        Mock<SeatsController> controller = new Mock<SeatsController>(serviceMock.Object) { CallBase = true };

        // Configure the mock to throw an exception when the user already has a reserved seat
        serviceMock.Setup(s => s.ReserveSeat(It.IsAny<string>(), It.IsAny<int>())).Throws(new UserAlreadySeatedException());
        // Mock the UserId property to return the desired user ID
        controller.Setup(c => c.UserId).Returns("11111");

        // Call the ReserveSeat method of the controller
        var actionResult = controller.Object.ReserveSeat(1);

        // Verify that the result is a BadRequestResult
        var result = actionResult.Result as BadRequestResult;

        // Assertion: verify that the result is a BadRequestResult
        Assert.IsNotNull(result);
    }
}
