using SkiaSharp;
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
    public enum SearchModes { Filter, Search, None }

    public enum DropDownSelectionModes { Single, Multiple }

    [ContentProperty("ItemsSource")]
    public class DropDown : TextBox
    {
        private const string _itemsViewName = "PART_ItemsView";
        private const string _closeButtonName = "PART_CloseButton";
        private const string _searchBoxName = "PART_SearchBox";
        private const string _captionLabelName = "PART_CaptionLabel";

        private Popup _popup = null;

        // Active popup content search result ItemsView
        private DropDownMenuItemsView _itemsView = null;
        private Button _closeButton = null;
        private TextBox _searchBox = null;
        private Xamarin.Forms.Label _captionLabel = null;

        private View _selectedItemsCountView = null;
        private Size _selectedItemsCountViewSize = new Size();

        private bool _ignoreInvalidation = false;

        private List<View> _selectedItemsViews = null;

        // Popup contents
        private View _fullScreenPopupContent = null;
        private View _popupContent = null;

        private TapGestureRecognizer _tapGesture = null;

        private bool _ignoreSearchTextChanged = false;

        private bool _isSelectedItemsVisible = true;

        private const double _selectedItemsHideX = 10000;

        #region BindingProperties

        /// <summary>
        /// Filter = All items is show immediatley and could filter by search key. Search = Items is shown when search is dXamKit. None = Search is not used. All items is show immediatley.
        /// </summary>
        public static readonly BindableProperty SearchModeProperty =
            BindableProperty.Create("SearchMode", typeof(SearchModes), typeof(DropDown), SearchModes.None, propertyChanged: OnSearchModeChanged);

        private static void OnSearchModeChanged(BindableObject bindable, object oldValue, object newValue)
        {
            ((DropDown)bindable).OnSearchModeChanged((SearchModes)oldValue, (SearchModes)newValue);
        }

        public SearchModes SearchMode
        {
            get { return (SearchModes)GetValue(SearchModeProperty); }
            set { SetValue(SearchModeProperty, value); }
        }

        public static readonly BindableProperty PopupStyleProperty =
            BindableProperty.Create("PopupStyle", typeof(Style), typeof(DropDown), null);

        public Style PopupStyle
        {
            get { return (Style)GetValue(PopupStyleProperty); }
            set { SetValue(PopupStyleProperty, value); }
        }

        public static readonly BindableProperty PopupSpacingProperty =
            BindableProperty.Create("PopupSpacing", typeof(double), typeof(DropDown), 0.0);

        public double PopupSpacing
        {
            get { return (double)GetValue(PopupSpacingProperty); }
            set { SetValue(PopupSpacingProperty, value); }
        }

        #endregion

        #region Properties - Items

        /// <summary>
        /// Items data source
        /// </summary>
        public static readonly BindableProperty ItemsSourceProperty =
            BindableProperty.Create("ItemsSource", typeof(IList), typeof(DropDown), null, propertyChanged: OnItemsSourceChanged);

        private static void OnItemsSourceChanged(BindableObject bindable, object oldValue, object newValue)
        {
            (bindable as DropDown).OnItemsSourceChanged(oldValue as IList, newValue as IList);
        }

        public IList ItemsSource
        {
            get { return (IList)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        /// <summary>
        /// How many items is show on popup. If more, then do selection from full screen popup. If search is enabled then show always full screen popup.
        /// </summary>
        public static readonly BindableProperty MaxItemsTresholdProperty =
            BindableProperty.Create("MaxItemsTreshold", typeof(int), typeof(DropDown), 5);

        public int MaxItemsTreshold
        {
            get { return (int)GetValue(MaxItemsTresholdProperty); }
            set { SetValue(MaxItemsTresholdProperty, value); }
        }

        /// <summary>
        /// Item datatemplate
        /// </summary>
        public static readonly BindableProperty ItemTemplateProperty =
            BindableProperty.Create("ItemTemplate", typeof(DataTemplate), typeof(DropDown), null);

        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }

        /// <summary>
        /// Items datatemplate selector
        /// </summary>
        public static readonly BindableProperty ItemTemplateSelectorProperty =
            BindableProperty.Create("ItemTemplateSelector", typeof(DataTemplateSelector), typeof(DropDown), null);

        public DataTemplateSelector ItemTemplateSelector
        {
            get { return (DataTemplateSelector)GetValue(ItemTemplateSelectorProperty); }
            set { SetValue(ItemTemplateSelectorProperty, value); }
        }

        /// <summary>
        /// SelectedItem datatemplate
        /// </summary>
        public static readonly BindableProperty SelectedItemTemplateProperty =
            BindableProperty.Create("SelectedItemTemplate", typeof(DataTemplate), typeof(DropDown), null);

        public DataTemplate SelectedItemTemplate
        {
            get { return (DataTemplate)GetValue(SelectedItemTemplateProperty); }
            set { SetValue(SelectedItemTemplateProperty, value); }
        }

        /// <summary>
        /// SelectedItem datatemplate selector
        /// </summary>
        public static readonly BindableProperty SelectedItemTemplateSelectorProperty =
            BindableProperty.Create("SelectedItemTemplateSelector", typeof(DataTemplateSelector), typeof(DropDown), null);

        public DataTemplateSelector SelectedItemTemplateSelector
        {
            get { return (DataTemplateSelector)GetValue(SelectedItemTemplateSelectorProperty); }
            set { SetValue(SelectedItemTemplateSelectorProperty, value); }
        }

        /// <summary>
        /// SelectedItem count indicator datatemplate
        /// </summary>
        public static readonly BindableProperty SelectedItemsCountTemplateProperty =
            BindableProperty.Create("SelectedItemsCountTemplate", typeof(DataTemplate), typeof(DropDown), null);

        public DataTemplate SelectedItemsCountTemplate
        {
            get { return (DataTemplate)GetValue(SelectedItemsCountTemplateProperty); }
            set { SetValue(SelectedItemsCountTemplateProperty, value); }
        }

        /// <summary>
        /// Popup template
        /// </summary>
        public static readonly BindableProperty PopupTemplateProperty =
            BindableProperty.Create("PopupTemplate", typeof(DataTemplate), typeof(DropDown), null);

        public DataTemplate PopupTemplate
        {
            get { return (DataTemplate)GetValue(PopupTemplateProperty); }
            set { SetValue(PopupTemplateProperty, value); }
        }

        /// <summary>
        /// Full screen popup template
        /// </summary>
        public static readonly BindableProperty FullScreenPopupTemplateProperty =
            BindableProperty.Create("FullScreenPopupTemplate", typeof(DataTemplate), typeof(DropDown), null);

        public DataTemplate FullScreenPopupTemplate
        {
            get { return (DataTemplate)GetValue(FullScreenPopupTemplateProperty); }
            set { SetValue(FullScreenPopupTemplateProperty, value); }
        }

        #endregion

        #region Properties - Selection

        /// <summary>
        /// Selected item model or item container if 'SelectionMode' is 'Single'
        /// </summary>
        public static readonly BindableProperty SelectedItemProperty =
            BindableProperty.Create("SelectedItem", typeof(object), typeof(DropDown), null, propertyChanged: OnSelectedItemChanged);

        private static void OnSelectedItemChanged(BindableObject bindable, object oldValue, object newValue)
        {
            ((DropDown)bindable).OnSelectedItemChanged(oldValue as object, newValue as object);
        }

        public object SelectedItem
        {
            get { return (object)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        /// <summary>
        /// List of selected item models or item containers if 'SelectionMode' is 'Multiple'
        /// </summary>
        public static readonly BindableProperty SelectedItemsProperty =
            BindableProperty.Create("SelectedItems", typeof(IList), typeof(DropDown), null, propertyChanged: OnSelectedItemsChanged);

        private static void OnSelectedItemsChanged(BindableObject bindable, object oldValue, object newValue)
        {
            ((DropDown)bindable).OnSelectedItemsChanged(oldValue as IList, newValue as IList);
        }

        public IList SelectedItems
        {
            get { return (IList)GetValue(SelectedItemsProperty); }
            set { SetValue(SelectedItemsProperty, value); }
        }

        /// <summary>
        /// Items selection mode. Single = only one item can be selected. Multiple = multiple items can be selected. 
        /// </summary>
		public static readonly BindableProperty SelectionModeProperty =
            BindableProperty.Create("SelectionMode", typeof(DropDownSelectionModes), typeof(DropDown), DropDownSelectionModes.Single);
        
        public DropDownSelectionModes SelectionMode
        {
            get { return (DropDownSelectionModes)GetValue(SelectionModeProperty); }
            set { SetValue(SelectionModeProperty, value); }
        }

        /// <summary>
        /// Command to execute when search text changes
        /// </summary>
		public static readonly BindableProperty SearchTextCommandProperty =
			BindableProperty.Create("SearchTextCommand", typeof(ICommand), typeof(DropDown), null);
        
		public ICommand SearchTextCommand
		{
			get { return (ICommand)GetValue(SearchTextCommandProperty); }
			set { SetValue(SearchTextCommandProperty, value); }
		}

		/// <summary>
		/// Search text
		/// </summary>
		public static readonly BindableProperty SearchTextProperty =
            BindableProperty.Create("SearchText", typeof(string), typeof(DropDown), null, propertyChanged: OnSearchTextChanged);

        private static void OnSearchTextChanged(BindableObject bindable, object oldValue, object newValue)
        {
            DropDown dropDown = new DropDown();
            if (dropDown.SearchTextCommand != null)
            {
                dropDown.SearchTextCommand.Execute(newValue);
            }
        }

        public string SearchText
		{
			get { return (string)GetValue(SearchTextProperty); }
			set { SetValue(SearchTextProperty, value); }
		}

        /// <summary>
        /// Selected items count
        /// </summary>
        public static readonly BindableProperty SelectedItemsCountProperty =
            BindableProperty.Create("SelectedItemsCount", typeof(int), typeof(DropDown), 0);

        public int SelectedItemsCount
        {
            get { return (int)GetValue(SelectedItemsCountProperty); }
            protected set { SetValue(SelectedItemsCountProperty, value); }
        }

        /// <summary>
        /// Hidden selected items count
        /// </summary>
        public static readonly BindableProperty HiddenSelectedItemsCountProperty =
            BindableProperty.Create("HiddenSelectedItemsCount", typeof(int), typeof(DropDown), 0);

        public int HiddenSelectedItemsCount
        {
            get { return (int)GetValue(HiddenSelectedItemsCountProperty); }
            protected set { SetValue(HiddenSelectedItemsCountProperty, value); }
        }

        public static readonly BindableProperty SelectedItemsSpacingProperty =
            BindableProperty.Create("SelectedItemsSpacing", typeof(double), typeof(DropDown), 0.0);

        public double SelectedItemsSpacing
        {
            get { return (double)GetValue(SelectedItemsSpacingProperty); }
            set { SetValue(SelectedItemsSpacingProperty, value); }
        }

        #endregion

        #region Properties

        protected override bool ActualIsFocused
        {
            get
            {
                if (_textElement != null)
                {
                    return _textElement.IsFocused || (_popup != null && _popup.IsOpen);
                }
                else
                {
                    return false;
                }
            }
        }

        protected override bool ActualHasValue
        {
            get
            {
                return base.ActualHasValue || (_selectedItemsViews != null && _selectedItemsViews.Count > 0);
            }
        }

        private bool IsSelectedItemsVisible
        {
            get
            {
                return _isSelectedItemsVisible;
            }
            set
            {
                _isSelectedItemsVisible = value;

                SetSelectedItemsVisibility(value);
            }
        }

        #endregion

        public DropDown() : base()
        {
            ItemsSource = new ObservableCollection<object>();
			SelectedItems = new ObservableCollection<object>();
            _selectedItemsViews = new List<View>();

            OnSearchModeChanged(SearchMode, SearchMode);
            UpdateInteraction();
            OnSelectedItemsCountTemplateChanged(SelectedItemsCountTemplate);
        }

        #region Property changed callbacks

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (_itemsView != null)
            {
                if (propertyName == DropDown.ItemTemplateProperty.PropertyName)
                {
                    _itemsView.ItemTemplate = ItemTemplate;
                }
                else if (propertyName == DropDown.ItemTemplateSelectorProperty.PropertyName)
                {
                    _itemsView.ItemTemplateSelector = ItemTemplateSelector;
                }
                else if (propertyName == DropDown.ItemsSourceProperty.PropertyName)
                {
                    _itemsView.ItemsSource = ItemsSource;
                    UpdateInteraction();
                }
                else if (propertyName == DropDown.FullScreenPopupTemplateProperty.PropertyName)
                {
                    UpdateInteraction();
                }
                else if (propertyName == DropDown.SelectionModeProperty.PropertyName)
                {
                    OnSelectionModeChanged(SelectionMode);
                }
                else if (propertyName == DropDown.LeftIconAssemblyNameProperty.PropertyName || propertyName == DropDown.LeftIconResourceKeyProperty.PropertyName)
                {
                    if (LeftIconResourceKey != null && LeftIconAssemblyName != null)
                    {
                        _itemsView.SetItemsLeftPadding(LeftIconWidthRequest + LeftIconMargin.HorizontalThickness);
                    }
                }
            }

            if (propertyName == SelectedItemsCountTemplateProperty.PropertyName || propertyName == DropDown.SelectionModeProperty.PropertyName)
            {
                OnSelectedItemsCountTemplateChanged(SelectedItemsCountTemplate);
            }
            else
            {
                base.OnPropertyChanged(propertyName);
            }
        }

        private void OnItemsSourceChanged(IList oldItemsSource, IList newItemsSource)
        {
            if (oldItemsSource is INotifyCollectionChanged oldList)
            {
                oldList.CollectionChanged -= OnItemsSourceCollectionChanged;
            }

            if (newItemsSource is INotifyCollectionChanged newList)
            {
                newList.CollectionChanged += OnItemsSourceCollectionChanged;
            }

            UpdateInteraction();

        }

        private void OnItemsSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateInteraction();
        }

        private void OnSelectedItemsCountTemplateChanged(DataTemplate selectedItemsCountTemplate)
        {
            if (_selectedItemsCountView != null)
            {
                Children.Remove(_selectedItemsCountView);
                _selectedItemsCountView = null;
            }

            if (selectedItemsCountTemplate != null && SelectionMode == DropDownSelectionModes.Multiple)
            {
                _selectedItemsCountView = selectedItemsCountTemplate.CreateContent() as View;

                if (Children.Contains(_selectedItemsCountView) == false)
                {
                    Children.Add(_selectedItemsCountView);
                }
            }
        }

        #endregion

        #region Ignore invalidation

        protected override void InvalidateLayout()
        {
            if (_ignoreInvalidation == false)
            {
                base.InvalidateLayout();
            }
        }

        protected override void InvalidateMeasure()
        {
            if (_ignoreInvalidation == false)
            {
                base.InvalidateMeasure();
            }
        }

        protected override void OnChildMeasureInvalidated()
        {
            if (_ignoreInvalidation == false)
            {
                base.OnChildMeasureInvalidated();
            }
        }

        protected override bool ShouldInvalidateOnChildAdded(View child)
        {
            return base.ShouldInvalidateOnChildAdded(child) && _ignoreInvalidation == false;
        }

        protected override bool ShouldInvalidateOnChildRemoved(View child)
        {
            return base.ShouldInvalidateOnChildRemoved(child) && _ignoreInvalidation == false;
        }

        #endregion

        #region Measure / Layout

        /// <summary>
        /// Do entry measure with selected items
        /// </summary>
        protected override Size MeasureEntry(double width, double height, Size actualLeftIconSize, Size actualRightIconSize, Size actualCaptionSize)
        {
            Size entrySize = base.MeasureEntry(width, height, actualLeftIconSize, actualRightIconSize, actualCaptionSize);

            double availableWidth = width - actualLeftIconSize.Width - actualRightIconSize.Width - BorderThickness * 2 - TextMargin.HorizontalThickness;

            if (CaptionPlacement == CaptionPlacements.Left && string.IsNullOrEmpty(Caption) == false)
            {
                availableWidth -= actualCaptionSize.Width;
            }

            double captionHeightWithPadding = 0;
            if (string.IsNullOrEmpty(Caption) == false)
            {
                captionHeightWithPadding = ActualTextPadding.Top;
            }

            Size selectedItemsSize = new Size(0, 0);
            if (_selectedItemsViews != null && _selectedItemsViews.Count > 0)
            {
                foreach (View child in _selectedItemsViews)
                {
                    Size size = child.Measure(availableWidth, height, MeasureFlags.IncludeMargins).Request;

                    selectedItemsSize.Width += size.Width + SelectedItemsSpacing;
                    selectedItemsSize.Height = Math.Max(selectedItemsSize.Height, size.Height);                
                }

                if (CaptionPlacement == CaptionPlacements.Inside)
                {
                    selectedItemsSize.Height += captionHeightWithPadding;
                }
            }

            entrySize.Height = Math.Max(selectedItemsSize.Height, entrySize.Height);
            entrySize.Height = Math.Min(height, entrySize.Height);

            entrySize.Width = Math.Max(selectedItemsSize.Width - SelectedItemsSpacing, entrySize.Width);
            entrySize.Width = Math.Min(availableWidth, entrySize.Width);

            return entrySize;
        }

        /// <summary>
        /// Layout selected items
        /// </summary>
        protected override void LayoutChildren(double x, double y, double width, double height)
        {
            base.LayoutChildren(x, y, width, height);

            int hiddenSelectedItemsCount = 0;

            if (_selectedItemsViews != null)
            {
                double selectedItemsX = _entryLocation.Left + TextPadding.Left;
                double selectedItemsMidY = 0;

                SKRect skBorderLocation = GetBorderLocation();
                Rectangle borderLocation = new Rectangle(skBorderLocation.Left / DeviceScale, skBorderLocation.Top / DeviceScale, skBorderLocation.Width / DeviceScale, skBorderLocation.Height / DeviceScale);

                if (CaptionPlacement == CaptionPlacements.Inside)
                {
                    selectedItemsMidY = borderLocation.Top + ActualTextPadding.Top + (borderLocation.Height - ActualTextPadding.Top - ActualLineThickness) / 2;
                }
                else 
                {
                    selectedItemsMidY = borderLocation.Center.Y - ActualLineThickness / 2;
                }

                bool isFull = false;

                for (int i = 0; i < _selectedItemsViews.Count; i++)
                {
                    View child = _selectedItemsViews.ElementAt(i);
                    Size selectedItemSize = child.Measure(width, height, MeasureFlags.IncludeMargins).Request;

                    // If no enought space for any other selected items
                    if (isFull)
                    {
                        LayoutChildIntoBoundingRegion(child, new Rectangle(10000, selectedItemsMidY - (selectedItemSize.Height / 2), selectedItemSize.Width, selectedItemSize.Height));
                        hiddenSelectedItemsCount++;
                    }
                    // If not enought space for this item
                    else if (selectedItemsX + selectedItemSize.Width > _entryLocation.Right && SelectionMode == DropDownSelectionModes.Multiple)
                    {
                        hiddenSelectedItemsCount++;

                        // Measure selected items count indicator size
                        if (_selectedItemsCountView != null && Children.Contains(_selectedItemsCountView))
                        {
                            _selectedItemsCountViewSize = _selectedItemsCountView.Measure(width, height, MeasureFlags.IncludeMargins).Request;
                        }

                        // Remove previous selected items if not enought space for count indicator
                        if (selectedItemsX + _selectedItemsCountViewSize.Width > _entryLocation.Right - TextPadding.Right)
                        {
                            for (int j = i - 1; j >= 0; j--)
                            {
                                hiddenSelectedItemsCount++;

                                View previousChild = _selectedItemsViews.ElementAt(j);
                                Size previousChildSize = previousChild.Measure(width, height, MeasureFlags.IncludeMargins).Request;

                                LayoutChildIntoBoundingRegion(previousChild, new Rectangle(_selectedItemsHideX, selectedItemsMidY - (previousChildSize.Height / 2), previousChildSize.Width, previousChildSize.Height));

                                selectedItemsX -= (previousChildSize.Width + SelectedItemsSpacing);

                                if (selectedItemsX + _selectedItemsCountViewSize.Width <= _entryLocation.Right - TextPadding.Right)
                                {
                                    break;
                                }
                            }
                        }

                        if (_selectedItemsCountView != null && Children.Contains(_selectedItemsCountView))
                        {
                            LayoutChildIntoBoundingRegion(_selectedItemsCountView, new Rectangle(selectedItemsX, selectedItemsMidY - (_selectedItemsCountViewSize.Height / 2), _selectedItemsCountViewSize.Width, _selectedItemsCountViewSize.Height));
                        }

                        LayoutChildIntoBoundingRegion(child, new Rectangle(_selectedItemsHideX, selectedItemsMidY - (selectedItemSize.Height / 2), selectedItemSize.Width, selectedItemSize.Height));

                        isFull = true;
                    }
                    // If enought space for selected item
                    else
                    {
                        LayoutChildIntoBoundingRegion(child, new Rectangle(selectedItemsX, selectedItemsMidY - (selectedItemSize.Height / 2), Math.Min(_entryLocation.Width - TextPadding.HorizontalThickness, selectedItemSize.Width), selectedItemSize.Height));
                    }

                    selectedItemsX += selectedItemSize.Width + SelectedItemsSpacing;
                }

                if (isFull == false && _selectedItemsCountView != null && Children.Contains(_selectedItemsCountView))
                {
                    LayoutChildIntoBoundingRegion(_selectedItemsCountView, new Rectangle(_selectedItemsHideX, selectedItemsMidY - (_selectedItemsCountViewSize.Height / 2), _selectedItemsCountViewSize.Width, _selectedItemsCountViewSize.Height));
                }
            }

            HiddenSelectedItemsCount = hiddenSelectedItemsCount;
        }

        #endregion

        #region Search

        /// <summary>
        /// Handle search mode changes
        /// </summary>
        private void OnSearchModeChanged(SearchModes oldValue, SearchModes newValue)
        {
            UpdateInteraction();
        }

        /// <summary>
        /// Handle text changes. Do search or filtering if needed.
        /// </summary>
        protected override void OnTextChanged(string oldText, string newText)
        {
            if (_ignoreSearchTextChanged)
            {
                return;
            }

            OnDoSearch(newText);
        }

        private void OnDoSearch(string text)
        {
            if (SearchMode == SearchModes.Filter)
            {
                _itemsView.DoFilter(text);
            }
            else if (SearchMode == SearchModes.Search)
            {
                if (SearchTextCommand != null)
                {
                    SearchTextCommand.Execute(text);
                }
            }
        }

        #endregion

        #region Selection

        private void OnSelectionModeChanged(DropDownSelectionModes selectionMode)
        {
            _itemsView.SelectionMode = SelectionMode == DropDownSelectionModes.Multiple ? SelectionModes.Multiple : SelectionModes.Single;
        }

        /// <summary>
        /// Selected item changed on single selection mode
        /// </summary>
        private void OnSelectedItemChanged(object oldItem, object newItem)
        {
            _ignoreInvalidation = true;
            Reset();
            _ignoreInvalidation = false;

            AddItem(0, new List<object>() { newItem });

            // Close popup always when item is selected on single seelction mode
            _popup.IsOpen = false;
            _itemsView.CloseSubMenus();

            SelectedItemsCount = newItem != null ? 1: 0;

            if (IsCaptionAnimationDone() == false)
            {
                _captionAnimationProcess = SelectedItemsCount > 0 ? 1 : 0;
            }
        }

        /// <summary>
        /// Handle SelectedItems list changes
        /// </summary>
        private void OnSelectedItemsChanged(IList oldValue, IList newValue)
        {
            if (oldValue != null && oldValue is INotifyCollectionChanged)
            {
                (oldValue as INotifyCollectionChanged).CollectionChanged -= OnSelectedItemsSourceCollectionChangedInternal;
            }

            if (newValue != null && newValue is INotifyCollectionChanged)
            {
                (newValue as INotifyCollectionChanged).CollectionChanged += OnSelectedItemsSourceCollectionChangedInternal;
            }

            Reset();
            AddItem(0, newValue);

            SelectedItemsCount = SelectedItems.Count;

            if (IsCaptionAnimationDone() == false)
            {
                _captionAnimationProcess = SelectedItemsCount > 0 ? 1 : 0;
            }
        }

        /// <summary>
        /// Handle SelectedItems list internal changes
        /// </summary>
        private void OnSelectedItemsSourceCollectionChangedInternal(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Add new items
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                AddItem(e.NewStartingIndex, e.NewItems);
            }
            // Move current items
            else if (e.Action == NotifyCollectionChangedAction.Move)
            {
                MoveItem();
            }
            // Remove single or more items
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                RemoveItem(e.OldStartingIndex, e.OldItems);
            }
            // Replace items
            else if (e.Action == NotifyCollectionChangedAction.Replace)
            {
                ReplaceItem(e);
            }
            // Remove ALL items
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                Reset();
            }
            else
            {
                throw new Exception("ItemsView: Invalid action!");
            }

            SelectedItemsCount = SelectedItems.Count;

            if (IsCaptionAnimationDone() == false)
            {
                _captionAnimationProcess = SelectedItemsCount > 0 ? 1 : 0;
            }
        }

        private void AddItem(int startingIndex, IList items)
        {
            if (items.Count == 0)
            {
                return;
            }

            int index = (startingIndex == -1) ? 0 : startingIndex;

            foreach (object item in items)
            {
                View itemContainer = CreateSelectedItem(item);
                itemContainer.BindingContext = item;
                _selectedItemsViews.Insert(index, itemContainer);
                
                if (IsSelectedItemsVisible == false)
                {
                    itemContainer.Opacity = 0;
                    itemContainer.InputTransparent = true;
                }
                
                Children.Add(itemContainer);
                index++;
            }
        }

        private void MoveItem()
        {
            throw new NotImplementedException();
        }

        private void RemoveItem(int oldStartingIndex, IList items)
        {
            int index = (oldStartingIndex == -1) ? 0 : oldStartingIndex;
            int removeCount = items.Count;

            for (int i = 0; i < removeCount; i++)
            {
                View selectedItem = _selectedItemsViews[index];
                _selectedItemsViews.RemoveAt(index);
                Children.Remove(selectedItem);
            }
        }

        private void ReplaceItem(NotifyCollectionChangedEventArgs e)
        {
            RemoveItem(e.OldStartingIndex, e.OldItems);
            AddItem(e.NewStartingIndex, e.NewItems);
        }

        private void Reset()
        {
            if (_selectedItemsViews != null)
            {
                foreach (View view in _selectedItemsViews)
                {
                    Children.Remove(view);
                }

                _selectedItemsViews.Clear();
            }
        }

        private View CreateSelectedItem(object item)
        {
            View selectedItemView = null;

            if (SelectedItemTemplateSelector != null)
            {
                DataTemplate actualTemplate = SelectedItemTemplateSelector.SelectTemplate(item, null);
                selectedItemView = SelectedItemTemplate.CreateContent() as View;
            }
            else if (SelectedItemTemplate != null)
            {
                selectedItemView = SelectedItemTemplate.CreateContent() as View;
            }
            else
            {
                throw new Exception("DropDown SelectedItemTemplateSelector and SelectedItemTemplate is null!");
            }

            return selectedItemView;
        }

        private void SetSelectedItemsVisibility(bool isVisible)
        {
            if (isVisible)
            {
                _ignoreSearchTextChanged = true;
                Text = "";
                _ignoreSearchTextChanged = false;

                foreach (View selectedItem in _selectedItemsViews)
                {
                    selectedItem.Opacity = 1;
                    selectedItem.InputTransparent = true;
                }

                if (_selectedItemsCountView != null)
                {
                    _selectedItemsCountView.Opacity = 1;
                    _selectedItemsCountView.InputTransparent = true;
                }
            }
            else
            {
                OnDoSearch("");

                foreach (View selectedItem in _selectedItemsViews)
                {
                    selectedItem.Opacity = 0;
                    selectedItem.InputTransparent = true;
                }

                if (_selectedItemsCountView != null)
                {
                    _selectedItemsCountView.Opacity = 0;
                    _selectedItemsCountView.InputTransparent = true;
                }
            }
        }

        #endregion

        #region Interaction

        private void UpdateInteraction()
        {
            if (_textElement == null)
            {
                return;
            }

            if (SearchMode == SearchModes.None || IsFullScreen())
            {
                _textElement.IsVisible = false;
                _textElement.InputTransparent = true;
                _textElement.IsEnabled = false;

                if (_tapGesture == null)
                {
                    _tapGesture = new TapGestureRecognizer();
                    _tapGesture.Tapped += OnTapped;
                }

                if (GestureRecognizers.Contains(_tapGesture) == false)
                {
                    GestureRecognizers.Add(_tapGesture);
                }
            }
            else
            {
                bool isActive = IsFullScreen() == false;
                if (isActive)
                {
                    _textElement.IsVisible = true;
                    _textElement.InputTransparent = false;
                    _textElement.IsEnabled = true;
                }
                else
                {
                    _textElement.IsVisible = false;
                    _textElement.InputTransparent = true;
                    _textElement.IsEnabled = false;
                }

                if (_tapGesture != null && GestureRecognizers.Contains(_tapGesture))
                {
                    GestureRecognizers.Remove(_tapGesture);
                }
            }
        }

        /// <summary>
        /// Event when tap gesture event is raised when search mode is disabled
        /// </summary>
        private void OnTapped(object sender, EventArgs e)
        {
            OpenDropDown();
        }

        /// <summary>
        /// Event when text element got focus when search mode enabled
        /// </summary>
        protected override void OnGotFocus()
        {
            if (_popup != null && _popup.IsOpen)
            {
                return;
            }

            OpenDropDown();
        }

        /// <summary>
        /// Create and open drop down popup
        /// </summary>
        private void OpenDropDown()
        {
            // Create popup if not created
            if (_popup == null)
            {
                _popup = new Popup();
                _popup.Placement = PopupPlacements.BottomLeft;
                _popup.PlacementTarget = this;
                _popup.Spacing = PopupSpacing;
                _popup.Style = PopupStyle;
                _popup.HasModalBackground = false;
                _popup.IsOpenChanged += OnPopupIsOpenedChanged;
                _popup.AnimationFinished += OnPopupIsOpenAnimationFinished;
            }

            SKRect borderLocation = GetBorderLocation();

            _popup.PlacementRectangle = new Rectangle(
                borderLocation.Left / DeviceScale, 
                borderLocation.Top / DeviceScale, 
                borderLocation.Width / DeviceScale,
                borderLocation.Height / DeviceScale);

            if (IsFullScreen())
            {
                _popup.Placement = PopupPlacements.FullScreen;
                _popup.CornerRadius = 0;
                _popup.ShadowLenght = 0;
                _popup.BorderThickness = 1;
                _popup.BackgroundColor = Color.White;
                _popup.HasModalBackground = true;

                if (_fullScreenPopupContent == null)
                {
                    _fullScreenPopupContent = FullScreenPopupTemplate.CreateContent() as View;
                }

                InitializePopup(_fullScreenPopupContent);

                if (_popup.Content != _fullScreenPopupContent)
                {
                    _popup.Content = _fullScreenPopupContent;
                }
            }
            else
            {
                _popup.Placement = PopupPlacements.BottomLeft;

                if (_popupContent == null)
                {
                    _popupContent = PopupTemplate.CreateContent() as View;
                }

                InitializePopup(_popupContent);

                _popupContent.WidthRequest = Width;
                    
                if (_popup.Content != _popupContent)
                {
                    _popup.Content = _popupContent;
                }
            }

            _popup.IsOpen = true;
        }

        private bool IsFullScreen()
        {
            return FullScreenPopupTemplate != null && ((ItemsSource != null && ItemsSource.Count > MaxItemsTreshold) || (Device.Idiom != TargetIdiom.Desktop && SearchMode != SearchModes.None));
        }

        protected override bool IsCaptionAnimationDone()
        {
            return base.IsCaptionAnimationDone() && (_selectedItemsViews == null || _selectedItemsViews.Count == 0) && IsFullScreen() == false;
        }

        private void InitializePopup(View popupContent)
        {
            if (_itemsView != null)
            {
                _itemsView.SubMenuItemTapped -= OnItemTapped;
            }

            if (_closeButton != null)
            {
                _closeButton.Tapped -= OnClosePopupButtonTapped;
            }

            _itemsView = popupContent.FindByName<XamKit.DropDownMenuItemsView>(_itemsViewName);
            _closeButton = popupContent.FindByName<XamKit.Button>(_closeButtonName);
            _searchBox = popupContent.FindByName<XamKit.TextBox>(_searchBoxName);
            _captionLabel = popupContent.FindByName<Xamarin.Forms.Label>(_captionLabelName);

            if (_itemsView != null)
            {
                _itemsView.ItemTemplate = ItemTemplate;
                _itemsView.ItemTemplateSelector = ItemTemplateSelector;
                _itemsView.SubMenuItemTapped += OnItemTapped;
                _itemsView.SelectionMode = SelectionMode == DropDownSelectionModes.Multiple ? SelectionModes.Multiple : SelectionModes.Single;

                _itemsView.ItemsSource = ItemsSource;

                if (LeftIconResourceKey != null && LeftIconAssemblyName != null)
                {
                    _itemsView.SetItemsLeftPadding(LeftIconWidthRequest + LeftIconMargin.HorizontalThickness);
                }
            }

            if (_closeButton != null)
            {
                _closeButton.Tapped += OnClosePopupButtonTapped;
            }

            if (_searchBox != null)
            {
                if (SearchMode == SearchModes.None)
                {
                    _searchBox.IsVisible = false;
                    _searchBox.IsEnabled = false;
                }
                else
                {
                    _searchBox.IsVisible = true;
                    _searchBox.IsEnabled = true;
                }

                _searchBox.TextChanged += OnFullScreenPopupSearchTextBoxTextChanged;
            }

            if (_captionLabel != null)
            {
                if (SearchMode == SearchModes.None)
                {
                    _captionLabel.IsVisible = true;
                }
                else
                {
                    _captionLabel.IsVisible = false;
                }
            }
        }

        private void OnFullScreenPopupSearchTextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            OnDoSearch(e.NewTextValue);
        }

        private void OnClosePopupButtonTapped(object sender, EventArgs e)
        {
            _popup.IsOpen = false;
        }

        private void OnPopupIsOpenedChanged(object sender, bool isOpened)
        {
            if (isOpened)
            {
                if (SearchMode != SearchModes.None)
                {
                    IsSelectedItemsVisible = false;
                }

                GotFocus();
            }
            else
            {
                LostFocus();

                IsSelectedItemsVisible = true;
            }
        }

        /// <summary>
        /// Handle item selection
        /// </summary>
        private void OnItemTapped(object sender, EventArgs e)
        {
            if (SelectionMode == DropDownSelectionModes.Single)
            {
                if (ItemsSource[0] is View)
                {
                    SelectedItem = sender;
                }
                else
                {
                    SelectedItem = (sender as View).BindingContext;
                }
            }
            else
            {
                if (SelectedItems == null)
                {
                    SelectedItems = new List<object>();
                }

                if (ItemsSource[0] is View)
                {
                    if (SelectedItems.Contains(sender))
                    {
                        SelectedItems.Remove(sender);
                    }
                    else
                    {
                        SelectedItems.Add(sender);
                    }
                }
                else
                {
                    object model = (sender as View).BindingContext;

                    if (SelectedItems.Contains(model))
                    {
                        SelectedItems.Remove(model);
                    }
                    else
                    {
                        SelectedItems.Add(model);
                    }
                }
            }

            if (SelectionMode == DropDownSelectionModes.Multiple)
            {
                Focus();
            }
        }

        private void OnPopupIsOpenAnimationFinished(object sender, bool isOpen)
        {
            if (isOpen && IsFullScreen() && _searchBox != null)
            {
                _searchBox.Focus();
            }
        }

        #endregion
    }
}
