using System.Diagnostics;
using System.Globalization;
using SiteBuilder.Pipeline;

// Usage (from the repository root):
//   dotnet run --project build/SiteBuilder                  build the live site into dist/
//   dotnet run --project build/SiteBuilder -- --preview     also render drafts and scheduled items
//   options: --root <dir>  --out <dir>  --today yyyy-MM-dd
var root = Directory.GetCurrentDirectory();
string? output = null;
var today = DateOnly.FromDateTime(DateTime.UtcNow);
var preview = false;

for (var i = 0; i < args.Length; i++)
{
    switch (args[i])
    {
        case "--root": root = args[++i]; break;
        case "--out": output = args[++i]; break;
        case "--today": today = DateOnly.ParseExact(args[++i], "yyyy-MM-dd", CultureInfo.InvariantCulture); break;
        case "--preview": preview = true; break;
        default:
            Console.Error.WriteLine($"Unknown option: {args[i]}");
            return 2;
    }
}

root = Path.GetFullPath(root);
if (!Directory.Exists(Path.Combine(root, "content")))
{
    Console.Error.WriteLine($"No content/ folder in {root}. Run from the repository root or pass --root.");
    return 2;
}

var options = new BuildOptions(root, Path.GetFullPath(output ?? Path.Combine(root, "dist")), today, preview);
var stopwatch = Stopwatch.StartNew();

try
{
    BuildPipeline.Default().Run(options, Console.WriteLine);
}
catch (BuildException ex)
{
    Console.Error.WriteLine($"Build failed: {ex.Message}");
    return 1;
}

Console.WriteLine($"Built {options.OutputDirectory} in {stopwatch.ElapsedMilliseconds} ms{(preview ? " (PREVIEW)" : "")}");
return 0;
