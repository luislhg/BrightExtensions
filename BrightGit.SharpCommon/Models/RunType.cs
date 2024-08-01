namespace BrightGit.SharpCommon.Models;
public enum RunType
{
    EFMigrationDown,
    EFMigrationUp,
    GitUndoChanges,
    GitStashChanges,
    GitStashPop,
    GitStashApply,
}
