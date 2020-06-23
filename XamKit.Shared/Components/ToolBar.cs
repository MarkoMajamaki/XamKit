using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

// Xamarin
using Xamarin.Forms;

namespace XamKit
{
    [ContentProperty("ItemsSource")]
    public class ToolBar : Layout<View>
    {
        private enum ItemActionTypes { Add, Remove, MoveToDefault, MoveToMenu, Removing, Adding }

        private static List<ToolBar> _openToolBars = new List<ToolBar>();

        private List<ItemInfo> _items = null;
        private List<ItemInfo> _menuItems = null;

        // Parts
        private BoxView _line = null;
        private ScrollView _menuScrollView = null;
        private View _menuButton = null;
        private Popup _popup = null;
        private BoxView _itemsBackground = null;
        private GradientView _shadowView = null;

        private const int _backgroundChildrenCount = 2; // shadow, line

        // Animation names
        private const string _showAnimationName = "showAnimationName";
        private const string _hideAnimationName = "hideAnimationName";
        private const string _openMenuAnimationName = "openMenuAnimationName";
        private const string _closeMenuAnimationName = "closeMenuAnimationName";
        private const string _addAnimationName = "addAnimationName";
        private const string _removeAnimationName = "removeAnimationName";

        // Animation process
        private double _menuAnimationProcess = 0;
        private double _visibilityAnimationProcess = 1;
        private double _bottomSafeAreaHeight = 0;

        private bool _ignoreIsMenuOpenChanges = false;
        private bool _ignoreInvalidation = false;
        private bool _isInitializationRunning = false;
        private bool _ignoreItemsSourceChanges = false;

        private SizeRequest _menuSize = new SizeRequest();
        private SizeRequest _menuButtonSize = new SizeRequest();

        /// <summary>
        /// Event when menu is opened
        /// </summary>
        public event EventHandler<bool> IsMenuOpenChanged;

        /// <summary>
        /// Event when any item is tapped. Works only if item implement ITappable interface.
        /// </summary>
        public event EventHandler ItemTapped;

        /// <summary>
        /// Is debug text enabled
        /// </summary>
        public bool IsDebugEnabled { get; set; } = false;

        /// <summary>
        /// True if any toolbar is open
        /// </summary>
        public static bool IsAnyToolBarMenuOpen
        {
            get
            {
                return _openToolBars.Count > 0;
            }
        }

        #region Properties

        /// <summary>
        /// Items data source
        /// </summary>
        public static readonly BindableProperty ItemsSourceProperty =
            BindableProperty.Create("ItemsSource", typeof(IList), typeof(ToolBar), null, propertyChanged: OnItemsSourceChanged);

        private static void OnItemsSourceChanged(BindableObject bindable, object oldValue, object newValue)
        {
            Task t = (bindable as ToolBar).OnItemsSourceChanged(oldValue as IList, newValue as IList);
        }

