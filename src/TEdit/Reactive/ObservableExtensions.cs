using System;
using ReactiveUI;

namespace TEdit.Reactive;

internal static class ObservableExtensions
{
    public static IObservable<T> ObserveOnMainThread<T>(this IObservable<T> source) =>
        ReactiveUI.Primitives.LinqExtensions.ObserveOn(source, RxSchedulers.MainThreadScheduler);
}
