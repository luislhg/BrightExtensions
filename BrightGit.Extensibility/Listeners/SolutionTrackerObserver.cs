using Microsoft.VisualStudio.ProjectSystem.Query;

namespace BrightGit.Extensibility.Listeners;

// Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
#pragma warning disable VSEXTPREVIEW_PROJECTQUERY_TRACKING 

public class SolutionTrackerObserver : IObserver<IQueryTrackUpdates<IProjectSnapshot>>
{
    public void OnCompleted()
    { }

    public void OnError(Exception error)
    { }

    public void OnNext(IQueryTrackUpdates<IProjectSnapshot> value)
    { }
}

// Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
#pragma warning restore VSEXTPREVIEW_PROJECTQUERY_TRACKING