        public IList ItemsSource
        {
            get { return (IList)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        /// <summary>
        /// Items source which are always added to menu
        /// </summary>
        public static readonly BindableProperty MenuItemsSourceProperty =
            BindableProperty.Create("MenuItemsSource", typeof(IList), typeof(ToolBar), propertyChanged: OnMenuItemsSourceChanged);

        private static void OnMenuItemsSourceChanged(BindableObject bindable, object oldValue, object newValue)
        {
            Task t = (bindable as ToolBar).OnMenuItemsSourceChanged(oldValue as IList, newValue as IList);
        }

        public IList MenuItemsSource
        {
            get { return (IList)GetValue(MenuItemsSourceProperty); }
            set { SetValue(MenuItemsSourceProperty, value); }
        }

        /// <summary>
        /// Item datatemplate
        /// </summary>
        public static readonly BindableProperty ItemTemplateProperty =
            BindableProperty.Create("ItemTemplate", typeof(DataTemplate), typeof(ToolBar), null);

        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }

        /// <summary>
        /// Items datatemplate selector
        /// </summary>
        public static readonly BindableProperty ItemTemplateSelectorProperty =
            BindableProperty.Create("ItemTemplateSelector", typeof(DataTemplateSelector), typeof(ToolBar), null);

        public DataTemplateSelector ItemTemplateSelector
        {
            get { return (DataTemplateSelector)GetValue(ItemTemplateSelectorProperty); }
            set { SetValue(ItemTemplateSelectorProperty, value); }
        }

        /// <summary>
        /// Menu Items container datatemplate selector. 
        /// </summary>
        public static readonly BindableProperty MenuItemContainerGeneratorProperty =
            BindableProperty.Create("MenuItemContainerGenerator", typeof(IMenuItemContainerGenerator), typeof(ToolBar), null);

        public IMenuItemContainerGenerator MenuItemContainerGenerator
        {
            get { return (IMenuItemContainerGenerator)GetValue(MenuItemContainerGeneratorProperty); }
            set { SetValue(MenuItemContainerGeneratorProperty, value); }
        }

        /// <summary>
        /// Template to create menu items layout
        /// </summary>
        public static readonly BindableProperty MenuItemsLayoutTemplateProperty =
            BindableProperty.Create("MenuItemsLayoutTemplate", typeof(DataTemplate), typeof(ToolBar), propertyChanged: OnMenuItemsLayoutTemplateChanged);

        private static void OnMenuItemsLayoutTemplateChanged(BindableObject bindable, object oldValue, object newValue)
        {
            (bindable as ToolBar).OnMenuItemsLayoutTemplateChanged(oldValue as DataTemplate, newValue as DataTemplate);
        }

        public DataTemplate MenuItemsLayoutTemplate
        {
            get { return (DataTemplate)GetValue(MenuItemsLayoutTemplateProperty); }
            set { SetValue(MenuItemsLayoutTemplateProperty, value); }
        }

        /// <summary>
        /// Actual menu items layout
        /// </summary>
        public static readonly BindableProperty MenuItemsLayoutProperty =
            BindableProperty.Create("MenuItemsLayout", typeof(Layout<View>), typeof(ToolBar), propertyChanged: OnMenuItemsLayoutChanged);

        private static void OnMenuItemsLayoutChanged(BindableObject bindable, object oldValue, object newValue)
        {
            (bindable as ToolBar).OnMenuItemsLayoutChanged(oldValue as Layout<View>, newValue as Layout<View>);
        }

        public Layout<View> MenuItemsLayout
        {
            get { return (Layout<View>)GetValue(MenuItemsLayoutProperty); }
            set { SetValue(MenuItemsLayoutProperty, value); }
        }

        /// <summary>
        /// Max count of visible items. Fill available space with buttons if value is null
        /// </summary>
        public static readonly BindableProperty MaxItemsProperty =
            BindableProperty.Create("MaxItems", typeof(int), typeof(ToolBar), int.MaxValue);

        public int MaxItems
        {
            get { return (int)GetValue(MaxItemsProperty); }
            set { SetValue(MaxItemsProperty, value); }
        }

        /// <summary>
        /// How visible items are located in available space
        /// </summary>
        public static readonly BindableProperty ItemsAlignmentProperty =
            BindableProperty.Create("ItemsAlignment", typeof(ItemsAlignments), typeof(ToolBar), ItemsAlignments.Left);

        public ItemsAlignments ItemsAlignment
        {
            get { return (ItemsAlignments)GetValue(ItemsAlignmentProperty); }
            set { SetValue(ItemsAlignmentProperty, value); }
        }

        /// <summary>
        /// Visibility modes
        /// </summary>
        public static readonly BindableProperty VisibilityModeProperty =
            BindableProperty.Create("VisibilityMode", typeof(ToolBarVisibilityModes), typeof(ToolBar), ToolBarVisibilityModes.Auto, propertyChanged: OnToolBarVisibilityModeChanged);

        private static void OnToolBarVisibilityModeChanged(BindableObject bindable, object oldValue, object newValue)
        {
            (bindable as ToolBar).OnVisibilityModeChanged((ToolBarVisibilityModes)oldValue, (ToolBarVisibilityModes)newValue);
        }

        public ToolBarVisibilityModes VisibilityMode
        {
            get { return (ToolBarVisibilityModes)GetValue(VisibilityModeProperty); }
            set { SetValue(VisibilityModeProperty, value); }
        }

        /// <summary>
        /// How menu is located (popup or bottom menu). If menu is bottom, then add page content inside 'Content'
        /// </summary>
        public static readonly BindableProperty MenuModeProperty =
            BindableProperty.Create("MenuMode", typeof(ToolBarMenuModes), typeof(ToolBar), ToolBarMenuModes.Popup);

        public ToolBarMenuModes MenuMode
        {
            get { return (ToolBarMenuModes)GetValue(MenuModeProperty); }
            set { SetValue(MenuModeProperty, value); }
        }

        /// <summary>
        /// Is animations enabled
        /// </summary>
        public static readonly BindableProperty IsAnimationEnabledProperty =
            BindableProperty.Create("IsAnimationEnabled", typeof(bool), typeof(ToolBar), true);

        public bool IsAnimationEnabled
        {
            get { return (bool)GetValue(IsAnimationEnabledProperty); }
            set { SetValue(IsAnimationEnabledProperty, value); }
        }

        /// <summary>
        /// Space between items
        /// </summary>
        public static readonly BindableProperty SpacingProperty =
            BindableProperty.Create("Spacing", typeof(double), typeof(ToolBar), 0.0);

        public double Spacing
        {
            get { return (double)GetValue(SpacingProperty); }
            set { SetValue(SpacingProperty, value); }
        }

        /// <summary>
        /// All items padding
        /// </summary>
        public static readonly BindableProperty ItemsPaddingProperty =
            BindableProperty.Create("ItemsPadding", typeof(Thickness), typeof(ToolBar), new Thickness(0));

        public Thickness ItemsPadding
        {
            get { return (Thickness)GetValue(ItemsPaddingProperty); }
            set { SetValue(ItemsPaddingProperty, value); }
        }

        /// <summary>
        /// Command to execute when any item (or sub menu item) tapped
        /// </summary>
        public static readonly BindableProperty ItemCommandProperty =
            BindableProperty.Create("ItemCommand", typeof(ICommand), typeof(ToolBar), null);

        public ICommand ItemCommand
        {
            get { return (ICommand)GetValue(ItemCommandProperty); }
            set { SetValue(ItemCommandProperty, value); }
        }

        /// <summary>
        /// Default item resources
        /// </summary>
        public static readonly BindableProperty DefaultResourcesProperty =
            BindableProperty.Create("DefaultResources", typeof(ResourceDictionary), typeof(ToolBar), null, propertyChanged: OnDefaultResourcesChanged);

        private static void OnDefaultResourcesChanged(BindableObject bindable, object oldValue, object newValue)
        {
            (bindable as ToolBar).Resources = newValue as ResourceDictionary;
        }

        public ResourceDictionary DefaultResources
        {
            get { return (ResourceDictionary)GetValue(DefaultResourcesProperty); }
            set { SetValue(DefaultResourcesProperty, value); }
        }

        /// <summary>
        /// Drop down shadow height
        /// </summary>
        public static readonly BindableProperty ShadowHeightProperty =
            BindableProperty.Create("ShadowHeight", typeof(double), typeof(ToolBar), 0.0);

        public double ShadowHeight
        {
            get { return (double)GetValue(ShadowHeightProperty); }
            set { SetValue(ShadowHeightProperty, value); }
        }

        #endregion

        #region Menu

        /// <summary>
        /// Template to create menu button. Root item must implement IToggable.
        /// </summary>
        public static readonly BindableProperty MenuButtonTemplateProperty =
            BindableProperty.Create("MenuButtonTemplate", typeof(DataTemplate), typeof(ToolBar), null, propertyChanged: OnMenuButtonTemplateChanged);

        private static void OnMenuButtonTemplateChanged(BindableObject bindable, object oldValue, object newValue)
        {
            (bindable as ToolBar).OnMenuButtonTemplateChanged(oldValue as DataTemplate, newValue as DataTemplate);
        }

        public DataTemplate MenuButtonTemplate
        {
            get { return (DataTemplate)GetValue(MenuButtonTemplateProperty); }
            set { SetValue(MenuButtonTemplateProperty, value); }
        }

        /// <summary>
        /// Is bottom menu open
        /// </summary>
        public static readonly BindableProperty IsMenuOpenProperty =
            BindableProperty.Create("IsMenuOpen", typeof(bool), typeof(ToolBar), false, propertyChanged: OnIsMenuOpenChanged);

        private static async void OnIsMenuOpenChanged(BindableObject bindable, object oldValue, object newValue)
        {
            await (bindable as ToolBar).SetIsMenuOpenAsync((bool)newValue, true);
        }

        public bool IsMenuOpen
        {
            get { return (bool)GetValue(IsMenuOpenProperty); }
            set { SetValue(IsMenuOpenProperty, value); }
        }

        /// <summary>
        /// Popup spacing
        /// </summary>
        public static readonly BindableProperty PopupSpacingProperty =
            BindableProperty.Create("PopupSpacing", typeof(double), typeof(ToolBar), 0.0);

        public double PopupSpacing
        {
            get { return (double)GetValue(PopupSpacingProperty); }
            set { SetValue(PopupSpacingProperty, value); }
        }
        
        /// <summary>
        /// Popup placement
        /// </summary>
        public static readonly BindableProperty PopupPlacementProperty =
            BindableProperty.Create("PopupPlacement", typeof(PopupPlacements), typeof(ToolBar), PopupPlacements.TopRight);

        public PopupPlacements PopupPlacement
        {
            get { return (PopupPlacements)GetValue(PopupPlacementProperty); }
            set { SetValue(PopupPlacementProperty, value); }
        }

        /// <summary>
        /// Menu button horizontal location in available space
        /// </summary>
        public static readonly BindableProperty MenuButtonAlignmentProperty =
            BindableProperty.Create("MenuButtonAlignment", typeof(HorizontalLocations), typeof(ToolBar), HorizontalLocations.Left);

        public HorizontalLocations MenuButtonAlignment
        {
            get { return (HorizontalLocations)GetValue(MenuButtonAlignmentProperty); }
            set { SetValue(MenuButtonAlignmentProperty, value); }
        }

        /// <summary>
        /// Bottom menu max height
        /// </summary>
        public static readonly BindableProperty BottomMenuMaxHeightProperty =
            BindableProperty.Create("BottomMenuMaxHeight", typeof(double), typeof(ToolBar), 0.0);

        public double BottomMenuMaxHeight
        {
            get { return (double)GetValue(BottomMenuMaxHeightProperty); }
            set { SetValue(BottomMenuMaxHeightProperty, value); }
        }

        #endregion

        #region ItemsCount

        /// <summary>
        /// Currently visible default items count
        /// </summary>
        public static readonly BindableProperty VisibleItemsCountProperty =
            BindableProperty.Create("VisibleItemsCount", typeof(int), typeof(ToolBar), 0);

        public int VisibleItemsCount
        {
            get { return (int)GetValue(VisibleItemsCountProperty); }
            protected set { SetValue(VisibleItemsCountProperty, value); }
        }

        /// <summary>
        /// Count of menu items
        /// </summary>
        public static readonly BindableProperty MenuItemsCountProperty =
            BindableProperty.Create("MenuItemsCount", typeof(int), typeof(ToolBar), 0);

        public int MenuItemsCount
        {
            get { return (int)GetValue(MenuItemsCountProperty); }
            protected set { SetValue(MenuItemsCountProperty, value); }
        }

        /// <summary>
        /// Has any visible or menu items items
        /// </summary>
        public static readonly BindableProperty HasItemsProperty =
            BindableProperty.Create("HasItems", typeof(bool), typeof(ToolBar), false);

        public bool HasItems
        {
            get { return (bool)GetValue(HasItemsProperty); }
            protected set { SetValue(HasItemsProperty, value); }
        }

        #endregion

        #region Colors

        /// <summary>
        /// Background horizontal line color
        /// </summary>
        public static readonly BindableProperty LineHeightRequestProperty =
            BindableProperty.Create("LineHeightRequest", typeof(double), typeof(ToolBar), 1.0);
        
        public double LineHeightRequest
        {
            get { return (double)GetValue(LineHeightRequestProperty); }
            set { SetValue(LineHeightRequestProperty, value); }
        }

        /// <summary>
        /// Background horizontal line color
        /// </summary>
        public static readonly BindableProperty LineColorProperty =
            BindableProperty.Create("LineColor", typeof(Color), typeof(ToolBar), Color.Transparent);
        
        public Color LineColor
        {
            get { return (Color)GetValue(LineColorProperty); }
            set { SetValue(LineColorProperty, value); }
        }

        /// <summary>
        /// Background horizontal line color
        /// </summary>
        public static readonly BindableProperty ItemsBackgroundColorProperty =
            BindableProperty.Create("ItemsBackgroundColor", typeof(Color), typeof(ToolBar), Color.Transparent);
        
        public Color ItemsBackgroundColor
        {
            get { return (Color)GetValue(ItemsBackgroundColorProperty); }
            set { SetValue(ItemsBackgroundColorProperty, value); }
        }

        /// <summary>
        /// Background horizontal line color
        /// </summary>
        public static readonly BindableProperty MenuItemsBackgroundColorProperty =
            BindableProperty.Create("MenuItemsBackgroundColor", typeof(Color), typeof(ToolBar), Color.Transparent);
        
        public Color MenuItemsBackgroundColor
        {
            get { return (Color)GetValue(MenuItemsBackgroundColorProperty); }
            set { SetValue(MenuItemsBackgroundColorProperty, value); }
        }

        #endregion

        #region Show and hide animation

        /// <summary>
        /// Show control animation duration
        /// </summary>
        public static readonly BindableProperty ShowDurationProperty =
            BindableProperty.Create("ShowDuration", typeof(int), typeof(ToolBar), 300);

        public int ShowDuration
        {
            get { return (int)GetValue(ShowDurationProperty); }
            set { SetValue(ShowDurationProperty, value); }
        }

        /// <summary>
        /// Show items control animation easing function
        /// </summary>
        public static readonly BindableProperty ShowEasingFunctionProperty =
            BindableProperty.Create("ShowEasingFunction", typeof(Easing), typeof(ToolBar), Easing.Linear);

        public Easing ShowEasingFunction
        {
            get { return (Easing)GetValue(ShowEasingFunctionProperty); }
            set { SetValue(ShowEasingFunctionProperty, value); }
        }

        /// <summary>
        /// Hide control animation duration
        /// </summary>
        public static readonly BindableProperty HideDurationProperty =
            BindableProperty.Create("HideDuration", typeof(int), typeof(ToolBar), 200);

        public int HideDuration
        {
            get { return (int)GetValue(HideDurationProperty); }
            set { SetValue(HideDurationProperty, value); }
        }

        /// <summary>
        /// Hide items control animation easing function
        /// </summary>
        public static readonly BindableProperty HideEasingFunctionProperty =
            BindableProperty.Create("HideEasingFunction", typeof(Easing), typeof(ToolBar), Easing.Linear);

        public Easing HideEasingFunction
        {
            get { return (Easing)GetValue(HideEasingFunctionProperty); }
            set { SetValue(HideEasingFunctionProperty, value); }
        }

        #endregion

        #region Menu open and close animation

        /// <summary>
        /// Show menu animation duration
        /// </summary>
        public static readonly BindableProperty ShowMenuDurationProperty =
            BindableProperty.Create("ShowMenuDuration", typeof(int), typeof(ToolBar), 500);

        public int ShowMenuDuration
        {
            get { return (int)GetValue(ShowMenuDurationProperty); }
            set { SetValue(ShowMenuDurationProperty, value); }
        }

        /// <summary>
        /// Show menu animation easing function
        /// </summary>
        public static readonly BindableProperty ShowMenuEasingFunctionProperty =
            BindableProperty.Create("ShowMenuEasingFunction", typeof(Easing), typeof(ToolBar), Easing.Linear);

        public Easing ShowMenuEasingFunction
        {
            get { return (Easing)GetValue(ShowMenuEasingFunctionProperty); }
            set { SetValue(ShowMenuEasingFunctionProperty, value); }
        }

        /// <summary>
        /// Hide menu animation duration
        /// </summary>
        public static readonly BindableProperty HideMenuDurationProperty =
            BindableProperty.Create("HideMenuDuration", typeof(int), typeof(ToolBar), 500);

        public int HideMenuDuration
        {
            get { return (int)GetValue(HideMenuDurationProperty); }
            set { SetValue(HideMenuDurationProperty, value); }
        }

        /// <summary>
        /// Hide menu animation easing function
        /// </summary>
        public static readonly BindableProperty HideMenuEasingFunctionProperty =
            BindableProperty.Create("HideMenuEasingFunction", typeof(Easing), typeof(ToolBar), Easing.Linear);

        public Easing HideMenuEasingFunction
        {
            get { return (Easing)GetValue(HideMenuEasingFunctionProperty); }
            set { SetValue(HideMenuEasingFunctionProperty, value); }
        }

        #endregion

        #region Default item animation

        /// <summary>
        /// Animation when item collection changes and item is added
        /// </summary>
        public static readonly BindableProperty ItemAddAnimationProperty =
            BindableProperty.Create("ItemAddAnimation", typeof(IAnimation), typeof(ToolBar), null);

        public IAnimation ItemAddAnimation
        {
            get { return (IAnimation)GetValue(ItemAddAnimationProperty); }
            set { SetValue(ItemAddAnimationProperty, value); }
        }

        /// <summary>
        /// Animation when whole item collection changes
        /// </summary>
        public static readonly BindableProperty ItemAddAllAnimationProperty =
            BindableProperty.Create("ItemAddAllAnimation", typeof(IAnimation), typeof(ToolBar), null);

        public IAnimation ItemAddAllAnimation
        {
            get { return (IAnimation)GetValue(ItemAddAllAnimationProperty); }
            set { SetValue(ItemAddAllAnimationProperty, value); }
        }

        /// <summary>
        /// Animation when item collection changes and item is removed
        /// </summary>
        public static readonly BindableProperty ItemRemoveAnimationProperty =
            BindableProperty.Create("ItemRemoveAnimation", typeof(IAnimation), typeof(ToolBar), null);

        public IAnimation ItemRemoveAnimation
        {
            get { return (IAnimation)GetValue(ItemRemoveAnimationProperty); }
            set { SetValue(ItemRemoveAnimationProperty, value); }
        }

        /// <summary>
        /// Animation when all items is removed
        /// </summary>
        public static readonly BindableProperty ItemRemoveAllAnimationProperty =
            BindableProperty.Create("ItemRemoveAllAnimation", typeof(IAnimation), typeof(ToolBar), null);

        public IAnimation ItemRemoveAllAnimation
        {
            get { return (IAnimation)GetValue(ItemRemoveAllAnimationProperty); }
            set { SetValue(ItemRemoveAllAnimationProperty, value); }
        }

        /// <summary>
        /// Item add animation duration
        /// </summary>
        public static readonly BindableProperty ItemAddAnimationDurationProperty =
            BindableProperty.Create("ItemAddAnimationDuration", typeof(int), typeof(ToolBar), 500);

        public int ItemAddAnimationDuration
        {
            get { return (int)GetValue(ItemAddAnimationDurationProperty); }
            set { SetValue(ItemAddAnimationDurationProperty, value); }
        }

        /// <summary>
        /// Items refresh animation duration
        /// </summary>
        public static readonly BindableProperty ItemAddAllAnimationDurationProperty =
            BindableProperty.Create("ItemAddAllAnimationDuration", typeof(int), typeof(ToolBar), 1000);

        public int ItemAddAllAnimationDuration
        {
            get { return (int)GetValue(ItemAddAllAnimationDurationProperty); }
            set { SetValue(ItemAddAllAnimationDurationProperty, value); }
        }

        /// <summary>
        /// Item remove animation duration
        /// </summary>
        public static readonly BindableProperty ItemRemoveAnimationDurationProperty =
            BindableProperty.Create("ItemRemoveAnimationDuration", typeof(int), typeof(ToolBar), 500);

        public int ItemRemoveAnimationDuration
        {
            get { return (int)GetValue(ItemRemoveAnimationDurationProperty); }
            set { SetValue(ItemRemoveAnimationDurationProperty, value); }
        }

        /// <summary>
        /// Items clear animation duration
        /// </summary>
        public static readonly BindableProperty ItemRemoveAllAnimationDurationProperty =
            BindableProperty.Create("ItemRemoveAllAnimationDuration", typeof(int), typeof(ToolBar), 1000);

        public int ItemRemoveAllAnimationDuration
        {
            get { return (int)GetValue(ItemRemoveAllAnimationDurationProperty); }
            set { SetValue(ItemRemoveAllAnimationDurationProperty, value); }
        }

        #endregion

        #region Popup animation

        /// <summary>
        /// Popup style if menu is shown inside popup
        /// </summary>
        public static readonly BindableProperty PopupStyleProperty =
            BindableProperty.Create("PopupStyle", typeof(Style), typeof(ToolBar), null);

        public Style PopupStyle
        {
            get { return (Style)GetValue(PopupStyleProperty); }
            set { SetValue(PopupStyleProperty, value); }
        }

        #endregion

        #region Attached properties

        /// <summary>
        /// Is item hidden (internal use only)
        /// </summary>
        private static readonly BindableProperty IsHiddenProperty =
            BindableProperty.CreateAttached("IsHidden", typeof(bool), typeof(ToolBar), false);

        private static bool GetIsHidden(BindableObject view)
        {
            return (bool)view.GetValue(IsHiddenProperty);
        }

        private static void SetIsHidden(BindableObject view, bool value)
        {
            view.SetValue(IsHiddenProperty, value);
        }

        #endregion

        public ToolBar()
        {
            _items = new List<ItemInfo>();
            _menuItems = new List<ItemInfo>();

            ItemsSource = new ObservableCollection<object>();
            MenuItemsSource = new ObservableCollection<object>();

            // Shadow
            _itemsBackground = new BoxView();
            _itemsBackground.InputTransparent = true;
            Children.Add(_itemsBackground);

            Binding bind = new Binding(ItemsBackgroundColorProperty.PropertyName);
            bind.Source = this;
            _itemsBackground.SetBinding(BoxView.ColorProperty, bind);

            // Horizontal line
            _line = new BoxView();
            _line.VerticalOptions = LayoutOptions.Start;
            Children.Add(_line);

            bind = new Binding(LineHeightRequestProperty.PropertyName);
            bind.Source = this;
            _line.SetBinding(BoxView.HeightRequestProperty, bind);

            bind = new Binding(LineColorProperty.PropertyName);
            bind.Source = this;
            _line.SetBinding(BoxView.BackgroundColorProperty, bind);

            // MenuButton
            if (_menuButton == null)
            {
                _menuButton = CreateMenuButton();
                SetIsHidden(_menuButton, true);
            }
            if (Children.Contains(_menuButton) == false)
            {
                Children.Add(_menuButton);
            }

            // Bottom menu scroll viewer
            _menuScrollView = new ScrollView();
            _menuScrollView.Content = MenuItemsLayout;

            bind = new Binding(MenuItemsBackgroundColorProperty.PropertyName);
            bind.Source = this;
            _menuScrollView.SetBinding(View.BackgroundColorProperty, bind);

            _shadowView = new GradientView();
            _shadowView.StartColor = Color.Transparent;
            _shadowView.EndColor = Color.Black;
            _shadowView.Opacity = 0.1;
            _shadowView.HeightRequest = ShadowHeight;
            Children.Add(_shadowView);

            if (Device.RuntimePlatform == Device.iOS)
            {
                // Memory leaks!
                RootPage.Instance.SafeAreaInsetsChanged += (s, t) =>
                {
                    _bottomSafeAreaHeight = t.Bottom;
                    InvalidateMeasure();
                    InvalidateLayout();
                };
            }
        }

        /// <summary>
        /// Close all toolbar menus
        /// </summary>
        public static void CloseAll()
        {
            foreach (ToolBar toolBar in _openToolBars)
            {
                toolBar.IsMenuOpen = false;
            }
        }

        /// <summary>
        /// Properties changes
        /// </summary>
        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (propertyName == ItemsAlignmentProperty.PropertyName || 
                propertyName == MenuButtonAlignmentProperty.PropertyName)
            {
                InvalidateMeasure();
                InvalidateLayout();
            }
            else
            {
                base.OnPropertyChanged(propertyName);
            }
        }

