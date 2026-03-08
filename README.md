# openrouter-blazor-chat-test

A Blazor Server application for chatting with free [OpenRouter](https://openrouter.ai) AI models, with optional document context (RAG) support.

## Features

- 🤖 **Chat with free OpenRouter models** — select from a configurable list of free models
- 📄 **RAG Document Upload** — upload `.pdf`, `.txt`, `.md`, `.csv`, `.json`, `.xml`, or `.html` files to inject as context
- ⌨️ **Keyboard shortcut** — press `Enter` to send, `Shift+Enter` for a new line
- 🔒 **API key via Developer Secrets** — never commit your key to source control

## Getting Started

### 1. Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- An [OpenRouter API key](https://openrouter.ai/keys) (free account supported)

### 2. Configure the API Key

Store your OpenRouter API key using [.NET User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets):

```bash
cd src/OpenRouterChat
dotnet user-secrets set "OpenRouter:ApiKey" "your-api-key-here"
```

### 3. Configure Models (optional)

Edit `src/OpenRouterChat/appsettings.json` to add, remove, or rename the free models listed under `OpenRouter.Models`. Each entry requires:

- `Id` — the OpenRouter model identifier (e.g., `openrouter/cypher-alpha:free`)
- `DisplayName` — a human-friendly label shown in the dropdown

### 4. Run the App

```bash
cd src/OpenRouterChat
dotnet run
```

Open your browser at `http://localhost:5018` (or the URL shown in the terminal).

## Project Structure

```
src/OpenRouterChat/
├── Components/
│   ├── Layout/           # Main layout and navigation
│   └── Pages/
│       └── Home.razor    # Chat UI page
├── Models/
│   ├── ChatMessage.cs    # Chat message model
│   └── OpenRouterSettings.cs  # Configuration model
├── Services/
│   ├── OpenRouterService.cs   # OpenRouter API client
│   └── DocumentService.cs     # PDF/text extraction for RAG
├── appsettings.json      # App config (models list, base URL)
└── Program.cs            # DI setup and app pipeline
```

## Security

- API keys are stored in **Developer Secrets** (not in `appsettings.json`)
- Uploaded documents are processed in-memory and never persisted to disk
