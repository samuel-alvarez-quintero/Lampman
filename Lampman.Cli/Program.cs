namespace Lampman.Cli;

class Program
{
    public static async Task<int> Main(string[] args)
    {
        var app = new LampmanApp();
        return await app.RunAsync(args);
    }
}