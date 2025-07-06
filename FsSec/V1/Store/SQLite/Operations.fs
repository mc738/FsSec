namespace FsSec.V1.Store.SQLite

open Freql.Sqlite
open FsToolbox.Core.Results
open FunkyPM.V1.Store.SQLite.Persistence

module Operations =

    let initialize (ctx: SqliteContext) =
        try
            match Initialization.runInTransaction true ctx with
            | Ok _ -> ActionResult.Success()
            | Error errorValue ->
                ActionResult.Failure
                    { Message = errorValue.Message
                      DisplayMessage = "Initialize failed"
                      Exception = errorValue.Exception
                      IsTransient = false
                      Metadata = Map.empty }
        with exn ->
            ActionResult.Failure
                { Message = $"Failed to initialize SQLiteFsSecStore. Message: {exn.Message}"
                  DisplayMessage = "Failed to initialize SQLiteFsSecStore"
                  Exception = Some exn
                  IsTransient = false
                  Metadata = Map.empty }
