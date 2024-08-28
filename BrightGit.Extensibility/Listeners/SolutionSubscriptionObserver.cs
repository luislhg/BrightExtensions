using Microsoft.VisualStudio.ProjectSystem.Query;

namespace BrightGit.Extensibility.Listeners;
public class SolutionSubscriptionObserver : IObserver<IQueryResults<IProjectSnapshot>>
{
    public void OnCompleted()
    { }

    public void OnError(Exception error)
    { }

    public void OnNext(IQueryResults<IProjectSnapshot> value)
    { }
}