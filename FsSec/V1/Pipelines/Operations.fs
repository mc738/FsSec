namespace FsSec.V1.Pipelines

open System
open System.ComponentModel.DataAnnotations
open FsToolbox.Core.Results

module Operations =

    let rewriteFailureMetadata (newValues: List<string * string>) (failure: FailureResult) =
        { failure with
            Metadata =
                newValues
                |> List.fold (fun (metadata: Map<string, string>) -> metadata.Add) failure.Metadata }


    [<RequireQualifiedAccess>]
    module Initialization =

        let private initializeGlobal (ctx: PipelineContext) = ActionResult.Success()

        let private initializeStep (ctx: PipelineContext) (step: PipelineStep) =
            let logger = ctx.GetLogger()

            ctx.Time(fun _ -> step.Handlers.Initialize(ctx))
            |> fun timedResult ->
                let result =
                    match timedResult.Result with
                    | PipelineStepInitializationResult.Success as r ->
                        logger.LogInfo(
                            "init-steps",
                            $"Initialization for step `{step.Name}` was successful.",
                            timedResult.Elapsed
                        )

                        r
                    | PipelineStepInitializationResult.Skipped _ as r ->
                        logger.LogInfo(
                            "init-steps",
                            $"Initialization for step `{step.Name}` was skipped.",
                            timedResult.Elapsed
                        )

                        r
                    | PipelineStepInitializationResult.Failure _ as r when step.Mandatory ->
                        logger.LogWarning(
                            "init-steps",
                            $"Initialization for step `{step.Name}` failed. However the step is not marked as mandatory will return skipped. Time elapsed:  "
                        )

                        PipelineStepInitializationResult.Skipped "Non-mandatory step failed"
                    | PipelineStepInitializationResult.Failure failureResult ->
                        logger.LogError(
                            "init-steps",
                            $"Initialization for step `{step.Name}` failed. Message: {failureResult.Message}",
                            timedResult.Elapsed
                        )

                        failureResult
                        |> rewriteFailureMetadata [ "stage", "initialization"; "step-name", step.Name ]
                        |> PipelineStepInitializationResult.Failure

                ctx.SaveInitializationStepResult(timedResult)

                result

        let private initializeSteps (ctx: PipelineContext) (steps: PipelineStep list) =
            steps
            |> List.fold
                (fun (result: PipelineStepInitializationResult) step ->
                    match result with
                    | PipelineStepInitializationResult.Success
                    | PipelineStepInitializationResult.Skipped _ -> initializeStep ctx step
                    | PipelineStepInitializationResult.Failure _ -> result)
                PipelineStepInitializationResult.Success
            |> fun r ->
                match r with
                | PipelineStepInitializationResult.Success
                | PipelineStepInitializationResult.Skipped _ -> ActionResult.Success()
                | PipelineStepInitializationResult.Failure failureResult -> ActionResult.Failure failureResult

        let run (ctx: PipelineContext) (steps: PipelineStep list) =
            initializeGlobal ctx |> ActionResult.bind (fun _ -> initializeSteps ctx steps)

    module StepUp =

        let private setUpGlobal (ctx: PipelineContext) = ActionResult.Success()

        let private setUpStep (ctx: PipelineContext) (step: PipelineStep) =
            let logger = ctx.GetLogger()

            ctx.Time(fun _ -> step.Handlers.SetUp(ctx))
            |> fun timedResult ->
                let result =
                    match timedResult.Result with
                    | PipelineStepSetUpResult.Success as r ->
                        logger.LogInfo(
                            "init-steps",
                            $"Initialization for step `{step.Name}` was successful.",
                            timedResult.Elapsed
                        )

                        r
                    | PipelineStepSetUpResult.Skipped _ as r ->
                        logger.LogInfo(
                            "set-up-steps",
                            $"Set up for step `{step.Name}` was skipped.",
                            timedResult.Elapsed
                        )

                        r
                    | PipelineStepSetUpResult.Failure _ as r when step.Mandatory ->
                        logger.LogWarning(
                            "init-steps",
                            $"Set up for step `{step.Name}` failed. However the step is not marked as mandatory will return skipped."
                        )

                        PipelineStepSetUpResult.Skipped "Non-mandatory step failed"
                    | PipelineStepSetUpResult.Failure failureResult ->
                        logger.LogError(
                            "init-steps",
                            $"SetUp for step `{step.Name}` failed. Message: {failureResult.Message}",
                            timedResult.Elapsed
                        )

                        failureResult
                        |> rewriteFailureMetadata [ "stage", "set-up"; "step-name", step.Name ]
                        |> PipelineStepSetUpResult.Failure

                ctx.SaveSetUpStepResult(timedResult)

                result

        let private setUpSteps (ctx: PipelineContext) (steps: PipelineStep list) =
            steps
            |> List.fold
                (fun (result: PipelineStepSetUpResult) step ->
                    match result with
                    | PipelineStepSetUpResult.Success
                    | PipelineStepSetUpResult.Skipped _ -> setUpStep ctx step
                    | PipelineStepSetUpResult.Failure _ -> result)
                PipelineStepSetUpResult.Success
            |> fun r ->
                match r with
                | PipelineStepSetUpResult.Success
                | PipelineStepSetUpResult.Skipped _ -> ActionResult.Success()
                | PipelineStepSetUpResult.Failure failureResult -> ActionResult.Failure failureResult

        let run (ctx: PipelineContext) (steps: PipelineStep list) =
            setUpGlobal ctx |> ActionResult.bind (fun _ -> setUpSteps ctx steps)


    module Preprocessing =

        let private preprocessGlobal (ctx: PipelineContext) = ActionResult.Success()

        let private preprocessStep (ctx: PipelineContext) (step: PipelineStep) =
            let logger = ctx.GetLogger()

            ctx.Time(fun _ -> step.Handlers.Preprocessing(ctx))
            |> fun timedResult ->
                let result =
                    match timedResult.Result with
                    | PipelineStepPreprocessingResult.Success as r ->
                        logger.LogInfo(
                            "init-steps",
                            $"Initialization for step `{step.Name}` was successful.",
                            timedResult.Elapsed
                        )

                        r
                    | PipelineStepPreprocessingResult.Skipped _ as r ->
                        logger.LogInfo(
                            "set-up-steps",
                            $"Set up for step `{step.Name}` was skipped.",
                            timedResult.Elapsed
                        )

                        r
                    | PipelineStepPreprocessingResult.Failure _ as r when step.Mandatory ->
                        logger.LogWarning(
                            "init-steps",
                            $"Set up for step `{step.Name}` failed. However the step is not marked as mandatory will return skipped."
                        )

                        PipelineStepPreprocessingResult.Skipped "Non-mandatory step failed"
                    | PipelineStepPreprocessingResult.Failure failureResult ->
                        logger.LogError(
                            "init-steps",
                            $"Preprocess for step `{step.Name}` failed. Message: {failureResult.Message}",
                            timedResult.Elapsed
                        )

                        failureResult
                        |> rewriteFailureMetadata [ "stage", "set-up"; "step-name", step.Name ]
                        |> PipelineStepPreprocessingResult.Failure

                ctx.SavePreprocessStepResult(timedResult)

                result

        let private preprocessSteps (ctx: PipelineContext) (steps: PipelineStep list) =
            steps
            |> List.fold
                (fun (result: PipelineStepPreprocessingResult) step ->
                    match result with
                    | PipelineStepPreprocessingResult.Success
                    | PipelineStepPreprocessingResult.Skipped _ -> preprocessStep ctx step
                    | PipelineStepPreprocessingResult.Failure _ -> result)
                PipelineStepPreprocessingResult.Success
            |> fun r ->
                match r with
                | PipelineStepPreprocessingResult.Success
                | PipelineStepPreprocessingResult.Skipped _ -> ActionResult.Success()
                | PipelineStepPreprocessingResult.Failure failureResult -> ActionResult.Failure failureResult


        let run (ctx: PipelineContext) (steps: PipelineStep list) =
            preprocessGlobal ctx |> ActionResult.bind (fun _ -> preprocessSteps ctx steps)

    module Execution =

        let run (ctx: PipelineContext) (steps: PipelineStep list) = ActionResult.Success()


    module PostProcessing =
        let private postProcessGlobal (ctx: PipelineContext) = ActionResult.Success()

        let private postProcessStep (ctx: PipelineContext) (step: PipelineStep) =
            let logger = ctx.GetLogger()

            ctx.Time(fun _ -> step.Handlers.PostProcessing(ctx))
            |> fun timedResult ->
                let result =
                    match timedResult.Result with
                    | PipelineStepPostProcessingResult.Success as r ->
                        logger.LogInfo(
                            "init-steps",
                            $"Initialization for step `{step.Name}` was successful.",
                            timedResult.Elapsed
                        )

                        r
                    | PipelineStepPostProcessingResult.Skipped _ as r ->
                        logger.LogInfo(
                            "set-up-steps",
                            $"Set up for step `{step.Name}` was skipped.",
                            timedResult.Elapsed
                        )

                        r
                    | PipelineStepPostProcessingResult.Failure _ as r when step.Mandatory ->
                        logger.LogWarning(
                            "init-steps",
                            $"Set up for step `{step.Name}` failed. However the step is not marked as mandatory will return skipped."
                        )

                        PipelineStepPostProcessingResult.Skipped "Non-mandatory step failed"
                    | PipelineStepPostProcessingResult.Failure failureResult ->
                        logger.LogError(
                            "init-steps",
                            $"PostProcess for step `{step.Name}` failed. Message: {failureResult.Message}",
                            timedResult.Elapsed
                        )

                        failureResult
                        |> rewriteFailureMetadata [ "stage", "set-up"; "step-name", step.Name ]
                        |> PipelineStepPostProcessingResult.Failure

                ctx.SavePostProcessStepResult(timedResult)

                result

        let private postProcessSteps (ctx: PipelineContext) (steps: PipelineStep list) =
            steps
            |> List.fold
                (fun (result: PipelineStepPostProcessingResult) step ->
                    match result with
                    | PipelineStepPostProcessingResult.Success
                    | PipelineStepPostProcessingResult.Skipped _ -> postProcessStep ctx step
                    | PipelineStepPostProcessingResult.Failure _ -> result)
                PipelineStepPostProcessingResult.Success
            |> fun r ->
                match r with
                | PipelineStepPostProcessingResult.Success
                | PipelineStepPostProcessingResult.Skipped _ -> ActionResult.Success()
                | PipelineStepPostProcessingResult.Failure failureResult -> ActionResult.Failure failureResult


        let run (ctx: PipelineContext) (steps: PipelineStep list) =
            postProcessGlobal ctx |> ActionResult.bind (fun _ -> postProcessSteps ctx steps)

    module CleanUp =

        let private cleanUpGlobal (ctx: PipelineContext) = ActionResult.Success()

        let private cleanUpStep (ctx: PipelineContext) (step: PipelineStep) =
            let logger = ctx.GetLogger()

            ctx.Time(fun _ -> step.Handlers.CleanUp(ctx))
            |> fun timedResult ->
                let result =
                    match timedResult.Result with
                    | PipelineStepCleanUpResult.Success as r ->
                        logger.LogInfo(
                            "init-steps",
                            $"Initialization for step `{step.Name}` was successful.",
                            timedResult.Elapsed
                        )

                        r
                    | PipelineStepCleanUpResult.Skipped _ as r ->
                        logger.LogInfo(
                            "set-up-steps",
                            $"Set up for step `{step.Name}` was skipped.",
                            timedResult.Elapsed
                        )

                        r
                    | PipelineStepCleanUpResult.Failure _ as r when step.Mandatory ->
                        logger.LogWarning(
                            "init-steps",
                            $"Set up for step `{step.Name}` failed. However the step is not marked as mandatory will return skipped."
                        )

                        PipelineStepCleanUpResult.Skipped "Non-mandatory step failed"
                    | PipelineStepCleanUpResult.Failure failureResult ->
                        logger.LogError(
                            "init-steps",
                            $"CleanUp for step `{step.Name}` failed. Message: {failureResult.Message}",
                            timedResult.Elapsed
                        )

                        failureResult
                        |> rewriteFailureMetadata [ "stage", "set-up"; "step-name", step.Name ]
                        |> PipelineStepCleanUpResult.Failure

                ctx.SaveCleanUpStepResult(timedResult)

                result

        let private cleanUpSteps (ctx: PipelineContext) (steps: PipelineStep list) =
            steps
            |> List.fold
                (fun (result: PipelineStepCleanUpResult) step ->
                    match result with
                    | PipelineStepCleanUpResult.Success
                    | PipelineStepCleanUpResult.Skipped _ -> cleanUpStep ctx step
                    | PipelineStepCleanUpResult.Failure _ -> result)
                PipelineStepCleanUpResult.Success
            |> fun r ->
                match r with
                | PipelineStepCleanUpResult.Success
                | PipelineStepCleanUpResult.Skipped _ -> ActionResult.Success()
                | PipelineStepCleanUpResult.Failure failureResult -> ActionResult.Failure failureResult


        let run (ctx: PipelineContext) (steps: PipelineStep list) =
            cleanUpGlobal ctx |> ActionResult.bind (fun _ -> cleanUpSteps ctx steps)
