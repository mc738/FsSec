namespace FsSec.V1.Store.SQLite

open System.Text.Json
open FsSec.V1.Core.Configuration
open FsToolbox.Core

module Configuration =

    type SQLiteFsSecStoreConfiguration =
        { Path: ConfigurationValueType }

        static member TryFromJson(element: JsonElement) =
            match Json.tryGetStringProperty "path" element with
            | None -> Error "Missing `path` property"
            | Some value -> { Path = ConfigurationValueType.Deserialize value } |> Ok
