using BrightXaml.Extensibility.Models;

namespace BrightXaml.Extensibility.Utilities;
public static class ShowDefinitionHelper
{
    public static string GetBindingPathAtCaret(string lineContent, int caretOffset)
    {
        return string.Empty;
    }

    public static string GetWordAtCaret(string lineContent, int caretOffset)
    {
        if (string.IsNullOrEmpty(lineContent) || caretOffset < 0 || caretOffset > lineContent.Length)
            return string.Empty;

        // Find the start of the word.
        int wordStart = caretOffset;
        while (wordStart > 0 && char.IsLetterOrDigit(lineContent[wordStart - 1]))
        {
            wordStart--;
        }

        // Find the end of the word.
        int wordEnd = caretOffset;
        while (wordEnd < lineContent.Length && char.IsLetterOrDigit(lineContent[wordEnd]))
        {
            wordEnd++;
        }

        // Extract the word.
        return lineContent.Substring(wordStart, wordEnd - wordStart);
    }

    public static bool IsWordBindingCommand(string bindingWord)
    {
        return bindingWord.EndsWith("Command", StringComparison.InvariantCultureIgnoreCase);
    }

    public static bool IsWordBindingInViewModel(string bindingWord, string viewModelPath)
    {
        return File.ReadAllText(viewModelPath).Contains(bindingWord, StringComparison.Ordinal);
    }

    public static BindingDefinitionOffsets GetRelayCommandOffsets_MVVMToolkit(string bindingWord, string viewModelPath)
    {
        var binding = new BindingDefinitionOffsets();
        binding.BindingWord = bindingWord;

        binding.Line = GetRelayCommandLine_MVVMToolkit(bindingWord, viewModelPath);
        binding.OffsetFromFile = GetRelayCommandOffset_MVVMToolkit(bindingWord, viewModelPath);
        binding.OffsetFromLine = GetRelayCommandOffsetFromLine_MVVMToolkit(bindingWord, viewModelPath);

        return binding;
    }

    public static int GetRelayCommandOffsetFromLine_MVVMToolkit(string bindingWord, string viewModelPath)
    {
        // Remove the Command suffix.
        if (bindingWord.EndsWith("Command"))
            bindingWord = bindingWord[..^7];

        // Add the opening parenthesis to find the method only.
        bindingWord += "(";

        var viewModelContent = File.ReadAllText(viewModelPath);
        var bindingIndex = viewModelContent.IndexOf(bindingWord);

        // Remove the count of characters before the current line.
        var lineIndex = viewModelContent.Substring(0, bindingIndex).Count(c => c == '\n');
        var lines = viewModelContent.Split('\n');
        var offset = bindingIndex - lines.Take(lineIndex).Sum(l => l.Length + 1);

        return offset;
    }

    public static int GetRelayCommandOffset_MVVMToolkit(string bindingWord, string viewModelPath)
    {
        // Remove the Command suffix.
        if (bindingWord.EndsWith("Command"))
            bindingWord = bindingWord[..^7];

        // Add the opening parenthesis to find the method only.
        bindingWord += "(";

        var viewModelContent = File.ReadAllText(viewModelPath);
        var bindingIndex = viewModelContent.IndexOf(bindingWord);
        return bindingIndex;
    }

    public static int GetRelayCommandLine_MVVMToolkit(string bindingWord, string viewModelPath)
    {
        // Remove the Command suffix.
        if (bindingWord.EndsWith("Command"))
            bindingWord = bindingWord[..^7];

        // Add the opening parenthesis to find the method only.
        bindingWord += "(";

        var viewModelContent = File.ReadAllLines(viewModelPath);
        for (int i = 0; i < viewModelContent.Length; i++)
        {
            if (viewModelContent[i].Contains(bindingWord))
            {
                // Check if the line contains the RelayCommand attribute.
                if (viewModelContent[i].Contains("[RelayCommand"))
                {
                    return i;
                }
                else
                {
                    // Check the lines above if the RelayCommand attribute is there.
                    for (int j = i - 1; j >= 0; j--)
                    {
                        if (viewModelContent[j].Contains("[RelayCommand"))
                        {
                            // j is the line number of the RelayCommand attribute.
                            // i is the line number of the binding word.
                            return i;
                        }
                        else if (viewModelContent[j].Contains('}') || viewModelContent[j].Contains('{'))
                        {
                            break;
                        }
                    }
                }
            }
        }

        return -1;
    }

    public static int GetObservablePropertyOffsetFromLine_MVVMToolkit(string bindingWord, string viewModelPath)
    {
        var viewModelContent = File.ReadAllText(viewModelPath);
        var bindingIndex = viewModelContent.IndexOf(bindingWord);

        // Remove the count of characters before the current line.
        var lineIndex = viewModelContent.Substring(0, bindingIndex).Count(c => c == '\n');
        var lines = viewModelContent.Split('\n');
        var offset = bindingIndex - lines.Take(lineIndex).Sum(l => l.Length + 1);

        return offset;
    }

    public static int GetObservablePropertyOffset_MVVMToolkit(string bindingWord, string viewModelPath)
    {
        var viewModelContent = File.ReadAllText(viewModelPath);
        var bindingIndex = viewModelContent.IndexOf(bindingWord);
        return bindingIndex;
    }

    public static int GetObservablePropertyLine_MVVMToolkit(string fieldName, string viewModelPath)
    {
        var viewModelContent = File.ReadAllLines(viewModelPath);
        for (int i = 0; i < viewModelContent.Length; i++)
        {
            if (viewModelContent[i].Contains(fieldName))
            {
                // Check if the line contains the ObservableProperty attribute.
                if (viewModelContent[i].Contains("[ObservableProperty]"))
                {
                    return i;
                }
                else
                {
                    // Check the lines above if the ObservableProperty attribute is there.
                    for (int j = i - 1; j >= 0; j--)
                    {
                        if (viewModelContent[j].Contains("[ObservableProperty]"))
                        {
                            // j is the line number of the ObservableProperty attribute.
                            // i is the line number of the binding word.
                            return i;
                        }
                        else if (viewModelContent[j].Contains('}') || viewModelContent[j].Contains('{'))
                        {
                            break;
                        }
                    }
                }
            }
        }

        return -1;
    }
}
