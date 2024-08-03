# Bright Extensions Suite

**Bright Extensions Suite** is a collection of Visual Studio extensions designed to enhance productivity for .NET developers in general.
The suite includes two main projects at the moment: **BrightGit** and **BrightXaml**.

---
## Projects

### Bright Xaml
_https://marketplace.visualstudio.com/items?itemName=luislhg.BrightXaml_

**BrightXaml** offers features specifically for C# XAML developers working with WPF, MAUI, and WinUI. It includes the following features:

- **Show View and ViewModel**: Easily open and switch between the corresponding view and view model files _(CTRL+E+Q)_.
  <details>
    <summary>Click to see demo</summary>
    <img src="https://github.com/user-attachments/assets/2f30bab8-32d0-4d25-988f-208dd86a77a0" width="80%" />
  </details>
  
- **Convert Properties to Properties with Set Method**: Converts regular properties to properties with a Set (INPC) method - easily use bindings with your properties _(CRTL+E+P)_.
  <details>
    <summary>Click to see demo</summary>
    <img src="https://github.com/user-attachments/assets/c8c0e95f-cf03-43b0-bffb-a156d687e9b2" width="80%" />
  </details>

- _(Beta)_ **Go To Binding Definition**: When pressing F12 for a Command which was generated by MVVM Toolkit [RelayCommand], automatically open the ViewModel where the actual method is - instead of the generator source.
  <details>
    <summary>Click to see demo</summary>
    <img src="https://github.com/user-attachments/assets/0821d763-13b5-45ec-aec1-fc3d6495a825" width="80%" />
  </details>

- **Format XAML**: Simple and quick Format Xaml which respects your lines, tags and indentation preferences - improving code readability and maintainability. 
  <details>
    <summary>Click to see demo</summary>
    <img src="https://github.com/user-attachments/assets/dc36425a-403d-4ac5-b8bb-74dfc35aaaf9" width="80%" />
  </details>

- **Clean Bin and Obj**: Cleans the solution for real by removing bin and obj folders from all projects.
- **Kill XAML Designer**: Kills the XAML designer process to fix/restart any issues with the designer.

### _(BETA)_ Bright Git

**BrightGit** provides a set of features to streamline daily VS/Git operations and Entity Framework (EF) migrations within Visual Studio. It includes the following features:

- **Auto Apply EF Migrations**: Automatically detects and applies EF migrations _(if any)_ when changing branches.
- **Auto Save/Restore Tabs**: Saves and restores all open tabs when switching branches, preserving the context of each branch/issue.

## Releases

**BrightXaml:** https://marketplace.visualstudio.com/items?itemName=luislhg.BrightXaml

**BrightGit:** This is still in beta and can only be installed via local release at this moment

---
## Roadmap / FAQ

The roadmap is subject to change based on feedback and development progress.

- Several tweaks and QoL improvements are dependent on the new Visual Studio 2022 API.
- Save/Restore Tabs should be more sophisticated, with a window for manual tab handling.
- BrightGit could eventually be renamed to BrightVS (includes everything that most .NET developers use).
- Adding common templates for items and projects is under consideration.
- A third extension to the suite is under consideration.

---
## Principles guiding this project
- Developers should focus on bringing ideas to life and not on repetitive tasks.
- Extension won't do things that Visual Studio can already do.
- Extension only uses the new Extensibility API (out-of-proc).
- Extension only supports projects .NET 6.0 and above.
- Extension should only perform fast and snap actions.
- Extension should be easy to use and/or integrate with Visual Studio workflow.
- Everything that is generated (output) should be standard or EASILY configurable.

---
## Getting Started to Build / Contribute

### Prerequisites

- Visual Studio 2022 (>= 17.10)
- .NET 8.0 SDK

### Projects Structure

- `BrightExtensions.sln`: The solution file.
- `BrightGit`: The project containing the BrightGit extension.
- `BrightXaml`: The project containing the BrightXaml extension.
- `BrightGit.SharpCommon`: A shared library for common models and enums used by BrightGit and BrightGit.SharpRun.
- `BrightGit.SharpAutoMigrator`: A console application to be used implicitly by git hooks and auto apply EF Core migrations when branches have been switched.
- `BrightGit.SharpRun`: A console application to handle Git operations using LibGit2Sharp.
- `BrightGit.SharpHook`: A console application to handle Git events (hooks) using LibGit2Sharp (VS API lacks git events, this project adds native git hook handling).

### Contributing

Contributions are welcome! Please follow these steps to contribute:

1. Fork the repository.
2. Create a new branch (`git checkout -b feature/YourFeature`).
3. Commit your changes (`git commit -am 'Add some feature'`).
4. Push to the branch (`git push origin feature/YourFeature`).
5. Create a new Pull Request.

### License

This project is licensed under the MIT License. See the LICENSE file for details.

### Contact

For any questions or suggestions, please open an issue or contact luislhg (repository owner).