        #region Measure / Layout

        /// <summary>
        /// Measure ToolBar total size
        /// </summary>
        protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
        {
            if (IsDebugEnabled) { System.Diagnostics.Debug.WriteLine("ToolBar.OnMeasure"); }

            ItemChanges changes = GetItemChanges(widthConstraint);

            if (changes.HasChanges && 
                AnimationExtensions.AnimationIsRunning(this, _hideAnimationName) == false &&
                AnimationExtensions.AnimationIsRunning(this, _showAnimationName) == false)
            {
                Task t = DoItemChanges(changes, false, true);
            }

            // Default items size (includes menu button)
            Size defaultItemsSize = new Size();

            // Calculate one child available width
            double childAvailableWidth = CalculateChildAvailableWidth(widthConstraint); 

            // Measure children
            foreach (ItemInfo info in _items)
            {
                // If item should be visible
                if (info.ItemContainer != null && GetIsHidden(info.ItemContainer) == false)
                {
                    if (info.DefaultSize.Request.IsZero)
                    {
                        info.DefaultSize = info.ItemContainer.Measure(double.PositiveInfinity, double.PositiveInfinity, MeasureFlags.IncludeMargins);
                    }

                    defaultItemsSize.Height = Math.Max(info.DefaultSize.Request.Height, defaultItemsSize.Height);
                    
                    if (double.IsInfinity(childAvailableWidth))
                    {
                        defaultItemsSize.Width += info.DefaultSize.Request.Width + Spacing;
                    }
                    else
                    {
                        defaultItemsSize.Width += childAvailableWidth + Spacing;
                    }
                }
            }

            // Measure menu button
            if (_menuButton != null && (GetIsHidden(_menuButton) == false || _menuItems.Count > 0))
            {
                if (_menuButtonSize.Request.IsZero)
                {
                    _menuButtonSize = MeasureView(_menuButton);
                }

                defaultItemsSize.Height = Math.Max(_menuButtonSize.Request.Height, defaultItemsSize.Height);

                if (double.IsInfinity(childAvailableWidth))
                {
                    defaultItemsSize.Width += _menuButtonSize.Request.Width + Spacing;
                }
                else
                {
                    defaultItemsSize.Width += childAvailableWidth + Spacing;
                }

                // If menu button is located on right and items on center, then no spacing between menu button and last default item
                if (MenuButtonAlignment == HorizontalLocations.Right && ItemsAlignment == ItemsAlignments.Center)
                {
                    defaultItemsSize.Width = Math.Max(0, defaultItemsSize.Width - Spacing);
                }
            }
            else
            {
                // Remove last spacing if menu button is not visible
                defaultItemsSize.Width = Math.Max(0, defaultItemsSize.Width - Spacing);
            }

            // If has any default items, then add items padding
            if (defaultItemsSize.Width > 0)
            {
                defaultItemsSize.Width += ItemsPadding.HorizontalThickness;
                defaultItemsSize.Height += ItemsPadding.VerticalThickness;
            }

            // Measure bottom menu
            if (MenuItemsLayout != null && MenuMode == ToolBarMenuModes.Bottom)
            {
                _menuSize = _menuScrollView.Measure(widthConstraint, BottomMenuMaxHeight, MeasureFlags.IncludeMargins);
            }
            else
            {
                _menuSize = new SizeRequest();
            }

            Size totalSize = new Size();

            // Total height
            if (VisibilityMode == ToolBarVisibilityModes.Auto)
            {
                totalSize.Height = (Math.Min(_menuSize.Request.Height, BottomMenuMaxHeight) * _menuAnimationProcess) + (defaultItemsSize.Height * _visibilityAnimationProcess);
            }
            else if (VisibilityMode == ToolBarVisibilityModes.Hidden)
            {
                totalSize.Height = 0;
            }
            else
            {
                totalSize.Height = Math.Max(MinimumHeightRequest, BottomMenuMaxHeight + defaultItemsSize.Height);
            }

            if (totalSize.Height > 0)
            {
                totalSize.Height += LineHeightRequest;
            }

            totalSize.Height += _bottomSafeAreaHeight;

            return new SizeRequest(totalSize, totalSize);
        }

