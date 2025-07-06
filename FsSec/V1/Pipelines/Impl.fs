namespace FsSec.V1.Pipelines

open System
open FsSec.V1.Pipelines.Configuration
open FsSec.V1.Store.Impl
open FsToolbox.Core.Results

type Pipeline =
    { Context: PipelineContext
      Steps: PipelineStep list }

    interface IDisposable with
        member this.Dispose() = this.Context.Store.Dispose()


    static member TryCreate(cfg: PipelineConfiguration) =
        // First try and create the context.
        try
            let runId = Guid.NewGuid().ToString("n")

            let store = getStore cfg.Store.StoreType

            let ctx = PipelineContext.Create(runId, "", store)


            Ok(
                { Context = PipelineContext.Create(runId, "", store)
                  Steps = failwith "" }
            )
        with ex ->
            Error $"Error while creating pipeline. Message: {ex.Message}"

    member p.Initialize() =
        Operations.Initialization.run p.Context p.Steps

    member p.Run(?skipInitialization: bool) =
        // Initialize first
        if skipInitialization |> Option.defaultValue false then
            ActionResult.Success()
        else
            ActionResult.Success()
        |> ActionResult.bind (fun _ -> Operations.StepUp.run p.Context p.Steps)
        |> ActionResult.bind (fun _ -> Operations.Preprocessing.run p.Context p.Steps)
        |> ActionResult.bind (fun _ -> Operations.Execution.run p.Context p.Steps)
        |> ActionResult.bind (fun _ -> Operations.PostProcessing.run p.Context p.Steps)
        |> ActionResult.bind (fun _ -> Operations.CleanUp.run p.Context p.Steps)
        |> function
            | ActionResult.Success _ -> ()
            | ActionResult.Failure failureResult -> failwith "todo"
