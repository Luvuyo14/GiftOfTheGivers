using System.Net;
using GiftOfTheGivers2.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using Xunit;

namespace GiftOfTheGivers2.Tests
{
    // Moq is used to fake IConfiguration and the HTTP call to the Azure Function,
    // so these tests never contact a real Azure service.
    public class EmployeeControllerTests
    {
        private static EmployeeController CreateController(
            HttpStatusCode functionResponse, string? role, out Mock<HttpMessageHandler> handler)
        {
            handler = new Mock<HttpMessageHandler>();
            handler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(functionResponse));

            var config = new Mock<IConfiguration>();
            config.Setup(c => c["AzureSettings:FunctionBaseUrl"]).Returns("http://fake-function/api/");

            var session = new TestSession();
            if (role != null) session.SetString("Role", role);

            return new EmployeeController(new HttpClient(handler.Object), config.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { Session = session }
                }
            };
        }

        [Fact]
        public void Index_NotEmployee_RedirectsToLogin()
        {
            var controller = CreateController(HttpStatusCode.OK, "Donor", out _);

            var result = controller.Index();

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirect.ActionName);
            Assert.Equal("Account", redirect.ControllerName);
        }

        [Fact]
        public void Index_Employee_ReturnsView()
        {
            var controller = CreateController(HttpStatusCode.OK, "Employee", out _);

            Assert.IsType<ViewResult>(controller.Index());
        }

        [Fact]
        public async Task PostUpdate_FunctionSucceeds_ShowsSuccessMessageAndCallsFunctionOnce()
        {
            var controller = CreateController(HttpStatusCode.OK, "Employee", out var handler);

            var result = await controller.PostUpdate("Flood Relief", "Delivered 200 blankets");

            Assert.IsType<ViewResult>(result);
            Assert.Contains("successfully", (string)controller.ViewBag.Message);
            handler.Protected().Verify("SendAsync", Times.Once(),
                ItExpr.Is<HttpRequestMessage>(r => r.RequestUri!.ToString() == "http://fake-function/api/employee/update"),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task PostUpdate_FunctionFails_ShowsFailureMessage()
        {
            var controller = CreateController(HttpStatusCode.InternalServerError, "Employee", out _);

            await controller.PostUpdate("Flood Relief", "Update");

            Assert.Contains("failed", (string)controller.ViewBag.Message);
        }

        [Fact]
        public async Task PostUpdate_NotEmployee_RedirectsAndNeverCallsFunction()
        {
            var controller = CreateController(HttpStatusCode.OK, null, out var handler);

            var result = await controller.PostUpdate("X", "Y");

            Assert.IsType<RedirectToActionResult>(result);
            handler.Protected().Verify("SendAsync", Times.Never(),
                ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
        }
    }
}