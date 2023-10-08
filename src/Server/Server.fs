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

let rec private unwrapEx (ex: Exception) =
    if not (isNull ex.InnerException) then
        printfn $"Parent Ex: %s{ex.Message}"
        printfn "Unwrapping inner exception..."
        unwrapEx ex.InnerException
    else
        ex

let fableRemotingErrorHandler (ex: Exception) (ri: RouteInfo<HttpContext>) =
    let logger = ri.httpContext.GetLogger ()
    logger.LogError $"Error at %s{ri.path} on method %s{ri.methodName}"
    // Decide whether or not you want to propagate the error to the client
    let ex = unwrapEx ex
    logger.LogError $"Error: %s{ex.Message}"
    Propagate "An error occurred while processing the request."

let helloWorld () = async {return "Hello From Saturn!"}

let serverApi: IServerApi = {GetValue = helloWorld}

let routeBuilder (typeName: string) (methodName: string) =
    $"/api/%s{typeName}/%s{methodName}"

let docs = Docs.createFor<IServerApi>()
let serverApiDocs =
    Remoting.documentation "Server Api"
        [
            docs.route <@ fun (api: IServerApi) -> api.GetValue @>
            |> docs.alias "Get a Value"
            |> docs.description "Returns a value set on the backend"
        ]

let serverApiHandler: HttpHandler =
    Remoting.createApi ()
    |> Remoting.withRouteBuilder routeBuilder
    |> Remoting.fromValue serverApi
    |> Remoting.withErrorHandler fableRemotingErrorHandler
    |> Remoting.withDocs "/api/docs" serverApiDocs
    |> Remoting.buildHttpHandler

let webApp =
    choose [
        serverApiHandler

        GET >=> routeCi "/api/ping" >=> text "pong"
    ]

// Configure Serilog to write to console at minimum info level, silence microsoft and system related logs
let configureSerilog () =
    LoggerConfiguration()
        .Enrich.FromLogContext()
        .Destructure.FSharpTypes()
        .MinimumLevel.Override("Microsoft", Events.LogEventLevel.Warning)
        .MinimumLevel.Override("System", Events.LogEventLevel.Warning)
        .MinimumLevel.Override("HealthChecks", Events.LogEventLevel.Warning)
        .WriteTo.Console()
        .CreateLogger ()

let configureServices (services: IServiceCollection) =
    services.AddHealthChecks () |> ignore

let configureApp (app: IApplicationBuilder) =
    app.UseHealthChecks (PathString "/health")

let configureLogger (logger: ILoggingBuilder) =
    logger
        .SetMinimumLevel(LogLevel.Debug)
        .AddFilter("Microsoft", LogLevel.Warning)
        .AddFilter("System", LogLevel.Warning)
        .AddSerilog ()
    |> ignore

let configureHost (host: IHostBuilder) =
    host.ConfigureServices (configureServices)

let app ipPort =
    application {
        url ipPort
        use_router webApp
        memory_cache
        use_static "public"
        use_json_serializer (Thoth.Json.Giraffe.ThothSerializer ())
        use_gzip

        app_config configureApp
        host_config configureHost
        logging configureLogger
    }

[<EntryPoint>]
let main _ =
    Log.Logger <- configureSerilog ()

    Log.Information "-----------------------------------------"
    Log.Information $"Port: {Settings.port}"
    Log.Information "-----------------------------------------"

    let ipPort = $"http://0.0.0.0:{Settings.port}/"
    run (app ipPort)
    0