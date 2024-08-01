using System.Diagnostics;
using System.Windows;

namespace BrightXaml.Extensibility.Helpers;
public static class ClipboardHelper
{
    /// <summary>
    /// Most reliable and fastest way to copy text to clipboard.
    /// </summary>
    public static void SetClipboard(string text)
    {
        if (text == null)
            throw new ArgumentNullException("Attempt to set clipboard with null");

        var clipboardExecutable = new Process();
        clipboardExecutable.StartInfo = new ProcessStartInfo
        {
            RedirectStandardInput = true,
            FileName = @"clip",
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        clipboardExecutable.Start();

        // CLIP uses STDIN as input.
        clipboardExecutable.StandardInput.Write(text);

        // When we are done writing all the string, close it so clip doesn't wait and get stuck.
        clipboardExecutable.StandardInput.Close();

        return;
    }

    /// <summary>
    /// Really slow and unreliable.
    /// </summary>
    public static void SetTextClipboard(string text)
    {
        if (!string.IsNullOrWhiteSpace(text))
        {
            var thread = new Thread(() =>
            {
                int retryCount = 10;
                bool success = false;

                while (!success && retryCount-- > 0)
                {
                    try
                    {
                        Clipboard.SetText(text);
                        success = true;
                    }
                    catch (System.Runtime.InteropServices.COMException)
                    {
                        // Wait and retry.
                        Thread.Sleep(100);
                    }
                }

                if (!success)
                {
                    Debug.WriteLine("Failed to copy text to clipboard. Please try again.");
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
        }
    }

    /// <summary>
    /// [WIP] It works cross-platform.
    /// Add:
    /// using Windows.ApplicationModel.DataTransfer;
    /// </summary>
    public static void SetContentClipboardPackage(string text)
    {
        //DataPackage package = new DataPackage();
        //package.SetText("text to copy");
        //Clipboard.SetContent(package);
    }
}
