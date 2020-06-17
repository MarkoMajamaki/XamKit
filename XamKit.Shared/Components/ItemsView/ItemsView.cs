using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

// Xamarin
using Xamarin.Forms;

namespace XamKit
{
    /// <summary>
    /// ItemsView selection modes
    /// </summary>
    public enum SelectionModes { None, Single, Multiple }

    /// <summary>
    /// ItemsView item checkbox visibility mode
    /// </summary>
    public enum CheckBoxVisibilityMode { Visible, Hidden, Auto }

    /// <summary>
    /// ItemsView HasItems changed event handler
    /// </summary>
    public delegate void HasItemsChangedEventHandler(ItemsView sender, bool hasItems);

    /// <summary>
    /// ItemsView items changed event handler
    /// </summary>
    public delegate void ItemsChangedEventHandler(ItemsView sender, IList oldItems, IList newItems);

    /// <summary>
    /// ItemsView single SelectedItem changed event handler
    /// </summary>
    public delegate void SelectedItemChangedEventHandler(ItemsView sender, object unSelectedItem, object selectedItem);

    [ContentProperty("ItemsSource")]
    public class ItemsView : Layout<View>, IGeneratorHost
    {
        protected enum ItemActionTypes { Add, Remove }

        private bool m_ignoreFocusedItemChanges = false;
        private bool m_internalIsAnimationEnabled = true;
        private bool m_isResetRunning = false;

        // Animation names
        private const string c_addAnimationName = "addAnimationName";
        private const string c_removeAnimationName = "removeAnimationName";

        private ScrollView m_hostScrollView = null;

        private List<View> m_itemContainersInRemoveAnimation = new List<View>();
        private List<View> m_itemContainersInAddAnimation = new List<View>();

        /// <summary>
        /// Raised wen items source property or items collection changed
        /// </summary>
        public event NotifyCollectionChangedEventHandler ItemsSourceCollectionChanged;

        /// <summary>
        /// Raised wen items source property or items collection changed
        /// </summary>
        public event ItemsChangedEventHandler ItemsSourceChanged;

        /// <summary>
        /// Event for selected items list changes
        /// </summary>
        public event ItemsChangedEventHandler SelectedItemsChanged;

        /// <summary>
        /// Event for single selected item changes
        /// </summary>
        public event SelectedItemChangedEventHandler SelectedItemChanged;

        /// <summary>
        /// Event for HasItems changes
        /// </summary>
        public event HasItemsChangedEventHandler HasItemsChanged;

        /// <summary>
        /// Event when clickable item container is clicked
        /// </summary>
        public event EventHandler ItemTapped;

        /// <summary>
        /// Items generator for item generation and management
        /// </summary>
        public ItemsGenerator ItemsGenerator { get; private set; }

        #region Dependency properties

        /// <summary>
        /// Items data source
        /// </summary>
        public static readonly BindableProperty ItemsSourceProperty =
            BindableProperty.Create("ItemsSource", typeof(IList), typeof(ItemsView), null, propertyChanged: OnItemsSourceChanged);

        private static void OnItemsSourceChanged(BindableObject d, object oldValue, object newValue)
        {
            ItemsView c = d as ItemsView;
            INotifyCollectionChanged oldList = oldValue as INotifyCollectionChanged;
            INotifyCollectionChanged newList = newValue as INotifyCollectionChanged;

            if (oldList != null)
            {
                oldList.CollectionChanged -= c.OnItemsSourceCollectionChangedInternal;
            }

            if (newList != null)
            {
                newList.CollectionChanged += c.OnItemsSourceCollectionChangedInternal;
            }

            c.OnItemsSourceChangedInternal(oldValue as IList, newValue as IList);
        }

