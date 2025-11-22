using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.AzureAISearch;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;
using Microsoft.SemanticKernel.Data;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Models;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Embeddings;

#region Index Schema


#endregion

#region Azure AI Search Service


#endregion

#region Azure AI Search SK Plugin

#endregion

class Program
{
    static async Task Main(string[] args)
    {
        #pragma warning disable SKEXP0010
        string filePath = Path.GetFullPath("appsettings.json");
        var config = new ConfigurationBuilder()
            .AddJsonFile(filePath)
            .Build();

        // 1. Create the Kernel Builder
        

        // 2. Add Azure OpenAI Chat Completion
        


        // 3. SearchIndexClient from Azure .NET SDK to perform search operations.
        

        // 4. Custom AzureAISearchService to configure request parameters and make a request.
        

        // 5. Embedding generation service to convert string query to vector
        

        // 6. Register Azure AI Search Plugin
        

        // 7. Create kernel
        

        // 8. Invoke the prompt
        
    }
}
