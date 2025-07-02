module FsSec.V1.Store.Core

open System
open FsToolbox.Core.Results

type IFsSecStore =
    
    inherit IDisposable
    
    abstract member Initialize: unit -> ActionResult<unit>
    