        public IList ItemsSource
        {
            get { return (IList)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        /// <summary>
        /// Item DataTemplate
        /// </summary>
        public static readonly BindableProperty ItemTemplateProperty =
            BindableProperty.Create("ItemTemplate", typeof(DataTemplate), typeof(ItemsView), null);

        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }

        /// <summary>
        /// Items DataTemplate selector
        /// </summary>
        public static readonly BindableProperty ItemTemplateSelectorProperty =
            BindableProperty.Create("ItemTemplateSelector", typeof(DataTemplateSelector), typeof(ItemsView), null);

        public DataTemplateSelector ItemTemplateSelector
        {
            get { return (DataTemplateSelector)GetValue(ItemTemplateSelectorProperty); }
            set { SetValue(ItemTemplateSelectorProperty, value); }
        }

        /// <summary>
        /// Items panel template. First child should be type of Panel
        /// </summary>
        public static readonly BindableProperty ItemsLayoutTemplateProperty =
            BindableProperty.Create("ItemsLayoutTemplate", typeof(DataTemplate), typeof(ItemsView), null, propertyChanged: OnItemsLayoutTemplateChanged);

        private static void OnItemsLayoutTemplateChanged(BindableObject d, object oldValue, object newValue)
        {
            ItemsView c = d as ItemsView;

            if (newValue != null)
            {
                c.ItemsLayout = (newValue as DataTemplate).CreateContent() as Layout<View>;
            }
            else if (oldValue != null && newValue == null)
            {
                c.ItemsLayout = null;
            }
        }

        public DataTemplate ItemsLayoutTemplate
        {
            get { return (DataTemplate)GetValue(ItemsLayoutTemplateProperty); }
            set { SetValue(ItemsLayoutTemplateProperty, value); }
        }

        /// <summary>
        /// Actual items layout which is generated from DataTemplate
        /// </summary>
        public static readonly BindableProperty ItemsLayoutProperty =
            BindableProperty.Create("ItemsLayout", typeof(Layout<View>), typeof(ItemsView), null, propertyChanged: OnItemsLayoutChanged);

        private static void OnItemsLayoutChanged(BindableObject d, object oldValue, object newValue)
        {
            (d as ItemsView).OnItemsLayoutChangedInternal(oldValue as Layout<View>, newValue as Layout<View>);
        }

        public Layout<View> ItemsLayout
        {
            get { return (Layout<View>)GetValue(ItemsLayoutProperty); }
            set { SetValue(ItemsLayoutProperty, value); }
        }

        public static readonly BindableProperty KeyboardEventListenerProperty =
            BindableProperty.Create("KeyboardEventListener", typeof(TextChangedEventArgs), typeof(ItemsView), null, propertyChanged: OnKeyboardEventListenerChanged);

        private static void OnKeyboardEventListenerChanged(BindableObject d, object oldValue, object newValue)
        {
            // (d as ItemsView).OnKeyboardEventListenerChanged((TextChangedEventArgs)newValue);
        }

        public TextChangedEventArgs KeyboardEventListener
        {
            get { return (TextChangedEventArgs)GetValue(KeyboardEventListenerProperty); }
            set { SetValue(KeyboardEventListenerProperty, value); }
        }

        public static readonly BindableProperty ItemCommandProperty =
            BindableProperty.Create("ItemCommand", typeof(ICommand), typeof(ItemsView), null);

        public ICommand ItemCommand
        {
            get { return (ICommand)GetValue(ItemCommandProperty); }
            set { SetValue(ItemCommandProperty, value); }
        }

        public static readonly BindableProperty ItemCommandParameterProperty =
            BindableProperty.Create("ItemCommandParameter", typeof(object), typeof(ItemsView), null);

        public object ItemCommandParameter
        {
            get { return (object)GetValue(ItemCommandParameterProperty); }
            set { SetValue(ItemCommandParameterProperty, value); }
        }

        public static readonly BindableProperty HasItemsProperty =
            BindableProperty.Create("HasItems", typeof(bool), typeof(ItemsView), false, propertyChanged: OnHasItemsChanged);

        private static void OnHasItemsChanged(BindableObject d, object oldValue, object newValue)
        {
            ItemsView c = d as ItemsView;
            if (c.HasItemsChanged != null)
            {
                c.HasItemsChanged(c, (bool)newValue);
            }
        }

        public bool HasItems
        {
            get { return (bool)GetValue(HasItemsProperty); }
            protected set { SetValue(HasItemsProperty, value); }
        }

        #endregion
        
        #region Animations

        /// <summary>
        /// Add single item animation duration
        /// </summary>
        public static readonly BindableProperty ItemAddDurationProperty =
            BindableProperty.Create("ItemAddDuration", typeof(int), typeof(ItemsView), 800);

        public int ItemAddDuration
        {
            get { return (int)GetValue(ItemAddDurationProperty); }
            set { SetValue(ItemAddDurationProperty, value); }
        }

        /// <summary>
        /// Rmove single item animation duration
        /// </summary>
        public static readonly BindableProperty ItemRemoveDurationProperty =
            BindableProperty.Create("ItemRemoveDuration", typeof(int), typeof(ItemsView), 400);

        public int ItemRemoveDuration
        {
            get { return (int)GetValue(ItemRemoveDurationProperty); }
            set { SetValue(ItemRemoveDurationProperty, value); }
        }

        /// <summary>
        /// Add all items animation duration
        /// </summary>
        public static readonly BindableProperty ItemAddAllDurationProperty =
            BindableProperty.Create("ItemAddAllDuration", typeof(int), typeof(ItemsView), 400);

        public int ItemAddAllDuration
        {
            get { return (int)GetValue(ItemAddAllDurationProperty); }
            set { SetValue(ItemAddAllDurationProperty, value); }
        }

        /// <summary>
        /// Rmove all items animation duration
        /// </summary>
        public static readonly BindableProperty ItemRemoveAllDurationProperty =
            BindableProperty.Create("ItemRemoveAllDuration", typeof(int), typeof(ItemsView), 400);

        public int ItemRemoveAllDuration
        {
            get { return (int)GetValue(ItemRemoveAllDurationProperty); }
            set { SetValue(ItemRemoveAllDurationProperty, value); }
        }

        /// <summary>
        /// Animation when item collection changes and item is added
        /// </summary>
        public static readonly BindableProperty ItemAddAnimationProperty =
            BindableProperty.Create("ItemAddAnimation", typeof(IAnimation), typeof(ItemsView), null);

        public IAnimation ItemAddAnimation
        {
            get { return (IAnimation)GetValue(ItemAddAnimationProperty); }
            set { SetValue(ItemAddAnimationProperty, value); }
        }

        /// <summary>
        /// Animation when item collection changes and item is removed
        /// </summary>
        public static readonly BindableProperty ItemRemoveAnimationProperty =
            BindableProperty.Create("ItemRemoveAnimation", typeof(IAnimation), typeof(ItemsView), null);

        public IAnimation ItemRemoveAnimation
        {
            get { return (IAnimation)GetValue(ItemRemoveAnimationProperty); }
            set { SetValue(ItemRemoveAnimationProperty, value); }
        }

        /// <summary>
        /// Animation when all item is added
        /// </summary>
        public static readonly BindableProperty ItemAddAllAnimationProperty =
            BindableProperty.Create("ItemAddAllAnimation", typeof(IAnimation), typeof(ItemsView), null);

        public IAnimation ItemAddAllAnimation
        {
            get { return (IAnimation)GetValue(ItemAddAllAnimationProperty); }
            set { SetValue(ItemAddAllAnimationProperty, value); }
        }

        /// <summary>
        /// Animation when all item is removed
        /// </summary>
        public static readonly BindableProperty ItemRemoveAllAnimationProperty =
            BindableProperty.Create("ItemRemoveAllAnimation", typeof(IAnimation), typeof(ItemsView), null);

        public IAnimation ItemRemoveAllAnimation
        {
            get { return (IAnimation)GetValue(ItemRemoveAllAnimationProperty); }
            set { SetValue(ItemRemoveAllAnimationProperty, value); }
        }

        /// <summary>
        /// Add single item animation easing function
        /// </summary>
        public static readonly BindableProperty ItemAddEasingProperty =
            BindableProperty.Create("ItemAddEasing", typeof(Easing), typeof(ItemsView), null);

        public Easing ItemAddEasing
        {
            get { return (Easing)GetValue(ItemAddEasingProperty); }
            set { SetValue(ItemAddEasingProperty, value); }
        }

        /// <summary>
        /// Rmove single item animation easing function
        /// </summary>
        public static readonly BindableProperty ItemRemoveEasingProperty =
            BindableProperty.Create("ItemRemoveEasing", typeof(Easing), typeof(ItemsView), null);

        public Easing ItemRemoveEasing
        {
            get { return (Easing)GetValue(ItemRemoveEasingProperty); }
            set { SetValue(ItemRemoveEasingProperty, value); }
        }

        /// <summary>
        /// Add all items animation easing function
        /// </summary>
        public static readonly BindableProperty ItemAddAllEasingProperty =
            BindableProperty.Create("ItemAddAllEasing", typeof(Easing), typeof(ItemsView), null);

        public Easing ItemAddAllEasing
        {
            get { return (Easing)GetValue(ItemAddAllEasingProperty); }
            set { SetValue(ItemAddAllEasingProperty, value); }
        }

        /// <summary>
        /// Rmove all items animation easing function
        /// </summary>
        public static readonly BindableProperty ItemRemoveAllEasingProperty =
            BindableProperty.Create("ItemRemoveAllEasing", typeof(Easing), typeof(ItemsView), null);

        public Easing ItemRemoveAllEasing
        {
            get { return (Easing)GetValue(ItemRemoveAllEasingProperty); }
            set { SetValue(ItemRemoveAllEasingProperty, value); }
        }

        /// <summary>
        /// Delay between item add or remove
        /// </summary>
        public static readonly BindableProperty ItemAsyncDelayProperty =
            BindableProperty.Create("ItemAsyncDelay", typeof(double), typeof(ItemsView), 0.0);

        public double ItemAsyncDelay
        {
            get { return (double)GetValue(ItemAsyncDelayProperty); }
            set { SetValue(ItemAsyncDelayProperty, value); }
        }

        public static readonly BindableProperty IsAnimationEnabledProperty =
            BindableProperty.Create("IsAnimationEnabled", typeof(bool), typeof(ItemsView), true);

        public bool IsAnimationEnabled
        {
            get { return (bool)GetValue(IsAnimationEnabledProperty); }
            set { SetValue(IsAnimationEnabledProperty, value); }
        }

        /// <summary>
        /// Attached property to ignore single item animation
        /// </summary>
        public static readonly BindableProperty IgnoreAnimationProperty =
            BindableProperty.CreateAttached("IgnoreAnimation", typeof(bool), typeof(ItemsView), false);

        public static bool GetIgnoreAnimation(BindableObject obj)
        {
            return (bool)obj.GetValue(IgnoreAnimationProperty);
        }

        public static void SetIgnoreAnimation(BindableObject obj, bool value)
        {
            obj.SetValue(IgnoreAnimationProperty, value);
        }

        #endregion

        #region Selection

        public static readonly BindableProperty SelectionModeProperty =
            BindableProperty.Create("SelectionMode", typeof(SelectionModes), typeof(ItemsView), SelectionModes.Multiple, propertyChanged: OnSelectionModeChanged);

        private static void OnSelectionModeChanged(BindableObject d, object oldValue, object newValue)
        {
            ((ItemsView)d).OnSelectionModeChangedInternal((SelectionModes)oldValue, (SelectionModes)newValue);
        }

        public SelectionModes SelectionMode
        {
            get { return (SelectionModes)GetValue(SelectionModeProperty); }
            set { SetValue(SelectionModeProperty, value); }
        }

        public static readonly BindableProperty SelectedItemProperty =
            BindableProperty.Create("SelectedItem", typeof(object), typeof(ItemsView), null, propertyChanged: OnSelectedItemChanged);

        private static void OnSelectedItemChanged(BindableObject d, object oldValue, object newValue)
        {
            ((ItemsView)d).OnSelectedItemChangedInternal(oldValue as object, newValue as object);
        }

        public object SelectedItem
        {
            get { return (object)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        public static readonly BindableProperty SelectedItemsProperty =
            BindableProperty.Create("SelectedItems", typeof(IList), typeof(ItemsView), null, propertyChanged: OnSelectedItemsChanged);

        private static void OnSelectedItemsChanged(BindableObject d, object oldValue, object newValue)
        {
            ItemsView c = d as ItemsView;

            INotifyCollectionChanged oldItemsList = oldValue as INotifyCollectionChanged;
            if (oldItemsList != null)
            {
                oldItemsList.CollectionChanged -= c.OnSelectedItemsCollectionChanged;
            }

            INotifyCollectionChanged newItemsList = newValue as INotifyCollectionChanged;
            if (newItemsList != null)
            {
                newItemsList.CollectionChanged += c.OnSelectedItemsCollectionChanged;
            }

            c.OnSelectedItemsChangedInternal((IList)oldValue, (IList)newValue);
        }

        public IList SelectedItems
        {
            get { return (IList)GetValue(SelectedItemsProperty); }
            set { SetValue(SelectedItemsProperty, value); }
        }

        /// <summary>
        /// Checkbox visibility mode if item container is ACMenuButton (TODO: Maybe in wrong class...)
        /// </summary>
        public static readonly BindableProperty CheckBoxVisibilityProperty =
            BindableProperty.Create("CheckBoxVisibility", typeof(CheckBoxVisibilityMode), typeof(ItemsView), CheckBoxVisibilityMode.Auto);

        public CheckBoxVisibilityMode CheckBoxVisibility
        {
            get { return (CheckBoxVisibilityMode)GetValue(CheckBoxVisibilityProperty); }
            set { SetValue(CheckBoxVisibilityProperty, value); }
        }

        #endregion

        #region Virtualizing

        /// <summary>
        /// Is virtualizing enabled if panel is virtualized
        /// </summary>
        public static readonly BindableProperty IsVirtualizingEnabledProperty =
            BindableProperty.Create("IsVirtualizingEnabled", typeof(bool), typeof(ItemsView), true);

        public bool IsVirtualizingEnabled
        {
            get { return (bool)GetValue(IsVirtualizingEnabledProperty); }
            set { SetValue(IsVirtualizingEnabledProperty, value); }
        }

        /// <summary>
        /// Is items virtualized
        /// </summary>
        private bool IsVirtualzed
        {
            get
            {
                return ItemsLayout != null && ItemsLayout is IVirtualizingLayout && IsVirtualizingEnabled;
            }
        }

        /// <summary>
        /// Actual virtualizing panel
        /// </summary>
        private IVirtualizingLayout VirtualizingPanel
        {
            get
            {
                return ItemsLayout as IVirtualizingLayout;
            }
        }

        #endregion

        #region Focus

        /// <summary>
        /// Focused item container or container datacontext. If item is added in its own container, then focused item is container. If ItemsSource is 
        /// list of models, then focused item is model object.
        /// </summary>
        public static readonly BindableProperty FocusedItemProperty =
            BindableProperty.Create("FocusedItem", typeof(object), typeof(ItemsView), null, propertyChanged: OnFocusedItemChanged);

        private static void OnFocusedItemChanged(BindableObject d, object oldValue, object newValue)
        {
            (d as ItemsView).OnFocusedItemChanged((object)oldValue, (object)newValue);
        }

        public object FocusedItem
        {
            get { return (object)GetValue(FocusedItemProperty); }
            set { SetValue(FocusedItemProperty, value); }
        }

        /// <summary>
        /// Focused item index
        /// </summary>
        public static readonly BindableProperty FocusedItemIndexProperty =
            BindableProperty.Create("FocusedItemIndex", typeof(int), typeof(ItemsView), -1, propertyChanged: OnFocusedItemIndexChanged);

        private static void OnFocusedItemIndexChanged(BindableObject d, object oldValue, object newValue)
        {
            (d as ItemsView).OnFocusedItemIndexChanged((int)oldValue, (int)newValue);
        }

        public int FocusedItemIndex
        {
            get { return (int)GetValue(FocusedItemIndexProperty); }
            set { SetValue(FocusedItemIndexProperty, value); }
        }

        /// <summary>
        /// Is item focused on mouse hover
        /// </summary>
        public static readonly BindableProperty IsFocusedOnHoverProperty =
            BindableProperty.Create("IsFocusedOnHover", typeof(bool), typeof(ItemsView), true);

        public bool IsFocusedOnHover
        {
            get { return (bool)GetValue(IsFocusedOnHoverProperty); }
            set { SetValue(IsFocusedOnHoverProperty, value); }
        }

        /// <summary>
        /// Attached property to set item focused
        /// </summary>
        public static readonly BindableProperty IsItemFocusedProperty =
            BindableProperty.CreateAttached("IsItemFocused", typeof(bool), typeof(ItemsView), false);

        public static bool GetIsItemFocused(BindableObject obj)
        {
            return (bool)obj.GetValue(IsItemFocusedProperty);
        }

        public static void SetIsItemFocused(BindableObject obj, bool value)
        {
            obj.SetValue(IsItemFocusedProperty, value);
        }

        #endregion

        #region Empty content

        public static readonly BindableProperty IsEmptyContentEnabledProperty =
            BindableProperty.Create("IsEmptyContentEnabled", typeof(bool), typeof(ItemsView), true);

        public bool IsEmptyContentEnabled
        {
            get { return (bool)GetValue(IsEmptyContentEnabledProperty); }
            set { SetValue(IsEmptyContentEnabledProperty, value); }
        }

        public static readonly BindableProperty EmptyContentProperty =
            BindableProperty.Create("EmptyContent", typeof(View), typeof(ItemsView), null, propertyChanged: OnEmptyContentChanged);

        private static void OnEmptyContentChanged(BindableObject d, object oldValue, object newValue)
        {
            // Set empty content to null and create new when needed
            (d as ItemsView).OnEmptyContentChanged(oldValue as View, newValue as View);
        }

        public View EmptyContent
        {
            get { return (View)GetValue(EmptyContentProperty); }
            set { SetValue(EmptyContentProperty, value); }
        }

        public static readonly BindableProperty EmptyContentTemplateProperty =
            BindableProperty.Create("EmptyContentTemplate", typeof(DataTemplate), typeof(ItemsView), null, propertyChanged: OnEmptyContentTemplateChanged);

        private static void OnEmptyContentTemplateChanged(BindableObject d, object oldValue, object newValue)
        {
            // Set empty content to null and create new when needed
            (d as ItemsView).OnEmptyContentTemplateChanged(oldValue as DataTemplate, newValue as DataTemplate);
        }

        public DataTemplate EmptyContentTemplate
        {
            get { return (DataTemplate)GetValue(EmptyContentTemplateProperty); }
            set { SetValue(EmptyContentTemplateProperty, value); }
        }

        #endregion

        public ItemsView()
        {
            ItemsGenerator = new ItemsGenerator(this);
            SetValue(SelectedItemsProperty, new ObservableCollection<object>());

            if (ItemsLayout != null && Children.Contains(ItemsLayout) == false)
            {
                OnItemsLayoutChangedInternal(null, ItemsLayout);
            }

            if (ItemsSource == null)
            {
                SetValue(ItemsSourceProperty, new ObservableCollection<View>());
            }
        }

        #region Measure / Layout

        protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
        {
            Size totalSize = new Size();

            if (m_hostScrollView == null)
            {
                m_hostScrollView = GetHostScrollViewer();
                if (m_hostScrollView != null)
                {
                    m_hostScrollView.Scrolled += OnScrollChanged;
                }
            }

            if (HorizontalOptions.Alignment == LayoutAlignment.Fill && 
                VerticalOptions.Alignment == LayoutAlignment.Fill &&
                double.IsNaN(widthConstraint) == false && double.IsInfinity(widthConstraint) == false &&
                double.IsNaN(heightConstraint) == false && double.IsInfinity(heightConstraint) == false)
            {
                totalSize = new Size(widthConstraint, heightConstraint);
            }
            else
            {
                if (ItemsLayout != null && ItemsLayout.IsVisible && Children.Contains(ItemsLayout))
                {
                    if (IsVirtualzed)
                    {
                        UpdateVirtualizedPanelProperties(VirtualizingPanel, widthConstraint, heightConstraint);
                    }

                    SizeRequest size = ItemsLayout.Measure(widthConstraint, heightConstraint, MeasureFlags.IncludeMargins);

                    totalSize = new Size(size.Request.Width, size.Request.Height);
                }

                if (EmptyContent != null && EmptyContent.IsVisible && Children.Contains(EmptyContent))
                {
                    SizeRequest size = EmptyContent.Measure(widthConstraint, heightConstraint, MeasureFlags.IncludeMargins);
                    totalSize = new Size(Math.Max(totalSize.Width, size.Request.Width), Math.Max(totalSize.Height, size.Request.Height));
                }

                if (double.IsNaN(widthConstraint) == false && double.IsInfinity(widthConstraint) == false && HorizontalOptions.Alignment == LayoutAlignment.Fill)
                {
                    totalSize.Width = widthConstraint;
                }
                if (double.IsNaN(heightConstraint) == false && double.IsInfinity(heightConstraint) == false && VerticalOptions.Alignment == LayoutAlignment.Fill)
                {
                    totalSize.Height = heightConstraint;
                }
            }

            return new SizeRequest(totalSize, totalSize);
        }

        protected override void LayoutChildren(double x, double y, double width, double height)
        {
            if (m_hostScrollView == null)
            {
                m_hostScrollView = GetHostScrollViewer();
                if (m_hostScrollView != null)
                {
                    m_hostScrollView.Scrolled += OnScrollChanged;
                }
            }

            if (IsVirtualzed)
            {
                UpdateVirtualizedPanelProperties(VirtualizingPanel, width, height);
            }

            if (ItemsLayout != null && ItemsLayout.IsVisible && Children.Contains(ItemsLayout))
            {
                LayoutChild(ItemsLayout, new Rectangle(0, 0, width, height));
            }

            if (EmptyContent != null && EmptyContent.IsVisible && Children.Contains(EmptyContent))
            {
                LayoutChild(EmptyContent, new Rectangle(0, 0, width, height));
            }
        }

        /// <summary>
        /// Layout view if needed (optimization)
        /// </summary>
        private void LayoutChild(View child, Rectangle newLocation)
        {
            if (child != null && child.Bounds != newLocation && Children.Contains(child))
            {
                LayoutChildIntoBoundingRegion(child, newLocation);
            }
        }

        #endregion

        /// <summary>
        /// Called when tappable item container is tapped
        /// </summary>
        /// <param name="itemContainer"></param>
        protected virtual void OnItemContainerTapped(View itemContainer)
        {
            return;
        }

        /// <summary>
        /// Item container click event handler
        /// </summary>
        protected void OnItemContainerTapped(object sender, EventArgs e)
        {
            OnItemContainerTapped(sender as View);

            if (ItemCommand != null)
            {
                if (ItemCommandParameter != null)
                {
                    ItemCommand.Execute(ItemCommandParameter);
                }
                else
                {
                    ItemCommand.Execute((sender as View).BindingContext);
                }
            }

            if (ItemTapped != null)
            {
                ItemTapped(sender, e);
            }
        }
        
        /// <summary>
        /// Update virtualizing panel viewport size based on panel size
        /// </summary>
        private void UpdateVirtualizedPanelProperties(IVirtualizingLayout panel, double width, double height)
        {
            double verticalOffset = GetAbsolutePosition(this).Y;

            Size viewportSize = new Size(width, height);
            if (m_hostScrollView != null)
            {
                viewportSize.Width = m_hostScrollView.Width;
                viewportSize.Height = m_hostScrollView.Height;
            }

            if (panel.VerticalOffset.Equals(-verticalOffset) == false)
            {
                panel.VerticalOffset = -verticalOffset;
            }

            if (panel.ViewportSize.Width.Equals(viewportSize.Width) == false || panel.ViewportSize.Height.Equals(viewportSize.Height) == false)
            {
                panel.ViewportSize = new Size(viewportSize.Width, viewportSize.Height);
            }
        }

        /// <summary>
        /// Get placement target absolute position on app window
        /// </summary>
        private Point GetAbsolutePosition(View element)
        {
            Point point = new Point();
            point.Y = element.Y; // - element.Margin.Top;
            point.X = element.X; // - element.Margin.Left;

            VisualElement e = element.Parent as VisualElement;

            while (e != null)
            {
                if (e is Xamarin.Forms.ScrollView scrollViewer)
                {
                    Xamarin.Forms.ScrollView s = e as Xamarin.Forms.ScrollView;
                    point.Y -= s.ScrollY;
                    point.X -= s.ScrollX;
                    return point;
                }

                point.Y += e.Y;
                point.X += e.X;

                e = e.Parent as VisualElement;
            }

            return new Point();
        }

        #region ItemContainer generation

        /// <summary>
        /// Is container generation needed
        /// </summary>
        public virtual bool IsItemItsOwnContainer(object item)
        {
            return item is View;
        }

        /// <summary>
        /// Get item container content template from ItemTemplate or ItemTemplateSelector
        /// </summary>
        /// <param name="itemContainer">Item container</param>
        /// <param name="model">Item model</param>
        /// <returns>Item container content datatemplate</returns>
        public DataTemplate CreateItemTemplate(object model)
        {
            if (ItemTemplateSelector != null)
            {
                return ItemTemplateSelector.SelectTemplate(model, null);
            }
            else if (ItemTemplate != null)
            {
                return ItemTemplate;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Prepare item container
        /// </summary>
        /// <param name="itemView">Item element</param>
        /// <param name="model">Item model. Null if item is its own container.</param>
        public void PrepareItemView(View itemView, object model)
        {
            if (model != null)
            {
                itemView.BindingContext = model;
            }

            if (itemView is IToggable toggableItem)
            {
                if (SelectionMode == SelectionModes.Multiple)
                {
                    if (SelectedItems != null && (SelectedItems.Contains(itemView) || (model != null && SelectedItems.Contains(model))))
                    {
                        if (toggableItem.IsToggled == false)
                        {
                            toggableItem.IsToggled = true;
                        }
                    }
                    else if (toggableItem.IsToggled == true)
                    {
                        toggableItem.IsToggled = false;
                    }
                }
                else if (SelectionMode == SelectionModes.Single)
                {
                    if (SelectedItem == itemView || (model != null && SelectedItems == model))
                    {
                        if (toggableItem.IsToggled == false)
                        {
                            toggableItem.IsToggled = true;
                        }
                    }
                    else if (toggableItem.IsToggled == true)
                    {
                        toggableItem.IsToggled = false;
                    }
                }

                toggableItem.IsToggledChanged += OnItemContainerIsCheckedChanged;
            }

            if (itemView is ITappable tappableItem)
            {
                tappableItem.Tapped += OnItemContainerTapped;
            }

            PrepareItemView(itemView);
        }

        /// <summary>
        /// Do item container custom preparation
        /// </summary>
        protected virtual void PrepareItemView(View itemView)
        {
            return;
        }

        #endregion

        #region ItemsLayout

        /// <summary>
        /// Called when 'ItemsLayout' changes. Children are moved to new panel after this.
        /// </summary>
        protected virtual void OnItemsLayoutChanged(Layout<View> oldLayout, Layout<View> newLayout)
        {
            return;
        }

        /// <summary>
        /// Handle ItemsLayout changes
        /// </summary>
        private async void OnItemsLayoutChangedInternal(Layout<View> oldLayout, Layout<View> newLayout)
        {
            if (Children == null)
            {
                return;
            }

            if (IsVirtualzed)
            {
                VirtualizingPanel.ItemsGenerator = ItemsGenerator;
            }

            if (oldLayout != null && Children.Contains(oldLayout))
            {
                Children.Remove(oldLayout);
            }
            if (newLayout != null && Children.Contains(newLayout) == false)
            {
                Children.Add(newLayout);
            }

            OnItemsLayoutChanged(oldLayout, newLayout);

            if (oldLayout == null && newLayout != null)
            {
                if (ItemsSource != null && ItemsSource.Count > 0)
                {
                    ItemsGenerator.RemoveAll();

                    // Add children from ItemsSource
                    await AddItem(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, ItemsSource, 0));
                }
            }
            else if (oldLayout != null && newLayout != null)
            {
                List<View> oldPanelChildren = new List<View>();

                // Clear first panel
                foreach (var c in oldLayout.Children)
                {
                    oldPanelChildren.Add(c as View);
                }

                oldLayout.Children.Clear();

                // Add children from old panel to new
                foreach (var c in oldPanelChildren)
                {
                    if (newLayout is IItemsViewLayout itemsViewLayout && c != oldPanelChildren[oldPanelChildren.Count - 1])
                    {
                        itemsViewLayout.AddChild(c);
                    }
                    else
                    {
                        newLayout.Children.Add(c);
                    }
                }
            }
            else if (oldLayout != null && newLayout == null)
            {
                foreach (View view in oldLayout.Children)
                {
                    ReleaseItemContainer(view);
                }

                oldLayout.Children.Clear();
            }
        }

        #endregion

        #region Items

        /// <summary>
        /// Called when 'ItemsSource' property changes
        /// </summary>
        private async void OnItemsSourceChangedInternal(IList oldSource, IList newSource)
        {
            if (ItemsSourceChanged != null)
            {
                ItemsSourceChanged(this, oldSource, newSource);
            }

            OnItemsSourceChanged(oldSource, newSource);

            HasItems = ItemsSource != null && ItemsSource.Count > 0;

            if (EmptyContent != null && EmptyContent.IsVisible == true && IsEmptyContentEnabled)
            {
                IsEmptyContentVisible(HasItems == false);
            }

            if (m_isResetRunning == false)
            {
                bool hasOldItems = oldSource != null && oldSource.Count > 0;
                bool hasNewItems = newSource != null && newSource.Count > 0;

                if (hasOldItems == true || (hasOldItems == false && hasNewItems == false))
                {
                    await Reset();
                }

                if (hasNewItems)
                {
                    await AddItem(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, ItemsSource, 0));
                }            
            }

            if (EmptyContent != null && EmptyContent.IsVisible == false && IsEmptyContentEnabled)
            {
                IsEmptyContentVisible(HasItems == false);
            }
        }

        /// <summary>
        /// Called when ItemsSource property changes.
        /// </summary>
        protected virtual void OnItemsSourceChanged(IList oldItems, IList newItems)
        {
            return;
        }

        /// <summary>
        /// Event when ItemsSource internal collection changes
        /// </summary>
        private async void OnItemsSourceCollectionChangedInternal(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (ItemsSourceCollectionChanged != null)
            {
                ItemsSourceCollectionChanged(this, e);
            }

            OnItemsSourceCollectionChanged(e);

            await OnItemsSourceCollectionChangedInternalAsync(sender, e, false);

            HasItems = (ItemsSource != null && ItemsSource.Count > 0) || (e.NewItems != null && e.NewItems.Count > 0);

            IsEmptyContentVisible(HasItems == false);
        }

        /// <summary>
        /// Called when ItemsSource collection changes. Collection changes only if 'ItemsSource' implements INotifyCollectionChanged.
        /// </summary>
        protected virtual void OnItemsSourceCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            return;
        }

        /// <summary>
        /// Called when items source collection changes (when used ObservableCollection)
        /// </summary>
        private async Task OnItemsSourceCollectionChangedInternalAsync(object sender, NotifyCollectionChangedEventArgs e, bool allChanged)
        {
            // Add new items
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                await AddItem(e);
            }
            // Move current items
            else if (e.Action == NotifyCollectionChangedAction.Move)
            {
                await MoveItem(e);
            }
            // Remove single or more items
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                await RemoveItem(e);
            }
            // Replace items
            else if (e.Action == NotifyCollectionChangedAction.Replace)
            {
                await ReplaceItem(e);
            }
            // Remove ALL items
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                await Reset();
            }
            else
            {
                throw new Exception("ItemsView: Invalid action!");
            }
        }

