module Urls

[<Literal>]
val Root: string = "/"

module API =
    [<Literal>]
    val AuthRoot: string = "/api/auth"

    module Auth =
        [<Literal>]
        val Signin: string = "/api/auth/signin"

        [<Literal>]
        val SignOut: string = "/api/auth/signout"

        [<Literal>]
        val ContestSession: string = "/api/auth/contest-session"

    module Todo =
        [<Literal>]
        val TodosRoot: string = "/api/todos"

        [<Literal>]
        val Todo: string = "/api/todos/{%O:guid}"
