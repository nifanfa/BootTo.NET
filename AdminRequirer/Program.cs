using System.Diagnostics;

public static class Program
{
    public static void Main(string[] args)
    {
        string s = string.Empty;
        for(int i = 0; i < args.Length; i++)
        {
            s += args[i] + " ";
        }
        ProcessStartInfo sti = new ProcessStartInfo()
        {
            FileName = "cmd.exe",
            UseShellExecute = false,
            Arguments = $"/c {s}"
        };
        Process.Start(sti).WaitForExit();
    }
}