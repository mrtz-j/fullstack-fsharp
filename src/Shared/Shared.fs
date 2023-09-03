module Shared

type IServerApi = { GetValue: unit -> Async<string> }
