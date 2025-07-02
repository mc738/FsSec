namespace FsSec.V1.Pipelines

open System
open FsSec.V1.Store.Core
open FsToolbox.Core.Results

module Core =

    module Internal =

        let initialize () =



            ()

    type PipelineContext = { Store: IFsSecStore }

    [<RequireQualifiedAccess>]
    type PipelineStepInitializationResult =
        | Success
        | Skipped
        | Error of Message: string
        | Failure of FailureResult

    [<RequireQualifiedAccess>]
    type PipelineStepExecutionResult =
        | Success
        | Skipped
        | Error of Message: string
        | Failure of FailureResult

    [<RequireQualifiedAccess>]
    type PipelineStepReportResult =
        | Success
        | Skipped
        | Error of Message: string
        | Failure of FailureResult

    type IPipelineStep =

        abstract member Initialize: Ctx: PipelineContext -> PipelineStepInitializationResult

        abstract member Execute: Ctx: PipelineContext -> PipelineStepExecutionResult

        abstract member GetReport: Ctx: PipelineContext -> PipelineStepReportResult

    type PipelineStep =
        { Name: string
          Handler: IPipelineStep
          Mandatory: bool }

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
                        | PipelineStepInitializationResult.Skipped ->
                            match step.Handler.Initialize(p.Context) with
                            | PipelineStepInitializationResult.Success as r -> r
                            | PipelineStepInitializationResult.Skipped as r -> r
                            | PipelineStepInitializationResult.Error _ as r when step.Mandatory -> r
                            | PipelineStepInitializationResult.Failure _ as r when step.Mandatory -> r
                            | _ -> PipelineStepInitializationResult.Skipped
                        | PipelineStepInitializationResult.Error _ -> result
                        | PipelineStepInitializationResult.Failure _ -> result)
                    PipelineStepInitializationResult.Success
                |> function
                    | PipelineStepInitializationResult.Success
                    | PipelineStepInitializationResult.Skipped -> ActionResult.Success()
                    | PipelineStepInitializationResult.Error message ->
                        ActionResult.Failure
                            { Message = message
                              DisplayMessage = message
                              Exception = None
                              IsTransient = false
                              Metadata = Map.empty }
                    | PipelineStepInitializationResult.Failure failureResult -> ActionResult.Failure failureResult)

        member p.Run() =



            ()
