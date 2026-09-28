namespace Application.AiAgents;

public interface IAiAgent<in TArgument, TResult>
{
    string SystemPrompt { get; }
    string UserPrompt { get; }
    
    Task<TResult> Handle(TArgument argument);
}