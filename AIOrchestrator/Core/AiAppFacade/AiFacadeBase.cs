namespace AIOrchestrator.Core.AiAppFacade;

using Types;

public abstract class AiAppFacadeBase(bool multipleFunctionsAtOneResponse)
{
    public bool MultipleFunctionsAtOneResponse => multipleFunctionsAtOneResponse;

    public Action? OnExit { get; set; }

    public void Exit()
    {
        if (OnExit == null)
        {
            throw new Exception(
                $"The {nameof(OnExit)} action is not set. Set it in {nameof(AiManager)} class."
            );
        }

        OnExit.Invoke();
    }

    public abstract AppDescription GetDescription();

    public abstract string GetConstraints();
}
