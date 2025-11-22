---
lab:
    title: 'Apply filters on functions'
    description: 'Learn how to apply filters on functions with Semantic Kernel.'
---

# Apply filters on functions

In this exercise, you consume a previous chat conversation between the user and the assistant to generate a new response. To do this, you need to apply a trust filter on the function result. Let's get started!


## Deploy a chat completion model

1. In a web browser, open the [Azure AI Foundry portal](https://ai.azure.com) at `https://ai.azure.com` and sign in using your Azure credentials. Close any tips or quick start panes that are opened the first time you sign in, and if necessary use the **Azure AI Foundry** logo at the top left to navigate to the home page, which looks similar to the following image (close the **Help** pane if it's open):

    ![Screenshot of Azure AI Foundry portal.](../Media/ai-foundry-home.png)

1. In the home page, in the **Explore models and capabilities** section, search for the `gpt-4o` model; which we'll use in our project.
1. In the search results, select the **gpt-4o-mini** model to see its details, and then at the top of the page for the model, select **Use this model**.
1. When prompted to create a project, enter a valid name for your project and expand **Advanced options**.
1. Select **Customize** and specify the following settings for your hub:
    - **Azure AI Foundry resource**: *A valid name for your Azure AI Foundry resource*
    - **Subscription**: *Your Azure subscription*
    - **Resource group**: *Create or select a resource group*
    - **Region**: *Select any **AI Services supported location***\*

    > \* Some Azure AI resources are constrained by regional model quotas. In the event of a quota limit being exceeded later in the exercise, there's a possibility you may need to create another resource in a different region.

1. Select **Create** and wait for your project, including the gpt-4o model deployment you selected, to be created.
1. When your project is created, the chat playground will be opened automatically.
1. In the navigation pane on the left, select **Overview** to see the main page for your project; which looks like this:

    > **Note**: If an *Insufficient permissions** error is displayed, use the **Fix me** button to resolve it.

    ![Screenshot of a Azure AI Foundry project overview page.](../Media/ai-foundry-project.png)

1. Under the **Libraries** section of the overview page, select **Azure OpenAI**

    You'll use the data here in the next task to build your kernel. Remember to keep your keys private and secure!

## Create an AI client app

Now you're ready to create a Semantic Kernel client app that provides different functions filters. Some code is provided for you in a GitHub repository.

### Prepare the environment

1. In the terminal, navigate to the folder containing the application code files:  

    > **Note**: Follow the steps for your chosen programming language.

    **C#**
    ```
    cd part-2/Labfiles/04-apply-function-filters/C-sharp
    ```

    **Python**
    ```
    cd part-2/Labfiles/04-apply-function-filters/Python
    ```

2. In the terminal, enter the following command to install the libraries you'll use:

    **C#**
    ```
    dotnet add package Microsoft.SemanticKernel
    ```

    **Python**
    ```
    python -m venv labenv
    ./labenv/bin/Activate.ps1
    pip install python-dotenv semantic-kernel[azure]
    ```

3. Enter the following command to edit the configuration file that has been provided:

    **C#**
    ```
    code appsettings.json
    ```

    **Python**
    ```
    code .env
    ```

    The file should open in a code editor.

4. In the code file, replace the **your_azure_openai_endpoint** and **your_azure_openai_key** placeholders with the Azure OpenAI endpoint and API key for your project (copied from the project **Overview** page in the Azure AI Foundry portal), and replace the **your_deployment_name** placeholder with the name you assigned to your gpt-4o model.

5. After you replace the placeholders, in the code editor, use the **CTRL+S** command or **Right-click > Save** to save your changes and then use the **CTRL+Q** command or **Right-click > Quit** to close the code editor while keeping the cloud shell command line open.

### Request user approval for an operation

In this task, you add a filter function that will request the user's approval before allowing the assistant to book a flight on their behalf. Let's get started!

1. Enter the following command to edit the permission filter file that has been provided:

    **C#**
    ```
    code Program.cs
    ```

    **Python**
    ```
    code filters.py
    ```

1. Update the class definition under the comment **Create the function filer class**:

    **C#**
    ```c#
    // Create the function filer class
    public class PermissionFilter : IFunctionInvocationFilter
    ```
    **Python**
    ```python
    # Create the function filer class
    async def permission_filter(context: FunctionInvocationContext, 
        next: Callable[[FunctionInvocationContext], Awaitable[None]]) -> None:
    ```

1. Add the following code under the comment **Implement the function invocation method**

    **C#**
    ```c#
    // Implement the function invocation method
    public async Task OnFunctionInvocationAsync(FunctionInvocationContext context, Func<FunctionInvocationContext, Task> next)
    {
        if (!HasUserPermission(context.Function.PluginName, context.Function.Name))
        {
            context.Result = new FunctionResult(context.Result, "The operation was not approved by the user");
            return;
        }
        
        await next(context);
    }
    ```
    **Python**
    ```python
    # Implement the function invocation method
    if not has_user_permission(context.function.plugin_name, context.function.name):
        context.result = "The operation was not approved by the user"
        return
    
    await next(context)
    ```

1. Add the following code under the comment **Add the permission filter to the kernel**:

    **C#**
    ```c#
    // Add the permission filter to the kernel
    kernel.FunctionInvocationFilters.Add(new PermissionFilter());
    ```
    **Python**
    ```python
    # Add the permission filter to the kernel
    kernel.add_filter('function_invocation', permission_filter)
    ```

### Sign into Azure and run the app

1. In the Terminal, enter the following command to sign into Azure.

    ```
    az login --use-device-code
    ```

    **<font color="red">You must sign into Azure - even though the cloud shell session is already authenticated.</font>**

    > **Note**: In most scenarios, just using *az login* will be sufficient. However, if you have subscriptions in multiple tenants, you may need to specify the tenant by using the *--tenant* parameter. See [Sign into Azure interactively using the Azure CLI](https://learn.microsoft.com/cli/azure/authenticate-azure-cli-interactively) for details.
    
1. When prompted, follow the instructions to open the sign-in page in a new tab and enter the authentication code provided and your Azure credentials. Then complete the sign in process in the command line, selecting the subscription containing your Azure AI Foundry hub if prompted.

1. In the terminal to test your new filter.

    **C#**
    ```
    // Enter the command
    dotnet run
    ```
    **Python**
    ```python
    # Enter the command
    python filters.py
    ```

    Try denying the approval request. You should see a response similar to the following output:

    ```output
    User: Find me a flight to Tokyo on January 19
    Assistant: I found a flight to Tokyo on the 19th of January. The flight is with Air Japan and the price is $1200.
    User: Can you book this flight for me?
    System Message: The agent requires an approval to complete this operation. Do you approve (Y/N)
    User: N
    Assistant: I'm sorry, but I am unable to book the flight for you.
    ```

Now you successfully added a function filter service to your kernel. Great work!
