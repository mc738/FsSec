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
            member this.AddArtifact(runId, artifact, version) = failwith "todo"
            member this.AddGlobalArtifact(artifact, version) = failwith "todo"
            member this.AddGlobalResource(resource, version) = failwith "todo"
            member this.AddOrUpdateKeyValue(runId, key, value) = failwith "todo"
            member this.AddPipelineStepResult(runId) = failwith "todo"
            member this.AddResource(runId, resource, version) = failwith "todo"
            member this.GetArtifact(runId, artifactId, version) = failwith "todo"
            member this.GetGlobalArtifact(artifactId, version) = failwith "todo"
            member this.GetGlobalKeyValue(key) = failwith "todo"
            member this.GetGlobalResource(resourceId, version) = failwith "todo"
            member this.GetKeyValue(runId, key) = failwith "todo"
            member this.GetResource(runId, resourceId, version) = failwith "todo"
            member this.InitializePipelineRun(runId) = failwith "todo"
