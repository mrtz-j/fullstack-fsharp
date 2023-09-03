module Settings

open System.IO
open Thoth.Json.Net

type Settings = { Setting: string }

let tryGetEnv =
    System.Environment.GetEnvironmentVariable
    >> function
    | null
    | "" -> None
    | x -> Some x

let appsettings =
    let settings = File.ReadAllText "appsettings.json"

    match Decode.Auto.fromString<Settings> settings with
    | Ok s -> s
    | Error e -> failwith e

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
        | None -> Path.Join [| contentRoot; "public" |]

let port =
    "SERVER_PORT"
    |> tryGetEnv
    |> Option.map uint16
    |> Option.defaultValue 8085us

let useSSL =
    "SERVER_USE_HTTPS"
    |> tryGetEnv
    |> Option.map (int >> (<) 0)
    |> Option.defaultValue false

let listenAddress =
    if useSSL then
        "https://0.0.0.0:" + port.ToString()
    else
        "http://0.0.0.0:" + port.ToString()
