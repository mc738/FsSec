namespace FsSec.V1.Store

open System.Text.Json
open FsSec.V1.Store.SQLite.Configuration
open FsToolbox.Core

module Configuration =

    type StoreType =
        | SQLite of SQLiteFsSecStoreConfiguration

        static member TryFromJson(element: JsonElement) =
            match Json.tryGetStringProperty "type" element with
            | None -> Error "Missing `type` property"
            | Some value ->
                match value.ToLower() with
                | "sqlite" -> SQLiteFsSecStoreConfiguration.TryFromJson element |> Result.map StoreType.SQLite
                | _ -> Error $"Unknown store type: `{value}`"
