namespace TaskBuddy.Api

module Sql =
    open Dapper.FSharp.SQLite
    open Microsoft.Data.Sqlite
    open System.Threading.Tasks
    open Dapper

    // https://github.com/Dzoukr/Dapper.FSharp#getting-started
    OptionTypes.register ()

    let connectionString = "Data Source=data.db;" // ? Could get it from appsettings.json

    let openConnection =
        task {
            let connection = new SqliteConnection(connectionString)

            do! connection.OpenAsync()

            return connection
        }

    [<CLIMutable>]
    type TaskEntity = {
        Id: int
        Name: string
        Description: string
        Done: bool
    }

    let taskTable = table'<TaskEntity> "task"

    let getTasks (connection: SqliteConnection) =
        task {
            return!
                select {
                    for _ in taskTable do
                        selectAll
                }
                |> connection.SelectAsync<TaskEntity>
        }

    let getTask (connection: SqliteConnection) (id: int) =
        task {
            let! tasks =
                select {
                    for te in taskTable do
                        where (te.Id = id)
                }
                |> connection.SelectAsync<TaskEntity>

            return tasks |> Seq.tryHead
        }

    type CreationError = | PersistanceError

    type TableView = {
        Name: string
        Description: string
        Done: bool
    }

    // Special table view for inserting.
    let tb = table'<TableView> "task"

    let createTask
        (connection: SqliteConnection)
        (name: string, description: string)
        : Task<Result<int, CreationError>> =
        task {
            let! id =
                insert {
                    into tb

                    value {
                        Name = name
                        Description = description
                        Done = false
                    }
                }
                |> connection.InsertAsync

            // ? unsure if SQLite has RETURNING id like PostgreSQL but this works.
            let! id = connection.ExecuteScalarAsync<int>("SELECT last_insert_rowid();")

            return Ok(id)
        }

    type DeletionError = | NotFound

    let deleteTask (connection: SqliteConnection) (id: int) : Task<Result<unit, DeletionError>> =
        task {
            let! affected =
                delete {
                    for te in taskTable do
                        where (te.Id = id)
                }
                |> connection.DeleteAsync

            return
                match affected > 0 with
                | true -> Ok()
                | false -> Error NotFound
        }

// let updateTask (connection: SqliteConnection) (id: int) =
//     task {
//         update {
//             for te in taskTable do
//             set t
//         }
//     }
