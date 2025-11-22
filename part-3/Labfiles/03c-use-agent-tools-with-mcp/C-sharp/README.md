# Build AI Agent with MCP Tools in C# .NET

This project demonstrates how to create an Azure AI Agent that uses Model Context Protocol (MCP) tools to access Microsoft Learn documentation.

## Prerequisites

- Azure AI Foundry project with deployed GPT-4o model
- .NET 8.0 SDK
- Azure CLI

## Setup

1. Update `appsettings.json` with your Azure AI Foundry project endpoint:
   ```json
   {
     "PROJECT_ENDPOINT": "your_project_endpoint",
     "MODEL_DEPLOYMENT_NAME": "gpt-4o"
   }
   ```

2. Sign in to Azure:
   ```bash
   az login --use-device-code
   ```

3. Run the application:
   ```bash
   dotnet run
   ```

## How It Works

The application:
1. Connects to Azure AI Foundry using DefaultAzureCredential
2. Creates an MCP tool definition pointing to Microsoft Learn's MCP server
3. Creates an agent with the MCP tool
4. Processes user queries by automatically invoking MCP tools
5. Displays the conversation and MCP tool invocations

## MCP Server

This example connects to Microsoft Learn's MCP server:
- **URL**: https://learn.microsoft.com/api/mcp
- **Label**: mslearn
- **Capability**: Search Microsoft's official documentation

The agent can automatically search for technical information without requiring approval.

## Notes

- Requires Azure.AI.Agents.Persistent v1.2.0-beta.7 or higher for MCP support
- MCP tools execute automatically without approval in this example
- For production scenarios, consider implementing approval mechanisms using `MCPToolResource.UpdateHeader()`