        /// <summary>
        /// Layout children
        /// </summary>
        protected override void LayoutChildren(double x, double y, double width, double height)
        {
            if (IsDebugEnabled) { System.Diagnostics.Debug.WriteLine("ToolBar.LayoutChildren"); }

            ItemChanges changes = GetItemChanges(width);

            if (changes.HasChanges &&
                AnimationExtensions.AnimationIsRunning(this, _hideAnimationName) == false &&
                AnimationExtensions.AnimationIsRunning(this, _showAnimationName) == false)
            {
                Task t = DoItemChanges(changes, false, true);
            }

            // Default items size
            Size defaultItemsSize = new Size();

            // Calculate one child available width
            double childAvailableWidth = CalculateChildAvailableWidth(width);

            // Measure children
            foreach (ItemInfo info in _items)
            {
                // If item is child
                if (info.ItemContainer != null && GetIsHidden(info.ItemContainer) == false)
                {
                    if (info.DefaultSize.Request.IsZero)
                    {
                        info.DefaultSize = info.ItemContainer.Measure(double.PositiveInfinity, double.PositiveInfinity, MeasureFlags.IncludeMargins);
                    }

                    defaultItemsSize.Height = Math.Max(info.DefaultSize.Request.Height, defaultItemsSize.Height);

                    if (double.IsInfinity(childAvailableWidth))
                    {
                        defaultItemsSize.Width += info.DefaultSize.Request.Width + Spacing;
                    }
                    else
                    {
                        defaultItemsSize.Width += childAvailableWidth + Spacing;
                    }
                }
            }

            // Measure menu button
            SizeRequest menuButtonSize = new SizeRequest();
            if (_menuButton != null && GetIsHidden(_menuButton) == false)
            {
                if (_menuButtonSize.Request.IsZero)
                {
                    _menuButtonSize = MeasureView(_menuButton);
                }

                defaultItemsSize.Height = Math.Max(_menuButtonSize.Request.Height, defaultItemsSize.Height);

                if (double.IsInfinity(childAvailableWidth))
                {
                    menuButtonSize = _menuButtonSize;
                }
                else
                {
                    Size s = new Size(childAvailableWidth, _menuButtonSize.Request.Height);
                    menuButtonSize = new SizeRequest(s, s);
                }

                // If menu button is located on right and items on center, then no spacing between menu button and last default item
                if (MenuButtonAlignment == HorizontalLocations.Right && ItemsAlignment == ItemsAlignments.Center)
                {
                    defaultItemsSize.Width = Math.Max(0, defaultItemsSize.Width - Spacing);
                }
            }
            else
            {
                // Remove last spacing if menu button is not visible
                defaultItemsSize.Width = Math.Max(0, defaultItemsSize.Width - Spacing);
            }

            double bottomMenuTop = height;
            double actualMenuHeight = 0;

            // Layout bottom menu
            if (MenuItemsLayout != null && MenuMode == ToolBarMenuModes.Bottom)
            {
                _menuSize = _menuScrollView.Measure(width, double.PositiveInfinity, MeasureFlags.IncludeMargins);

                actualMenuHeight = Math.Min(_menuSize.Request.Height, BottomMenuMaxHeight);
                bottomMenuTop = height - _bottomSafeAreaHeight - (actualMenuHeight * _menuAnimationProcess);

                LayoutChildIntoBoundingRegion(_menuScrollView, new Rectangle(
                    0,
                    bottomMenuTop,
                    width,
                    Math.Min(_menuSize.Request.Height, BottomMenuMaxHeight)));
            }
            else
            {
                _menuSize = new SizeRequest();
            }

            // Calculate start x coordinate
            double xOffset = 0;
            switch (ItemsAlignment)
            {
                case ItemsAlignments.Center:

                    // If menu button is left or not enought availabe space to go right
                    if (MenuButtonAlignment == HorizontalLocations.Left || width - defaultItemsSize.Width - menuButtonSize.Request.Width - ItemsPadding.HorizontalThickness < menuButtonSize.Request.Width / 2)
                    {
                        xOffset = (width - defaultItemsSize.Width - menuButtonSize.Request.Width) / 2;
                    }
                    else
                    {
                        xOffset = (width - defaultItemsSize.Width) / 2;
                    }
                    break;
                case ItemsAlignments.Stretch:
                case ItemsAlignments.Left:
                    xOffset = ItemsPadding.Left;
                    break;
                case ItemsAlignments.Right:
                    xOffset = width - defaultItemsSize.Width - ItemsPadding.Right - menuButtonSize.Request.Width;
                    break;
            }
            
            // Layout default children
            foreach (ItemInfo info in _items)
            {
                double actualChildWidth = info.DefaultSize.Request.Width;
                if (ItemsAlignment == ItemsAlignments.Stretch)
                {
                    actualChildWidth = childAvailableWidth;
                }

                Rectangle itemLocation = new Rectangle();

                if (GetIsHidden(info.ItemContainer) == false)
                {
                    itemLocation = new Rectangle(
                        xOffset + info.ItemContainer.Margin.Left,
                        bottomMenuTop - ItemsPadding.Bottom - info.DefaultSize.Request.Height, 
                        actualChildWidth - info.ItemContainer.Margin.HorizontalThickness, 
                        info.DefaultSize.Request.Height - info.ItemContainer.Margin.VerticalThickness);

                    xOffset += actualChildWidth + Spacing;
                }
                else
                {
                    itemLocation = new Rectangle(
                        10000, 
                        0, 
                        actualChildWidth - info.ItemContainer.Margin.HorizontalThickness, 
                        info.DefaultSize.Request.Height - info.ItemContainer.Margin.VerticalThickness);
                }

                info.ItemContainer.Layout(itemLocation);
            }

            // Layout menu button
            if (_menuButton != null)
            {
                Rectangle menuButtonLocation = new Rectangle();
                if (GetIsHidden(_menuButton) == false)
                {
                    if (MenuButtonAlignment == HorizontalLocations.Left)
                    {
                        menuButtonLocation = new Rectangle(
                            xOffset + _menuButton.Margin.Left,
                            bottomMenuTop - ItemsPadding.Bottom - _menuButtonSize.Request.Height,
                            menuButtonSize.Request.Width - _menuButton.Margin.HorizontalThickness, 
                            menuButtonSize.Request.Height - _menuButton.Margin.VerticalThickness);
                    }
                    else
                    {
                        menuButtonLocation = new Rectangle(
                            width - ItemsPadding.Right - menuButtonSize.Request.Width + _menuButton.Margin.Left,
                            bottomMenuTop - ItemsPadding.Bottom - _menuButtonSize.Request.Height,
                            menuButtonSize.Request.Width - _menuButton.Margin.HorizontalThickness, 
                            menuButtonSize.Request.Height - _menuButton.Margin.VerticalThickness);
                    }
                }
                else
                {
                    menuButtonLocation = new Rectangle(
                        10000, 
                        0, 
                        menuButtonSize.Request.Width - _menuButton.Margin.HorizontalThickness, 
                        menuButtonSize.Request.Height - _menuButton.Margin.VerticalThickness);
                }

                _menuButton.Layout(menuButtonLocation);
            }

            // Layout background and line
            LayoutChildIntoBoundingRegion(_line, new Rectangle(
                0, 
                height - defaultItemsSize.Height - (actualMenuHeight * _menuAnimationProcess) - _bottomSafeAreaHeight - LineHeightRequest, 
                width, 
                _line.HeightRequest));

            LayoutChildIntoBoundingRegion(_shadowView, new Rectangle(
                0,
                height - defaultItemsSize.Height - ItemsPadding.VerticalThickness - _shadowView.HeightRequest - (actualMenuHeight * _menuAnimationProcess) - _bottomSafeAreaHeight - LineHeightRequest,
                width,
                _shadowView.HeightRequest));

            LayoutChildIntoBoundingRegion(_itemsBackground, new Rectangle(
                0, 
                height - defaultItemsSize.Height - ItemsPadding.VerticalThickness - (actualMenuHeight * _menuAnimationProcess) - _bottomSafeAreaHeight, 
                width, 
                defaultItemsSize.Height + ItemsPadding.VerticalThickness + (_menuSize.Request.Height * _menuAnimationProcess)));
        }

        /// <summary>
        /// Calculate default items available width
        /// </summary>
        private double CalculateChildAvailableWidth(double widthConstraint)
        {
            double childAvailableWidth = double.PositiveInfinity;

            if (ItemsAlignment == ItemsAlignments.Stretch && double.IsInfinity(widthConstraint) == false && double.IsNaN(widthConstraint) == false && HorizontalOptions.Alignment == LayoutAlignment.Fill)
            {
                int otherChildrenCount = _backgroundChildrenCount;
                if (Children.Contains(_menuScrollView))
                {
                    otherChildrenCount++;
                }
                double totalSpacing = Spacing * (Children.Count - otherChildrenCount - 1);
                childAvailableWidth = (widthConstraint - totalSpacing - ItemsPadding.HorizontalThickness) / (Children.Count - otherChildrenCount);
            }

            return childAvailableWidth;
        }

        /// <summary>
        /// Measure view and add / remove it to children if needed
        /// </summary>
        private SizeRequest MeasureView(View view)
        {
            SizeRequest size = new SizeRequest();
            
            if (view.HeightRequest >= 0 || view.WidthRequest >= 0)
            {
                Size s = new Size(view.WidthRequest, view.HeightRequest);
                size = new SizeRequest(s, s);
            }
            else
            {
                size = view.Measure(double.PositiveInfinity, double.PositiveInfinity, MeasureFlags.IncludeMargins);
            }

            return size;
        }

        #endregion

        #region Measure invalidation
        
        protected override bool ShouldInvalidateOnChildAdded(View child)
        {
            return base.ShouldInvalidateOnChildAdded(child) && _ignoreInvalidation == false;
        }

        protected override bool ShouldInvalidateOnChildRemoved(View child)
        {
            return base.ShouldInvalidateOnChildRemoved(child) && _ignoreInvalidation == false;
        }

        protected override void OnChildMeasureInvalidated()
        {
            _menuButtonSize = new SizeRequest();

            if (_items != null)
            {
                foreach (ItemInfo info in _items)
                {
                    if (info.Action != ItemActionTypes.Remove)
                    {
                        info.DefaultSize = new SizeRequest(Size.Zero, Size.Zero);
                    }
                }
            }

            base.OnChildMeasureInvalidated();
        }
        
        #endregion

        #region UpdateItems