        /// <summary>
        /// Add item
        /// </summary>
        private async Task AddItem(NotifyCollectionChangedEventArgs e)
        {
            int index = (e.NewStartingIndex == -1) ? 0 : e.NewStartingIndex;

            // Removing items count before index
            int removingItemsCount = GetRemovingItemsCountBefore(index);

            foreach (object item in e.NewItems)
            {
                int actualIndex = index - removingItemsCount;

                View itemContainer = null;
                if (IsItemItsOwnContainer(item))
                {
                    itemContainer = item as View;
                    PrepareItemView(itemContainer, null);
                    ItemsGenerator.Insert(actualIndex, null, itemContainer, ItemStateTypes.Adding);
                }
                else
                {
                    ItemsGenerator.Insert(actualIndex, item, null, ItemStateTypes.Adding);

                    if (IsVirtualzed == false)
                    {
                        itemContainer = ItemsGenerator.GenerateItemView(actualIndex);
                    }
                }

                index++;
            }

            bool addAll = e.NewItems.Count == ItemsSource.Count;

            if (ItemsLayout != null)
            {
                if (IsVirtualzed && ItemsLayout.Children.Count == 0 && ItemsSource.Count > 0 && (ItemsGenerator.HasItemViewGenerated(0) == false || ItemsLayout.Children.Count() == 0))
                {
                    (ItemsLayout as VirtualizingLayout).Initialize();
                }

                // Update visible item
                await UpdateItems(addAll, IsAnimationEnabled);
            }
        }

