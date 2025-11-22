---
lab:
    title: 'Connect AI Agents to a remote MCP server'
    description: 'Learn how to integrate Model Context Protocol tools with AI agents using C# .NET'
---

# Connect AI agents to tools using Model Context Protocol (MCP)

In this exercise, you'll build an agent that connects to a cloud-hosted MCP server. The agent will use AI-powered search to help developers find accurate, real-time answers from Microsoft's official documentation. This is useful for building assistants that support developers with up-to-date guidance on tools like Azure, .NET, and Microsoft 365. The agent will use the available MCP tools to query the documentation and return relevant results.

> **Tip**: The code used in this exercise is based on the Azure AI Agent service MCP support sample repository. Refer to [Connect to Model Context Protocol servers](https://learn.microsoft.com/en-us/azure/ai-foundry/agents/how-to/tools/model-context-protocol) for more details.

This exercise should take approximately **30** minutes to complete.

> **Note**: Some of the technologies used in this exercise are in preview or in active development. You may experience some unexpected behavior, warnings, or errors.

## Create an Azure AI Foundry project

Let's start by creating an Azure AI Foundry project.

1. In a web browser, open the [Azure AI Foundry portal](https://ai.azure.com) at `https://ai.azure.com` and sign in using your Azure credentials. Close any tips or quick start panes that are opened the first time you sign in, and if necessary use the **Azure AI Foundry** logo at the top left to navigate to the home page, which looks similar to the following image (close the **Help** pane if it's open):

    ![Screenshot of Azure AI Foundry portal.](./Media/ai-foundry-home.png)

1. In the home page, select **Create an agent**.
1. When prompted to create a project, enter a valid name for your project and expand **Advanced options**.
1. Confirm the following settings for your project:
    - **Azure AI Foundry resource**: *A valid name for your Azure AI Foundry resource*
    - **Subscription**: *Your Azure subscription*
    - **Resource group**: *Create or select a resource group*
    - **Region**: *Select any of the following supported locations:* 
      

    > \* Some Azure AI resources are constrained by regional model quotas. In the event of a quota limit being exceeded later in the exercise, there's a possibility you may need to create another resource in a different region.

2. Select **Create** and wait for your project to be created.
3. If prompted, deploy a **gpt-4o** model using either the *Global Standard* or *Standard* deployment option (depending on your quota availability).

    >**Note**: If quota is available, a GPT-4o base model may be deployed automatically when creating your Agent and project.

4. When your project is created, the Agents playground will be opened.

5. In the navigation pane on the left, select **Overview** to see the main page for your project; which looks like this:

    ![Screenshot of a Azure AI Foundry project overview page.](./Media/ai-foundry-project.png)

6. Copy the **Azure AI Foundry project endpoint** value. You'll use it to connect to your project in a client application.

## Develop an agent that uses MCP function tools

Now that you've created your project in AI Foundry, let's develop an app that integrates an AI agent with an MCP server.

### Clone the repo containing the application code

1. In the terminal, enter the following command to change the working directory to the folder containing the code files and list them all.

    ```
   cd part-3/Labfiles/03c-use-agent-tools-with-mcp/C-sharp
   ls -a -l
    ```

### Configure the application settings

1. Enter the following command to edit the configuration file that has been provided:

    ```
   code appsettings.json
    ```

    The file is opened in a code editor.

1. In the code file, replace the **your_project_endpoint** placeholder with the endpoint for your project (copied from the project **Overview** page in the Azure AI Foundry portal) and ensure that the MODEL_DEPLOYMENT_NAME variable is set to your model deployment name (which should be *gpt-4o*).

1. After you've replaced the placeholder, use the **CTRL+S** command to save your changes and then use the **CTRL+Q** command to close the code editor while keeping the cloud shell command line open.

### Connect an Azure AI Agent to a remote MCP server

In this task, you'll connect to a remote MCP server, prepare the AI agent, and run a user prompt.

1. Enter the following command to edit the code file that has been provided:

    ```
   code Program.cs
    ```

    The file is opened in the code editor.

1. Review the existing code, which loads the application configuration settings and sets up the MCP server configuration. The rest of the file includes comments where you'll add the necessary code to implement your MCP-enabled agent.

1. Find the comment **Connect to the agents client** and add the following code to connect to the Azure AI project using the current Azure credentials.

    ```csharp
   // Connect to the agents client
   PersistentAgentsClient agentsClient = new(projectEndpoint, new DefaultAzureCredential());
    ```

1. Find the comment **Create MCP tool definition** and add the following code:

    ```csharp
   // Create MCP tool definition
   MCPToolDefinition mcpTool = new(mcpServerLabel, mcpServerUrl);
    ```

    This code will connect to the Microsoft Learn Docs remote MCP server. This is a cloud-hosted service that enables clients to access trusted and up-to-date information directly from Microsoft's official documentation.

1. Find the comment **Create agent with MCP tool** and add the following code:

    ```csharp
   // Create agent with MCP tool
   PersistentAgent agent = await agentsClient.Administration.CreateAgentAsync(
        model: modelDeploymentName,
        name: "my-mcp-agent",
        instructions: """
            You have access to an MCP server called `microsoft.docs.mcp` - this tool allows you to 
            search through Microsoft's latest official documentation. Use the available MCP tools 
            to answer questions and perform tasks.
            """,
        tools: [mcpTool]);

   Console.WriteLine($"Created agent, ID: {agent.Id}");
   Console.WriteLine($"MCP Server: {mcpServerLabel} at {mcpServerUrl}");
    ```

    In this code, you provide instructions for the agent and provide it with the MCP tool definition.

1. Find the comment **Create thread for communication** and add the following code:

    ```csharp
   // Create thread for communication
   PersistentAgentThread thread = await agentsClient.Threads.CreateThreadAsync();
   Console.WriteLine($"Created thread, ID: {thread.Id}");
    ```

1. Find the comment **Get user prompt** and note that the code prompts the user for input and uses a default prompt if none is provided.

1. Find the comment **Create message to thread** and add the following code:

    ```csharp
   // Create message to thread
   PersistentThreadMessage message = await agentsClient.Messages.CreateMessageAsync(
        thread.Id,
        MessageRole.User,
        prompt);
   Console.WriteLine($"Created message, ID: {message.Id}");
    ```

1. Find the comment **Create MCP tool resource (no approval required - "never" mode)** and add the following code:

    ```csharp
    // Create MCP tool resource (no approval required - "never" mode)
    MCPToolResource mcpToolResource = new(mcpServerLabel);
    ToolResources toolResources = mcpToolResource.ToToolResources();

    // Create and process agent run in thread with MCP tools
    ThreadRun run = agentsClient.Runs.CreateRun(thread, agent, toolResources);
    Console.WriteLine($"Created run, ID: {run.Id}");

    ```

    This allows the agent to automatically invoke the MCP tools without requiring user approval. If you want to require approval, you must supply a header value using `mcpToolResource.UpdateHeader` and handle the approval in the run loop.

2. Find the comment **Create and process agent run in thread with MCP tools**, The AI Agent automatically invokes the connected MCP tools to process the prompt request. To illustrate this process, the next section of code will output any invoked tools from the MCP server.

3. Find the comment **Check run status** and note that the code checks for any failures in the run.

4. Find the comment **Display run steps and tool calls** and note that this code displays information about the MCP tools that were invoked during the run.

5. Find the comment **Fetch and log all messages** and note that this code retrieves and displays the conversation history, showing both the user's question and the agent's response.

6. Find the comment **Clean-up and delete the agent** and note that this code deletes the agent when it's no longer needed.

7. Review the complete code, using the comments to understand how it:
    - Connects to the AI Foundry project.
    - Creates an MCP tool definition pointing to the Microsoft Learn documentation server.
    - Creates a new agent that uses the MCP tool.
    - Creates a thread and adds a user message.
    - Runs the agent with automatic MCP tool invocation (no approval required).
    - Displays the run steps showing which MCP tools were called.
    - Displays the conversation history.
    - Deletes the agent when it's no longer required.

8. Save the code file (*CTRL+S*) when you have finished. You can also close the code editor (*CTRL+Q*); though you may want to keep it open in case you need to make any edits to the code you added. In either case, keep the cloud shell command-line pane open.

### Sign into Azure and run the app

1. In the cloud shell command-line pane, enter the following command to sign into Azure.

    ```
    az login --use-device-code
    ```

    **<font color="red">You must sign into Azure - even though the cloud shell session is already authenticated.</font>**

    > **Note**: In most scenarios, just using *az login* will be sufficient. However, if you have subscriptions in multiple tenants, you may need to specify the tenant by using the *--tenant* parameter. See [Sign into Azure interactively using the Azure CLI](https://learn.microsoft.com/cli/azure/authenticate-azure-cli-interactively) for details.
    
1. When prompted, follow the instructions to open the sign-in page in a new tab and enter the authentication code provided and your Azure credentials. Then complete the sign in process in the command line, selecting the subscription containing your Azure AI Foundry hub if prompted.

1. After you have signed in, enter the following command to run the application:

    ```
   dotnet run
    ```

1. When prompted, enter a request for technical information such as:

    ```
    Give me the Azure CLI commands to create an Azure Container App with a managed identity.
    ```

1. Wait for the agent to process your prompt, using the MCP server to find a suitable tool to retrieve the requested information. You should see some output similar to the following:

    ```
    Created agent, ID: <<agent-id>>
    MCP Server: mslearn at https://learn.microsoft.com/api/mcp
    Created thread, ID: <<thread-id>>
    Created message, ID: <<message-id>>
    Created run, ID: <<run-id>>
    Run completed with status: Completed
    Step <<step1-id>> status: Completed

    Step <<step2-id>> status: Completed
    MCP Tool calls:
        Tool Call ID: <<tool-call-id>>
        Type: RunStepMcpToolCall
        Name: microsoft_code_sample_search


    Conversation:
    --------------------------------------------------
    ASSISTANT: You can use Azure CLI to create an Azure Container App with a managed identity (either system-assigned or user-assigned). Below are the relevant commands and workflow:

    ---

    ### **1. Create a Resource Group**
    '''azurecli
    az group create --name myResourceGroup --location eastus
    '''
    

    {{continued...}}

    By following these steps, you can deploy an Azure Container App with either system-assigned or user-assigned managed identities to integrate seamlessly with other Azure services.
    --------------------------------------------------
    USER: Give me the Azure CLI commands to create an Azure Container App with a managed identity.
    --------------------------------------------------
    Deleted agent
    ```

    Notice that the agent was able to invoke the MCP tool `microsoft_code_sample_search` automatically to fulfill the request.

1. You can run the app again (using the command `dotnet run`) to ask for different information. In each case, the agent will attempt to find technical documentation by using the MCP tool.

## Summary

In this exercise, you used the Azure AI Agent Service SDK for .NET to create a client application that uses an AI agent with MCP (Model Context Protocol) tools. The agent can automatically connect to remote MCP servers to access real-time information from external sources, such as Microsoft's official documentation.

## Clean up

Now that you've finished the exercise, you should delete the cloud resources you've created to avoid unnecessary resource usage.

1. Open the [Azure portal](https://portal.azure.com) at `https://portal.azure.com` and view the contents of the resource group where you deployed the hub resources used in this exercise.
1. On the toolbar, select **Delete resource group**.
1. Enter the resource group name and confirm that you want to delete it.
