using Microsoft.Extensions.Configuration;
using StudyPlanner.Infrastructure.NenTang;

if (args.Length != 1 || args[0] is not ("--export-only" or "--apply")) throw new ArgumentException("Use --export-only to back up, or --apply to export, verify and remove the two legacy catalog tables.");
var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../../"));
var apiPath = Path.Combine(root, "backend/StudyPlanner.Api");
if (!File.Exists(Path.Combine(apiPath, "appsettings.json"))) throw new InvalidOperationException("Run a standard build of CatalogTransfer within this repository.");
var config = new ConfigurationBuilder().SetBasePath(apiPath).AddJsonFile("appsettings.json")
    .AddJsonFile("appsettings.Development.json", optional: true).AddEnvironmentVariables().Build();
var file = config["Catalog:FilePath"] ?? Path.Combine(apiPath, "Administration/Data/danh-muc-mau.json");
var store = new DanhMucJsonStore(file, Path.Combine(apiPath, "Administration/Data/danh-muc-mau.seed.json"));
var result = await CatalogTransfer.RunAsync(config.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Database configuration missing."), store, args[0] == "--apply");
Console.WriteLine(result);
