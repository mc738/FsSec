namespace FsSec.V1.Core

open System
open FsToolbox.Extensions.Strings

module Configuration =


    type ConfigurationValueType =
        | Literal of string
        | EnvironmentalVariable of string

    let tryGetEnvironmentalVariable (name: string) =
        match name.IsNullOrWhiteSpace() with
        | true -> None
        | false ->
            match name.[0] with
            | '#' -> Environment.GetEnvironmentVariable(name.Substring(1)).ToOption()
            | _ -> Environment.GetEnvironmentVariable(name).ToOption()
