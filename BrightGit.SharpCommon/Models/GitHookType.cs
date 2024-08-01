namespace BrightGit.SharpCommon.Models;
public enum GitHookType
{
    None,
    ApplypatchMsg,
    CommitMsg,
    PostUpdate,
    PreApplypatch,
    PreCommit,
    PreMergeCommit,
    PrepareCommitMsg,
    PrePush,
    PreRebase,
    PreReceive,
    PushToCheckout,
    Update
}
