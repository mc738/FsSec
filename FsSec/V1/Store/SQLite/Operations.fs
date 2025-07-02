namespace FsSec.V1.Store.SQLite

open Freql.Sqlite
open FsToolbox.Core.Results

module Operations =

    let initialize (ctx: SqliteContext) =
        try
            ActionResult.Success()
        with exn ->
            ActionResult.Failure
                { Message = $"Failed to initialize SQLiteFsSecStore. Message: {exn.Message}"
                  DisplayMessage = "Failed to initialize SQLiteFsSecStore"
                  Exception = Some exn
                  IsTransient = false
                  Metadata = Map.empty }
