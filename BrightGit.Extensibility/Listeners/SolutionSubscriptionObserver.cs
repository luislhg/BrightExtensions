using Microsoft.VisualStudio.ProjectSystem.Query;
using System.Diagnostics;

namespace BrightGit.Extensibility.Listeners;
public class SolutionSubscriptionObserver : IObserver<IQueryResults<IProjectSnapshot>>
{
    public void OnCompleted()
    {
        Debug.WriteLine("SolutionSubscriptionObserver.OnCompleted");
    }

    public void OnError(Exception error)
    { }

    public void OnNext(IQueryResults<IProjectSnapshot> value)
    {
        Debug.WriteLine("SolutionSubscriptionObserver.OnNext");
    }
}