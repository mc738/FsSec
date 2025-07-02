namespace FsSec.V1.Store.SQLite

open System
open Freql.Sqlite
open FsSec.V1.Store.Core
open FsToolbox.Core.Results

[<AutoOpen>]
module Impl =

    type SQLiteFsSecStore(ctx: SqliteContext) =

        interface IFsSecStore with

            member this.Initialize() = Operations.initialize ctx

            member this.Dispose() = (ctx :> IDisposable).Dispose()