        /// <summary>
        /// Create, measure, add and remove items to/from layout
        /// </summary>
        private ItemChanges GetItemChanges(double width)
        {
            ItemChanges itemChanges = new ItemChanges();

            int index = 0;
            int defaultButtonsCount = 0;
            bool noAvailableSpace = false;
            double xOffset = ItemsPadding.Left;
           
            foreach (ItemInfo itemInfo in _items)
            {
                if (itemInfo.Action == ItemActionTypes.Removing)
                {
                    // Do nothing if going to be removed
                }
                // If item is going to be removed
                else if (itemInfo.Action == ItemActionTypes.Remove)
                {
                    RemoveItem(itemInfo, itemChanges);
                }
                // If no enought space for item, then add item to menu
                else if (noAvailableSpace || defaultButtonsCount >= MaxItems - 1)
                {
                    bool doChanges = AddMenuItem(itemInfo, itemChanges);

                    if (doChanges)
                    {
                        itemInfo.Action = ItemActionTypes.MoveToMenu;
                    }
                }
                // Maybe enought space for item
                else
                {
                    // If first item
                    if (index == 0 && itemInfo.DefaultSize.Request.IsZero)
                    {
                        itemInfo.DefaultSize = MeasureView(itemInfo.ItemContainer);
                    }

                    double actualNextItemWidth = GetNextItemWidth(index, width);

                    if (index < _items.Count - 1)
                    {
                        actualNextItemWidth = Spacing + actualNextItemWidth;
                    }

                    SizeRequest actualMenuButtonSize = new SizeRequest();

                    // Measure menu button if menu items which are always in menu
                    bool hasMenuItems = _menuItems != null && _menuItems.Count > 0;
                    if (hasMenuItems)
                    {
                        if (_menuButtonSize.Request.IsZero)
                        {
                            _menuButtonSize = MeasureView(_menuButton);
                        }

                        actualMenuButtonSize = _menuButtonSize;
                    }

                    // If not enought space for current and next item OR current item which is last
                    if (xOffset + itemInfo.DefaultSize.Request.Width + actualNextItemWidth + actualMenuButtonSize.Request.Width > width - ItemsPadding.Right)
                    {
                        if (hasMenuItems == false)
                        {
                            if (_menuButtonSize.Request.IsZero)
                            {
                                _menuButtonSize = MeasureView(_menuButton);
                            }

                            actualMenuButtonSize = _menuButtonSize;
                        }

                        // If enought space for current default item
                        if (xOffset + itemInfo.DefaultSize.Request.Width + Spacing <= width - ItemsPadding.Right)
                        {
                            bool changes = false;
                            
                            if (itemInfo.Action != ItemActionTypes.Adding)
                            {
                                AddDefaultItem(itemInfo, itemChanges);
                            }

                            xOffset += itemInfo.ItemContainer.IsVisible ? itemInfo.DefaultSize.Request.Width + Spacing : 0;
                            defaultButtonsCount++;

                            if (changes)
                            {
                                itemInfo.Action = ItemActionTypes.MoveToDefault;
                            }
                        }
                        // if not, add to menu
                        else
                        {
                            bool doChanges = AddMenuItem(itemInfo, itemChanges);

                            if (doChanges)
                            {
                                itemInfo.Action = ItemActionTypes.MoveToMenu;
                            }
                        }

                        // If enought space for menu button
                        if (xOffset + actualMenuButtonSize.Request.Width <= width - ItemsPadding.Right)
                        {
                            // Add menu button width to offset
                            xOffset += actualMenuButtonSize.Request.Width;
                        }
                        // else, make space for menu button
                        else
                        {
                            // Remove previous items and add those to menu. Create menu items if not created.
                            for (int i = defaultButtonsCount - 1; i >= 0; i--)
                            {
                                ItemInfo previousItemInfo = _items.ElementAt(i);

                                bool doChanges = AddMenuItem(previousItemInfo, itemChanges);
                                xOffset -= previousItemInfo.DefaultSize.Request.Width + Spacing;
                                defaultButtonsCount--;

                                if (doChanges)
                                {
                                    itemInfo.Action = ItemActionTypes.MoveToMenu;
                                }

                                // Stop removing when enought space for menu button
                                if (width - (xOffset + ItemsPadding.Right) > actualMenuButtonSize.Request.Width)
                                {
                                    xOffset += actualMenuButtonSize.Request.Width;
                                    break;
                                }
                            }
                        }
                        
                        noAvailableSpace = true;
                    }
                    // If enought space for current default item
                    else
                    {
                        bool doChanges = false;

                        if (itemInfo.Action != ItemActionTypes.Adding)
                        {
                            AddDefaultItem(itemInfo, itemChanges);
                        }

                        xOffset += itemInfo.ItemContainer.IsVisible ? itemInfo.DefaultSize.Request.Width + Spacing : 0;
                        defaultButtonsCount++;

                        if (doChanges && itemInfo.Action == null)
                        {
                            itemInfo.Action = ItemActionTypes.MoveToDefault;
                        }
                    }
                }

                index++;
            }

            // Handle always menu items
            foreach (ItemInfo itemInfo in _menuItems)
            {
                if (itemInfo.Action == ItemActionTypes.Remove)
                {
                    RemoveItem(itemInfo, itemChanges);
                }
                else if (itemInfo.Action == ItemActionTypes.Removing)
                {
                    // Do nothing if going to be removed
                }
                else if (itemInfo.Action == ItemActionTypes.Add)
                {
                    AddMenuItem(itemInfo, itemChanges);
                }
            }

            // If all menu items is going to be removed            
            if (itemChanges.MenuItemsToRemove.Count > 0 && itemChanges.MenuItemsToRemove.Count == MenuItemsLayout.Children.Count && itemChanges.MenuItemsToAdd.Count == 0)
            {
                itemChanges.MenuButtonAction = ItemActionTypes.Remove;
            }
            // If menu items are going to be added and there is no menu items
            else if (itemChanges.MenuItemsToAdd.Count > 0 && MenuItemsLayout.Children.Count == 0)
            {
                itemChanges.MenuButtonAction = ItemActionTypes.Add;
            }

            return itemChanges;
        }

        /// <summary>
        /// Do item changes with or without animation
        /// </summary>
        private async Task DoItemChanges(ItemChanges itemChanges, bool isAnimationEnabled = true, bool ignoreInvalidation = false)
        {
            int defaultItemsCount = CalculateDefaultItemsCount(itemChanges);

            bool updateAll = 
                (_items.Count == itemChanges.DefaultItemsToAdd.Count && itemChanges.DefaultItemsToRemove.Count == 0) || 
                (_items.Count == itemChanges.DefaultItemsToRemove.Count && itemChanges.DefaultItemsToAdd.Count == 0) ||
                (_items.Count == itemChanges.DefaultItemsToRemove.Count + itemChanges.DefaultItemsToAdd.Count);

            //
            // Remove animation
            //

            Animation removeAnimation = new Animation();
            int removeDuration = 0;
            IAnimation removeAnimationCreator = null;

            if (isAnimationEnabled)
            {
                if (updateAll)
                {
                    removeAnimationCreator = ItemRemoveAllAnimation;
                    removeDuration = ItemRemoveAllAnimationDuration;
                }
                else if (ItemRemoveAnimation != null)
                {
                    removeAnimationCreator = ItemRemoveAnimation;
                    removeDuration = ItemRemoveAnimationDuration;
                }
            }

            // Remove menu items immediatley without animation
            foreach (ItemInfo itemInfo in itemChanges.MenuItemsToRemove)
            {
                MenuItemsLayout.Children.Remove(itemInfo.MenuItemContainer);

                // If item is going to be fully removed, then remove also default item which is always generated (and currently hidden)
                if (itemInfo.Action == ItemActionTypes.Remove)
                {
                    _items.Remove(itemInfo);
                    _menuItems.Remove(itemInfo);

                    _ignoreInvalidation = true;
                    Children.Remove(itemInfo.ItemContainer);
                    _ignoreInvalidation = false;
                }
            }

            // Add menu items immediatley without animation
            foreach (ItemInfo itemInfo in itemChanges.MenuItemsToAdd)
            {
                // Clear current action
                if (itemInfo.Action != ItemActionTypes.MoveToMenu)
                {
                    itemInfo.Action = null;
                }

                // Get correct index to add
                int menuIndex = _items.IndexOf(itemInfo) - defaultItemsCount;
                menuIndex = Math.Min(MenuItemsLayout.Children.Count, menuIndex);
                menuIndex = Math.Max(0, menuIndex);

                _ignoreInvalidation = true;

                if (menuIndex < MenuItemsLayout.Children.Count)
                {
                    MenuItemsLayout.Children.Insert(menuIndex, itemInfo.MenuItemContainer);
                }
                else
                {
                    MenuItemsLayout.Children.Add(itemInfo.MenuItemContainer);
                }

                _ignoreInvalidation = false;
            }
            
            // Remove default items with correct animation
            foreach (ItemInfo itemInfo in itemChanges.DefaultItemsToRemove)
            {
                if (removeAnimationCreator != null)
                {
                    removeAnimation.Add(0, 1, removeAnimationCreator.Create(itemInfo.ItemContainer));
                    
                    // If do actual remove (not move to menu items) then set to removing state
                    if (itemInfo.Action == ItemActionTypes.Remove)
                    {
                        itemInfo.Action = ItemActionTypes.Removing;
                    }
                }
            }

            // Remove menu button with animation
            if (removeAnimationCreator != null && itemChanges.MenuButtonAction == ItemActionTypes.Remove)
            {
                removeAnimation.Add(0, 1, removeAnimationCreator.Create(_menuButton));
            }

            // Set items to add "Adding" state before animation
            foreach (ItemInfo itemInfo in itemChanges.DefaultItemsToAdd)
            {
                itemInfo.Action = ItemActionTypes.Adding;
            }

            bool isRemoveAborted = false;

            // Do remove animations
            if (removeAnimation.HasSubAnimations())
            {
                TaskCompletionSource<bool> removeTcs = new TaskCompletionSource<bool>();
                
                Device.BeginInvokeOnMainThread(() =>
                {
                    removeAnimation.Commit(this, _removeAnimationName, 64, (uint)removeDuration, Easing.Linear, finished: (d, a) =>
                    {
                        removeTcs.SetResult(a);
                    });
                });

                isRemoveAborted = await removeTcs.Task;
            }

            if (isRemoveAborted)
            {
                foreach (ItemInfo itemInfo in _items.ToList())
                {
                    if (itemInfo.Action == ItemActionTypes.Remove || itemInfo.Action == ItemActionTypes.Removing)
                    {
                        _items.Remove(itemInfo);
                        Children.Remove(itemInfo.ItemContainer);
                    }
                }

                return;
            }

            // Do after remove animation operations to default items which is going to be removed or moved to menu
            foreach (ItemInfo itemInfo in itemChanges.DefaultItemsToRemove)
            {
                // If do actual remove from layout, then remove from items and children
                if (itemInfo.Action == ItemActionTypes.Remove || itemInfo.Action == ItemActionTypes.Removing)
                {
                    _items.Remove(itemInfo);

                    // _ignoreInvalidation = true;
                    Children.Remove(itemInfo.ItemContainer);
                    // _ignoreInvalidation = false;
                }
                // If move to menu set default item hidden
                else if (itemInfo.Action == ItemActionTypes.MoveToMenu)
                {
                    SetIsHidden(itemInfo.ItemContainer, true);
                }

                itemInfo.Action = null;
            }

            //
            // Add animation
            //

            Animation addAnimation = new Animation();
            int addDuration = 0;
            IAnimation addAnimationCreator = null;

            if (isAnimationEnabled)
            {
                if (updateAll)
                {
                    addAnimationCreator = ItemAddAllAnimation;
                    addDuration = ItemAddAllAnimationDuration;
                }
                else if (ItemAddAnimation != null)
                {
                    addAnimationCreator = ItemAddAnimation;
                    addDuration = ItemAddAnimationDuration;
                }
            }

            bool isAnyItemSetToVisible = false;

            // Add default items
            foreach (ItemInfo itemInfo in itemChanges.DefaultItemsToAdd)
            {
                if (itemInfo.Action == ItemActionTypes.Adding || itemInfo.Action == ItemActionTypes.Add || itemInfo.Action == ItemActionTypes.MoveToDefault)
                {
                    if (addAnimationCreator != null)
                    {
                        addAnimation.Add(0, 1, addAnimationCreator.Create(itemInfo.ItemContainer));
                    }
                    else
                    {
                        ResetViewAnimationProperties(itemInfo.ItemContainer);
                    }

                    itemInfo.Action = null;
                    SetIsHidden(itemInfo.ItemContainer, false);
                    isAnyItemSetToVisible = true;
                }
            }

            // Add menu button
            if (itemChanges.MenuButtonAction == ItemActionTypes.Add)
            {
                SetIsHidden(_menuButton, false);
                isAnyItemSetToVisible = true;

                if (addAnimationCreator != null)
                {
                    addAnimation.Add(0, 1, addAnimationCreator.Create(_menuButton));
                }
                else
                {
                    ResetViewAnimationProperties(_menuButton);
                }
            }
            else if (itemChanges.MenuButtonAction == ItemActionTypes.Remove)
            {
                SetIsHidden(_menuButton, true);
                isAnyItemSetToVisible = true;
            }

            // Update item counts
            VisibleItemsCount = defaultItemsCount;
            MenuItemsCount = MenuItemsLayout.Children.Count;
            HasItems = (ItemsSource != null && ItemsSource.Count > 0) || (MenuItemsSource != null && MenuItemsSource.Count > 0);

            if (isAnyItemSetToVisible && ignoreInvalidation == false)
            {
                InvalidateMeasure();
                InvalidateLayout();
            }

            // Do add animation
            if (addAnimation.HasSubAnimations())
            {
                TaskCompletionSource<bool> addTcs = new TaskCompletionSource<bool>();

                addAnimation.Commit(this, _addAnimationName, 64, (uint)addDuration, Easing.Linear, finished: (d, isAborted) =>
                {
                    addTcs.SetResult(true);
                });

                await addTcs.Task;
            }
        }

