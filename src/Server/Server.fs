module Server

open System
open System.IO
open System.Text.Json
open System.Text.Json.Serialization
open System.Threading.Tasks
open Oxpecker
open Thoth.Json.Oxpecker
open Microsoft.AspNetCore
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open Microsoft.Extensions.FileProviders
open Microsoft.OpenApi.Models
open OpenTelemetry
open OpenTelemetry.Metrics
open Serilog

open Shared

module Storage =
    let todos = ResizeArray()

    let addTodo todo =
        if Todo.isValid todo.Description then
            todos.Add todo
            Ok()
        else
            Error "Invalid todo"

    do
        addTodo (Todo.create "Create new SAFE project") |> ignore
        addTodo (Todo.create "Write your app") |> ignore
        addTodo (Todo.create "Ship it!!!") |> ignore

type TodoDTO = {Description: string}

/// <summary>
/// Fetches all available todos.
/// </summary>
let getTodos: EndpointHandler =
    let res = Storage.todos |> List.ofSeq
    json res

/// <summary>
/// Creates a new todo.
/// </summary>
let addTodo (ctx: HttpContext) =
    task {
        let! todo = ctx.BindJson<TodoDTO> ()
        let newTodo = Todo.create todo.Description
        match Storage.addTodo newTodo with
        | Ok () -> return! json newTodo ctx
        | Error e ->
            Log.Error "Could not add todo."
            return! ctx.Write <| TypedResults.Problem e
    }
    :> Task

let endpoints = [
    GET [
        route "/" (text "Hello World!")
        route Urls.API.Todo.TodosRoot <| getTodos
        route "/error" <| text "Something went wrong"
    ]
    POST [route Urls.API.Todo.TodosRoot <| addTodo]
]

let notFoundHandler (ctx: HttpContext) =
    Log.Warning "Unhandled 404 error"
    ctx.SetStatusCode StatusCodes.Status404NotFound
    ctx.Write <| TypedResults.NotFound {|Error = "Resource was not found"|}

// TODO: Make it handle innerexn with rec
let errorHandler (ctx: HttpContext) (next: RequestDelegate) =
    task {
        try
            return! next.Invoke ctx
        with
        | :? ModelBindException
        | :? JsonException
        | :? RouteParseException as ex ->
            Log.Warning $"Unhandled 400 error %s{ex.Message}"
            ctx.SetStatusCode StatusCodes.Status400BadRequest
            return! ctx.Write <| TypedResults.BadRequest {|Error = ex.Message|}
        | ex ->
            Log.Error
                $"An unhandled exception has occurred while executing the request. %s{ex.Message}"
            ctx.SetStatusCode StatusCodes.Status500InternalServerError
            return! ctx.WriteText <| string ex
    }
    :> Task

// Configure Serilog to write to console at minimum info level, silence microsoft and system related logs
let configureSerilog () =
    LoggerConfiguration().Enrich.FromLogContext ()
    |> fun x ->
        if Settings.containerized then
            x.MinimumLevel.Information ()
        else
            x.MinimumLevel.Debug ()
    |> fun x ->
        x.MinimumLevel
            .Override("Microsoft", Events.LogEventLevel.Warning)
            .MinimumLevel.Override("System", Events.LogEventLevel.Warning)
            .MinimumLevel.Override("HealthChecks", Events.LogEventLevel.Warning)
            .WriteTo.Console()
            .CreateLogger()

