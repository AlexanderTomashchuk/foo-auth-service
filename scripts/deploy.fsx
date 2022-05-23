#r "paket:
nuget Fake.IO.FileSystem
nuget Fake.DotNet.MSBuild
nuget Fake.DotNet.Cli
nuget Fake.Core.Target //"
#load "./.fake/deploy.fsx/intellisense.fsx"

// Dependencies
open Fake.Core
open Fake.Core.TargetOperators
open Fake.DotNet
open Fake.IO
open Fake.IO.Globbing.Operators

// Properties
let binDir = "../src/**/bin"
let objDir = "../src/**/obj"
let projPaths = "../src/**/*.*proj"
let testProjPaths = "../test/**/*.*proj"
let apiProjPath = "../src/api"
let awsConfigPath = "./aws-config/aws-lambda-tools-defaults.json"

// Targets
Target.create "Clean" (fun _ -> !!binDir ++ objDir |> Shell.cleanDirs)

Target.create "Build" (fun _ -> !!projPaths |> Seq.iter (DotNet.build id))

Target.create "Test" (fun _ -> !!testProjPaths |> Seq.iter (DotNet.test id))

Target.create "Deploy" (fun _ ->
    let withWorkingDir path =
        DotNet.Options.withWorkingDirectory path

    DotNet.exec (withWorkingDir apiProjPath) "lambda deploy-function" ("-cfg " + awsConfigPath)
    |> ignore

    Trace.trace "Deployment process is finished")

"Clean" ==> "Build" ==> "Test" ==> "Deploy"

// Start
Target.runOrDefault "Deploy"
