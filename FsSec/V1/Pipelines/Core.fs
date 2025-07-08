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
    type PipelineStepPreprocessingResult =
        | Success
        | Skipped of Reason: string
        | Failure of FailureResult

    [<RequireQualifiedAccess>]
    type PipelineStepExecutionResult =
        | Success
        | Skipped of Reason: string
        | Failure of FailureResult

    [<RequireQualifiedAccess>]
    type PipelineStepPostProcessingResult =
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
        internal
            { RunId: string
              RootPath: string
              Store: IFsSecStore
              Timer: Stopwatch
              LogItemHandler: LogItem -> unit }

        interface IDisposable with
            member this.Dispose() = this.Store.Dispose()

        static member Create(runId: string, rootPath: string, store: IFsSecStore) =
            { RunId = runId
              RootPath = rootPath
              Store = store
              Timer = Stopwatch()
              LogItemHandler = fun _ -> () }

        member ctx.WithLogging() =
            { ctx with
                LogItemHandler = fun _ -> () }

        member pc.GetRunId() = pc.RunId

        member pc.GetStore() = pc.Store

        member pc.GetLogger() =
            { Scope = None
              Store = pc.Store
              ItemHandler = pc.LogItemHandler }

        member pc.GetScopedLogger(scope) =
            { Scope = Some scope
              Store = pc.Store
              ItemHandler = pc.LogItemHandler }

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

        member pc.TryGetVariable(key: string) = Some ""

        member pc.TryResolvePath(key: string) = Some ""

        member internal pc.SaveInitializationStepResult(timedResult: TimedResult<PipelineStepInitializationResult>) = ()
        member internal pc.SaveSetUpStepResult(timedResult: TimedResult<PipelineStepSetUpResult>) = ()
        member internal pc.SavePreprocessStepResult(timedResult: TimedResult<PipelineStepPreprocessingResult>) = ()
        member internal pc.SavePostProcessStepResult(timedResult: TimedResult<PipelineStepPostProcessingResult>) = ()

        member internal pc.SaveCleanUpStepResult(timedResult: TimedResult<PipelineStepCleanUpResult>) = ()

    and PipelineLogger =
        private
            { Scope: string option
              Store: IFsSecStore
              ItemHandler: LogItem -> unit }

        member pc.Log
            (
                itemType: LogItemType,
                from: string,
                message: string,
                timeElapsed: TimeSpan option,
                metadata: Map<string, string> option
            ) =
            { ItemType = itemType
              Scope = pc.Scope
              From = from
              Message = message
              TimeElapsed = timeElapsed
              Metadata = metadata |> Option.defaultValue Map.empty }
            |> pc.ItemHandler

        member pc.LogInfo(from: string, message: string, ?timeElapsed: TimeSpan, ?metadata: Map<string, string>) =
            pc.Log(LogItemType.Info, from, message, timeElapsed, metadata)

        member pc.LogTrace(from: string, message: string, ?timeElapsed: TimeSpan, ?metadata: Map<string, string>) =
            pc.Log(LogItemType.Trace, from, message, timeElapsed, metadata)

        member pc.LogWarning(from: string, message: string, ?timeElapsed: TimeSpan, ?metadata: Map<string, string>) =
            pc.Log(LogItemType.Warning, from, message, timeElapsed, metadata)

        member pc.LogError(from: string, message: string, ?timeElapsed: TimeSpan, ?metadata: Map<string, string>) =
            pc.Log(LogItemType.Error, from, message, timeElapsed, metadata)


    and LogItem =
        { ItemType: LogItemType
          Scope: string option
          From: string
          Message: string
          TimeElapsed: TimeSpan option
          Metadata: Map<string, string> }

    and [<RequireQualifiedAccess>] LogItemType =
        | Info
        | Trace
        | Debug
        | Warning
        | Error
        | Critical

    type PipelineStep =
        { Name: string
          Handlers: PipelineStepOperationHandlers
          Mandatory: bool }

    and PipelineStepOperationHandlers =
        { Initialization: PipelineContext -> PipelineStepInitializationResult
          SetUp: PipelineContext -> PipelineStepSetUpResult
          Preprocessing: PipelineContext -> PipelineStepPreprocessingResult
          Execution: PipelineContext -> PipelineStepExecutionResult
          PostProcessing: PipelineContext -> PipelineStepPostProcessingResult
          CleanUp: PipelineContext -> PipelineStepCleanUpResult }

        static member Stub =
            { Initialization = fun _ -> PipelineStepInitializationResult.Skipped "Not implemented"
              SetUp = fun _ -> PipelineStepSetUpResult.Skipped "Not implemented"
              Preprocessing = fun _ -> PipelineStepPreprocessingResult.Skipped "Not implemented"
              Execution = fun _ -> PipelineStepExecutionResult.Skipped "Not implemented"
              PostProcessing = fun _ -> PipelineStepPostProcessingResult.Skipped "Not implemented"
              CleanUp = fun _ -> PipelineStepCleanUpResult.Skipped "Not implemented" }
