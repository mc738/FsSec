namespace FsSec.V1.Core

open System
open FsToolbox.Extensions.Strings

module Configuration =


    [<RequireQualifiedAccess>]
    type ConfigurationValueType =
        | Literal of string
        | EnvironmentalVariable of string
        | Variable of string
        | Path of string

        static member Deserialize(value: string) =
            match value.[0] with
            | '$' -> ConfigurationValueType.EnvironmentalVariable(value.Substring(1))
            | '@' -> ConfigurationValueType.Path(value.Substring(1))
            | '#' -> ConfigurationValueType.Variable(value.Substring(1))
            | _ -> ConfigurationValueType.Literal value
