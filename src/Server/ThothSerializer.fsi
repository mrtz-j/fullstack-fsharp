namespace Thoth.Json.Oxpecker

open System.Text
open System.Text.Json
open System.Threading.Tasks
open Microsoft.IO
open System.IO
open Newtonsoft.Json
open Newtonsoft.Json.Linq
open Microsoft.AspNetCore.Http
open Oxpecker
open Thoth.Json.Net

type ThothSerializer =
    new:
        ?caseStrategy: CaseStrategy * ?extra: ExtraCoders * ?skipNullField: bool * ?options: JsonSerializerOptions ->
            ThothSerializer

    /// Responds a JSON
    static member RespondRawJson: body: JToken -> (EndpointHandler -> HttpContext -> Task<HttpContext option>)

    /// Responds a JSON
    static member RespondJson:
        body: 'T -> encoder: Encoder<'T> -> (EndpointHandler -> HttpContext -> Task<HttpContext option>)

    /// Responds a JSON array by writing items
    /// into response stream one by one
    static member RespondRawJsonSeq: items: JToken seq -> (EndpointHandler -> HttpContext -> Task<HttpContext option>)

    /// Responds a JSON array by serializing items
    /// into response stream one by one
    static member RespondJsonSeq:
        items: 'T seq -> encoder: Encoder<'T> -> (EndpointHandler -> HttpContext -> Task<HttpContext option>)

    static member ReadBodyRaw: ctx: HttpContext -> Task<Result<JToken, string>>
    static member ReadBody: ctx: HttpContext -> decoder: Decoder<'T> -> Task<Result<'T, string>>
    static member ReadBodyUnsafe: ctx: HttpContext -> decoder: Decoder<'T> -> Task<'T>
    interface Serializers.IJsonSerializer
