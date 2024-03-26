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
    val todos: ResizeArray<Todo>
    val addTodo: todo: Todo -> Result<unit, string>

type TodoDTO = { Description: string }
/// <summary>
/// Fetches all available todos.
/// </summary>
val getTodos: EndpointHandler
/// <summary>
/// Creates a new todo.
/// </summary>
val addTodo: ctx: HttpContext -> Task
val endpoints: Endpoint list
val notFoundHandler: ctx: HttpContext -> Task
val errorHandler: ctx: HttpContext -> next: RequestDelegate -> Task
val configureSerilog: unit -> Core.Logger
val configureApp: appBuilder: IApplicationBuilder -> unit
val configureServices: services: IServiceCollection -> unit
val configureLogging: builder: ILoggingBuilder -> unit

[<EntryPoint>]
val main: args: string array -> int
