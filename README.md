# AIOrchestrator

[![NuGet](https://img.shields.io/nuget/v/AIOrchestrator)](https://www.nuget.org/packages/AIOrchestrator)
[![GitHub](https://img.shields.io/badge/github-repo-black.svg)](https://github.com/notNullThen/ai-orchestrator-dotnet)

This project is a .NET library that structurizes and handles fully local agentic functions (tools) execution via LLM model.

This NuGet is used in Local Agentic AI Demo project - https://github.com/notNullThen/ai-orchestrator-dotnet

### 🎬 YouTube Video Demo: https://youtu.be/qbJpvD6T8rs

Features:
- Runs locally
- Uses any local LLM model from Ollama
- Tries to be **model-agnostic**. Currently works well with gemma4:e4b and ministral-3:3b
- Supports LLM responses containing multiple functions.

### Installation

Install the NuGet package:

```bash
dotnet add package AIOrchestrator
```

### Quick Usage

```csharp
using AIOrchestrator.Core;
using AIOrchestrator.Core.AiAppFacade;
using AIOrchestrator.Core.AiAppFacade.Types;
using AIOrchestrator.OllamaClient.Types;

// 1. Define your app capabilities by inheriting from AiAppFacadeBase
public class MyAiApp : AiAppFacadeBase
{
    public string GetSystemStatus() => "All systems nominal.";

    public override string GetConstraints() => "Do your functionality...";

    public override AppDescription GetDescription() => [
        new() { Name = nameof(GetSystemStatus), Description = "Returns current system status" },
        new() { Name = nameof(Exit), Description = "Terminates the interaction" }
    ];
}

// 2. Initialize and run with optional parameters
var options = new ApiRequestOptions { Temperature = 0.7f };
var ai = new AiManager(
    modelName: "qwen2.5-coder:7b", 
    appInstance: new MyAiApp(),
    options: options,
    ollamaBaseUrl: "http://localhost:11434"
);
await ai.StartAsync(userInput);
```