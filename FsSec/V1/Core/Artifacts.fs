namespace FsSec.V1.Core

open System.IO

module Artifacts =
    
    type Artifact =
        {
            Name: string
            DataStream: MemoryStream
            FileType: FileType
            Metadata: Map<string, string>
        }

