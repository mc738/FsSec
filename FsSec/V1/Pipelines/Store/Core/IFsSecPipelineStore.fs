namespace FsSec.V1.Pipelines.Store.Core

open FsSec.V1.Core.Artifacts
open FsSec.V1.Core.Resources
open FsToolbox.Core.Results

type IFsSecPipelineStore =
    
    abstract member Initialize: unit -> ActionResult<unit>
    
    abstract member InitializePipelineRun: RunId: string -> ActionResult<unit>
    
    abstract member AddPipelineStepResult: RunId: string -> ActionResult<unit>
    
    abstract member GetGlobalKeyValue: Key: string -> FetchResult<string option>
    
    abstract member GetKeyValue: RunId: string * Key: string -> FetchResult<string option>
    
    abstract member AddOrUpdateKeyValue: RunId: string * Key: string * Value: string -> ActionResult<unit>
    
    abstract member AddArtifact: RunId: string * Artifact: Artifact * Version: Version -> ActionResult<unit>
    
    abstract member GetArtifact: RunId: string * ArtifactId: string * Version: Version -> FetchResult<Artifact option>
    
    abstract member AddResource: RunId: string * Resource: Resource * Version: Version -> ActionResult<unit>
    
    abstract member GetResource: RunId: string * ResourceId: string * Version: Version -> FetchResult<Resource option>
    
