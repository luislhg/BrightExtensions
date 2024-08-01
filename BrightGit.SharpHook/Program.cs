using System.IO.Pipes;

// The timeout for the connection to the pipe.
const int timeout = 10000;

// First argument is the hook name, the rest are the arguments.
if (args.Length < 1)
{
    Console.WriteLine("Error, at least one argument is required (Example: `BrightGit.SharpHook.exe <hook-name> <args...>`.");
    return;
}

string eventName = args[0];
string arguments = string.Join(":", args, 1, args.Length - 1);

using (var pipeClient = new NamedPipeClientStream(".", "BrightSharpHook", PipeDirection.Out))
{
    try
    {
        pipeClient.Connect(timeout);
        using (var writer = new StreamWriter(pipeClient))
        {
            writer.AutoFlush = true;
            writer.WriteLine($"{eventName}:{arguments}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Failed to send message: {ex.Message}");
    }
}
