namespace FsSec.V1.Core

open System.IO

module Resources =

    type Resource =
        { Name: string
          DataStream: MemoryStream
          FileType: FileType
          Metadata: Map<string, string> }
