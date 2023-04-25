module TaskBuddy.Api.App

open System
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Giraffe
open Tasks

// ---------------------------------
// Web app
// ---------------------------------

let webApp =
    choose [
        GET >=> choose [ route "/tasks" >=> getTasks ]
        POST >=> choose [ route "/tasks" >=> createTask ]
        GET >=> choose [ routef "/tasks/%i" getTask ]
        DELETE >=> choose [ routef "/tasks/%i" deleteTask ]
    ]

// ---------------------------------
// Error handler
// ---------------------------------

let errorHandler (ex: Exception) (logger: ILogger) =
    logger.LogError(ex, "An unhandled exception has occurred while executing the request.")
    clearResponse >=> setStatusCode 500 >=> text ex.Message

// ---------------------------------
// Main
// ---------------------------------

[<EntryPoint>]
let main args =
    let builder = WebApplication.CreateBuilder(args)

    builder.Services.AddGiraffe() |> ignore

    let app = builder.Build()

    (match builder.Environment.IsDevelopment() with
     | true -> app.UseDeveloperExceptionPage()
     | false -> app.UseGiraffeErrorHandler(errorHandler).UseHttpsRedirection())
        .UseGiraffe(webApp)

    app.Run()
    0
