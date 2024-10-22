﻿using BrightGit.Extensibility.Helpers;
using BrightGit.Extensibility.Models;
using Microsoft;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Shell;
using Microsoft.VisualStudio.RpcContracts.Documents;
using Microsoft.VisualStudio.RpcContracts.OpenDocument;
using System.Diagnostics;

namespace BrightGit.Extensibility.Services;
public class TabManagerService
{
    // TODO: Maybe this should be injected and then TabManagerService would need to be Scoped (accordingly to samples/doc in VS API).
    public VisualStudioExtensibility Extensibility { get; set; }

    private readonly TraceSource logger;
    private readonly SettingsService settingsService;
    private readonly TabsStorageService tabsStorageService;

    public TabManagerService(TraceSource traceSource, SettingsService settingsService, TabsStorageService tabsStorageService)
    {
        this.logger = Requires.NotNull(traceSource, nameof(traceSource));
        this.settingsService = settingsService;
        this.tabsStorageService = tabsStorageService;
    }

    public async Task<bool> SaveAndRestoreTabsAsync(string repoDir, string oldBranchName, string newBranchName)
    {
        try
        {
            var sw = Stopwatch.StartNew();

            var shell = Extensibility.Shell();
            var documents = Extensibility.Documents();
            var configuration = Extensibility.Configuration();
            var workspaces = Extensibility.Workspaces();

            // Save the current tabs to old branch name.
            var tabsSaved = await SaveTabsAsync(false, oldBranchName, shell, documents, workspaces, CancellationToken.None);

            // Refresh snapshots.
            shell = Extensibility.Shell();
            documents = Extensibility.Documents();
            workspaces = Extensibility.Workspaces();

            // Restore the tabs from new branch name (if any).
            var tabsRestored = await RestoreTabsAsync(false, newBranchName, shell, documents, workspaces, CancellationToken.None);

            sw.Stop();

            // Inform the user.
            var msg = $"Saved {tabsSaved?.Tabs.Count ?? 0} tabs to '{tabsSaved?.BranchName}'\n" +
                      $"Restored {tabsRestored?.Tabs.Count ?? 0} tabs from '{tabsRestored?.BranchName}'\n" +
                      $"({sw.ElapsedMilliseconds}ms)";
            Debug.WriteLine(msg.Replace("\n", ". "));
            await shell.ShowPromptAsync(msg, PromptOptions.OK, CancellationToken.None);

            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            logger.TraceEvent(TraceEventType.Error, 0, ex.ToString());
            return false;
        }
    }

    public async Task<TabsInfo> SaveTabsAsync(bool isCustom,
                                              string gitBranchName,
                                              ShellExtensibility shell,
                                              DocumentsExtensibility documents,
                                              WorkspacesExtensibility workspaces,
                                              CancellationToken cancellationToken)
    {
        try
        {
            var sw = Stopwatch.StartNew();

            // Check if we are in a solution.
            var solutionName = await VSHelper.GetSolutionNameAsync(workspaces, cancellationToken);
            if (string.IsNullOrWhiteSpace(solutionName))
            {
                await shell.ShowPromptAsync("Please open a solution before saving tabs", PromptOptions.OK, cancellationToken);
                return null;
            }

            // Get all opened documents.
            var openedDocuments = await documents.GetOpenDocumentsAsync(cancellationToken);
            Debug.WriteLine($"openedDocuments: {openedDocuments.Count}");

            // Check if there are any documents opened.
            if (openedDocuments.Count == 0)
            {
                Debug.WriteLine("No documents opened to save tabs");
                return null;
            }

            // Check if any document is dirty.
            foreach (var document in openedDocuments)
            {
                if (document.IsDirty)
                {
                    await shell.ShowPromptAsync("Please save all documents before saving tabs", PromptOptions.OK, cancellationToken);
                    return null;
                }
            }

            // TODO: In the future find a way to figure out/save the pinned state.

            // Generate tabs info.
            TabsInfo tabsInfo = new()
            {
                Id = $"{solutionName}.{gitBranchName}",
                Name = $"{solutionName}.{gitBranchName}",
                SolutionName = solutionName,
                BranchName = gitBranchName,
                DateSaved = DateTime.Now,
                Tabs = openedDocuments.Where(d => !d.IsReadOnly)
                                      .Select((d, i) => new TabDocumentInfo() { FilePath = d.Moniker.LocalPath, Index = i, IsPinned = false })
                                      .ToList()
            };

            // Add the tabs to the correspondent storage.
            if (isCustom)
                tabsStorageService.AddTabsCustom(tabsInfo);
            else
                tabsStorageService.AddTabsBranch(tabsInfo);

            // Save to disk.
            tabsStorageService.Save();

            // Close all documents if needed.
            bool closeTabs = (isCustom && settingsService.Data.Tabs.CloseTabsOnSave) || (!isCustom && settingsService.Data.Tabs.CloseTabsOnBranchChange);
            if (closeTabs)
            {
                // TODO: In the future use some faster way to close all documents (similar to VS -> "Close All Tabs").

                await Task.WhenAll(openedDocuments.Select(document => document.CloseAsync(SaveDocumentOption.PromptSave, Extensibility, cancellationToken)));
            }

            sw.Stop();
            Debug.WriteLine($"Saved tabs {openedDocuments.Count} for {solutionName}.{gitBranchName} ({sw.ElapsedMilliseconds}ms)");

            return tabsInfo;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            logger.TraceEvent(TraceEventType.Error, 0, ex.ToString());
            await shell.ShowPromptAsync($"Error saving tabs: {ex.Message}", PromptOptions.OK, cancellationToken);
            return null;
        }
    }

