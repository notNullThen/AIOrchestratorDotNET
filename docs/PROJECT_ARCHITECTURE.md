# AIOrchestrator Architecture

This document is a code-oriented reference for future maintenance. It describes the current implementation, including behavior that may be accidental rather than desirable. When it conflicts with source code, source code wins and this document should be corrected.

## System Purpose and Boundary

AIOrchestrator turns a local Ollama text-generation model into a function-calling loop without depending on a model-native tools API. The library teaches the model a strict JSON protocol in a generated prompt, parses calls from plain response text, dispatches them onto a consumer-provided facade, and feeds results back to the model as history.

The library owns orchestration, prompt construction, parsing, reflection dispatch, history, error diagnostics, and minimal Ollama HTTP transport. The consuming application owns the callable methods, their descriptions, domain constraints, side effects, and the decision about whether the selected model can follow the protocol reliably.

There are two projects:

| Project | Role |
| --- | --- |
| `AIOrchestrator/AIOrchestrator.csproj` | Packable `net10.0` library. It has no external runtime package references. |
| `AIOrchestrator.Tests/AIOrchestratorTests.csproj` | `net10.0` MSTest project with internal access to the library through `InternalsVisibleTo`. |

## Component Responsibilities

| Component | Responsibility | Key collaborators |
| --- | --- | --- |
| `AiManager` | Owns run state, builds the management prompt, requests model output, dispatches calls, records results, and controls termination. | `AiAppFacadeBase`, `OllamaClient`, `FunctionsDeserializer`, `MethodInvoker`, `ContextHandler`, `ErrorHandler` |
| `AiAppFacadeBase` | Consumer extension point. Stores the multiple-call mode and exposes the inherited `Exit` tool through an `OnExit` callback. | Consumer facade, `AiManager` |
| `AppDescription` and related types | Describe callable method names, their purpose, and ordered parameters to the model. | Management prompt |
| `FunctionsDeserializer` | Finds zero or more function-shaped JSON objects in response text and deserializes each to `FunctionCall`. | Generated regexes, `System.Text.Json` |
| `MethodInvoker` | Resolves a method by name, converts positional strings to declared parameter types, and invokes it. | Reflection, `TypeDescriptor` |
| `ContextHandler<T>` | Holds the in-memory append-only history and serializes it with camel-case properties and indented JSON. | `AiManager`, prompt, observers |
| `ErrorHandler` | Captures run diagnostics and formats them into the manager's outer exception. | `AiManager`, context |
| `OllamaClient` | Calls Ollama's tags and generate endpoints with `HttpClient`; generation is non-streaming. | Ollama REST API |
| `OllamaModels` | Public convenience API for listing locally available Ollama models. | `OllamaClient.GetTagsAsync` |

## Public API Surface

The main consumer workflow uses these public types:

- `AiManager`: constructed with model name, facade, optional `ApiRequestOptions`, Ollama base URL, and HTTP timeout. Exposes `StartAsync`, `ConversationAsync`, `Exit`, `GetManagementPrompt`, `ContextHandler`, and `ErrorHandler`.
- `AiAppFacadeBase`: subclass with `GetDescription` and `GetConstraints`; constructor selects whether the prompt permits multiple calls in one response.
- `AppDescription`, `FunctionDescription`, and `FunctionParameter`: model-facing tool metadata.
- `FunctionCall` and `FunctionCallResponse`: parsed instruction and history record.
- `ContextHandler<T>`: observable in-memory context with JSON helpers.
- `ErrorHandler`: diagnostic state and message construction.
- `ApiRequestOptions`: currently exposes Ollama `temperature` and `num_predict`.
- `OllamaModels`, `OllamaModel`, `OllamaModelDetails`, and `OllamaTagsResponse`: model discovery.
- `Role`: public enum used internally by request construction; generation defaults to `User`.

`OllamaClient`, wire request/response DTOs, `MethodInvoker`, and generated regex holders are internal implementation details.