// Configure app
let configureApp (appBuilder: IApplicationBuilder) =
    let staticFileOptions = StaticFileOptions()
    let publicPath' =
        Path.Combine (Directory.GetCurrentDirectory(), Settings.publicPath)
    staticFileOptions.FileProvider <- new PhysicalFileProvider (publicPath')

    (match Settings.containerized with
     | true -> appBuilder.UseDeveloperExceptionPage() |> ignore
     | false -> appBuilder.UseExceptionHandler("/error", true) |> ignore)
    appBuilder
        .UseDefaultFiles()
        .UseStaticFiles(staticFileOptions)
        .UseHealthChecks(PathString "/healthz")
        .UseSerilogRequestLogging()
        .UseRouting()
        .Use(errorHandler)
        .UseOxpecker(endpoints)
        .UseSwagger() // for generating OpenApi spec
        .UseSwaggerUI()
        .UseOpenTelemetryPrometheusScrapingEndpoint()
        .Run(notFoundHandler)

// Configure services
let configureServices (services: IServiceCollection) =
    let jsonFSharpOptions =
        JsonFSharpOptions
            .Default()
            .WithAllowNullFields(true)
            .WithUnionFieldsName("value")
            .WithUnionTagNamingPolicy(JsonNamingPolicy.CamelCase)
            .WithUnionTagCaseInsensitive(true)
            .WithUnionEncoding(
                JsonUnionEncoding.ExternalTag
                ||| JsonUnionEncoding.UnwrapFieldlessTags
                ||| JsonUnionEncoding.UnwrapSingleFieldCases
                ||| JsonUnionEncoding.UnwrapSingleCaseUnions
                ||| JsonUnionEncoding.NamedFields
            )
            .WithUnwrapOption(true)

    let jsonOptions = JsonSerializerOptions ()
    jsonOptions.AllowTrailingCommas <- true
    jsonOptions.Converters.Add (JsonFSharpConverter(jsonFSharpOptions))
    jsonOptions.DefaultBufferSize <- 64 * 1024
    jsonOptions.DefaultIgnoreCondition <- JsonIgnoreCondition.WhenWritingDefault // jsonOptions.IgnoreNullValues is deprecated. This is the new way to say it.
    jsonOptions.NumberHandling <- JsonNumberHandling.AllowReadingFromString
    jsonOptions.PropertyNameCaseInsensitive <- true // Case sensitivity is from the 1970's. We should let it go.
    // jsonOptions.PropertyNamingPolicy <- JsonNamingPolicy.CamelCase
    jsonOptions.ReadCommentHandling <- JsonCommentHandling.Skip
    jsonOptions.ReferenceHandler <- ReferenceHandler.IgnoreCycles
    jsonOptions.UnknownTypeHandling <- JsonUnknownTypeHandling.JsonElement
    jsonOptions.WriteIndented <- true
    jsonOptions.MaxDepth <- 16 // Default is 64, but if we exceed a depth of 16, we're probably doing something wrong.
    jsonOptions.PropertyNameCaseInsensitive <- true

    let openApiInfo = OpenApiInfo()
    openApiInfo.Description <- "A simple Todo App to Demo Swagger with F#"
    openApiInfo.Title <- "Todo Server API"
    openApiInfo.Version <- "v1"
    openApiInfo.Contact <- OpenApiContact()
    openApiInfo.Contact.Name <- "Joe Developer"
    openApiInfo.Contact.Email <- "joe.developer@tempuri.org"
    openApiInfo.License <- OpenApiLicense()
    openApiInfo.License.Name <- "MIT"
    openApiInfo.License.Url <- Uri("https://opensource.org/license/MIT")

    let meterProvider =
        Sdk
            .CreateMeterProviderBuilder()
            .AddPrometheusExporter()
            .AddMeter(
                [|
                    "System.Runtime"
                    "Microsoft.AspNetCore.Hosting"
                    "Microsoft.AspNetCore.Server.Kestrel"
                |]
            )
            .Build()

    services.AddRouting() |> ignore
    services.AddOxpecker() |> ignore
    services.AddEndpointsApiExplorer() |> ignore // use the API Explorer to discover and describe endpoints
    services.AddSwaggerGen(fun opt ->
        opt.SwaggerDoc ("v1", openApiInfo)
        // TODO: Does not work
        let xmlPath = Path.Combine(AppContext.BaseDirectory, "Server.xml")
        opt.IncludeXmlComments(xmlPath)
    )
    |> ignore // swagger dependencies
    services.AddSingleton(meterProvider) |> ignore
    services.AddSingleton<Serializers.IJsonSerializer>(ThothSerializer())
    |> ignore
    services.AddMetrics() |> ignore
    services.AddHealthChecks () |> ignore
    services.AddLogging() |> ignore
    services.AddSerilog() |> ignore

// Configure logging
let configureLogging (builder: ILoggingBuilder) =
    builder
        .Configure(fun opt ->
            opt.ActivityTrackingOptions <-
                ActivityTrackingOptions.SpanId
                ||| ActivityTrackingOptions.ParentId
                ||| ActivityTrackingOptions.TraceId
                ||| ActivityTrackingOptions.Baggage
                ||| ActivityTrackingOptions.Tags
        )
        .SetMinimumLevel(
            if not Settings.containerized then
                LogLevel.Debug
            else
                LogLevel.Warning
        )
        .AddFilter("Microsoft", LogLevel.Warning)
        .AddFilter("System", LogLevel.Warning)
        .AddSerilog()
    |> ignore

[<EntryPoint>]
let main args =
    Log.Logger <- configureSerilog ()
    Log.Information "-----------------------------------------"
    Log.Information $"Containerized: %A{Settings.containerized}"
    Log.Information $"Settings: %A{Settings.appsettings.Setting}"
    Log.Information "-----------------------------------------"

    let ipPort = $"http://0.0.0.0:%i{Settings.port}/"

    // The max size of Content-Length request header that will be accepted
    let maxRequestBodySize = Int64.MaxValue

    WebHost
        .CreateDefaultBuilder(args)
        .Configure(Action<IApplicationBuilder> configureApp)
        .ConfigureServices(configureServices)
        .UseWebRoot(Settings.publicPath)
        .ConfigureKestrel(fun kestrelOptions ->
            kestrelOptions.Limits.MaxRequestBodySize <- maxRequestBodySize
        )
        .ConfigureLogging(configureLogging)
        .UseUrls(ipPort)
        .Build()
        .Run()

    0
