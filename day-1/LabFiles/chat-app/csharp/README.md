# .NET Chat App

This console application connects to an Azure AI Foundry project and chats with the deployed `gpt-4o` model using the Azure AI Projects preview SDK.

## Prerequisites

- .NET 8 SDK
- Access to an Azure AI Foundry project with a deployed chat model (for example, `gpt-4o`).

## Configuration

1. Duplicate the provided `.env.example` file (rename the copy to `.env`) or create one from scratch.
2. Set `PROJECT_ENDPOINT` to your Azure AI Foundry project endpoint (or model-specific target URI).
3. Set `MODEL_DEPLOYMENT` to the name of the deployed model (for example, `gpt-4o`).

## Authenticate

The app uses `DefaultAzureCredential`, so make sure you're signed in to Azure CLI before running it:

```bash
az login
```

If you work in multiple tenants or subscriptions, pass the `--tenant` or `--subscription` parameters to target the correct environment.

## Run the app

```bash
dotnet restore
dotnet run
```

When prompted, enter your questions. Type `quit` to exit.