    public async Task<TabsInfo> RestoreTabsAsync(bool isCustom,
                                                 string gitBranchName,
                                                 ShellExtensibility shell,
                                                 DocumentsExtensibility documents,
                                                 WorkspacesExtensibility workspaces,
                                                 CancellationToken cancellationToken)
    {
        try
        {
            var sw = Stopwatch.StartNew();

            // Check if we are in a solution.
            var solutionName = await VSHelper.GetSolutionNameAsync(workspaces, cancellationToken);
            if (string.IsNullOrWhiteSpace(solutionName))
            {
                await shell.ShowPromptAsync("Please open a solution before restoring tabs", PromptOptions.OK, cancellationToken);
                return null;
            }

            string tabsSaveKey = $"{solutionName}.{gitBranchName}";
            var tabsInfo = isCustom
                         ? tabsStorageService.Data.TabsCustom.FirstOrDefault(x => x.Id == tabsSaveKey)
                         : tabsStorageService.Data.TabsBranch.FirstOrDefault(x => x.Id == tabsSaveKey);

            if (tabsInfo != null)
            {
                // TODO: No longer intersecting because .xaml files (MAUI projet) are not being closed properly.
                // We close via the API, VS closes it, but GetOpenDocuments is still listing it.

                // Intersect opened documents with saved documents (spare processing time and inform the user about re-restoring something already opened).
                //var openedDocuments = await documents.GetOpenDocumentsAsync(cancellationToken);
                //var tabDocuments = tabsInfo.Tabs.Where(sd => !openedDocuments.Any(od => od.Moniker.LocalPath == sd.FilePath)).ToList();
                var tabDocuments = tabsInfo.Tabs;

                // Check if there are any documents to restore.
                if (tabDocuments.Count > 0)
                {
                    // TODO: In the future find some way to open documents faster, without activating/viewing one by one.

                    // Open documents.
                    var openDocumentOptions = new OpenDocumentOptions(activate: false);
                    await Task.WhenAll(tabDocuments.Select(document => documents.OpenDocumentAsync(new Uri(document.FilePath), openDocumentOptions, cancellationToken)));

                    // TODO: We need some way to pin documents again (once we figure out how to do it).

                    sw.Stop();
                    Debug.WriteLine($"Restored {tabDocuments.Count} tabs for {solutionName}.{gitBranchName} ({sw.ElapsedMilliseconds}ms)");
                }
                else
                {
                    Debug.WriteLine($"No tabs to restore for {solutionName}.{gitBranchName}");
                }
            }
            else
            {
                Debug.WriteLine($"No tabs to restore for {solutionName}.{gitBranchName}");
            }

            return tabsInfo;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            logger.TraceEvent(TraceEventType.Error, 0, ex.ToString());
            await shell.ShowPromptAsync($"Error restoring tabs: {ex.Message}", PromptOptions.OK, cancellationToken);
            return null;
        }
    }
}
