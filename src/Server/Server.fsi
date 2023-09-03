module Server

open System
open Saturn
open Giraffe
open Fable.Remoting.Server
open Fable.Remoting.Giraffe
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Giraffe.SerilogExtensions
open Serilog

open Shared

val fableRemotingErrorHandler: ex: Exception -> ri: RouteInfo<HttpContext> -> ErrorResult
val helloWorld: unit -> Async<string>
val serverApi: IServerApi
val routeBuilder: typeName: string -> methodName: string -> string
val serverApiHandler: HttpHandler
val webApp: (HttpFunc -> HttpContext -> HttpFuncResult)
val configureSerilog: unit -> Core.Logger
val configureServices: services: IServiceCollection -> unit
val configureApp: app: IApplicationBuilder -> IApplicationBuilder
val configureLogger: logger: ILoggingBuilder -> unit
val configureHost: host: IHostBuilder -> IHostBuilder
val app: ipPort: string -> IHostBuilder

[<EntryPoint>]
val main: string array -> int
