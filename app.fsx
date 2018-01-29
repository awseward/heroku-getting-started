#if BOOTSTRAP
System.Environment.CurrentDirectory <- __SOURCE_DIRECTORY__
if not (System.IO.File.Exists "paket.exe") then let url = "https://github.com/fsprojects/Paket/releases/download/3.13.3/paket.exe" in use wc = new System.Net.WebClient() in let tmp = System.IO.Path.GetTempFileName() in wc.DownloadFile(url, tmp); System.IO.File.Move(tmp,System.IO.Path.GetFileName url);;
#r "paket.exe"
Paket.Dependencies.Install (System.IO.File.ReadAllText "paket.dependencies")
#endif

//---------------------------------------------------------------------

#I "packages/Suave/lib/net40"
open System.Web.UI.WebControls.WebParts
#r "packages/Suave/lib/net40/Suave.dll"

open System
open Suave                 // always open suave
open Suave.Http
open Suave.Filters
open Suave.Successful // for OK-result
open Suave.Web             // for config
open System.Net
open Suave.Operators
open Suave.Utils.Collections

printfn "initializing script..."

module Option =
  let ofChoice : Choice<string, string> -> string option =
    function
    | Choice1Of2 str -> Some (str)
    | _ -> None

let config =
    let port = System.Environment.GetEnvironmentVariable("PORT")
    let ip127  = IPAddress.Parse("127.0.0.1")
    let ipZero = IPAddress.Parse("0.0.0.0")

    { defaultConfig with
        bindings=[ (if port = null then HttpBinding.create HTTP ip127 (uint16 8080)
                    else HttpBinding.create HTTP ipZero (uint16 port)) ] }

printfn "starting web server..."

let sprintQuery query =
  let getQueryOrError paramName =
    (query ^^ paramName)
    |> Option.ofChoice
    |> function
       | Some value -> sprintf "%s: %s" paramName value
       | _ -> sprintf "ERROR: Missing param: %s" paramName

  ["api_key"; "org"; "repo"; "pr_numbers"]
  |> List.map getQueryOrError
  |> String.concat "\n"
  
let sample : WebPart =
  path "/hello" >=> choose [
    GET >=> request (fun r -> OK (sprintQuery r.query)) ]

let jsonMime = Writers.setMimeType "application/json"
let app =
  choose
    [ GET >=> choose
                [ path "/" >=> OK "TODO" ]
      sample ]

startWebServer config app
printfn "exiting server..."