        /// <summary>
        /// Reset item container properties which may change when animated
        /// </summary>
        private void ResetViewAnimationProperties(View itemContainer)
        {
            itemContainer.TranslationX = 0.001;
            itemContainer.TranslationY = 0.001;
            itemContainer.Scale = 1;
            itemContainer.ScaleX = 1;
            itemContainer.ScaleY = 1;
            itemContainer.Opacity = 1;
        }

        /// <summary>
        /// Add item 
        /// </summary>
        /// <returns>True if changes is done</returns>
        private bool AddDefaultItem(ItemInfo itemInfo, ItemChanges itemChanges)
        {
            bool doChanges = false;

            if (itemChanges.MenuItemsToAdd.Contains(itemInfo))
            {
                itemChanges.MenuItemsToAdd.Remove(itemInfo);
                doChanges = true;
            }
            if (itemChanges.DefaultItemsToRemove.Contains(itemInfo))
            {
                itemChanges.DefaultItemsToRemove.Remove(itemInfo);
                doChanges = true;
            }

            // Add default item
            if (GetIsHidden(itemInfo.ItemContainer) == true)
            {
                itemChanges.DefaultItemsToAdd.Add(itemInfo);
                doChanges = true;
            }

            // Remove item from menu
            if (itemInfo.MenuItemContainer != null && MenuItemsLayout.Children.Contains(itemInfo.MenuItemContainer))
            {
                itemChanges.MenuItemsToRemove.Add(itemInfo);
                doChanges = true;
            }

            return doChanges;
        }

        /// <summary>
        /// Update itemChanges to add menu item
        /// </summary>
        private bool AddMenuItem(ItemInfo itemInfo, ItemChanges itemChanges)
        {
            bool doChanges = false;

            // Remove default item from children if added
            if (itemInfo.ItemContainer != null && GetIsHidden(itemInfo.ItemContainer) == false)
            {
                itemChanges.DefaultItemsToRemove.Add(itemInfo);
                doChanges = true;
            }

            // Cancel adding to default items
            if (itemChanges.DefaultItemsToAdd.Contains(itemInfo))
            {
                itemChanges.DefaultItemsToAdd.Remove(itemInfo);
                doChanges = true;
            }

            // Create menu item if not created
            if (itemInfo.MenuItemContainer == null)
            {
                CreateMenuItem(itemInfo);
                doChanges = true;
            }

            if (itemInfo.MenuItemContainer == null)
            {
                return false;
            }

            // Add menu item if not already going to be added or menu already contains it
            if (MenuItemsLayout.Children.Contains(itemInfo.MenuItemContainer) == false && itemChanges.MenuItemsToAdd.Contains(itemInfo) == false)
            {
                itemChanges.MenuItemsToAdd.Add(itemInfo);
                doChanges = true;
            }

            // Cancel removing from menu items
            if (itemChanges.MenuItemsToRemove.Contains(itemInfo))
            {
                itemChanges.MenuItemsToRemove.Remove(itemInfo);
                doChanges = true;
            }

            return doChanges;
        }

        /// <summary>
        /// Update itemChanges to do item remove
        /// </summary>
        private bool RemoveItem(ItemInfo itemToRemove, ItemChanges itemChanges)
        {
            bool doChanges = false;

            if (itemToRemove.ItemContainer != null && GetIsHidden(itemToRemove.ItemContainer) == false)
            {
                itemChanges.DefaultItemsToRemove.Add(itemToRemove);
                doChanges = true;
            }
            if (itemToRemove.MenuItemContainer != null && MenuItemsLayout.Children.Contains(itemToRemove.MenuItemContainer))
            {
                itemChanges.MenuItemsToRemove.Add(itemToRemove);
                doChanges = true;
            }
            if (itemChanges.DefaultItemsToAdd.Contains(itemToRemove))
            {
                itemChanges.DefaultItemsToAdd.Remove(itemToRemove);
                doChanges = true;
            }
            if (itemChanges.MenuItemsToAdd.Contains(itemToRemove))
            {
                itemChanges.MenuItemsToAdd.Remove(itemToRemove);
                doChanges = true;
            }

            return doChanges;
        }

