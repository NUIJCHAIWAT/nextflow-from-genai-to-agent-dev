---
lab:
    title: 'Develop an AI agent'
    description: 'Use the Azure AI Agent Service to develop an agent that uses built-in tools.'
---

# Develop an AI agent

In this exercise, you'll use Azure AI Agent Service to create a simple agent that analyzes data and creates charts. The agent can use the built-in *Code Interpreter* tool to dynamically generate any code required to analyze data.

> **Tip**: The code used in this exercise is based on the for Azure AI Foundry SDK for .NET. You can develop similar solutions using the SDKs for Python, JavaScript, and Java. Refer to [Azure AI Foundry SDK client libraries](https://learn.microsoft.com/azure/ai-foundry/how-to/develop/sdk-overview) for details.

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
    - **Region**: *Select any **AI Foundry recommended***\*

    > \* Some Azure AI resources are constrained by regional model quotas. In the event of a quota limit being exceeded later in the exercise, there's a possibility you may need to create another resource in a different region.

1. Select **Create** and wait for your project to be created.
1. If prompted, deploy a **gpt-4o** model using either the *Global Standard* or *Standard* deployment option (depending on your quota availability).

    >**Note**: If quota is available, a GPT-4o base model may be deployed automatically when creating your Agent and project.

1. When your project is created, the Agents playground will be opened.

1. In the navigation pane on the left, select **Overview** to see the main page for your project; which looks like this:

    ![Screenshot of a Azure AI Foundry project overview page.](./Media/ai-foundry-project.png)

1. Copy the **Azure AI Foundry project endpoint** values to a notepad, as you'll use them to connect to your project in a client application.

## Create an agent client app

Now you're ready to create a client app that uses an agent. Some code has been provided for you in a GitHub repository.

### Clone the repo containing the application code

1. In the terminal, enter the following command to change the working directory to the folder containing the code files and list them all.

    ```
   cd part-3/Labfiles/02-build-ai-agent/C-sharp
   ls -a -l
    ```

    The provided files include application code, configuration settings, and data.

### Configure the application settings

1. Enter the following command to edit the configuration file that has been provided:

    ```
   code appsettings.json
    ```

    The file is opened in a code editor.

1. In the code file, replace the **your_project_endpoint** placeholder with the endpoint for your project (copied from the project **Overview** page in the Azure AI Foundry portal) and ensure that the MODEL_DEPLOYMENT_NAME variable is set to your model deployment name (which should be *gpt-4o*).
1. After you've replaced the placeholder, use the **CTRL+S** command to save your changes and then use the **CTRL+Q** command to close the code editor while keeping the cloud shell command line open.

### Write code for an agent app

> **Tip**: As you add code, be sure to maintain the correct indentation. Use the comment indentation levels as a guide.

1. Enter the following command to edit the code file that has been provided:

    ```
   code Program.cs
    ```

1. Review the existing code, which retrieves the application configuration settings and loads data from *data.txt* to be analyzed. The rest of the file includes comments where you'll add the necessary code to implement your data analysis agent.


2. Find the comment **Connect to the Agent client** and add the following code to connect to the Azure AI project.

    > **Tip**: Be careful to maintain the correct indentation level.

    ```csharp
    // Connect to the Agent client
    PersistentAgentsClient agentClient = new(projectEndpoint, new DefaultAzureCredential());
    ```

    The code connects to the Azure AI Foundry project using the current Azure credentials.

3. Find the comment **Upload the data file and create a CodeInterpreterTool** and add the following code to upload the data file to the project and create a CodeInterpreterTool that can access the data in it:

    ```csharp
    // Upload the data file and create a CodeInterpreterTool
    PersistentAgentFileInfo uploadedFile = await agentClient.Files.UploadFileAsync(
        filePath: filePath,
        purpose: PersistentAgentFilePurpose.Agents
    );
    Console.WriteLine($"Uploaded {uploadedFile.Filename}");

    List<ToolDefinition> tools = [new CodeInterpreterToolDefinition()];
    ```
    
4. Find the comment **Define an agent that uses the CodeInterpreterTool** and add the following code to define an AI agent that analyzes data and can use the code interpreter tool you defined previously:

    ```csharp
    // Define an agent that uses the CodeInterpreterTool
    var agent = await agentClient.Administration.CreateAgentAsync(
        modelDeployment,
        name: "data-agent",
        instructions: "You are an AI agent that analyzes the data in the file that has been uploaded. Use Python to calculate statistical metrics as necessary.",
        tools: tools,
        toolResources: new ToolResources
        {
            CodeInterpreter = new CodeInterpreterToolResource
            {
                FileIds = { uploadedFile.Id }
            }
        }
    );
    Console.WriteLine($"Using agent: {agent.Value.Name}");
    ```

5. Find the comment **Create a thread for the conversation** and add the following code to start a thread on which the chat session with the agent will run:

    ```csharp
    // Create a thread for the conversation
    var thread = await agentClient.Threads.CreateThreadAsync();
    ```
    
6. Note that the next section of code sets up a loop for a user to enter a prompt, ending when the user enters "quit".

7. Find the comment **Send a prompt to the agent** and add the following code to add a user message to the prompt (along with the data from the file that was loaded previously), and then run thread with the agent.

    ```csharp
    // Send a prompt to the agent
    await agentClient.Messages.CreateMessageAsync(
        threadId: thread.Value.Id,
        role: MessageRole.User,
        userPrompt
    );

    ThreadRun run = await agentClient.Runs.CreateRunAsync(
        thread.Value.Id,
        agent.Value.Id
    );

    // Wait for the run to complete
    do
    {
        await Task.Delay(TimeSpan.FromMilliseconds(500));
        run = await agentClient.Runs.GetRunAsync(thread.Value.Id, run.Id);
    }
    while (run.Status == RunStatus.Queued
        || run.Status == RunStatus.InProgress);
    ```

8. Find the comment **Check the run status for failures** and add the following code to check for any errors.

    ```csharp
    // Check the run status for failures
    if (run.Status == RunStatus.Failed)
    {
        Console.WriteLine($"Run failed: {run.LastError}");
    }
    ```

9. Find the comment **Show the latest response from the agent** and add the following code to retrieve the messages from the completed thread and display the last one that was sent by the agent.

    ```csharp
    // Show the latest response from the agent
    List<PersistentThreadMessage> messages = await agentClient.Messages.GetMessagesAsync(
        threadId: thread.Value.Id,
        order: ListSortOrder.Descending
    ).ToListAsync();

    var lastMessage = messages.FirstOrDefault(m => m.Role == MessageRole.Agent);

    if (lastMessage != null)
    {
        var content = lastMessage.ContentItems.OfType<MessageTextContent>().FirstOrDefault();
        if (content != null)
        {
            Console.WriteLine($"Last Message: {content.Text}");
        }
    }
    ```

10. Find the comment **Get the conversation history**, which is after the loop ends, and add the following code to print out the messages from the conversation thread in chronological sequence:

    ```csharp
    // Get the conversation history
        Console.WriteLine("\nConversation Log:\n");
        var allMessages = await agentClient.Messages.GetMessagesAsync(
                threadId: thread.Value.Id,
                order: ListSortOrder.Ascending
            ).ToListAsync();
    
        foreach (PersistentThreadMessage threadMessage in allMessages)
        {
            // Console.Write($"{threadMessage.CreatedAt:yyyy-MM-dd HH:mm:ss} - {threadMessage.Role,10}: ");
            foreach (MessageContent contentItem in threadMessage.ContentItems)
            {
                if (contentItem is MessageTextContent textItem)
                {
                    Console.Write(textItem.Text);
                }
                Console.WriteLine();
            }
        }
    ```

11. Find the comment **Clean up** and add the following code to delete the agent and thread when no longer needed.

    ```csharp
    // Clean up
    await agentClient.Threads.DeleteThreadAsync(thread.Value.Id);
    await agentClient.Administration.DeleteAgentAsync(agent.Value.Id);
    ```

12. Review the code, using the comments to understand how it:
    - Connects to the AI Foundry project.
    - Uploads the data file and creates a code interpreter tool that can access it.
    - Creates a new agent that uses the code interpreter tool and has explicit instructions to use Python as necessary for statistical analysis.
    - Runs a thread with a prompt message from the user along with the data to be analyzed.
    - Checks the status of the run in case there's a failure
    - Retrieves the messages from the completed thread and displays the last one sent by the agent.
    - Displays the conversation history
    - Deletes the agent and thread when they're no longer required.

13. Save the code file (*CTRL+S*) when you have finished. You can also close the code editor (*CTRL+Q*); though you may want to keep it open in case you need to make any edits to the code you added. In either case, keep the cloud shell command-line pane open.

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
    
    The application runs using the credentials for your authenticated Azure session to connect to your project and create and run the agent.

1. When prompted, view the data that the app has loaded from the *data.txt* text file. Then enter a prompt such as:

    ```
   What's the category with the highest cost?
    ```

    > **Tip**: If the app fails because the rate limit is exceeded. Wait a few seconds and try again. If there is insufficient quota available in your subscription, the model may not be able to respond.

1. View the response. Then enter another prompt, this time requesting a visualization:

    ```
   Create a text-based bar chart showing cost by category
    ```

1. View the response. Then enter another prompt, this time requesting a statistical metric:

    ```
   What's the standard deviation of cost?
    ```

    View the response.

1. You can continue the conversation if you like. The thread is *stateful*, so it retains the conversation history - meaning that the agent has the full context for each response. Enter `quit` when you're done.
1. Review the conversation messages that were retrieved from the thread - which may include messages the agent generated to explain its steps when using the code interpreter tool.

## Summary

In this exercise, you used the Azure AI Agent Service SDK to create a client application that uses an AI agent. The agent can use the built-in Code Interpreter tool to run dynamic Python code to perform statistical analyses.

## Clean up

If you've finished exploring Azure AI Agent Service, you should delete the resources you have created in this exercise to avoid incurring unnecessary Azure costs.

1. Return to the browser tab containing the Azure portal (or re-open the [Azure portal](https://portal.azure.com) at `https://portal.azure.com` in a new browser tab) and view the contents of the resource group where you deployed the resources used in this exercise.
1. On the toolbar, select **Delete resource group**.
1. Enter the resource group name and confirm that you want to delete it.
