module Urls

[<Literal>]
let Root = "/"

module API =

    [<Literal>]
    let AuthRoot = "/api/auth"

    module Auth =

        [<Literal>]
        let Signin = "/api/auth/signin"

        [<Literal>]
        let SignOut = "/api/auth/signout"

        [<Literal>]
        let ContestSession = "/api/auth/contest-session"

    module Todo =
        [<Literal>]
        let TodosRoot = "/api/todos"

        [<Literal>]
        let Todo = "/api/todos/{%O:guid}"
