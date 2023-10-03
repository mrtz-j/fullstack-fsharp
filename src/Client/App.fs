module App

open Feliz
open Fable.Remoting.Client
open FS.FluentUI

open Shared

let serverApi: IServerApi =
  Remoting.createApi ()
  |> Remoting.withBaseUrl "/api"
  |> Remoting.buildProxy<IServerApi>

let fetchHello () =
   async {
       let! hello = serverApi.GetValue()
       printfn $"%s{hello}"
   }
   |> Async.StartImmediate

[<ReactComponent>]
let Counter() =
    fetchHello ()

    let count, setCount = React.useState(0)

    Html.div [
      prop.children [
          Fui.text [
            text.as'.h2
            text.text $"Simple Counter"
          ]
          Html.div [
            prop.style [
              style.height (length.perc 50)
              style.width (length.perc 80)
              style.display.flex
              style.flexDirection.column
            ]
            prop.children [
              Fui.text [
                text.as'.h3
                text.text $"{count}"
              ]
              Fui.text [
                 text.as'.h4
                 text.text $"""{if count % 2 = 0 then "Even" else "Odd"}"""
              ]
            ]
          ]
          Html.div [
            prop.children [
              Fui.compoundButton [
                compoundButton.ariaLabel "decrement"
                compoundButton.onClick (fun _ -> setCount(count - 1))
                compoundButton.icon (Fui.icon.deleteRegular [])
              ]
              Fui.compoundButton [
                compoundButton.ariaLabel "increment"
                compoundButton.onClick (fun _ -> setCount(count + 1))
                compoundButton.icon (Fui.icon.addRegular [])
              ]
            ]
          ]
        ]
    ]

open Browser.Dom

let root = ReactDOM.createRoot(document.getElementById "root")
root.render(Counter())
