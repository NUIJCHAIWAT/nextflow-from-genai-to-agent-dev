---
lab:
    title: 'Create Semantic Kernel plugins'
    description: 'Learn how to build an AI flight booking assistant.'
---

# Create Semantic Kernel plugins

In this exercise, you'll use Semantic Kernel to create an AI assistant that can search for and book flights for a user. You'll create custom plugin functions to help accomplish the task. Let's get started!


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

Now that you deployed a model, you're ready to create a Semantic Kernel client app that defines custom plugin functions. Some code is provided for you in a GitHub repository.

### Prepare the environment

1. In the terminal, navigate to the folder containing the application code files:  

    > **Note**: Follow the steps for your chosen programming language.

    **C#**

    ```
    cd part-2/Labfiles/03-create-plugins/C-sharp
    ```


    **Python**

    ```
    cd part-2/Labfiles/03-create-plugins/Python
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

5. After you replace the placeholders, in the code editor, use the **CTRL+S** command.

Now you're ready to begin the exercise. Good luck!

### Create a flight booking plugin

Now you create a plugin class for your travel assistant. The class includes functions to search for available flights and book a selected flight for the user. A `flights.json` file is included in the project which contains some sample flight information.

> **Tip**: As you add code, be sure to maintain the correct indentation.

1. Enter the following command to edit the code file that has been provided:

    **C#**

    ```
    code FlightBookingPlugin.cs
    ```


    **Python**

    ```
    code flight_booking_plugin.py
    ```

    
1. Add the following code under the comment **Create a plugin function with kernel function attributes**:

    **C#**
    ```c#
    // Create a plugin function with kernel function attributes
    [KernelFunction("search_flights")]
    [Description("Searches for available flights based on the destination and departure date in the format YYYY-MM-DD")]
    [return: Description("A list of available flights")]
    public List<FlightModel> SearchFlights(string destination, string departureDate)
    {
        // Filter flights based on destination
    }
    ```

    **Python**
    ```python
    # Create a plugin function with kernel function attributes
    @kernel_function(description="Searches for available flights based on the destination and departure date in the format YYYY-MM-DD")
    def search_flights(self, destination, departure_date):
        # Filter flights based on destination
    ```
    
    

    The kernel function decorators help the AI understand how to call your function.

1. Add the following code under the comment **Filter flights based on destination**

    **C#**
    ```c#
    // Filter flights based on destination
    return flights.Where(flight =>
        flight.Destination.Equals(destination, StringComparison.OrdinalIgnoreCase) &&
        flight.DepartureDate.Equals(departureDate)).ToList();
    ```

    **Python**
    ```python
    # Filter flights based on destination
    matching_flights = [
        flight for flight in self.flights
        if flight.Destination.lower() == destination.lower() and flight.DepartureDate == departure_date
    ]
    return matching_flights
    ```

    

1. Add the following code under the comment **Create a kernel function to book flights**:


    **C#**
    ```c#
    // Create a kernel function to book flights
    [KernelFunction("book_flight")]
    [Description("Books a flight based on the flight ID provided")]
    [return: Description("Booking confirmation message")]
    public string BookFlight(int flightId)
    {
        // Add logic to book a flight
    }
    ```

    **Python**
    ```python
    # Create a kernel function to book flights
    @kernel_function(description="Books a flight based on the flight ID provided")
    def book_flight(self, flight_id):
        # Add logic to book a flight
    ```


1. Add the following code under the comment **Add logic to book a flight**:

    **C#**
    ```c#
    // Add logic to book a flight
    var flight = flights.FirstOrDefault(f => f.Id == flightId);
    if (flight == null)
    {
        return "Flight not found. Please provide a valid flight ID.";
    }

    if (flight.IsBooked)
    {
        return $"You've already booked this flight.";
    }

    flight.IsBooked = true;
    SaveFlightsToFile();
    
    return @$"Flight booked successfully! Airline: {flight.Airline}, 
            Destination: {flight.Destination}, 
            Departure: {flight.DepartureDate}, 
            Price: ${flight.Price}.";
    ```


    **Python**
    ```python
    # Add logic to book a flight
    flight = next((f for f in self.flights if f.Id == flight_id), None)
    
    if flight is None:
        return "Flight not found. Please provide a valid flight ID."

    if flight.IsBooked:
        return "You've already booked this flight."

    flight.IsBooked = True
    self.save_flights_to_file()

    return (
        f"Flight booked successfully! Airline: {flight.Airline}, "
        f"Destination: {flight.Destination}, Departure: {flight.DepartureDate}, "
        f"Price: ${flight.Price}."
    )
    ```

    

1. Enter the following command to edit the code file that has been provided:


    **C#**
    ```
    code Program.cs
    ```

    **Python**
    ```
    code plugins.py
    ```


1. Add the following code under the comment **Add a plugin to the kernel**:

     **C#**
    ```c#
    // Add a plugin to the kernel*
    kernel.Plugins.AddFromType<FlightBookingPlugin>("FlightBookingPlugin");
    ```

    **Python**
    ```python
    # Add the plugin to the kernel
    kernel.add_plugin(FlightBookingPlugin(), "flight_booking_plugin")
    ```

   

1. Add the following code under the comment **Configure function choice behavior**

    **C#**
    ```c#
    // Configure function choice behavior
    OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new() 
    {
        FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
    };
    ```

    **Python**
    ```python
    # Configure function choice behavior
    settings = AzureChatPromptExecutionSettings(
        function_choice_behavior=FunctionChoiceBehavior.Auto(),
    )
    ```


2. After you've updated the code, use the **CTRL+S** command to save your changes.

### Sign into Azure and run the app

1. In the Terminal, enter the following command to sign into Azure.

    ```
    az login --use-device-code
    ```

    **<font color="red">You must sign into Azure - even though the cloud shell session is already authenticated.</font>**

    > **Note**: In most scenarios, just using *az login* will be sufficient. However, if you have subscriptions in multiple tenants, you may need to specify the tenant by using the *--tenant* parameter. See [Sign into Azure interactively using the Azure CLI](https://learn.microsoft.com/cli/azure/authenticate-azure-cli-interactively) for details.
    
1. When prompted, follow the instructions to open the sign-in page in a new tab and enter the authentication code provided and your Azure credentials. Then complete the sign in process in the command line, selecting the subscription containing your Azure AI Foundry hub if prompted.

3. In the cloud shell command-line pane beneath the code editor, enter the following command to run the code:


    **C#**
    ```
    dotnet run
    ```

     **Python**
    ```
    python plugins.py
    ```


    You should see output similar to the following:

    ```output
    User: Find me a flight to Tokyo on the 19
    Assistant: I found a flight to Tokyo on January 19th. 

    - Airline: Air Japan
    - Price: $1200

    Would you like to book this flight?
    ```

4. Enter "Yes" to invoke the `BookFlight` function

    You should see output similar to the following:

    ```output
    User: Yes
    Assistant: Congratulations! Your flight to Tokyo on January 19th with Air Japan has been successfully booked. 
    The total price for the flight is $1200.
    ```

Now you successfully added plugin functions to your kernel that can be invoked automatically. Great work!

### Advertise selected functions

Now you configure your kernel so that only selected functions are available to assist in providing travel information. The assistant should be able to answer questions about flight availability, but not allow the user to book a flight.

1. Update the code under the comment **Configure function choice behavior** to the following code:

    **C#**
    ```c#
    // Configure function choice behavior
    KernelFunction searchFlights = kernel.Plugins.GetFunction("FlightBookingPlugin", "search_flights");

    PromptExecutionSettings openAIPromptExecutionSettings = new() 
    {
        FunctionChoiceBehavior = FunctionChoiceBehavior.Required(functions: [searchFlights]) 
    };
    ```


    **Python**
    ```python
        # Configure function choice behavior
        settings=AzureChatPromptExecutionSettings(
            function_choice_behavior=FunctionChoiceBehavior.Auto(filters={"included_functions": ["search_flights"]}),
        )
    ```

    
1. After you've updated the code, use the **CTRL+S** command to save your changes.

1. Run the code as is to observe the difference in behavior.

    If you request to book the flight, you should see output similar to the following:

    ```output
    User: Find me a flight to Tokyo on the 19
    Assistant: I found a flight to Tokyo on the 19th of January 2025. The flight is with Air Japan and the price is $1200. Please let me know if you would like to book this flight.
    User: Yes
    Assistant: I'm sorry, but I am just a virtual assistant and I don't have the capability to book flights.
    ```

## Summary

In this exercise, you used the Semantic Kernel SDK and Azure OpenAI to create an AI assistant that can help users search for and book flights. Great work!

## Clean up

If you've finished exploring Azure OpenAI and Semantic Kernel, you should delete the resources you have created in this exercise to avoid incurring unnecessary Azure costs.

1. Return to the browser tab containing the Azure portal (or re-open the [Azure portal](https://portal.azure.com) at `https://portal.azure.com` in a new browser tab) and view the contents of the resource group where you deployed the resources used in this exercise.
1. On the toolbar, select **Delete resource group**.
1. Enter the resource group name and confirm that you want to delete it.
