module MVVM

open System
open System.Collections.ObjectModel
open System.ComponentModel
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Quotations.Patterns
open System.Reactive.Linq
open System.Reactive.Subjects
open System.Reactive
open System.Reactive.Disposables

type ViewModelBase() =
    let propertyChanged = new Event<_, _>()

    interface INotifyPropertyChanged with
        [<CLIEvent>]
        member x.PropertyChanged = propertyChanged.Publish

    abstract member OnPropertyChanged: string -> unit

    default x.OnPropertyChanged(propertyName : string) =
        propertyChanged.Trigger(x, new PropertyChangedEventArgs(propertyName))

    member x.SetValue<'t>(s : 't byref, expr : Expr<'t>, v : 't) =
        if s <> v then
            s <- v
            x.OnPropertyChanged(expr)
        
    member x.OnPropertyChanged<'t>(expr : Expr<'t>) =
        let propName = Property.ToName(expr)
        x.OnPropertyChanged(propName)

    member x.WhenAny<'t, 'r>(expr : Expr<'t>, fn : Func<'t, 'r>) =
        Property.Observe(x, expr).Select(fn)

    member x.WhenAny<'t0, 't1, 'r>
        ( expr0 : Expr<'t0>
        , expr1 : Expr<'t1>
        , fn : Func<'t0, 't1, 'r>) =

        Property.Observe(x, expr0)
            .CombineLatest(Property.Observe(x, expr1), 
                fn)

    member x.WhenAny<'t0, 't1, 't2, 'r>
        ( expr0 : Expr<'t0>
        , expr1 : Expr<'t1>
        , expr2 : Expr<'t2>
        , fn : Func<'t0, 't1, 't2, 'r>) =

        Property.Observe(x, expr0)
            .CombineLatest(
                Property.Observe(x, expr1), 
                Property.Observe(x, expr2), 
                fn)

    member x.WhenAny<'t0, 't1, 't2, 't3, 'r>
        ( expr0 : Expr<'t0>
        , expr1 : Expr<'t1>
        , expr2 : Expr<'t2>
        , expr3 : Expr<'t3>
        , fn : Func<'t0, 't1, 't2, 't3, 'r>) =

        Property.Observe(x, expr0)
            .CombineLatest(
                Property.Observe(x, expr1), 
                Property.Observe(x, expr2), 
                Property.Observe(x, expr3), 
                fn)

type ObservableProperty<'t when 't : equality>
        ( obj : ViewModelBase
        , query : Expr<'t>
        , observable: IObservable<'t>
        , ?initialValue : 't) =

    let mutable _Value = defaultArg initialValue Unchecked.defaultof<'t> 

    let subscription = lazy( observable.Subscribe(fun v -> 
                _Value <- v
                obj.OnPropertyChanged(query)
        ))

    interface IDisposable with
        member x.Dispose() =
            subscription.Value.Dispose ()
            
    member x.Value
        with get() = 
            // start the subscription if it has
            // not been allready
            subscription.Value |> ignore
            // Return the current value
            _Value

    member x.Start() =
        subscription.Value |> ignore

let ToProperty<'t when 't : equality> (target:ViewModelBase) (query:Expr<'t>) (o:IObservable<'t>) =
    new ObservableProperty<'t>(target, query, o)
        

