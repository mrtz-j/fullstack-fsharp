module Settings

open System.IO
open Thoth.Json.Net

type Settings = { Setting: string }
val tryGetEnv: (string -> string option)
val appsettings: Settings
val contentRoot: string
val webRoot: string
val port: uint16
val useSSL: bool
val listenAddress: string
