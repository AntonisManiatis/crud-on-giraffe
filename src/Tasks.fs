namespace TaskBuddy.Api

module Tasks =
    open Microsoft.AspNetCore.Http
    open Giraffe
    open Sql

    let getTasks =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                use! connection = openConnection
                let! tasks = getTasks connection
                return! json tasks next ctx
            }

    let getTask (id: int) =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                use! connection = openConnection
                let! task = getTask connection id

                match task with
                | Some t -> return! json t next ctx
                | None -> return! RequestErrors.NOT_FOUND $"Task {id} not found." next ctx
            }

    type Create = { Name: string; Description: string }

    let createTask =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                let! create = ctx.BindJsonAsync<Create>()
                use! connection = openConnection

                let! result = createTask connection (create.Name, create.Description)

                match result with
                | Ok id -> return! Successful.CREATED id next ctx
                | Error err ->
                    match err with
                    | PersistanceError -> return! ServerErrors.INTERNAL_ERROR "Cannot save task!" next ctx
            }

    let deleteTask (id: int) =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                use! connection = openConnection

                let! result = deleteTask connection id

                match result with
                | Ok _ -> return! Successful.OK "" next ctx
                | Error err ->
                    match err with
                    | NotFound -> return! RequestErrors.NOT_FOUND "Task not found" next ctx
            }
