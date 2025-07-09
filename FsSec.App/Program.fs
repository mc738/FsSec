open System
open System.IO
open System.Net
open System.Net.Http
open FsSec.V1.Core.Configuration
open FsSec.V1.Pipelines
open FsSec.V1.Pipelines.Configuration
open FsSec.V1.Store.Configuration


module I =

    let logo =
        [ "  ____|       ___|             "
          "  |     __| \___ \    _ \   __|"
          "  __| \__ \       |   __/  (   "
          " _|   ____/ _____/  \___| \___|"
          ""
          "FsSec version 1"
          "" ]

    let printLogo () = logo |> List.iter (printfn "%s")

module Test =

    let cfg =
        ({ Store =
            ({ StoreType = StoreType.SQLite { Path = ConfigurationValueType.Literal "" } }: PipelineStoreConfiguration) }
        : PipelineConfiguration)

    let create (cfg: PipelineConfiguration) =
        match Pipeline.TryCreate(cfg) with
        | Error errorValue -> failwith "todo"
        | Ok pipeline -> pipeline

let test _ =

    let baseUrl = "http://localhost:8808/"

    let urls = [ "login.php"; "login"; "" ]

    use client = new HttpClient()

    let acceptList = [ 200 ]

    urls
    |> List.iter (fun url ->
        async {
            let fullUrl = $"{baseUrl}/{url}"

            let! response = client.GetAsync(fullUrl) |> Async.AwaitTask

            match acceptList |> List.contains (int response.StatusCode) with
            | false -> return ()
            | true ->
                let content = response.Content.ReadAsStream()

                use tr = new StreamReader(content)

                let result = tr.ReadToEnd()

                printfn $"Success for {fullUrl}"
                printfn "Response:"
                printfn $"{result}"

                return ()
        }
        |> fun computation -> Async.RunSynchronously computation)

    ()





test ()
