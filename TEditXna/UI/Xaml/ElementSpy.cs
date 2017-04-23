// Author: John Smith
// source: http://joshsmithonwpf.wordpress.com/2008/07/22/enable-elementname-bindings-with-elementspy/

using System;
using System.Reflection;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Threading;

namespace TEdit.UI.Xaml
{
    /// <summary>
    /// When placed into an element's Resources collection the 
    /// spy's Element property returns that containing element.
    /// Use the  NameScopeSource attached property to bridge an
    /// element's NameScope to other elements.
    /// </summary>
    public class ElementSpy
        : Freezable
    {
        #region Element

        DependencyObject _element;

        public DependencyObject Element
        {
            get
            {
                if (_element == null)
                {
                    var prop =
                        typeof(Freezable).GetProperty(
                        "InheritanceContext",
                        BindingFlags.Instance | BindingFlags.NonPublic
                        );

                    _element = prop.GetValue(this, null) as DependencyObject;

                    if (_element != null)
                        Freeze();
                }
                return _element;
            }
        }

        #endregion // Element

        #region NameScopeSource

        public static ElementSpy GetNameScopeSource(
            DependencyObject obj)
        {
            return (ElementSpy)obj.GetValue(NameScopeSourceProperty);
        }

        public static void SetNameScopeSource(
            DependencyObject obj, ElementSpy value)
        {
            obj.SetValue(NameScopeSourceProperty, value);
        }

        public static readonly DependencyProperty
            NameScopeSourceProperty =
            DependencyProperty.RegisterAttached(
            "NameScopeSource",
            typeof(ElementSpy),
            typeof(ElementSpy),
            new UIPropertyMetadata(
                null, OnNameScopeSourceChanged));

        static void OnNameScopeSourceChanged(
            DependencyObject depObj,
            DependencyPropertyChangedEventArgs e)
        {
            ElementSpy spy = e.NewValue as ElementSpy;
            if (spy == null || spy.Element == null)
                return;

            INameScope scope =
                NameScope.GetNameScope(spy.Element);
            if (scope == null)
                return;

            depObj.Dispatcher.BeginInvoke(
                DispatcherPriority.Normal,
                (Action)delegate
                {
                    NameScope.SetNameScope(depObj, scope);
                });
        }

        #endregion // NameScopeSource

        #region CreateInstanceCore

        protected override Freezable CreateInstanceCore()
        {
            // We are required to override this abstract method.
            throw new NotSupportedException();
        }

        #endregion // CreateInstanceCore
    }
}