        /// <summary>
        /// Get next element desired width. Return 0 if no next item.
        /// </summary>
        private double GetNextItemWidth(int index, double availableWidth)
        {
            // Get next item
            if (index + 1 < _items.Count)
            {
                ItemInfo nextItem = _items.ElementAt(index + 1);   
                
                if (nextItem.Action == ItemActionTypes.Remove)
                {
                    return GetNextItemWidth(index + 2, availableWidth);
                }

                if (nextItem.DefaultSize.Request.IsZero)
                {
                    nextItem.DefaultSize = MeasureView(nextItem.ItemContainer);
                }
                return nextItem.DefaultSize.Request.Width;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Calculate default items count based on Children and item changes
        /// </summary>
        private int CalculateDefaultItemsCount(ItemChanges itemChanges)
        {
            int count = 0;
            // Default children as child
            if (_menuButton != null && Children.Contains(_menuButton))
            {
                count = Children.Count - 1;
            }
            else
            {
                count = Children.Count;
            }

            count += itemChanges.DefaultItemsToAdd.Count;
            count -= itemChanges.DefaultItemsToRemove.Count;

            count -= 2; // Background and line
            if (Children.Contains(_menuScrollView))
            {
                count--;
            }

            return count;
        }

        /// <summary>
        /// Change menu button IsToggled state without event raise
        /// </summary>
        private void SetIsMenuButtonToggled(bool isToggled)
        {
            if (_menuButton is IToggable toggableMenuButton && toggableMenuButton.IsToggled != isToggled)
            {
                toggableMenuButton.IsToggledChanged -= OnMenuButtonToggled;
                toggableMenuButton.IsToggled = isToggled;
                toggableMenuButton.IsToggledChanged += OnMenuButtonToggled;
            }
        }

        #endregion

        #region Show / Hide

        /// <summary>
        /// Show toolbar by measure invalidation
        /// </summary>
        private async Task ShowAsync(bool isAnimated = true)
        {
            this.AbortAnimation(_hideAnimationName);

            if (AnimationExtensions.AnimationIsRunning(this, _showAnimationName))
            {
                return;
            }

            if (isAnimated)
            {
                if (_visibilityAnimationProcess.Equals(1))
                {
                    _visibilityAnimationProcess = 0;
                }

                Animation showAnim = new Animation(d =>
                {
                    _visibilityAnimationProcess = d;
                    InvalidateMeasure();

                }, _visibilityAnimationProcess, 1);

                TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

                showAnim.Commit(this, _showAnimationName, 64, (uint)ShowDuration, ShowEasingFunction, (d, isAborted) =>
                {
                    tcs.SetResult(true);
                });

                await tcs.Task;
            }
            else
            {
                _visibilityAnimationProcess = 1;
                InvalidateMeasure();
            }
        }

        /// <summary>
        /// Hide whole toolbar using measure invalidation
        /// </summary>
        private async Task HideAsync(bool isAnimated = true)
        {
            this.AbortAnimation(_showAnimationName);

            if (AnimationExtensions.AnimationIsRunning(this, _hideAnimationName))
            {
                return;
            }

            if (isAnimated)
            {
                Animation hideAnim = new Animation(d =>
                {
                    _visibilityAnimationProcess = d;
                    InvalidateMeasure();

                }, _visibilityAnimationProcess, 0);

                TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

                hideAnim.Commit(this, _hideAnimationName, 64, (uint)HideDuration, HideEasingFunction, (d, isAborted) =>
                {
                    tcs.SetResult(true);
                });

                await tcs.Task;
            }
            else
            {
                _visibilityAnimationProcess = 0;
                InvalidateMeasure();
            }
        }

        /// <summary>
        /// Change default items visibility mode if has no items
        /// </summary>
        private async void OnVisibilityModeChanged(ToolBarVisibilityModes oldMode, ToolBarVisibilityModes newMode)
        {
            if (newMode == ToolBarVisibilityModes.Auto)
            {
                if (_items.Count == 0 && Bounds.Height > 0)
                {
                    await HideAsync();
                }
                else if (_items.Count > 0 && Bounds.Height.Equals(0))
                {
                    await ShowAsync();
                }
            }
            else
            {
                InvalidateMeasure();
            }
        }

        #endregion

        #region Menu

        /// <summary>
        /// Open menu async
        /// </summary>
        public async Task SetIsMenuOpenAsync(bool isOpen, bool isAnimated)
        {
            if (_ignoreIsMenuOpenChanges)
            {
                return;
            }

            _ignoreIsMenuOpenChanges = true;
            IsMenuOpen = isOpen;
            SetIsMenuButtonToggled(isOpen);
            _ignoreIsMenuOpenChanges = false;

            if (IsMenuOpenChanged != null)
            {
                IsMenuOpenChanged(this, isOpen);
            }

            if (MenuMode == ToolBarMenuModes.Bottom)
            {
                if (isOpen)
                {
                    if (_popup != null && _popup.Content == _menuScrollView)
                    {
                        _popup.Content = null;
                    }
                    if (Children.Contains(_menuScrollView) == false)
                    {
                        Children.Add(_menuScrollView);
                    }

                    await ShowBottomMenu();
                }
                else
                {
                    await HideBottomMenu();
                }
            }
            else
            {
                if (_popup == null)
                {
                    _popup = new Popup();
                    _popup.PlacementTarget = _menuButton;
                    _popup.Style = PopupStyle;
                    _popup.HasModalBackground = false;
                    _popup.IsOpenChanged += (s, e) =>
                    {
                        if (e == false)
                        {
                            IsMenuOpen = false;
                        }
                    };

                    Binding bind = new Binding("PopupPlacement");
                    bind.Source = this;
                    _popup.SetBinding(Popup.PlacementProperty, bind);
                }

                if (_popup.Content != _menuScrollView)
                {
                    if (Children.Contains(_menuScrollView))
                    {
                        Children.Remove(_menuScrollView);
                    }

                    _popup.Content = _menuScrollView;
                }

                // Show or hide popup
                _popup.IsOpen = isOpen;
            }

            if (isOpen)
            {
                _openToolBars.Add(this);
            }
            else
            {
                _openToolBars.Remove(this);
            }
        }

        /// <summary>
        /// Event when menu button is toggled
        /// </summary>
        private async void OnMenuButtonToggled(object sender, bool isToggled)
        {
            await SetIsMenuOpenAsync(isToggled, true);
        }

        /// <summary>
        /// Show bottom menu
        /// </summary>
        private async Task ShowBottomMenu()
        {
            if (AnimationExtensions.AnimationIsRunning(this, _openMenuAnimationName))
            {
                return;
            }

            this.AbortAnimation(_closeMenuAnimationName);

            Animation showBottomMenuAnim = new Animation(d =>
            {
                _menuAnimationProcess = d;
                LayoutChildren(0, 0, Width, Height);
            }, _menuAnimationProcess, 1);

            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            Device.BeginInvokeOnMainThread(() =>
            {
                showBottomMenuAnim.Commit(this, _openMenuAnimationName, 64, (uint)ShowMenuDuration, ShowMenuEasingFunction, finished: (p, a) =>
                {
                    tcs.SetResult(true);
                });
            });

            await tcs.Task;
        }

        /// <summary>
        /// Hide bottom menu
        /// </summary>
        private async Task HideBottomMenu()
        {
            if (AnimationExtensions.AnimationIsRunning(this, _closeMenuAnimationName))
            {
                return;
            }

            this.AbortAnimation(_openMenuAnimationName);

            Animation hideBottomMenuAnim = new Animation(d =>
            {
                _menuAnimationProcess = d;
                LayoutChildren(0, 0, Width, Height);
            }, _menuAnimationProcess, 0);

            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            hideBottomMenuAnim.Commit(this, _closeMenuAnimationName, 64, (uint)HideMenuDuration, HideMenuEasingFunction, finished: (p, a) =>
            {
                tcs.SetResult(true);
            });

            await tcs.Task;
        }

        /// <summary>
        /// Change menu items layout
        /// </summary>
        private void OnMenuItemsLayoutChanged(Layout<View> oldLayout, Layout<View> newLayout)
        {
            if (_menuScrollView == null)
            {
                return;
            }

            List<View> oldLayoutChildren = new List<View>();
            if (oldLayout != null)
            {
                // Save old layout children
                oldLayoutChildren.AddRange(oldLayout.Children);

                // Clear old layout from children
                oldLayout.Children.Clear();

                // Remove layout from scroll view
                _menuScrollView.Content = null;
            }

            if (newLayout != null)
            {
                // Add old layout chilren to new layout
                foreach (View child in oldLayoutChildren)
                {
                    newLayout.Children.Add(child);
                }

                // Add new layout to scroll view
                if (_menuScrollView.Content != newLayout)
                {
                    _menuScrollView.Content = newLayout;
                }
            }
        }

        /// <summary>
        /// Create MenuItemsLayout from template
        /// </summary>
        private void OnMenuItemsLayoutTemplateChanged(DataTemplate oldDataTemplate, DataTemplate newDataTemplate)
        {
            if (newDataTemplate != null)
            {
                View newLayout = newDataTemplate.CreateContent() as View;
                if (newLayout is Layout<View> == false)
                {
                    throw new Exception("MenuItemsLayout root element is not Layout!");
                }

                MenuItemsLayout = newLayout as Layout<View>;
            }
            else
            {
                MenuItemsLayout = null;
            }
        }

        #endregion

        #region Items

        /// <summary>
        /// Called when ItemsSource collection changed
        /// </summary>
        protected virtual void OnItemsSourceCollectionChanged()
        {
            return;
        }

        /// <summary>
        /// Called when MenuItemsSource collection changed
        /// </summary>
        protected virtual void OnMenuItemsSourceCollectionChanged()
        {
            return;
        }

        /// <summary>
        /// Called when any item (which implements ITappable interface) is tapped
        /// </summary>
        /// <param name="item"></param>
        protected virtual void OnItemTapped(View item)
        {
            return;
        }

        /// <summary>
        /// Handle item tap event
        /// </summary>
        private void OnItemTapped(object sender, EventArgs e)
        {
            View itemContainer = sender as View;

            OnItemTapped(itemContainer);

            if (ItemTapped != null)
            {
                ItemTapped(sender, e);
            }

            if (ItemCommand != null)
            {
                ItemCommand.Execute(itemContainer.BindingContext);
            }
        }

        /// <summary>
        /// Called when ItemsSource changes
        /// </summary>
        private async Task OnItemsSourceChanged(IList oldSource, IList newSource)
        {
            if (oldSource != null && oldSource is INotifyCollectionChanged)
            {
                (oldSource as INotifyCollectionChanged).CollectionChanged -= OnItemsSourceCollectionChangedInternal;
            }

            if (newSource != null && newSource is INotifyCollectionChanged)
            {
                (newSource as INotifyCollectionChanged).CollectionChanged += OnItemsSourceCollectionChangedInternal;
            }

            if (_ignoreItemsSourceChanges)
            {
                return;
            }
            
            bool hasNewItems = newSource != null && newSource.Count > 0;

            AnimationExtensions.AbortAnimation(this, _removeAnimationName);

            if (oldSource != null && oldSource.Count > 0)
            {
                await Reset(_items, hasNewItems == false);
            }

            if (newSource != null && newSource.Count > 0)
            {
                await AddItem(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newSource, 0), _items, newSource);
            }
        }

        /// <summary>
        /// Called when MenuItemsSource changes
        /// </summary>
        private async Task OnMenuItemsSourceChanged(IList oldSource, IList newSource)
        {
            if (oldSource != null && oldSource is INotifyCollectionChanged)
            {
                (oldSource as INotifyCollectionChanged).CollectionChanged -= OnMenuItemsSourceCollectionChangedInternal;
            }

            if (newSource != null && newSource is INotifyCollectionChanged)
            {
                (newSource as INotifyCollectionChanged).CollectionChanged += OnMenuItemsSourceCollectionChangedInternal;
            }

            if (_ignoreItemsSourceChanges)
            {
                return;
            }

            if (oldSource != null && oldSource.Count > 0)
            {
                await Reset(_menuItems);
            }

            if (newSource != null && newSource.Count > 0)
            {
                await AddItem(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newSource, 0), _menuItems, newSource);
            }
        }

