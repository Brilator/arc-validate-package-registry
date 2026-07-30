open Helpers

initializeContext ()

open BasicTasks
open PackageTasks
open PortableModelTasks
open PortableCodecTasks

// Force module initialization so every target is registered.
let _testSolution = testSolution
let _testStagingArea = testStagingArea
let _packIndex = packIndex
let _packClient = packClient
let _testPortableModel = testPortableModel
let _testPortableCodecs = testPortableCodecs

[<EntryPoint>]
let main args =
    runOrDefault testSolution args
