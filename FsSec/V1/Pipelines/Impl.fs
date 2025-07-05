namespace FsSec.V1.Pipelines

open System
open FsToolbox.Core.Results

type Pipeline =
        { Context: PipelineContext
          Steps: PipelineStep list }

        interface IDisposable with
            member this.Dispose() = this.Context.Store.Dispose()

        member p.Initialize() =
            p.Context.Store.Initialize()
            |> ActionResult.bind (fun _ ->

                p.Steps
                |> List.fold
                    (fun (result: PipelineStepInitializationResult) step ->
                        match result with
                        | PipelineStepInitializationResult.Success -> step.Handler.Initialize(p.Context)
                        | PipelineStepInitializationResult.Skipped _ ->
                            match step.Handler.Initialize(p.Context) with
                            | PipelineStepInitializationResult.Success as r -> r
                            | PipelineStepInitializationResult.Skipped _ as r -> r
                            | PipelineStepInitializationResult.Failure _ as r when step.Mandatory -> r
                            | _ -> PipelineStepInitializationResult.Skipped "Non-mandatory step failed"
                        | PipelineStepInitializationResult.Failure _ -> result)
                    PipelineStepInitializationResult.Success
                |> function
                    | PipelineStepInitializationResult.Success
                    | PipelineStepInitializationResult.Skipped _ -> ActionResult.Success()
                    | PipelineStepInitializationResult.Failure failureResult -> ActionResult.Failure failureResult)

        member p.Run(?skipInitialization: bool) =


            // Initialize first
            if skipInitialization |> Option.defaultValue false then
                ActionResult.Success()
            else
                ActionResult.Success()
            |> ActionResult.bind (fun _ ->
                // Then run set up

                ActionResult.Success())
            |> ActionResult.bind (fun _ ->
                // Then execute

                ActionResult.Success())
            |> ActionResult.bind (fun _ ->
                // Then clean up

                ActionResult.Success())
            |> function
                | ActionResult.Success _ -> ()
                | ActionResult.Failure failureResult -> failwith "todo"





            ()
    