        /// <summary>
        /// Event when ItemsSource internal collection changes
        /// </summary>
        private async void OnItemsSourceCollectionChangedInternal(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Add new items
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                await AddItem(e, _items, ItemsSource);
            }
            // Move current items
            else if (e.Action == NotifyCollectionChangedAction.Move)
            {
                await MoveItem(e, _menuItems);
            }
            // Remove single or more items
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                await RemoveItem(e, _items);
            }
            // Replace items
            else if (e.Action == NotifyCollectionChangedAction.Replace)
            {
                await ReplaceItem(e, _items, ItemsSource);
            }
            // Remove ALL items
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                await Reset(_items);
            }
            else
            {
                throw new Exception("ToolBar: Invalid action!");
            }

            OnItemsSourceCollectionChanged();
        }

        /// <summary>
        /// Event when MenuItemsSource internal collection changes
        /// </summary>
        private async void OnMenuItemsSourceCollectionChangedInternal(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Add new items
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                await AddItem(e, _menuItems, MenuItemsSource);
            }
            // Move current items
            else if (e.Action == NotifyCollectionChangedAction.Move)
            {
                await MoveItem(e, _menuItems);
            }
            // Remove single or more items
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                await RemoveItem(e, _menuItems);
            }
            // Replace items
            else if (e.Action == NotifyCollectionChangedAction.Replace)
            {
                await ReplaceItem(e, _menuItems, MenuItemsSource);
            }
            // Remove ALL items
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                await Reset(_menuItems);
            }
            else
            {
                throw new Exception("ToolBar: Invalid action!");
            }

            OnMenuItemsSourceCollectionChanged();
        }

        /// <summary>
        /// Add item
        /// </summary>
        private async Task AddItem(NotifyCollectionChangedEventArgs e, IList internalItemsList, IList itemsSource)
        {
            if (e.NewItems.Count == 0)
            {
                return;
            }

            int index = (e.NewStartingIndex == -1) ? 0 : e.NewStartingIndex;

            // Removing items count before index
            int removingItemsCount = GetRemovingItemsCountBefore(index, internalItemsList);

            _ignoreInvalidation = true;

            View lastItem = e.NewItems[e.NewItems.Count - 1] as View;

            foreach (object item in e.NewItems)
            {
                int actualIndex = index - removingItemsCount;

                View itemContainer = null;
                if (IsItemItsOwnContainer(item))
                {
                    itemContainer = item as View;
                    PrepareItemContainer(itemContainer, null, null);
                }
                else
                {
                    itemContainer = CreateItemContainer(item);
                    PrepareItemContainer(itemContainer, null, item);
                }

                ItemInfo newItemInfo = new ItemInfo();
                newItemInfo.Action = ItemActionTypes.Add;
                newItemInfo.ItemContainer = itemContainer;
                newItemInfo.Item = item;
                SetIsHidden(newItemInfo.ItemContainer, true);

                internalItemsList.Insert(actualIndex, newItemInfo);

                index++;

                if (item == lastItem)
                {
                    _ignoreInvalidation = false;
                }

                if (internalItemsList == _items && Children.Contains(itemContainer) == false)
                {
                    Children.Add(itemContainer);
                }
            }

            _ignoreInvalidation = false;

            if (_isInitializationRunning == false && Width >= 0)
            {
                ItemChanges changes = GetItemChanges(Width);

                // If hidden and visibility mode is auto
                if (VisibilityMode == ToolBarVisibilityModes.Auto && Bounds.Height <= 0)
                {
                    // Add items immediatley and show toolbar with animation
                    await DoItemChanges(changes, false);
                    await ShowAsync();
                }
                else
                {
                    // Add items with animation and resize toolbar without animation
                    await DoItemChanges(changes);
                }
            }
        }

        /// <summary>
        /// Move item without animation
        /// </summary>
        private async Task MoveItem(NotifyCollectionChangedEventArgs e, IList internalItemList)
        {
            await Task.Delay(0);
            throw new NotImplementedException();
        }

        /// <summary>
        /// Remove item
        /// </summary>
        private async Task RemoveItem(NotifyCollectionChangedEventArgs e, IList internalItemsList, bool doItemsUpdate = true)
        {
            int index = (e.OldStartingIndex == -1) ? 0 : e.OldStartingIndex;
            int removeCount = e.OldItems.Count;

            // Removing items count before index
            int removingItemsBeforeCount = GetRemovingItemsCountBefore(index, internalItemsList);

            for (int i = removingItemsBeforeCount + index; i < internalItemsList.Count; i++)
            {
                ItemInfo itemInfo = internalItemsList[i] as ItemInfo;

                if (itemInfo.Action != ItemActionTypes.Remove && itemInfo.Action != ItemActionTypes.Removing)
                {
                    itemInfo.Action = ItemActionTypes.Remove;
                    removeCount--;

                    if (removeCount == 0)
                    {
                        break;
                    }
                }
            }

            if (doItemsUpdate == false)
            {
                return;
            }

            if (_isInitializationRunning == false)
            {
                ItemChanges changes = GetItemChanges(Width);

                // If visibility mode is auto and all items is removed
                if (VisibilityMode == ToolBarVisibilityModes.Auto && ItemsSource.Count == 0 && MenuItemsSource.Count == 0)
                {
                    // Hide toolbar with animation and do child changes immediatley without animation after toolbar hide animation
                    await HideAsync();
                    await DoItemChanges(changes, false);
                }
                else
                {
                    // Do item changes with animation
                    await DoItemChanges(changes);
                }
            }
        }

        /// <summary>
        /// Replace item without animation
        /// </summary>
        private async Task ReplaceItem(NotifyCollectionChangedEventArgs e, IList internalItemsList, IList itemsSource)
        {
            await RemoveItem(e, internalItemsList, false);
            await AddItem(e, internalItemsList, itemsSource);
        }

        /// <summary>
        /// Reset all items. If menu is open then items is removed after menu is closed.
        /// </summary>
        private async Task Reset(IList internalItemList, bool doItemsUpdate = true)
        {
            if (IsMenuOpen && MenuItemsLayout != null)
            {
                await SetIsMenuOpenAsync(false, true);
            }

            foreach (ItemInfo itemInfo in internalItemList)
            {
                if (itemInfo.Action != ItemActionTypes.Removing)
                {
                    itemInfo.Action = ItemActionTypes.Remove;
                }
            }

            if (doItemsUpdate == false)
            {
                return;
            }

            if (_isInitializationRunning == false)
            {
                ItemChanges changes = GetItemChanges(Width);

                bool hasItems = HasItems = (ItemsSource != null && ItemsSource.Count > 0) || (MenuItemsSource != null && MenuItemsSource.Count > 0);

                // If visibility mode is auto and all items is removed
                if (VisibilityMode == ToolBarVisibilityModes.Auto && hasItems == false)
                {
                    // Hide toolbar with animation and do child changes immediatley without animation after toolbar hide animation
                    await HideAsync();
                    await DoItemChanges(changes, false);
                }
                else
                {
                    // Do item changes with animation
                    await DoItemChanges(changes);
                }
            }
        }

        /// <summary>
        /// Get items count which is going to be removed befor giving index
        /// </summary>
        private int GetRemovingItemsCountBefore(int index, IList internalItemsList)
        {
            int count = 0;

            for (int i = 0; i < index; i++)
            {
                ItemInfo itemInfo = internalItemsList[i] as ItemInfo;
                if (itemInfo.Action == ItemActionTypes.Remove || itemInfo.Action == ItemActionTypes.Removing)
                {
                    count++;
                }
            }

            return count;
        }

        #endregion

        #region Container generation

        /// <summary>
        /// Is container generation needed
        /// </summary>
        private bool IsItemItsOwnContainer(object item)
        {
            return item is View;
        }

        /// <summary>
        /// Create item UI element for model
        /// </summary>
        private View CreateItemContainer(object model)
        {
            View item = null;
            if (ItemTemplateSelector != null)
            {
                DataTemplate containerTemplate = ItemTemplateSelector.SelectTemplate(model, null) as DataTemplate;
                item = containerTemplate.CreateContent() as View;
            }
            else if (ItemTemplate != null)
            {
                item = ItemTemplate.CreateContent() as View;
            }
            else
            {
                item = new ContentView();
            }

            if (item == null)
            {
                throw new Exception("ItemTemplate is not subclass of ContentView");
            }

            return item;
        }

        /// <summary>
        /// Add correct item template to item container content
        /// </summary>
        private void CreateItemTemplate(View itemContainer, object model)
        {
            View item = null;
            if (ItemTemplate != null)
            {
                item = ItemTemplate.CreateContent() as View;

                // If container is ContentView or it's subclass
                if (itemContainer is ContentView)
                {
                    (itemContainer as ContentView).Content = item;
                }
                // If container is any view which implements IContent interface
                else if (itemContainer is IContent)
                {
                    (itemContainer as IContent).Content = item;
                }
            }
        }

        /// <summary>
        /// Prepare item container
        /// </summary>
        /// <param name="itemContainer">Item container</param>
        /// <param name="itemTemplate">Item container content UI element which is generated from ItemTemplate or ItemTemplateSelector. Null if item is its own container.</param>
        /// <param name="model">Item model. Null if item is its own container.</param>
        public void PrepareItemContainer(View itemContainer, View itemTemplate, object model)
        {
            if (itemTemplate != null)
            {
                if (itemContainer is ContentView contentView)
                {
                    contentView.Content = itemTemplate;
                }
                else if (itemContainer is Border border)
                {
                    border.Content = itemTemplate;
                }
                else if (itemContainer is IContent iContentControl)
                {
                    iContentControl.Content = itemTemplate;
                }
            }

            if (model != null)
            {
                itemContainer.BindingContext = model;
            }

            if (itemContainer is ITappable tappable)
            {
                tappable.Tapped += OnItemTapped;
            }

            PrepareItemContainer(itemContainer, model);
        }


        /// <summary>
        /// Do item container custom preparation
        /// </summary>
        protected virtual void PrepareItemContainer(View itemContainer, object model)
        {
            return;
        }

        /// <summary>
        /// Create, measure and set MenuItem binding context
        /// </summary>
        private void CreateMenuItem(ItemInfo itemInfo)
        {
            itemInfo.MenuItemContainer = MenuItemContainerGenerator.GenerateContainer(itemInfo.Item);
        }

        #endregion

        #region MenuButton

        /// <summary>
        /// Handle menu button template changes
        /// </summary>
        private void OnMenuButtonTemplateChanged(DataTemplate oldTemplate, DataTemplate newTemplate)
        {
            bool isHidden = true;
            if (_menuButton != null)
            {
                isHidden = GetIsHidden(_menuButton);
                if (Children.Contains(_menuButton))
                {
                    Children.Remove(_menuButton);
                }
            }

            _menuButton = CreateMenuButton();
            SetIsHidden(_menuButton, isHidden);

            if (_menuButton != null && Children != null)
            {
                Children.Add(_menuButton);
            }
        }

        /// <summary>
        /// Create menu button from MenuButtonTemplate
        /// </summary>
        private View CreateMenuButton()
        {
            View menuButton = MenuButtonTemplate.CreateContent() as View;

            if (menuButton is IToggable toggableMenuButton)
            {
                toggableMenuButton.IsToggledChanged += OnMenuButtonToggled;
            }
            else
            {
                throw new Exception("MenuButtonTemplate root must implement IToggable interface!");
            }

            return menuButton;
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Helper class for item
        /// </summary>
        private class ItemInfo
        {
            public object Item { get; set; }
            public View ItemContainer { get; set; }
            public View MenuItemContainer { get; set; }
            public SizeRequest DefaultSize { get; set; }
            public ItemActionTypes? Action { get; set; }            
        }

        /// <summary>
        /// Helper class for items changes
        /// </summary>
        private class ItemChanges
        {
            public List<ItemInfo> DefaultItemsToRemove { get; set; } = new List<ItemInfo>();
            public List<ItemInfo> DefaultItemsToAdd { get; set; } = new List<ItemInfo>();
            public List<ItemInfo> MenuItemsToRemove { get; set; } = new List<ItemInfo>();
            public List<ItemInfo> MenuItemsToAdd { get; set; } = new List<ItemInfo>();

            public ItemActionTypes? MenuButtonAction { get; set; }

            public bool HasChanges
            {
                get
                {
                    return DefaultItemsToRemove.Count > 0 || DefaultItemsToAdd.Count > 0 || MenuItemsToRemove.Count > 0 || MenuItemsToAdd.Count > 0 || MenuButtonAction != null;
                }
            }
        }

        private class SizeInfo
        {
            public View Child { get; set; }
            public SizeRequest Size { get; set; }

            public SizeInfo(View child, SizeRequest size)
            {
                Child = child;
                Size = size;
            }
        }

        #endregion
    }

    /// <summary>
    /// Interface for menu item container generation based on default item
    /// </summary>
    public interface IMenuItemContainerGenerator
    {
        View GenerateContainer(object item);
    }

    /// <summary>
    /// How items are aligned
    /// </summary>
    public enum ItemsAlignments { Left, Right, Center, Stretch }

    /// <summary>
    /// ToolBar menu mode
    /// </summary>
    public enum ToolBarMenuModes { Popup, Bottom }
}
