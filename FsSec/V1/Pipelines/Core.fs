namespace FsSec.V1.Pipelines

open System
open System.Diagnostics
open FsSec.V1.Store.Core
open FsToolbox.Core.Results

[<AutoOpen>]
module Core =

    [<RequireQualifiedAccess>]
    type PipelineStepInitializationResult =
        | Success
        | Skipped of Reason: string
        | Failure of FailureResult

    [<RequireQualifiedAccess>]
    type PipelineStepSetUpResult =
        | Success
        | Skipped of Reason: string
        | Failure of FailureResult

    [<RequireQualifiedAccess>]
    type PipelineStepExecutionResult =
        | Success
        | Skipped of Reason: string
        | Failure of FailureResult

    [<RequireQualifiedAccess>]
    type PipelineStepCleanUpResult =
        | Success
        | Skipped of Reason: string
        | Failure of FailureResult


    type TimedResult<'T> =
        { Result: 'T
          MillisecondsElapsed: int64
          TicksElapsed: int64
          Elapsed: TimeSpan }

    type PipelineContext =
        { RunId: string
          RootPath: string
          Store: IFsSecStore
          Timer: Stopwatch }

        interface IDisposable with
            member this.Dispose() = this.Store.Dispose()

        member pc.GetRunId() = pc.RunId

        member pc.GetStore() = pc.Store
        
        member pc.GetLogger() = { Scope = None; Store = pc.Store }

        member pc.GetScopedLogger(scope) = { Scope = None; Store = pc.Store }
        
        member pc.Time(fn: unit -> 'T) =
            pc.Timer.Start()

            let result = fn ()

            pc.Timer.Stop()

            let timedResult =
                { Result = result
                  MillisecondsElapsed = pc.Timer.ElapsedMilliseconds
                  TicksElapsed = pc.Timer.ElapsedTicks
                  Elapsed = pc.Timer.Elapsed }

            pc.Timer.Reset()
            timedResult

        member internal pc.SaveInitializationStepResult(timedResult: TimedResult<PipelineStepInitializationResult>) = ()

    and PipelineLogger =
        private {
            Scope: string option
            Store: IFsSecStore
        }
        
        member pc.Log
            (level: string, from: string, message: string, scope: string option, timeElapsed: TimeSpan option)
            =
            ()
        
        member pc.LogInfo(from: string, message: string, ?timeElapsed: TimeSpan) =
            pc.Log("info", from, message, scope, )

        member pc.LogTrace(from: string, message: string, ?scope: string, ?timeElapsed: TimeSpan) =
            pc.Log("trace", from, message, scope)

        member pc.LogWarning(from: string, message: string, ?scope: string, ?timeElapsed: TimeSpan) =
            pc.Log("warn", from, message, scope)

        member pc.LogError(from: string, message: string, ?scope: string, ?timeElapsed: TimeSpan) =
            pc.Log("error", from, message, scope)


        
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
                            | PipelineStepInitializationResult.Failure _ as r when step.Mandatory -> r
                            | _ -> PipelineStepInitializationResult.Skipped "Non-mandatory step failed"
                        | PipelineStepInitializationResult.Error _ -> result
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
