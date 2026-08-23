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

### High-level overview of how it works

1. The available C# functions (tools) are structurally defined and described using the `AiAppFacadeBase` class.
1. A prepared prompt containing instructions, constraints, and tool definitions is sent to the LLM.
1. Depending on the user-defined configuration, the LLM responds with one or more function calls.
1. AIOrchestrator parses the response string and extracts only valid JSON objects.
1. The parsed JSON objects are deserialized and placed into an array.
1. The array is used to execute the C# functions sequentially.
1. The loop repeats until the user's request is fulfilled.
1. The constraints and instructions cause the LLM to call the `Exit()` function when the user's request is fulfilled.
1. The `Exit()` function breaks the loop.


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
