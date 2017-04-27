#tool "nuget:?package=OctopusTools"

var target = Argument("target", "Build");
var buildCounter = Argument("buildCounter", 0);
var majorVersion = 2;
var minorVersion = 1;

var solutionPath = "./WebApplication1.sln";
var startingDirectory = System.IO.Directory.GetCurrentDirectory();

var octopusOutputPath = "./Output/";
var octopusPackagePrefix = "REPLACE WITH YOUR PREFIX NAME";
var octopusPackageVersion = majorVersion + "." + minorVersion + "." + buildCounter;
var octopusApplicationName = "REPLACE WITH YOUR OCTOPUS NAME";
var octopusServer = "http://octopus.disney.com";
var octopusDeployEnvironment = "REPLACE WITH YOUR ENVIRONMENT NAME";
var octopusApiKey = EnvironmentVariable("OCTO_API");



Task("AddNuGetSources")
  .Does(() =>
  {
    if (!NuGetHasSource("https://www.nuget.org/api/v2/curated-feeds/microsoftdotnet/"))
    {
        NuGetAddSource("Microsoft NuGet 1","https://www.nuget.org/api/v2/curated-feeds/microsoftdotnet/");
    }

  });
  
  

Task("NuGetRestore")
  .Does(() =>
  {
    Environment.CurrentDirectory = startingDirectory;
    NuGetRestore(solutionPath);
  });

Task("MSBuild")
  .IsDependentOn("NuGetRestore")
  .Does(() =>
  {
    Information("Version" + octopusPackageVersion);
    MSBuild(solutionPath);
  });

Task("OctoPack")
  .Does(() =>
  {
      OctoPack(octopusPackagePrefix + "Api", new OctopusPackSettings()
        {
          Version = octopusPackageVersion,
          OutFolder = octopusOutputPath,
          BasePath = "./Api/"
        });
  });

Task("OctoPush")
  .Does(() =>
  {
      var apiKey = EnvironmentVariable("OCTO_API");
      var packagesToPush = GetFiles(octopusOutputPath + "*.nupkg").ToList();

      OctoPush(octopusServer, apiKey, packagesToPush, new OctopusPushSettings {
        ReplaceExisting = true
      });
  });

Task ("OctoCreateRelease")
  .Does(() =>
  {
    OctoCreateRelease(octopusApplicationName, new CreateReleaseSettings 
                                                  {
                                                    Server = octopusServer,
                                                    ApiKey = octopusApiKey,
                                                    ReleaseNumber = octopusPackageVersion
                                                  });
  });

Task ("OctoDeployRelease")
  .Does(() =>
  {
    OctoDeployRelease(octopusServer, octopusApiKey, octopusApplicationName, octopusDeployEnvironment, octopusPackageVersion, new OctopusDeployReleaseDeploymentSettings());
  });



Task("Build")
  .IsDependentOn("AddNuGetSources")
  .IsDependentOn("NuGetRestore")
  .IsDependentOn("MSBuild");



RunTarget(target);
