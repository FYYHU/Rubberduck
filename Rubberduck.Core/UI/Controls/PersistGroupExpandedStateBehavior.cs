using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using GongSolutions.Wpf.DragDrop.Utilities;


namespace Rubberduck.UI.Controls
{
    public class PersistGroupExpandedStateBehavior : Behavior<Expander>
    {
        public static readonly DependencyProperty InitialExpandedStateProperty =
            DependencyProperty.Register(nameof(InitialExpandedState), typeof(bool),
                typeof(PersistGroupExpandedStateBehavior), new PropertyMetadata(false));

        public static readonly DependencyProperty GroupNameProperty =
            DependencyProperty.Register(nameof(GroupName), typeof(object),
                typeof(PersistGroupExpandedStateBehavior), new PropertyMetadata(null));

        private static readonly DependencyProperty ExpandedStateStoreProperty =
            DependencyProperty.RegisterAttached("ExpandedStateStore",
                typeof(IDictionary<object, bool>), typeof(PersistGroupExpandedStateBehavior),
                new PropertyMetadata(null));

        public bool InitialExpandedState
        {
            get => (bool)GetValue(InitialExpandedStateProperty);
            set => SetValue(InitialExpandedStateProperty, value);
        }

        public object GroupName
        {
            get => GetValue(GroupNameProperty);
            set => SetValue(GroupNameProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            // Ensure the visual tree is fully loaded before trying to access parents
            AssociatedObject.Dispatcher.BeginInvoke(new System.Action(() =>
            {
                var states = GetExpandedStateStore();
                if (states == null) return;

                var key = GroupName ?? AssociatedObject.GetHashCode();
                if (!states.ContainsKey(key))
                {
                    states[key] = InitialExpandedState;
                }

                AssociatedObject.IsExpanded = states[key];
                AssociatedObject.Expanded += OnExpanded;
                AssociatedObject.Collapsed += OnCollapsed;
            }));
        }

        protected override void OnDetaching()
        {
            AssociatedObject.Expanded -= OnExpanded;
            AssociatedObject.Collapsed -= OnCollapsed;
            base.OnDetaching();
        }

        private void OnCollapsed(object sender, RoutedEventArgs e) => SetExpanded(false);

        private void OnExpanded(object sender, RoutedEventArgs e) => SetExpanded(true);

        private void SetExpanded(bool expanded)
        {
            var states = GetExpandedStateStore();
            if (states != null)
            {
                var key = GroupName ?? AssociatedObject.GetHashCode();
                states[key] = expanded;
            }
        }

        private IDictionary<object, bool> GetExpandedStateStore()
        {
            if (!(AssociatedObject?.GetVisualAncestor<ItemsControl>() is ItemsControl items))
            {
                return null;
            }

            if (!(items.GetValue(ExpandedStateStoreProperty) is IDictionary<object, bool> states))
            {
                states = new Dictionary<object, bool>();
                items.SetValue(ExpandedStateStoreProperty, states);
            }

            return states;
        }
    }
}