## Conversation Lifecycle

### Construction

`AiManager` captures its primary-constructor arguments. It immediately creates one internal `OllamaClient` and one `ContextHandler<FunctionCallResponse>`. `ErrorHandler` and `MethodInvoker` are initialized lazily and then reused.

Only `Temperature` and `NumPredict` are copied from the caller's `ApiRequestOptions` into a fresh options object for each request. Expanding request options therefore requires coordinated changes to the public DTO and this copy operation.

### Starting a run

`StartAsync(userInput, cancellationToken)` performs four actions:

1. Stores the user input.
2. Resets `_shouldExit` to `false`.
3. Updates the error handler's user-input diagnostic.
4. Assigns the facade's `OnExit` callback to `AiManager.Exit` and calls `ConversationAsync`.

It does not clear prior context. A reused manager is therefore a stateful continuation even though the user input is replaced.

### Iteration

`ConversationAsync` returns immediately if the manager is already in the exited state. Otherwise it loops while `_shouldExit` is false:

1. Check cancellation.
2. Render `ManagementPrompt` from current state.
3. Send it to Ollama with streaming disabled.
4. Store the raw `response` text in `ErrorHandler`.
5. Parse zero or more calls.
6. For each non-null call, invoke the facade method and append `{ function, parameters, response }` to context.

Each new prompt includes the complete serialized history. There is no context-window trimming, summarization, maximum-iteration guard, retry policy, or backoff.

### Termination and failure

Normal termination happens only when invoked application code calls the inherited facade `Exit`, whose callback sets the manager's `_shouldExit` flag. The `Exit` call itself is still recorded in context after reflection returns. In multiple-call mode, changing the flag does not break the current `foreach`, so remaining calls from that model response execute before the outer loop ends.

Cancellation observed inside the loop sets `_shouldExit`, then rethrows the original `OperationCanceledException`. A token that is already cancelled at method entry is checked before the `try` block and propagates without changing the flag. Any other exception sets `_shouldExit` and is wrapped with the message `An error occurred during AI conversation execution.` plus the immediate failure message, model name, user input, latest raw model output, and full context. Lower layers often add their own inner exception and context.

## Management Prompt Protocol

`AiManager.ManagementPrompt` is regenerated on access; `GetManagementPrompt()` is a diagnostic/testing view of the same property. It combines:

- a system role declaration;
- single-call or multiple-call instructions;
- the exact `{ "Function": string, "Parameters": string[] }` schema;
- rules directing the model to inspect history and call `Exit` when satisfied;
- serialized `AppDescription` output;
- free-form facade constraints;
- current user input and full history.

Descriptions are not validated against real methods. A mismatch fails only when reflection dispatch occurs. Parameter names in `FunctionParameter` are explanatory; the emitted call contains values only, in array order.

`AppDescription.ToString()` uses indented `System.Text.Json` output with its original PascalCase property names. History uses camelCase through `ContextHandler`'s separate serializer settings.

## Response Parsing

`FunctionsDeserializer.Deserialize` first extracts substrings, then deserializes each independently using case-insensitive property matching.

Extraction starts at text matching an opening object whose first property is `"function"`. For each start, it takes the first later ending matching either `]}` (a call with a parameters array) or `"}` (a function-only object). Whitespace and property-name casing around these delimiters are tolerated.

Consequences of this approach:

- Surrounding prose is ignored if valid-looking calls are present.
- Multiple adjacent objects are supported without a containing JSON array.
- A function-only object gets the `FunctionCall.Parameters` default empty array.
- No recognized start produces an empty list rather than an exception.
- Missing endings or invalid extracted JSON become a `JsonException` containing the raw AI response.
- Property ordering, nested JSON, braces/delimiters inside strings, and richer parameter values are outside the robust parsing contract.

Tests currently cover splitting multiple calls, parameterless calls, casing, and whitespace at endings.

## Reflection Dispatch

