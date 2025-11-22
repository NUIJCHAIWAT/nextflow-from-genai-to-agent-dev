using Azure;
using Azure.AI.Agents.Persistent;
using Azure.Identity;
using Microsoft.Extensions.Configuration;

// Load configuration
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

var projectEndpoint = configuration["PROJECT_ENDPOINT"] ?? throw new InvalidOperationException("PROJECT_ENDPOINT is not set");
var modelDeploymentName = configuration["MODEL_DEPLOYMENT_NAME"] ?? throw new InvalidOperationException("MODEL_DEPLOYMENT_NAME is not set");

// MCP server configuration
var mcpServerUrl = "https://learn.microsoft.com/api/mcp";
var mcpServerLabel = "mslearn";

// Connect to the agents client

// Create MCP tool definition


// Create agent with MCP tool


// Create thread for communication


// Get user prompt
Console.Write("\nHow can I help?: ");
var prompt = Console.ReadLine() ?? "Give me the Azure CLI commands to create an Azure Container App with a managed identity.";

// Create message to thread


// Create MCP tool resource (no approval required - "never" mode)


// Create and process agent run in thread with MCP tools


// Handle run execution and tool approvals
while (run.Status == RunStatus.Queued || run.Status == RunStatus.InProgress || run.Status == RunStatus.RequiresAction)
{
    await Task.Delay(TimeSpan.FromSeconds(1));
    run = agentsClient.Runs.GetRun(thread.Id, run.Id);

    if (run.Status == RunStatus.RequiresAction && run.RequiredAction is SubmitToolApprovalAction toolApprovalAction)
    {
        var toolApprovals = new List<ToolApproval>();
        foreach (var toolCall in toolApprovalAction.SubmitToolApproval.ToolCalls)
        {
            if (toolCall is RequiredMcpToolCall mcpToolCall)
            {
                Console.WriteLine($"Approving MCP tool call: {mcpToolCall.Name}");
                toolApprovals.Add(new ToolApproval(mcpToolCall.Id, approve: true));
            }
        }

        if (toolApprovals.Count > 0)
        {
            run = agentsClient.Runs.SubmitToolOutputsToRun(thread.Id, run.Id, toolApprovals: toolApprovals);
        }
    }
}

// Check run status
Console.WriteLine($"Run completed with status: {run.Status}");
if (run.Status == RunStatus.Failed)
{
    Console.WriteLine($"Run failed: {run.LastError}");
}

// Display run steps and tool calls
Pageable<RunStep> runSteps = agentsClient.Runs.GetRunSteps(run);
foreach (RunStep step in runSteps)
{
    Console.WriteLine($"Step {step.Id} status: {step.Status}");
    
    if (step.StepDetails is RunStepToolCallDetails toolCallDetails)
    {
        if (toolCallDetails.ToolCalls.Any())
        {
            Console.WriteLine("  MCP Tool calls:");
            foreach (var toolCall in toolCallDetails.ToolCalls)
            {
                Console.WriteLine($"    Tool Call ID: {toolCall.Id}");
                Console.WriteLine($"    Type: {toolCall.GetType().Name}");
                
                if (toolCall is RunStepMcpToolCall mcpToolCall)
                {
                    Console.WriteLine($"    Name: {mcpToolCall.Name}");
                }
            }
        }
    }
    
    Console.WriteLine();
}

// Fetch and log all messages
Pageable<PersistentThreadMessage> messages = agentsClient.Messages.GetMessages(
    threadId: thread.Id,
    order: ListSortOrder.Ascending
);

Console.WriteLine("\nConversation:");
Console.WriteLine(new string('-', 50));
foreach (PersistentThreadMessage threadMessage in messages)
{
    Console.Write($"{threadMessage.CreatedAt:yyyy-MM-dd HH:mm:ss} - {threadMessage.Role,10}: ");
    foreach (MessageContent contentItem in threadMessage.ContentItems)
    {
        if (contentItem is MessageTextContent textItem)
        {
            Console.Write(textItem.Text);
        }
        else if (contentItem is MessageImageFileContent imageFileItem)
        {
            Console.Write($"<image from ID: {imageFileItem.FileId}>");
        }
        Console.WriteLine();
    }
}

// Clean-up and delete the agent
await agentsClient.Threads.DeleteThreadAsync(thread.Id);
await agentsClient.Administration.DeleteAgentAsync(agent.Id);
Console.WriteLine("Deleted agent");
