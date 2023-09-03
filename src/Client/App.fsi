module App

open Feliz
open Fable.Remoting.Client
open Shared

val serverApi: IServerApi

[<ReactComponent>]
val Counter: unit -> Fable.React.ReactElement

open Browser.Dom
val root: ReactApi.IReactRoot
