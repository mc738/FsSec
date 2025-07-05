namespace FsSec.V1.Pipelines

open FsToolbox.Core.Results

module Operations =
    
    
    
    let rewriteFailureMetadata (newValues: List<string * string>) (failure:FailureResult) =
        {
            failure with
                Metadata =
                    newValues
                    |> List.fold (fun (metadata: Map<string, string>) -> metadata.Add)
                        failure.Metadata
        }
    
    let initializeStep (ctx:PipelineContext) (step:PipelineStep) =
        
        ctx.Time(fun _ -> step.Handler.Initialize(ctx))
        |> fun timedResult ->
            let result =
                match step.Handler.Initialize(ctx) with
                | PipelineStepInitializationResult.Success as r ->
                    
                    
                    r
                | PipelineStepInitializationResult.Skipped _ as r -> r
                | PipelineStepInitializationResult.Failure _ as r when step.Mandatory ->
                    ctx.LogTrace("init-steps", $"Initialization for step `{}` failed. However the step is not marked as mandatory will return skipped. Time elapsed:  ")
                    PipelineStepInitializationResult.Skipped "Non-mandatory step failed"
                | PipelineStepInitializationResult.Failure failureResult ->
                    
                    failureResult
                    |> rewriteFailureMetadata [ "stage", "initialization"; "step-name", step.Name ]
                    |> PipelineStepInitializationResult.Failure
                    
                
        
            ctx.SaveInitializationStepResult(timedResult)
            
            result
        
        
        
        
    
    let initializeSteps (ctx:PipelineContext) (steps:PipelineStep list) =
        steps
        |> List.fold
            (fun (result: PipelineStepInitializationResult) step ->
                match result with
                | PipelineStepInitializationResult.Success ->
                    
                    step.Handler.Initialize(ctx)
                | PipelineStepInitializationResult.Skipped _ ->
                    match step.Handler.Initialize(ctx) with
                    | PipelineStepInitializationResult.Success as r -> r
                    | PipelineStepInitializationResult.Skipped _ as r -> r
                    | PipelineStepInitializationResult.Error _ as r when step.Mandatory -> r
                    | PipelineStepInitializationResult.Failure _ as r when step.Mandatory -> r
                    | _ -> PipelineStepInitializationResult.Skipped "Non-mandatory step failed"
                | PipelineStepInitializationResult.Error _ -> result
                | PipelineStepInitializationResult.Failure _ -> result)
                PipelineStepInitializationResult.Success
        |> fun r ->
            match r with
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

