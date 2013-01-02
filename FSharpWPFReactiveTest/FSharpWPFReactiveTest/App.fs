module MainApp

open System
open System.Windows
open System.Windows.Controls
open FSharpx
open System.Reactive
open MVVM
open System.ComponentModel

type MainWindow1 = XAML<"MainWindow.xaml">

type TestModel() as this =
    inherit MVVM.ViewModelBase()

    let mutable _A = 10.0
    let mutable _B = 20.0

    let _C = 
        this.WhenAny(<@ this.A @>, <@ this.B @>, fun a b -> a + b )
        |> MVVM.ToProperty this <@ this.C @>

    member x.A 
        with get() = _A
        and set(v) = x.SetValue(&_A, <@ x.A @>, v)

    member x.B 
        with get() = _B
        and set(v) = x.SetValue(&_B, <@ x.B @>, v)

    member x.C 
        with get() = _C.Value

let loadWindow() =
   let window = MainWindow1()
   window.Root.DataContext <- TestModel()
   let model = TestModel()

   // Your awesome code code here and you have strongly typed access to the XAML via "window"
   
   window.Root 

[<STAThread>]
(new Application()).Run(loadWindow()) |> ignore