        /// <summary>
        /// Remove item
        /// </summary>
        private async Task RemoveItem(NotifyCollectionChangedEventArgs e, bool isItemsUpdated = true)
        {
            int index = (e.OldStartingIndex == -1) ? 0 : e.OldStartingIndex;
            int removeCount = e.OldItems.Count;

            // Removing items count before index
            int removingItemsBeforeCount = GetRemovingItemsCountBefore(index);

            for (int i = removingItemsBeforeCount + index; i < ItemsGenerator.TotalItemsCount; i++)
            {
                ItemStateTypes itemState = ItemsGenerator.GetState(i);

                if (itemState != ItemStateTypes.Removing)
                {
                    ItemsGenerator.SetState(i, ItemStateTypes.Removing);
                    removeCount--;

                    if (removeCount == 0)
                    {
                        break;
                    }
                }
            }

            if (isItemsUpdated && ItemsLayout != null)
            {
                await UpdateItems(false, IsAnimationEnabled);
            }
        }

        /// <summary>
        /// Move item without animation
        /// </summary>
        private async Task MoveItem(NotifyCollectionChangedEventArgs e)
        {
            int i = e.OldStartingIndex;
            int containerCount = 0;

            foreach (object item in e.OldItems)
            {
                View container = null;

                if (ItemsGenerator.HasItemViewGenerated(i))
                {
                    container = ItemsGenerator.GetItemViewFromIndex(i);

                    if (ItemsLayout != null)
                    {
                        if (ItemsLayout.Children.Contains(container))
                        {
                            if (ItemsLayout is IItemsViewLayout itemsViewLayout)
                            {
                                itemsViewLayout.RemoveChild(container);
                            }
                            else
                            {
                                ItemsLayout.Children.Remove(container);
                            }
                        }
                    }
                }

                // Update ItemContainerGenerator
                ItemsGenerator.Move(i, e.NewStartingIndex + containerCount);

                i++;
                containerCount++;
            }

            if (ItemsLayout != null)
            {
                await UpdateItems(false, false);
            }
        }

