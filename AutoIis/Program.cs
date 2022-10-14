using Microsoft.Web.Administration;
using System.Diagnostics;

namespace AutoIis;

internal class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");
    }
}

class App
{
    private readonly Config _config;
    private readonly ServerManager _serverManager = new ServerManager();

    public App(Config config)
    {
        _config = config;
    }

    public void Execute()
    {
        AddProjectsToIis();
        BuildSolutions();
    }

    private void AddProjectsToIis()
    {
        var configPaths = EnumerateFiles("*.config.xml");
        var csprojPaths = configPaths
            .Select(x => x[..^12] + ".csproj")
            .ToHashSet();
        var appModels = configPaths
            .Where(x => csprojPaths.Contains(x)) // sprawdzenie, ze plikowi .config.xml naprawde odpowiada istniejacy projekt, a to nie jakis inny losowy plik xml
            .Select(x => new AppModel(x))
            .ToList();

        foreach (var appModel in appModels)
        {
            var pool = _config.PoolMap.GetValueOrDefault(appModel.Pool, appModel.Pool);
            AddAppToIisIfNotExists(appModel.VirtualPath, appModel.PhysicalPath, pool);
        }

        _serverManager.CommitChanges();
    }

    private void BuildSolutions()
    {
        var solutions = EnumerateFiles("*.sln");
        foreach (var solution in solutions)
        {
            throw new NotImplementedException();
        }
    }

    private List<string> EnumerateFiles(string pattern)
    {
        var enumConfig = new EnumerationOptions
        {
            MaxRecursionDepth = 3,
            RecurseSubdirectories = true,
        };

        return Directory.EnumerateFileSystemEntries(_config.RootDirectory, pattern, enumConfig)
            .ToList();
    }

    private void AddAppToIisIfNotExists(string path, string physicalPath, string pool)
    {

    }
}

class Config
{
    public Config(string rootDirectory)
    {
        RootDirectory = rootDirectory;
    }

    public string RootDirectory { get; }
    public Dictionary<string, string> PoolMap { get; } = new Dictionary<string, string>();
}

class AppModel
{
    public AppModel(string configFilePath)
    {
        var configFileInfo = new FileInfo(configFilePath);
        var configFile = File.ReadAllText(configFilePath);
        var appConfig = DeserializeConfigFile(configFile);
        Name = configFileInfo.Name[..^12]; // ".config.xml".Length == 12
        PhysicalPath = configFileInfo.DirectoryName!;
        Pool = appConfig.Pool;
    }

    public string Name { get; }
    public string PhysicalPath { get; }
    public string Pool { get; }
    public string VirtualPath => "/" + Name;

    private AppConfig DeserializeConfigFile(string content)
    {
        throw new NotImplementedException();
    }
}

class AppConfig
{
    public string Pool { get; set; }
}