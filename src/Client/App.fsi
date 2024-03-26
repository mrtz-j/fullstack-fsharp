module App

open Feliz
open FS.FluentUI

open Shared

[<ReactComponent>]
val Counter: unit -> Fable.React.ReactElement

[<ReactComponent>]
val App: unit -> ReactElement

open Browser.Dom
val root: ReactApi.IReactRoot
