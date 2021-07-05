using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

using CommunityToolkit.Mvvm.DependencyInjection;

using WinRepo.Wpf.Views;

using WinRepoSearch.ViewModels;

namespace WinRepo.Wpf.Controls
{
    /// <summary>
    /// Interaction logic for ListDetailsView.xaml
    /// </summary>
    public partial class ListDetailsView : UserControl, INotifyPropertyChanged
    {
        public ListDetailsView()
        {
            InitializeComponent();

            SetValue(ListViewProperty, List);

            List.SelectionChanged += List_SelectionChanged;
            List.SourceUpdated += List_SourceUpdated;

            Details.Navigated += (s, e) =>
            {
                var viewModel = Ioc.Default.GetService<SearchViewModel>();

                viewModel.Status = "Details Navigation Completed.";
            };
        }

        private void List_SourceUpdated(object? sender, DataTransferEventArgs e)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ListViewItemsSource)));
        }

        private void List_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = e.AddedItems.AsQueryable().Cast<object>().FirstOrDefault();

            if (item != null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ListViewSelectedItem)));
            }
        }

        public ListView ListView
        {
            get => List;
        }

        private static PropertyChangedCallback Callback => (d, e) =>
        {
            Console.WriteLine(e.NewValue.ToString());

            if (e.NewValue is not null
                && e.Property.Name == nameof(ListViewSelectedItem)
                && e.NewValue != e.OldValue)
            {
                var cmd = d.GetValue(ItemSelectedCommandProperty);
                var parameter = d.GetValue(ItemSelectedCommandParameterProperty);
                if (cmd is ICommand command && parameter is SearchViewModel viewModel)
                {
                    command.Execute(viewModel);

                    if (d is ListDetailsView view)
                    {
                        view.Details.Navigate(new SearchDetailControl(viewModel));
                    }
                }
            }
        };

        public static DependencyProperty ListViewProperty =
            DependencyProperty.Register(nameof(ListView),
                typeof(ListView),
                typeof(ListDetailsView),
                new PropertyMetadata(Callback));

        public ICommand ItemSelectedCommand
        {
            get => (ICommand)GetAnimationBaseValue(ItemSelectedCommandProperty);
            set => SetValue(ItemSelectedCommandProperty, value);
        }

        public static DependencyProperty ItemSelectedCommandProperty =
            DependencyProperty.Register(nameof(ItemSelectedCommand),
                typeof(ICommand),
                typeof(ListDetailsView),
                new PropertyMetadata(Callback));

        public SearchViewModel ItemSelectedCommandParameter
        {
            get => (SearchViewModel)GetAnimationBaseValue(ItemSelectedCommandParameterProperty);
            set => SetValue(ItemSelectedCommandParameterProperty, value);
        }

        public static DependencyProperty ItemSelectedCommandParameterProperty =
            DependencyProperty.Register(nameof(ItemSelectedCommandParameter),
                typeof(SearchViewModel),
                typeof(ListDetailsView),
                new PropertyMetadata(Callback));

        public IEnumerable ListViewItemsSource
        {
            get => (IEnumerable)GetAnimationBaseValue(ListViewItemsSourceProperty);
            set => SetValue(ListViewItemsSourceProperty, value);
        }

        public static DependencyProperty ListViewItemsSourceProperty =
            DependencyProperty.Register(nameof(ListViewItemsSource),
                typeof(IEnumerable),
                typeof(ListDetailsView),
                new PropertyMetadata(Callback));

        public object ListViewSelectedItem
        {
            get => GetAnimationBaseValue(ListViewSelectedItemProperty);
            set => SetValue(ListViewSelectedItemProperty, value);
        }

        public static DependencyProperty ListViewSelectedItemProperty =
            DependencyProperty.Register(nameof(ListViewSelectedItem),
                typeof(object),
                typeof(ListDetailsView),
                new PropertyMetadata(Callback));

        #region Pass-through events
        //
        // Summary:
        //     Occurs just before any context menu on the element is closed.
        public new event ContextMenuEventHandler ContextMenuClosing
        {
            add
            {
                List.ContextMenuClosing += value;
            }
            remove
            {
                List.ContextMenuClosing -= value;
            }
        }

        //
        // Summary:
        //     Occurs when any context menu on the element is opened.
        public new event ContextMenuEventHandler ContextMenuOpening
        {
            add
            {
                List.ContextMenuOpening += value;
            }
            remove
            {
                List.ContextMenuOpening -= value;
            }
        }

        //
        // Summary:
        //     Occurs when the data context for this element changes.
        public new event DependencyPropertyChangedEventHandler DataContextChanged
        {
            add
            {
                List.DataContextChanged += value;
            }
            remove
            {
                List.DataContextChanged -= value;
            }
        }

        //
        // Summary:
        //     Occurs when this System.Windows.FrameworkElement is initialized. This event coincides
        //     with cases where the value of the System.Windows.FrameworkElement.IsInitialized
        //     property changes from false (or undefined) to true.
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public new event EventHandler Initialized
        {
            add
            {
                List.Initialized += value;
            }
            remove
            {
                List.Initialized -= value;
            }
        }

        //
        // Summary:
        //     Occurs when the element is laid out, rendered, and ready for interaction.
        public new event RoutedEventHandler Loaded
        {
            add
            {
                List.Loaded += value;
            }
            remove
            {
                List.Loaded -= value;
            }
        }

        //
        // Summary:
        //     Occurs when System.Windows.FrameworkElement.BringIntoView(System.Windows.Rect)
        //     is called on this element.
        public new event RequestBringIntoViewEventHandler RequestBringIntoView
        {
            add
            {
                List.RequestBringIntoView += value;
            }
            remove
            {
                List.RequestBringIntoView -= value;
            }
        }

        //
        // Summary:
        //     Occurs when either the System.Windows.FrameworkElement.ActualHeight or the System.Windows.FrameworkElement.ActualWidth
        //     properties change value on this element.
        public new event SizeChangedEventHandler SizeChanged
        {
            add
            {
                List.SizeChanged += value;
            }
            remove
            {
                List.SizeChanged -= value;
            }
        }

        //
        // Summary:
        //     Occurs when the source value changes for any existing property binding on this
        //     element.
        public new event EventHandler<DataTransferEventArgs> SourceUpdated
        {
            add
            {
                List.SourceUpdated += value;
            }
            remove
            {
                List.SourceUpdated -= value;
            }
        }

        //
        // Summary:
        //     Occurs when the target value changes for any property binding on this element.
        public new event EventHandler<DataTransferEventArgs> TargetUpdated
        {
            add
            {
                List.TargetUpdated += value;
            }
            remove
            {
                List.TargetUpdated -= value;
            }
        }

        //
        // Summary:
        //     Occurs just before any tooltip on the element is closed.
        public new event ToolTipEventHandler ToolTipClosing
        {
            add
            {
                List.ToolTipClosing += value;
            }
            remove
            {
                List.ToolTipClosing -= value;
            }
        }

        //
        // Summary:
        //     Occurs when any tooltip on the element is opened.
        public new event ToolTipEventHandler ToolTipOpening
        {
            add
            {
                List.ToolTipOpening += value;
            }
            remove
            {
                List.ToolTipOpening -= value;
            }
        }

        //
        // Summary:
        //     Occurs when the element is removed from within an element tree of loaded elements.
        public new event RoutedEventHandler Unloaded
        {
            add
            {
                List.Unloaded += value;
            }
            remove
            {
                List.Unloaded -= value;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        #endregion
    }

}
