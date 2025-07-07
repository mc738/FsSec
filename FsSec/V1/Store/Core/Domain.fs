namespace FsSec.V1.Store.Core

module Domain =

    type Version =
        | Latest
        | Specific of int

    type EntityId =
        | Generated
        | Specific of string