        /// <summary>
        /// Replace item without animation
        /// </summary>
        private async Task ReplaceItem(NotifyCollectionChangedEventArgs e)
        {
            m_internalIsAnimationEnabled = false;
            await RemoveItem(e, false);
            await AddItem(e);
            m_internalIsAnimationEnabled = true;
        }

        /// <summary>
        /// Reset all items. If menu is open then items is removed after menu is closed.
        /// </summary>
        private async Task Reset(bool doUpdate = true)
        {
            for (int i = 0; i < ItemsGenerator.TotalItemsCount; i++)
            {
                ItemsGenerator.SetState(i, ItemStateTypes.Removing);
            }

            if (doUpdate)
            {
                m_isResetRunning = true;
                await UpdateItems(true, IsAnimationEnabled);
                m_isResetRunning = false;
            }
        }

        /// <summary>
        /// Get items count which is going to be removed befor giving index
        /// </summary>
        private int GetRemovingItemsCountBefore(int index)
        {
            int count = 0;

            for (int i = 0; i < index; i++)
            {
                ItemStateTypes itemState = ItemsGenerator.GetState(i);
                if (itemState == ItemStateTypes.Removing)
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// Update ItemsLayout items with animation
        /// </summary>
        /// <param name="updateAll">Update all items</param>
        /// <param name="isAnimationEnabled">Is animation enabled</param>
        private async Task UpdateItems(bool updateAll = false, bool isAnimationEnabled = true)
        {
            bool actualIsAnimationEnabled = m_internalIsAnimationEnabled && isAnimationEnabled;

            List<View> itemContainersToRemove = new List<View>();
            List<View> itemContainersToAdd = new List<View>();

            // Get items to add and remove for animations
            for (int i = 0; i < ItemsGenerator.TotalItemsCount; i++)
            {
                ItemStateTypes state = ItemsGenerator.GetState(i);
                bool hasContainerGenerated = ItemsGenerator.HasItemViewGenerated(i);

                if (state == ItemStateTypes.Adding && hasContainerGenerated)
                {
                    View itemContainer = ItemsGenerator.GetItemViewFromIndex(i);
                    if (m_itemContainersInAddAnimation.Contains(itemContainer) == false)
                    {
                        itemContainersToAdd.Add(itemContainer);
                    }
                }
                else if (state == ItemStateTypes.Removing)
                {
                    if (hasContainerGenerated)
                    {
                        View itemContainer = ItemsGenerator.GetItemViewFromIndex(i);
                        if (m_itemContainersInRemoveAnimation.Contains(itemContainer) == false)
                        {
                            itemContainersToRemove.Add(itemContainer);
                        }
                    }
                    else
                    {
                        ItemsGenerator.Remove(i);
                        i--;
                    }
                }
            }

            Animation removeAnimation = new Animation();
            int removeDuration = 0;
            IAnimation removeAnimationCreator = null;

            if (actualIsAnimationEnabled)
            {
                if (updateAll)
                {
                    removeAnimationCreator = ItemRemoveAllAnimation;
                    removeDuration = ItemRemoveAllDuration;
                }
                else if (ItemRemoveAnimation != null)
                {
                    removeAnimationCreator = ItemRemoveAnimation;
                    removeDuration = ItemRemoveDuration;
                }
            }

            if (removeAnimationCreator != null)
            {
                // Remove default items with correct animation
                foreach (View itemToRemove in itemContainersToRemove)
                {
                    m_itemContainersInRemoveAnimation.Add(itemToRemove);
                    removeAnimation.Add(0, 1, removeAnimationCreator.Create(itemToRemove));
                }
            }

            // Do remove animations
            if (removeAnimation.HasSubAnimations())
            {
                TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
                removeAnimation.Commit(this, c_removeAnimationName, 64, (uint)removeDuration, finished: (d, b) => tcs.SetResult(true));
                await tcs.Task;
            }

            // Do actual remove from layout and container generator after animation

            View lastContainerToRemove = itemContainersToRemove.Where(x => ItemsLayout.Children.Contains(x)).LastOrDefault();

            foreach (View itemContainerToRemove in itemContainersToRemove)
            {
                if (ItemsLayout != null && ItemsLayout.Children.Contains(itemContainerToRemove))
                {
                    if (ItemsLayout is ILayout layout && itemContainerToRemove != lastContainerToRemove)
                    {
                        layout.RemoveChild(itemContainerToRemove, true);
                    }
                    else
                    {
                        ItemsLayout.Children.Remove(itemContainerToRemove);
                    }
                }

                m_itemContainersInRemoveAnimation.Remove(itemContainerToRemove);

                int index = ItemsGenerator.GetIndex(itemContainerToRemove);

                if (index >= 0)
                {
                    ItemsGenerator.SetState(index, ItemStateTypes.None);
                    ItemsGenerator.Remove(index);
                }
            }

            //
            // Add animation
            //

            Animation addAnimation = new Animation();
            int addDuration = 0;
            IAnimation addAnimationCreator = null;

            if (actualIsAnimationEnabled)
            {
                if (updateAll)
                {
                    addAnimationCreator = ItemAddAllAnimation;
                    addDuration = ItemAddAllDuration;
                }
                else if (ItemAddAnimation != null)
                {
                    addAnimationCreator = ItemAddAnimation;
                    addDuration = ItemAddDuration;
                }
            }

            View lastContainerToAdd = itemContainersToAdd.Where(x => ItemsLayout.Children.Contains(x) == false).LastOrDefault();

            // Add default items
            foreach (View itemContainerToAdd in itemContainersToAdd)
            {
                if (addAnimationCreator != null)
                {
                    m_itemContainersInAddAnimation.Add(itemContainerToAdd);
                    addAnimation.Add(0, 1, addAnimationCreator.Create(itemContainerToAdd));
                }

                if (IsVirtualzed == false)
                {
                    int index = ItemsGenerator.GetIndex(itemContainerToAdd);
                    int removingItemsBeforeCount = GetRemovingItemsCountBefore(index);
                    int actualIndex = index + removingItemsBeforeCount;
                    ItemsGenerator.SetState(actualIndex, ItemStateTypes.None);

                    if (IsVirtualzed == false)
                    {
                        if (ItemsLayout.Children.Contains(itemContainerToAdd) == false)
                        {
                            if (ItemsLayout.Children.Count < actualIndex)
                            {
                                if (ItemsLayout is ILayout layout && itemContainerToAdd != lastContainerToAdd)
                                {
                                    layout.AddChild(itemContainerToAdd, true);
                                }
                                else
                                {
                                    ItemsLayout.Children.Add(itemContainerToAdd);
                                }
                            }
                            else
                            {
                                if (ItemsLayout is ILayout layout && itemContainerToAdd != lastContainerToAdd)
                                {
                                    layout.InsertChild(actualIndex, itemContainerToAdd, true);
                                }
                                else
                                {
                                    ItemsLayout.Children.Insert(actualIndex, itemContainerToAdd);
                                }
                            }
                        }
                    }
                }
            }

            // Do add animation
            if (addAnimation.HasSubAnimations())
            {
                addAnimation.Commit(this, c_addAnimationName, 64, (uint)addDuration, finished: (d, p) =>
                {
                    foreach (View itemContainerToAdd in itemContainersToAdd)
                    {
                        m_itemContainersInAddAnimation.Remove(itemContainerToAdd);
                    }
                });
            }
        }

        /// <summary>
        /// Release item container from clicked and checked events.
        /// </summary>
        private void ReleaseItemContainer(View itemContainer)
        {
            if (itemContainer is IToggable toggableItem)
            {
                toggableItem.IsToggledChanged -= OnItemContainerIsCheckedChanged;
            }

            if (itemContainer is ITappable tappableItem)
            {
                tappableItem.Tapped -= OnItemContainerTapped;
            }
        }

        #endregion

        #region Host ScrollViewer

        /// <summary>
        /// Get host scrollviewer 
        /// </summary>
        private ScrollView GetHostScrollViewer()
        {
            Element v = this;
            while (v != null)
            {
                if (v is ScrollView scrollView)
                {
                    return scrollView;
                }

                v = v.Parent;
            }

            return null;
        }

        /// <summary>
        /// Event when host scrollviewer scroll changes. Update virtualizing panel viewport size and offsets.
        /// </summary>
        private void OnScrollChanged(object sender, ScrolledEventArgs e)
        {
            if (IsVirtualzed)
            {
                UpdateVirtualizedPanelProperties(VirtualizingPanel, Width, Height);
            }
        }

        #endregion

        #region Selection

        /// <summary>
        /// Called when SelectedItem is changed
        /// </summary>
        protected virtual void OnSelectedItemChanged(object oldItem, object newItem)
        {
            return;
        }

        /// <summary>
        /// Called when SelectedItems changes
        /// </summary>
        protected virtual void OnSelectedItemsChanged(IList oldItems, IList newItems)
        {
            return;
        }

        /// <summary>
        /// Called when SelectionMode changes
        /// </summary>
        protected virtual void OnSelectionModeChanged(SelectionModes oldValue, SelectionModes newValue)
        {
            return;
        }

        /// <summary>
        /// Event handler for item container IsChecked changes
        /// </summary>
        protected void OnItemContainerIsCheckedChanged(object sender, bool isChecked)
        {
            if (SelectionMode == SelectionModes.Single)
            {
                if (isChecked)
                {
                    View itemContainer = sender as View;
                    if (ItemsSource[0] is View)
                    {
                        SelectedItem = itemContainer;
                    }
                    else
                    {
                        SelectedItem = itemContainer.BindingContext;
                    }
                }
                else
                {
                    SelectedItem = null;
                }
            }
            else if (SelectionMode == SelectionModes.Multiple)
            {
                IList oldItems = SelectedItems;

                View itemContainer = sender as View;
                if (ItemsSource[0] is View)
                {
                    if (isChecked)
                    {
                        SelectedItems.Add(itemContainer);
                    }
                    else
                    {
                        SelectedItems.Remove(itemContainer);
                    }
                }
                else
                {
                    if (isChecked)
                    {
                        SelectedItems.Add(itemContainer.BindingContext);
                    }
                    else
                    {
                        SelectedItems.Remove(itemContainer.BindingContext);
                    }
                }

                IList newItems = SelectedItems;

                if (SelectedItems is INotifyCollectionChanged == false)
                {
                    OnSelectedItemsChanged(oldItems, newItems);
                }
            }
            else
            {
                // Do nothing...
            }
        }

        /// <summary>
        /// Handle selected item changes. Update item container IsChecked state.
        /// </summary>
        private void OnSelectedItemChangedInternal(object oldItem, object newItem)
        {
            SelectedItem = newItem;

            if (oldItem != null)
            {
                View oldItemContainer = null;
                if (oldItem is View)
                {
                    oldItemContainer = oldItem as View;
                }
                else
                {
                    int oldItemIndex = ItemsSource.IndexOf(oldItem);
                    if (ItemsGenerator.HasItemViewGenerated(oldItemIndex))
                    {
                        oldItemContainer = ItemsGenerator.GetItemViewFromIndex(oldItemIndex);
                    }
                }

                if (oldItemContainer is IToggable item)
                {
                    SetIsChecked(item, false, false);
                }
            }

            if (newItem != null)
            {
                View newItemContainer = null;
                if (newItem is View)
                {
                    newItemContainer = newItem as View;
                }
                else
                {
                    int newItemIndex = ItemsSource.IndexOf(newItem);
                    if (ItemsGenerator.HasItemViewGenerated(newItemIndex))
                    {
                        newItemContainer = ItemsGenerator.GetItemViewFromIndex(newItemIndex);
                    }
                }

                if (newItemContainer is IToggable item)
                {
                    SetIsChecked(item, true, false);
                }
            }

            if (SelectedItemChanged != null)
            {
                SelectedItemChanged(this, oldItem, newItem);
            }
        }

        /// <summary>
        /// Handle whole selected items list changes. Update item containers IsChecked state.
        /// </summary>
        private void OnSelectedItemsChangedInternal(IList oldItems, IList newItems)
        {
            if (oldItems != null)
            {
                foreach (object item in oldItems)
                {
                    View oldItemContainer = null;
                    if (item is View)
                    {
                        oldItemContainer = item as View;
                    }
                    else
                    {
                        if (ItemsSource != null && ItemsSource.Contains(item))
                        {
                            int oldItemIndex = ItemsSource.IndexOf(item);
                            if (ItemsGenerator.HasItemViewGenerated(oldItemIndex))
                            {
                                oldItemContainer = ItemsGenerator.GetItemViewFromIndex(oldItemIndex);
                            }
                        }
                    }

                    if (oldItemContainer is IToggable oldCheckableItem)
                    {
                        SetIsChecked(oldCheckableItem, false, false);
                    }
                }
            }

            if (newItems != null)
            {
                foreach (object item in newItems)
                {
                    View newItemContainer = null;
                    if (item is View)
                    {
                        newItemContainer = item as View;
                    }
                    else
                    {
                        int newItemIndex = ItemsSource.IndexOf(item);
                        if (ItemsGenerator.HasItemViewGenerated(newItemIndex))
                        {
                            newItemContainer = ItemsGenerator.GetItemViewFromIndex(newItemIndex);
                        }
                    }

                    if (newItemContainer is IToggable newCheckedItem)
                    {
                        SetIsChecked(newCheckedItem, true, false);
                    }
                }
            }

            if (oldItems == null && newItems == null && ItemsSource != null)
            {
                int i = 0;
                foreach (object item in ItemsSource)
                {
                    if (ItemsGenerator.HasItemViewGenerated(i))
                    {
                        View itemContainer = ItemsGenerator.GetItemViewFromIndex(i);
                        if (itemContainer is IToggable checkableItemContainer)
                        {
                            SetIsChecked(checkableItemContainer, false, false);
                        }
                    }
                    i++;
                }
            }

            OnSelectedItemsChanged(oldItems, newItems);

            if (SelectedItemsChanged != null)
            {
                SelectedItemsChanged(this, oldItems, newItems);
            }
        }

        /// <summary>
        /// Handle if SelectedItems list has added or removed item. Update all item containers IsChecked state.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSelectedItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnSelectedItemsChangedInternal(e.OldItems, e.NewItems);
        }

        /// <summary>
        /// Set item container IsChecked state with or without event raises
        /// </summary>
        private void SetIsChecked(IToggable item, bool isChecked, bool raiseEvents = false)
        {
            if (item.IsToggled != isChecked)
            {
                if (raiseEvents == false)
                {
                    item.IsToggledChanged -= OnItemContainerIsCheckedChanged;
                }

                item.IsToggled = isChecked;

                if (raiseEvents == false)
                {
                    item.IsToggledChanged += OnItemContainerIsCheckedChanged;
                }
            }
        }

        /// <summary>
        /// Handle SelectionMode changes. Unselect all items.
        /// </summary>
        private void OnSelectionModeChangedInternal(SelectionModes oldValue, SelectionModes newValue)
        {
            SelectedItem = null;
            SelectedItems?.Clear();
            OnSelectionModeChanged(oldValue, newValue);
        }

        #endregion

        #region Focus

        private void OnFocusedItemIndexChanged(int oldValue, int newValue)
        {
            m_ignoreFocusedItemChanges = true;

            // Remove old focus

            View oldContainer = null;
            if (oldValue > -1 && oldValue < ItemsSource.Count && ItemsSource[oldValue] is View oldElement)
            {
                oldContainer = oldElement;
            }
            else if (oldValue > -1 && ItemsGenerator.HasItemViewGenerated(oldValue))
            {
                oldContainer = ItemsGenerator.GetItemViewFromIndex(oldValue);
            }

            if (oldContainer != null)
            {
                SetIsItemFocused(oldContainer, false);
            }

            // Add new focus

            View newContainer = null;
            if (newValue > -1 && newValue < ItemsSource.Count && ItemsSource[newValue] is View newElement)
            {
                newContainer = newElement;
            }
            else if (newValue > -1 && ItemsGenerator.HasItemViewGenerated(newValue))
            {
                newContainer = ItemsGenerator.GetItemViewFromIndex(newValue);
            }

            if (newContainer != null)
            {
                SetIsItemFocused(newContainer, true);
            }

            // Select Focuse
            FocusedItem = newValue > -1 && ItemsSource.Count > newValue ? ItemsSource[newValue] : null;

            m_ignoreFocusedItemChanges = false;
        }

        private void OnFocusedItemChanged(object oldValue, object newValue)
        {
            if (m_ignoreFocusedItemChanges)
            {
                return;
            }

            FocusedItemIndex = ItemsSource.IndexOf(newValue);
        }

        #endregion

        #region EmptyContent

        private void OnEmptyContentTemplateChanged(DataTemplate oldDataTemplate, DataTemplate newDataTemplate)
        {
            if (newDataTemplate != null)
            {
                EmptyContent = newDataTemplate.CreateContent() as View;
            }
            else
            {
                EmptyContent = null;
            }
        }


        private void OnEmptyContentChanged(View oldEmptyContent, View newEmptyContent)
        {
            if (oldEmptyContent != null)
            {
                Children.Remove(oldEmptyContent);
            }

            if (newEmptyContent != null)
            {
                Children.Add(newEmptyContent);

                newEmptyContent.IsVisible = HasItems == false && IsEmptyContentEnabled;
            }
        }

        protected void IsEmptyContentVisible(bool isVisible)
        {
            if (isVisible && IsEmptyContentEnabled)
            {
                if (ItemsLayout != null)
                {
                    ItemsLayout.IsVisible = false;
                }

                if (EmptyContent != null)
                {
                    EmptyContent.IsVisible = true;
                }
            }
            else
            {
                if (ItemsLayout != null)
                {
                    ItemsLayout.IsVisible = true;
                }

                if (EmptyContent != null)
                {
                    EmptyContent.IsVisible = false;
                }
            }
        }

        #endregion        
    }
}