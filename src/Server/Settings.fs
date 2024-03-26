module Settings

open System.IO
open System.Text.Json.Serialization
open System.Text.Json

type Settings = {Setting: string}

let tryGetEnv =
    System.Environment.GetEnvironmentVariable
    >> function
        | null -> None
        | x when x.Length = 0 -> None
        | x -> Some x

type DeployEnvironment =
    | Production
    | Demo
    | Staging
    | Dev

    override this.ToString () =
        match this with
        | Production -> "production"
        | Demo -> "demo"
        | Staging -> "staging"
        | Dev -> "development"

let Environment =
    "DEPLOY_ENV"
    |> tryGetEnv
    |> function
        | None ->
            printfn "No DEPLOY_ENV. Defaulting to dev."
            Dev
        | Some e ->
            match e.Trim().ToLower () with
            | "prod"
            | "production"
            | "p" -> Production
            | "demo" -> Demo
            | "staging"
            | "s" -> Staging
            | "dev"
            | "review"
            | "test" -> Dev
            | _ ->
                printfn $"Could not parse DEPLOY_ENV %s{e}. Defaulting to Dev."
                Dev

let appsettings =
    let settings = File.ReadAllText "appsettings.json"
    let options = JsonFSharpOptions.ThothLike().ToJsonSerializerOptions ()

    JsonSerializer.Deserialize<Settings> (settings, options)

// Running in K8s
let containerized =
    "DOTNET_RUNNING_IN_CONTAINER"
    |> tryGetEnv
    |> function
        | Some _ -> true
        | _ -> false

// Server home
let contentRoot =
    tryGetEnv "SERVER_CONTENT_ROOT"
    |> function
        | Some root -> Path.GetFullPath root
        | None -> Path.GetFullPath "../Client"

// Webfiles and serveable assets
let webRoot =
    tryGetEnv "SERVER_WEB_ROOT"
    |> function
        | Some root -> Path.GetFullPath root
        | None -> Path.Join [|contentRoot; "public"|]

let port =
    "SERVER_PORT" |> tryGetEnv |> Option.map uint16 |> Option.defaultValue 8085us

let useSSL =
    "SERVER_USE_HTTPS"
    |> tryGetEnv
    |> Option.map (int >> (<) 0)
    |> Option.defaultValue false

let publicPath =
    Environment = DeployEnvironment.Dev
    |> function
        | true -> "./public"
        | _ -> "../../dist/public"
    |> Path.GetFullPath

let listenAddress =
    if useSSL then
        "https://0.0.0.0:" + port.ToString ()
    else
        "http://0.0.0.0:" + port.ToString ()
