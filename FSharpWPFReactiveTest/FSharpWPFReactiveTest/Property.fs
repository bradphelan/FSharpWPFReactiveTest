module Property

open System
open System.Collections.ObjectModel
open System.ComponentModel
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Quotations.Patterns
open System.Reactive.Linq
open System.Reactive.Subjects
open System.Reactive
open System.Reactive.Disposables
open System.Threading.Tasks
open System.Reactive.Threading.Tasks 


let ToName(query : Expr) = 
    match query with
    | PropertyGet(a, b, list) ->
        b.Name
    | _ -> ""

let SetValue<'t>(obj : Object, query : Expr<'t>, value : 't) =
    match query with
    | PropertyGet(a, b, list) ->
        b.SetValue(obj, value)
    | _ -> ()

let GetValue<'o, 't>(obj : 'o , query : Expr<'t>) : option<'t> =
    match query with
    | PropertyGet(a, b, list) ->
        option.Some(b.GetValue(obj) :?> 't )
    | _ -> option.None

// Observe the property including the current value
let Observe<'t>(x: INotifyPropertyChanged, p : Expr<'t>)  =
    let name = ToName(p)
    let observable = 
        x.PropertyChanged
         .Where(fun (v:PropertyChangedEventArgs) -> v.PropertyName = name)
         .Select(fun v -> GetValue(x, p).Value)

    Observable.Create<'t>(fun (observer : IObserver<'t>) ->
        Observable
            .Return(GetValue(x,p).Value)
            .Merge(observable)
            .SubscribeOnDispatcher()
            .Subscribe(observer)
    )

