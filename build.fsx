#r "nuget: Fun.Build, 1.1.2"
#r "nuget: Fake.IO.FileSystem, 6.0.0"

open System.IO
open Fake.IO
open Fake.IO.FileSystemOperators
open Fun.Build

let solutionFile = "Fullstack.sln"
let deployDir = Path.getFullName "dist"

let restoreStage =
    stage "Restore" {
        run "dotnet tool restore"
        run "dotnet restore --locked-mode"
    }

let clean (input: string seq) =
    async {
        input
        |> Seq.iter (fun dir ->
            if Directory.Exists(dir) then
                Directory.Delete(dir, true))
    }

pipeline "Build" {
    workingDir __SOURCE_DIRECTORY__
    restoreStage
    stage "Clean" { run (clean [| "dist"; "reports" |]) }
    stage "CheckFormat" { run "dotnet fantomas src build.fsx --check" }

    stage "Main" {
        paralle

        stage "Client" {
            workingDir "src/Client"
            run "bun i --frozen-lockfile"
            run "bunx --bun vite build"
        }

        stage "Server" {
            workingDir "src/Server"
            run $"dotnet publish -c Release -o %s{deployDir} -tl"
        }
    }

    runIfOnlySpecified false
}

pipeline "Watch" {
    workingDir __SOURCE_DIRECTORY__
    stage "Clean" { run (clean [| "dist"; "reports" |]) }

    stage "Main" {
        run "dotnet tool restore"
        paralle

        stage "Client" {
            workingDir "src/Client"
            run "bun i --frozen-lockfile"
            run "bunx --bun vite"
        }

        stage "Server" {
            workingDir "src/Server"
            run "dotnet watch run -tl"
        }
    }

    runIfOnlySpecified true
}

pipeline "Server" {
    workingDir __SOURCE_DIRECTORY__
    stage "Clean" { run (clean [| "dist"; "reports" |]) }
    restoreStage

    stage "Server" {
        workingDir "src/Server"
        run "dotnet watch run -tl"
    }

    runIfOnlySpecified true
}

pipeline "Client" {
    workingDir __SOURCE_DIRECTORY__
    stage "Clean" { run (clean [| "dist"; "reports" |]) }
    restoreStage

    stage "Client" {
        workingDir "src/Client"
        run "bun i --frozen-lockfile"
        run "bunx --bun vite"
    }

    runIfOnlySpecified true
}

pipeline "Analyze" {
    workingDir __SOURCE_DIRECTORY__
    stage "Report" { run $"dotnet msbuild /t:AnalyzeSolution %s{solutionFile}" }
    runIfOnlySpecified true
}

pipeline "Format" {
    workingDir __SOURCE_DIRECTORY__
    stage "Restore" { run "dotnet tool restore" }
    stage "Fantomas" { run "dotnet fantomas src build.fsx" }
    runIfOnlySpecified true
}

pipeline "Sign" {
    workingDir __SOURCE_DIRECTORY__
    stage "Restore" { run "dotnet tool restore" }

    stage "Main" {
        // NOTE: Could this be parallelized?
        run "dotnet telplin src/Server/Server.fsproj -- /p:Configuration=Release"
        run "dotnet telplin src/Client/Client.fsproj -- /p:Configuration=Release"
    }

    runIfOnlySpecified true
}

tryPrintPipelineCommandHelp ()
