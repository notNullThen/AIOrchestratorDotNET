namespace AIOrchestrator.Core.AiAppFacade.Types;

public class FunctionDescription
{
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required List<FunctionParameter> Parameters { get; init; }
}
