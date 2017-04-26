#addin "Cake.Npm"
#addin "Cake.Gulp"
#tool "nuget:?package=OctopusTools"

var target = Argument("target", "Build");
var buildCounter = Argument("buildCounter", 0);
var majorVersion = 2;
var minorVersion = 1;

var solutionPath = "./Solution.sln";
var startingDirectory = System.IO.Directory.GetCurrentDirectory();

var octopusOutputPath = "./Output/";
var octopusPackagePrefix = "REPLACE WITH YOUR PREFIX NAME";
var octopusPackageVersion = majorVersion + "." + minorVersion + "." + buildCounter;
var octopusApplicationName = "REPLACE WITH YOUR OCTOPUS NAME";
var octopusServer = "http://octopus.disney.com";
var octopusDeployEnvironment = "REPLACE WITH YOUR ENVIRONMENT NAME";
var octopusApiKey = EnvironmentVariable("OCTO_API");

Task("Npm")
  .Does(() =>
  {
    Environment.CurrentDirectory = "./UI/";
    Npm.Install();
  });

Task("Bower")
  .Does(() =>
  {
    Npm.RunScript("bower");
  });

Task("GulpBuild")
  .Does(() =>
  {
    Gulp.Local.Execute(settings => settings.WithArguments("ci"));
  });

Task("GulpTest")
  .Does(() =>
  {
    Gulp.Local.Execute(settings => settings.WithArguments("unitTest"));
  });

Task("AddNuGetSources")
  .Does(() =>
  {
    if (!NuGetHasSource("http://sm-gblo-vdcpnuget:9000/nuget/"))
    {
        NuGetAddSource("Disney NuGet 1","http://sm-gblo-vdcpnuget:9000/nuget/");
    }

    if (!NuGetHasSource("http://sm-gblo-vdcpnuget:9001/nuget/"))
    {
        NuGetAddSource("Disney NuGet 2","http://sm-gblo-vdcpnuget:9001/nuget/");
    }

    if (!NuGetHasSource("http://sm-gblo-vdcpnuget:9002/nuget/"))
    {
        NuGetAddSource("Disney NuGet 3","http://sm-gblo-vdcpnuget:9002/nuget/");
    }

    if (!NuGetHasSource("https://nexus.disney.com/nexus/service/local/nuget/nuget-public/"))
    {
        NuGetAddSource("Disney Nexus","https://nexus.disney.com/nexus/service/local/nuget/nuget-public/");
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

      OctoPack(octopusPackagePrefix + "UI", new OctopusPackSettings()
        {
          Version = octopusPackageVersion,
          OutFolder = octopusOutputPath,
          BasePath = "./UI/"
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


Task("Build-UI")
  .IsDependentOn("Npm")
  .IsDependentOn("Bower")
  .IsDependentOn("GulpBuild")
  .IsDependentOn("GulpTest");

Task("Build-API")
  .IsDependentOn("AddNuGetSources")
  .IsDependentOn("NuGetRestore")
  .IsDependentOn("MSBuild")
  .IsDependentOn("OctoPack");

Task("Build")
  .IsDependentOn("Build-UI")
  .IsDependentOn("Build-API");

RunTarget(target);
