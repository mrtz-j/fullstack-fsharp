module Settings

open System.IO
open System.Text.Json.Serialization
open System.Text.Json

type Settings = { Setting: string }
val tryGetEnv: (string -> string option)

type DeployEnvironment =
    | Production
    | Demo
    | Staging
    | Dev

    override ToString: unit -> string

val Environment: DeployEnvironment
val appsettings: Settings
val containerized: bool
val contentRoot: string
val webRoot: string
val port: uint16
val useSSL: bool
val publicPath: string
val listenAddress: string