`MethodInvoker.Execute` searches the concrete facade type for the emitted function name with instance, static, public, and non-public binding flags. Lookup is name-based and case-sensitive under normal reflection behavior. Overloads can make lookup ambiguous, and tool descriptions do not restrict a call to public methods.

Raw parameters are converted in declaration order using `TypeDescriptor.GetConverter(parameterType).ConvertFromString(...)`:

- Extra model-supplied parameters produce `TargetParameterCountException`.
- Missing parameters are replaced by `string.Empty`.
- Optional/default parameter metadata is ignored.
- Conversion behavior depends on the type converter and may be culture-sensitive.

Invocation is synchronous. An async method returns its task object as the tool response instead of being awaited. Exceptions are wrapped with a JSON rendering of the instruction; reflection-thrown application exceptions remain nested under `TargetInvocationException`.

## Context and Observability

After every successful invocation, `AiManager` stores a `FunctionCallResponse` containing the original name and raw parameter strings plus the returned object. Context JSON is indented, camel-cased, and configured to write enums as strings. The shared serializer options also enable case-insensitive property matching, although `ContextHandler` currently exposes serialization methods only.

`ContextHandler.Context` exposes a read-only view. `OnContextUpdated`, however, passes the backing `List<T>` itself as event data after each append. `GetLastContextPartJson` serializes `LastOrDefault`, producing JSON `null` for an empty context.

There is no built-in logging. Consumers can subscribe to `OnContextUpdated`, inspect `Context`, or call the JSON helpers.

## Ollama Transport

Generation uses `POST {baseUrl}/api/generate` with JSON containing model, prompt, role text, `stream: false`, and optional settings. The complete response body is read before the HTTP status is evaluated so a non-success response can include the raw Ollama error body in the exception. `ResponseHeadersRead` avoids buffering headers and content together, but the response text itself is still fully buffered.

Model discovery uses `GET {baseUrl}/api/tags` and returns the `models` list. The public discovery helper has a working default base URL. The manager path differs: its `ollamaBaseUrl` defaults to `null` and that explicit null is forwarded to the internal client, overriding the client's constructor default. Current manager consumers should pass an absolute URL explicitly.

Each internal client owns a new `HttpClient`; clients and managers are not disposable. There is no injected handler/client seam, authentication, streaming, chat endpoint support, or retry logic.

## Testing and Release

The current deterministic suite contains three MSTest cases:

- two parser tests in `FunctionsDeserializerTests`;
- one missing-argument conversion test in `MethodInvokerTests`.

Run `dotnet test AIOrchestrator.sln --configuration Release`. Avoid making the normal unit suite depend on a live Ollama process. Manager-loop tests need deterministic model responses and a guaranteed `Exit` or bounded failure path so they cannot run indefinitely.

The library project is packable and embeds the root README in the package. On a pushed `v*` tag, GitHub Actions runs `dotnet pack` in Release mode, overrides `PackageVersion` from the tag, obtains a temporary NuGet key through trusted publishing, and pushes the resulting package. Public API changes and README examples should be reviewed as package-consumer changes.

## Change-Impact Guide

- Changing the model call schema requires coordinated updates to `ManagementPrompt`, `FunctionCall`, parser extraction, tests, and possibly `MethodInvoker`.
- Adding an Ollama request option requires updating `ApiRequestOptions` and the per-request copy in `AiManager.GetFunctionAsync`.
- Changing history serialization affects both model behavior and diagnostics; update prompt-oriented tests when adding them.
- Adding native async tool support requires awaiting invocation results before constructing `FunctionCallResponse` and defining behavior for generic/non-generic task-like values.
- Adding context reset or multi-run semantics should explicitly decide whether `StartAsync` starts a fresh conversation or continues one.
- Tightening method visibility or lookup changes the consumer tool contract; add tests for inherited `Exit`, overloads, casing, and inaccessible methods.
- Replacing regex extraction should retain intentional support for multiple top-level objects and function-only calls unless the public protocol is deliberately changed.
