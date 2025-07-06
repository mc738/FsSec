namespace FsSec.V1.Pipelines

open System
open FsSec.V1.Store.Configuration
open FsToolbox.Extensions.Strings
open FsSec.V1.Core.Configuration

module Configuration =

    let retResolveConfigurationValue (ctx: PipelineContext) (value: ConfigurationValueType) =
        match value with
        | ConfigurationValueType.Literal s -> s.ToOption()
        | ConfigurationValueType.EnvironmentalVariable s -> Environment.GetEnvironmentVariable(s).ToOption()
        | ConfigurationValueType.Variable s -> ctx.TryGetVariable s
        | ConfigurationValueType.Path s -> ctx.TryResolvePath s


    type PipelineConfiguration = { Store: PipelineStoreConfiguration }

    and PipelineStoreConfiguration = { StoreType: StoreType }

    and PipelineStepConfiguration =
        {
            Name: string
            Mandatory: string
            StepType: string
        }