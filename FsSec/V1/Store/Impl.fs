namespace FsSec.V1.Store

open Freql.Sqlite
open FsSec.V1.Core.Configuration
open FsSec.V1.Store.Configuration
open FsSec.V1.Store.SQLite


module Impl =

    let getStore (cfg: Configuration.StoreType) =
        match cfg with
        | SQLite sqliteFsSecStoreConfiguration ->
            let path =
                match sqliteFsSecStoreConfiguration.Path with
                | ConfigurationValueType.Literal s -> s
                | ConfigurationValueType.EnvironmentalVariable s -> System.Environment.GetEnvironmentVariable s
                | ConfigurationValueType.Variable s -> failwith "todo"
                | ConfigurationValueType.Path s -> failwith "todo"

            let ctx = SqliteContext.Open path
            new SQLiteFsSecStore(ctx)
