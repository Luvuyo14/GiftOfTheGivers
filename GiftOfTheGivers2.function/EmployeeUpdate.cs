using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace GiftOfTheGivers2.function;

public class EmployeeUpdate
{
    private readonly ILogger<EmployeeUpdate> _logger;

    public EmployeeUpdate(ILogger<EmployeeUpdate> logger)
    {
        _logger = logger;
    }

    [Function("EmployeeUpdate")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "employee/update")] HttpRequest req)
    {
        string? employeeId;
        string? employeeName;
        string? project;
        string? updateText;

        try
        {
            using var reader = new StreamReader(req.Body);
            var body = await reader.ReadToEndAsync();
            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;
            employeeId = ReadString(root, "EmployeeId");
            employeeName = ReadString(root, "EmployeeName");
            project = ReadString(root, "Department");
            updateText = ReadString(root, "UpdateText");
        }
        catch (JsonException ex)
        {
            _logger.LogWarning("Employee update rejected because the body was not valid JSON: {Message}", ex.Message);
            return new BadRequestObjectResult(new { Success = false, Error = "The request body must be valid JSON." });
        }

        if (string.IsNullOrWhiteSpace(updateText))
        {
            return new BadRequestObjectResult(new { Success = false, Error = "UpdateText is required." });
        }

        var updateId = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
        _logger.LogInformation(
            "Employee update {UpdateId} from {EmployeeName} ({EmployeeId}) for project {Project}: {Length} characters",
            updateId, employeeName, employeeId, project, updateText.Length);

        return new OkObjectResult(new
        {
            Success = true,
            UpdateId = updateId,
            EmployeeName = employeeName,
            Project = project,
            ReceivedAt = DateTime.UtcNow
        });
    }

    private static string? ReadString(JsonElement root, string name)
    {
        return root.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
    }
}