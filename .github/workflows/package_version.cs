#:property PublishAot=false
#:package NuGet.Versioning

using System.Text.Json;
using System.Xml;
using System.Net;
using NuGet.Versioning;

const string projectName = "ProcFsCore";

string githubRunId = Environment.GetEnvironmentVariable("GITHUB_RUN_ID")!;
string githubRefName = Environment.GetEnvironmentVariable("GITHUB_REF_NAME")!;

XmlDocument doc = new();
doc.Load(Path.Combine(projectName, $"{projectName}.csproj"));
var baseVersion = SemanticVersion.Parse(doc.SelectSingleNode("//Version")!.InnerText);

IEnumerable<SemanticVersion> versions;
try
{
    using HttpClient client = new();
    var versionsJson = await client.GetStringAsync($"https://api.nuget.org/v3-flatcontainer/{projectName.ToLowerInvariant()}/index.json");
    versions = JsonSerializer.Deserialize<NuGetVersions>(versionsJson)!.versions.Select(v => SemanticVersion.Parse(v));
}
catch (HttpRequestException e) when (e.StatusCode == HttpStatusCode.NotFound)
{
    versions = [];
}

int[] patches = [.. from v in versions where v.Major == baseVersion.Major && v.Minor == baseVersion.Minor select v.Patch];
var newPatch = patches.Any() ? patches.Max() + 1 : 0;

var newRelease = githubRefName == "master" ? "" : $"beta-{githubRunId}";

var newVersion = new SemanticVersion(baseVersion.Major, baseVersion.Minor, newPatch, newRelease);

Console.WriteLine(newVersion);

record NuGetVersions(string[] versions);