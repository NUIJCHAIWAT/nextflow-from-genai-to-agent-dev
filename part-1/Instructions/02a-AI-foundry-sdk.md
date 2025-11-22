---
lab:
    title: 'Create a generative AI chat app'
    description: 'Learn how to use the Azure AI Foundry SDK to build an app that connects to your project and chats with a language model.'
---

# Create a generative AI chat app

In this exercise, you use the Azure AI Foundry .NET SDK to create a simple chat app that connects to a project and chats with a language model.

> **Note**: This exercise is based on pre-release SDK software, which may be subject to change. Where necessary, we've used specific versions of packages; which may not reflect the latest available versions. You may experience some unexpected behavior, warnings, or errors.

While this exercise focuses on the Azure AI Foundry .NET SDK, you can develop AI chat applications using multiple language-specific SDKs; including:

- [Azure AI Projects for Microsoft .NET](https://www.nuget.org/packages/Azure.AI.Projects)
- [Azure AI Projects for Python](https://pypi.org/project/azure-ai-projects)
- [Azure AI Projects for JavaScript](https://www.npmjs.com/package/@azure/ai-projects)


## Deploy a model in an Azure AI Foundry project

Let's start by deploying a model in an Azure AI Foundry project.

1. In a web browser, open the [Azure AI Foundry portal](https://ai.azure.com) at `https://ai.azure.com` and sign in using your Azure credentials. Close any tips or quick start panes that are opened the first time you sign in, and if necessary use the **Azure AI Foundry** logo at the top left to navigate to the home page, which looks similar to the following image (close the **Help** pane if it's open):

    ![Screenshot of Azure AI Foundry portal.](./media/ai-foundry-home.png)

1. In the home page, in the **Explore models and capabilities** section, search for the `gpt-4o` model; which we'll use in our project.
1. In the search results, select the **gpt-4o-mini** model to see its details, and then at the top of the page for the model, select **Use this model**.
1. When prompted to create a project, enter a valid name for your project and expand **Advanced options**.
1. Select **Customize** and specify the following settings for your project:
    - **Azure AI Foundry resource**: *A valid name for your Azure AI Foundry resource*
    - **Subscription**: *Your Azure subscription*
    - **Resource group**: *Create or select a resource group*
    - **Region**: *Select any **AI Foundry recommended***\*

    > \* Some Azure AI resources are constrained by regional model quotas. In the event of a quota limit being exceeded later in the exercise, there's a possibility you may need to create another resource in a different region.

1. Select **Create** and wait for your project, including the gpt-4o model deployment you selected, to be created.

    > <font color="red"><b>IMPORTANT</b>:</font> Depending on your available quota for gpt-4o models you might receive a additional prompt to deploy the model to a resource in a different region. If this happens, do so, using the default settings. Later in the exercise you will <b><u>not</u></b> be able to use the default project endpoint - you must use the model-specific target URI.

1. When your project is created, the chat playground will be opened automatically so you can test your model (if not, in the task pane on the left, select **Playgrounds** and then open the **Chat playground**).
1. In the **Setup** pane, note the name of your model deployment; which should be **gpt-4o**. You can confirm this by viewing the deployment in the **Models and endpoints** page (just open that page in the navigation pane on the left).

## Create a client application to chat with the model

Now that you have deployed a model, you can use the Azure AI Foundry and Azure OpenAI SDKs to develop an application that chats with it.

### Prepare the application configuration

1. In the Azure AI Foundry portal, view the **Overview** page for your project.
2. In the **Endpoints and keys** area, ensure that the **Azure AI Foundry** library is selected and view the **Azure AI Foundry project endpoint**. You'll use this endpoint to connect to your project and model in a client application.

    > **Note**: You can also use the Azure OpenAI endpoint!

3. From the cloned repository, navigate to the folder containing the chat application code files and view them:

    ```
   cd part-1/LabFiles/chat-app/csharp
   ls -a -l
    ```

    The folder contains a .NET console project (`ChatApp.csproj`), the `Program.cs` source file, and a `.env` configuration file for application settings.

4. In the shell command-line pane, enter the following command to restore the NuGet packages referenced by the project:

    ```
   dotnet restore
    ```
5. Enter the following command to edit the configuration file that has been provided:

    ```
   code .env
    ```

    The file is opened in a code editor.

    > <font color="red"><b>IMPORTANT</b>:</font> If you deployed your gpt-4o model in the default region for your project, you can use the <b>Azure AI Foundry project</b> or <b>Azure OpenAI</b> endpoint on the <b>Overview</b> page for your project to connect to your model. If you had insufficient quota and deployed the model to another region, on the <b>Models + Endpoints</b> page, select your model and use the <b>Target URI</b> for your model.

6. In the code file, replace the **your_project_endpoint** placeholder with the appropriate endpoint for your model, and the **your_model_deployment** placeholder with the name assigned to your gpt-4o model deployment.
7. After you've replaced the placeholders, within the code editor, use the **CTRL+S** command to save your changes

### Write code to connect to your project and chat with your model

> **Tip**: As you add code, be sure to maintain the correct indentation and namespace references.

1. Enter the following command to edit the C# source file that has been provided:

    ```
   code Program.cs
    ```

2. In the code file, note the existing statements that have been added at the top of the file to import the necessary SDK namespaces. Then, find the comment Add references, and add the following code to reference the namespaces in the libraries you installed previously:

    ```csharp
   using System.Text;
   using Azure.AI.OpenAI;
   using Azure.AI.Projects;
   using Azure.Identity;
   using OpenAI.Chat;
    ```
3. In the **main** function, under the comment **Get configuration settings**, note that the code loads the project connection string and model deployment name values you defined in the configuration file.

4. Find the comment **Initialize the project client**, and add the following code to connect to your Azure AI Foundry project:
   
    ```csharp
    // Initialize the project client
    var projectClient = new AIProjectClient(
            new Uri(endpoint),
            new DefaultAzureCredential(new DefaultAzureCredentialOptions
            {
                ExcludeEnvironmentCredential = true,
                ExcludeManagedIdentityCredential = true
            }));
    ```
5. Find the comment Get a chat client, and add the following code to create a client object for chatting with a model:

    ```csharp
    // Get the Azure OpenAI client and chat client for the deployment
    var openAIClient = projectClient.GetOpenAIClient(apiVersion: "2024-10-21");
    var chatClient = openAIClient.GetChatClient(modelDeployment);
    ```

6. Find the comment **Initialize prompt with system message**, and add the following code to initialize a collection of messages with a system prompt.

    ```csharp
   // Initialize prompt with system message
   var conversation = new List<ChatMessage>
   {
        new SystemChatMessage("You are a helpful AI assistant that answers questions.")
   };       
    ```
7. Note that the code includes a loop to allow a user to input a prompt until they enter "quit". Then in the loop section, find the comment **Get a chat completion** and add the following code to add the user input to the prompt, retrieve the completion from your model, and add the completion to the prompt (so that you retain chat history for future iterations):

    ```csharp
    // Get a chat completion
        conversation.Add(new UserChatMessage(inputText));

        var chatCompletion = await chatClient.CompleteChatAsync(
            conversation,
            new ChatCompletionOptions
            {
                Temperature = 0.8f
            });

        var completionText = ExtractContentText(chatCompletion);

        Console.WriteLine($"\nAssistant: {completionText}\n");

        conversation.Add(new AssistantChatMessage(completionText));
    ```

8. Use the **CTRL+S** command to save your changes to the code file and then close the editor.

### Sign into Azure and run the app

1. In the Terminal, enter the following command to sign into Azure.

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

1. When prompted, enter a question, such as `What is the fastest animal on Earth?` and review the response from your generative AI model.
1. Try some follow-up questions, like `Where can I see one?` or `Are they endangered?`. The conversation should continue, using the chat history as context for each iteration.
1. When you're finished, enter `quit` to exit the program.

> **Tip**: If the app fails because the rate limit is exceeded. Wait a few seconds and try again. If there is insufficient quota available in your subscription, the model may not be able to respond.

## Summary

In this exercise, you used the Azure AI Foundry SDK to create a client application for a generative AI model that you deployed in an Azure AI Foundry project.

## Clean up

If you've finished exploring Azure AI Foundry portal, you should delete the resources you have created in this exercise to avoid incurring unnecessary Azure costs.

1. Open the [Azure portal](https://portal.azure.com) and view the contents of the resource group where you deployed the resources used in this exercise.
1. On the toolbar, select **Delete resource group**.
1. Enter the resource group name and confirm that you want to delete it.
