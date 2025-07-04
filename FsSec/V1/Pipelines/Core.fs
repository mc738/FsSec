namespace FsSec.V1.Pipelines

open System
open System.Diagnostics
open FsSec.V1.Store.Core
open FsToolbox.Core.Results

module Core =

    module Internal =

        let initialize () =



            ()

    module Operations =
        
        let initialize () = ()
    
    
    type PipelineContext =
        { RunId: string
          Store: IFsSecStore }
        
        interface IDisposable with
            member this.Dispose() =
                this.Store.Dispose()

        member pc.Log() = ()

        member pc.LogInfo() = ()

        member pc.LogTrace() = ()

        member pc.LogWarning() = ()

        member pc.LogError() = ()
        
        member pc.GetRunId() = pc.RunId
        
        member pc.GetStore() = pc.Store
        
    [<RequireQualifiedAccess>]
    type PipelineStepInitializationResult =
        | Success
        | Skipped of Reason: string
        | Error of Message: string
        | Failure of FailureResult

    [<RequireQualifiedAccess>]
    type PipelineStepSetUpResult =
        | Success
        | Skipped of Reason: string
        | Error of Message: string
        | Failure of FailureResult

    [<RequireQualifiedAccess>]
    type PipelineStepExecutionResult =
        | Success
        | Skipped of Reason: string
        | Error of Message: string
        | Failure of FailureResult

    [<RequireQualifiedAccess>]
    type PipelineStepCleanUpResult =
        | Success
        | Skipped of Reason: string
        | Error of Message: string
        | Failure of FailureResult

    type IPipelineStep =

        abstract member Initialize: Ctx: PipelineContext -> PipelineStepInitializationResult

        abstract member StepUp: Ctx: PipelineContext -> unit

        abstract member Execute: Ctx: PipelineContext -> PipelineStepExecutionResult

        abstract member CleanUp: Ctx: PipelineContext -> unit

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
                        | PipelineStepInitializationResult.Skipped _ ->
                            match step.Handler.Initialize(p.Context) with
                            | PipelineStepInitializationResult.Success as r -> r
                            | PipelineStepInitializationResult.Skipped _ as r -> r
                            | PipelineStepInitializationResult.Error _ as r when step.Mandatory -> r
                            | PipelineStepInitializationResult.Failure _ as r when step.Mandatory -> r
                            | _ -> PipelineStepInitializationResult.Skipped "Non-mandatory step failed"
                        | PipelineStepInitializationResult.Error _ -> result
                        | PipelineStepInitializationResult.Failure _ -> result)
                    PipelineStepInitializationResult.Success
                |> function
                    | PipelineStepInitializationResult.Success
                    | PipelineStepInitializationResult.Skipped _ -> ActionResult.Success()
                    | PipelineStepInitializationResult.Error message ->
                        ActionResult.Failure
                            { Message = message
                              DisplayMessage = message
                              Exception = None
                              IsTransient = false
                              Metadata = Map.empty }
                    | PipelineStepInitializationResult.Failure failureResult -> ActionResult.Failure failureResult)

        member p.Run() =
            // Initialize first

            // Then run set up

            // Then execute

            // Then clean up


            ()
