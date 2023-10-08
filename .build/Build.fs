module Build

open Fake.Core
open Fake.IO
open Farmer
open Farmer.Builders

open Helpers

initializeContext()

/// The path to the projects
let serverPath = Path.getFullName "src/Server"
let clientPath = Path.getFullName "src/Client"
let testPath   = "test"
let libPath    = None

/// The path to the directories
let publicPath       = Path.getFullName "src/Client/public"
let deployDir        = Path.getFullName "dist"
let publicDeployDir  = Path.getFullName "dist/public"
let packPath         = Path.getFullName "packages"
let versionFile      = Path.getFullName ".version"

// Run cleanup
Target.create "Clean" (fun _ ->
  Shell.cleanDir deployDir
)

// Restore dotnet tools, if
Target.create "InstallClient" (fun _ ->
    run pnpm "install" "."
    run dotnet "tool restore" "."
)

// Start process which publishes the client project into js-files for production
Target.create "Bundle" (fun _ ->
    [
      "server", dotnet $"publish -c Release -o \"%s{deployDir}\"" serverPath
      "client", dotnet "fable --noCache -s -o .build --run vite build -c ../../vite.config.js" clientPath
    ]
    |> runParallel
)

// Start process which publishes the server project into binaries for debug
Target.create "BundleDebug" (fun _ ->
    [
      "server", dotnet $"publish -c Debug -o \"%s{deployDir}\"" serverPath
      "client", dotnet "fable --noCache -s -o .build --run vite build -c ../../vite.config.js" clientPath
    ]
    |> runParallel
)

// Start process for running the application while developing with hot-module reloading
Target.create "Run" (fun _ ->
    [
      "server", dotnet $"watch run" serverPath
      "client", dotnet "fable watch -s -o .build --run vite -c ../../vite.config.js" clientPath
    ]
    |> runParallel
)

Target.create "Client" (fun _ ->
    run dotnet "fable watch -s -o .build --run vite -c ../../vite.config.js" clientPath
)

Target.create "Pack" (fun _ ->
    match libPath with
    | Some p -> run dotnet $"pack -c Release -o \"{packPath}\"" p
    | None -> ()
)

Target.create "Format" (fun _ ->
    run dotnet "fantomas ./src/Server" "."
)

// if any test folders run tests
Target.create "Test" (fun _ ->
    if System.IO.Directory.Exists testPath then
      run dotnet "run" testPath
    else ()
)

open Fake.Core.TargetOperators

let dependencies = [
    "Clean" ==> "InstallClient" ==> "Bundle"

    "Clean" ==> "BundleDebug"

    "Clean" ==> "Test"

    "Clean" ==> "InstallClient" ==> "Run"

    "Clean" ==> "InstallClient" ==> "Client"

    "Clean" ==> "Format"

    "Clean" ==> "Pack"
]

[<EntryPoint>]
let main args = runOrDefault args
