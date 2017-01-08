var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

Task("Clean")
    .Does(() =>
{
    CleanDirectory("./src/obj");
    CleanDirectory("./src/bin");
});

Task("Build")
    .IsDependentOn("Clean")
    .Does(() =>
{
    NuGetRestore("./OrleansSerilogBootstrap.sln"); 

    MSBuild("./src/OrleansSerilogBootstrap.csproj", new MSBuildSettings()
        .SetConfiguration(configuration)
        .SetVerbosity(Verbosity.Minimal)
    );
});

Task("Pack")
    .IsDependentOn("Build")
    .Does(() =>
{
    var info = ParseAssemblyInfo("./src/Properties/AssemblyInfo.cs");

    var settings = new NuGetPackSettings 
    {
        Version = info.AssemblyInformationalVersion,        
        OutputDirectory = "./nuget"
    };

    NuGetPack("./OrleansSerilogBootstrap.nuspec", settings);
});

Task("Default")
    .IsDependentOn("Build")
    .Does(() =>
{

});

RunTarget(target);