module Shared

open System
type Todo = { Id: Guid; Description: string }

module Todo =
    val isValid: description: string -> bool
    val create: description: string -> Todo
