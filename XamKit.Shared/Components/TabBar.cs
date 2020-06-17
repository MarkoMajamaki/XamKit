using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

// Xamarin
using Xamarin.Forms;

namespace XamKit
{
    /// <summary>
    /// How header buttons are layouted
    /// </summary>
    public enum TabBarLayoutOptions { Start, Fill, Center }

    /// <summary>
    /// Tab header
    /// </summary>
	public class TabBar : Layout<View>, ITabBar
	{
        private class ChildInfo
        {
            public View Child { get; private set; }
            public SizeRequest Size { get; set; }
            public int Index { get; private set; }
            public ChildInfo(View child, int index)
            {
                Child = child;
                Index = index;

                Size = new SizeRequest(new Size(0, 0), new Size(0, 0));
            }
        }

        private List<ChildInfo> _children = null;

        private IToggable _currentToggledButton = null;

		private BoxView _bottomLine = null;
		private BoxView _focusLine = null;
        private GradientView _bottomShadow = null;

        private PanChangedArgs _previousPanChangedArgs = null;

        #region Properties

        /// <summary>
        /// TabView source
        /// </summary>
        public static readonly BindableProperty SourceProperty =
			BindableProperty.Create("Source", typeof(TabView), typeof(TabBar), null, propertyChanged: OnSourceChanged);

		private static void OnSourceChanged(BindableObject bindable, object oldValue, object newValue)
		{
			(bindable as TabBar).OnSourceChanged(oldValue as TabView, newValue as TabView);
		}

		public TabView Source
		{
			get { return (TabView)GetValue(SourceProperty); }
			set { SetValue(SourceProperty, value); }
		}

		/// <summary>
		/// How items are layouted
		/// </summary>
		public static readonly BindableProperty ItemsLayoutOptionsProperty =
            BindableProperty.Create("ItemsLayoutOptions", typeof(TabBarLayoutOptions), typeof(TabBar), TabBarLayoutOptions.Start);
        
        public TabBarLayoutOptions ItemsLayoutOptions
		{
			get { return (TabBarLayoutOptions)GetValue(ItemsLayoutOptionsProperty); }
			set { SetValue(ItemsLayoutOptionsProperty, value); }
		}

        /// <summary>
        /// Items container datatemplate selector
        /// </summary>
        public static readonly BindableProperty ItemTemplateSelectorProperty =
            BindableProperty.Create("ItemTemplateSelector", typeof(DataTemplateSelector), typeof(TabBar), null);

        public DataTemplateSelector ItemTemplateSelector
        {
            get { return (DataTemplateSelector)GetValue(ItemTemplateSelectorProperty); }
            set { SetValue(ItemTemplateSelectorProperty, value); }
        }

        /// <summary>
        /// Items container which content contains ItemTemplate view
        /// </summary>
        public static readonly BindableProperty ItemTemplateProperty =
            BindableProperty.Create("ItemTemplate", typeof(DataTemplate), typeof(TabBar), null);

        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }

        /// <summary>
        /// Min height if enought available height
        /// </summary>
        public static readonly BindableProperty MinHeightRequestProperty =
            BindableProperty.Create("MinHeightRequest", typeof(double), typeof(TabBar), 0.0);

        public double MinHeightRequest
        {
            get { return (double)GetValue(MinHeightRequestProperty); }
            set { SetValue(MinHeightRequestProperty, value); }
        }

        #endregion

        #region Colors

        /// <summary>
        /// Horizontal line color
        /// </summary>
        public static readonly BindableProperty BottomLineColorProperty =
            BindableProperty.Create("BottomLineColor", typeof(Color), typeof(TabBar), Color.Transparent, propertyChanged: OnBottomLineColorChanged);

        private static void OnBottomLineColorChanged(BindableObject bindable, object oldValue, object newValue)
        {
            (bindable as TabBar).OnBottomLineColorChanged((Color)oldValue, (Color)newValue);
        }

        public Color BottomLineColor
		{
			get { return (Color)GetValue(BottomLineColorProperty); }
			set { SetValue(BottomLineColorProperty, value); }
		}


        public static readonly BindableProperty BottomLineHeightRequestProperty =
            BindableProperty.Create("BottomLineHeightRequest", typeof(double), typeof(TabBar), 0.0, propertyChanged: OnBottomLineHeightRequestChanged);

        private static void OnBottomLineHeightRequestChanged(BindableObject bindable, object oldValue, object newValue)
        {
            (bindable as TabBar).OnBottomLineHeightRequestChanged((double)oldValue, (double)newValue);
		}

        public double BottomLineHeightRequest
		{
			get { return (double)GetValue(BottomLineHeightRequestProperty); }
			set { SetValue(BottomLineHeightRequestProperty, value); }
		}

		/// <summary>
		/// Horizontal tab item focus line color
		/// </summary>
		public static readonly BindableProperty FocusLineColorProperty =
            BindableProperty.Create("FocusLineColor", typeof(Color), typeof(TabBar), Color.Transparent, propertyChanged: OnFocusLineColorChanged);

        private static void OnFocusLineColorChanged(BindableObject bindable, object oldValue, object newValue)
        {
			(bindable as TabBar).OnFocusLineColorChanged((Color)oldValue, (Color)newValue);
		}

        public Color FocusLineColor
		{
			get { return (Color)GetValue(FocusLineColorProperty); }
			set { SetValue(FocusLineColorProperty, value); }
		}

        public static readonly BindableProperty FocusLineHeightRequestProperty =
            BindableProperty.Create("FocusLineHeightRequest", typeof(double), typeof(TabBar), 0.0, propertyChanged: OnFocusLineHeightRequestChanged);

        private static void OnFocusLineHeightRequestChanged(BindableObject bindable, object oldValue, object newValue)
        {
            (bindable as TabBar).OnFocusLineHeightRequestChanged((double)oldValue, (double)newValue);
		}

        public double FocusLineHeightRequest
		{
			get { return (double)GetValue(FocusLineHeightRequestProperty); }
			set { SetValue(FocusLineHeightRequestProperty, value); }
		}

        /// <summary>
        /// Bottom shadow lenght
        /// </summary>
        public static readonly BindableProperty BottomShadowLenghtProperty =
            BindableProperty.Create("BottomShadowLenght", typeof(double), typeof(TabBar), 0.0, propertyChanged: OnBottomShadowLenghtChanged);

        private static void OnBottomShadowLenghtChanged(BindableObject bindable, object oldValue, object newValue)
        {
            (bindable as TabBar).OnBottomShadowLenghtChanged((double)oldValue, (double)newValue);
        }

        public double BottomShadowLenght
        {
            get { return (double)GetValue(BottomShadowLenghtProperty); }
            set { SetValue(BottomShadowLenghtProperty, value); }
        }

        #endregion

        public TabBar()
		{
            _children = new List<ChildInfo>();

            // Line to separate header and view

            _bottomLine = new BoxView();
            _bottomLine.HorizontalOptions = LayoutOptions.Fill;
            _bottomLine.VerticalOptions = LayoutOptions.End;
            _bottomLine.Color = BottomLineColor;

            // Line to show focused item

            _focusLine = new BoxView();
            _focusLine.VerticalOptions = LayoutOptions.End;
            _focusLine.HeightRequest = FocusLineHeightRequest;
            _focusLine.Color = FocusLineColor;

            _bottomShadow = new GradientView();
            _bottomShadow.StartColor = Color.Black.MultiplyAlpha(0.1);
            _bottomShadow.EndColor = Color.Transparent;
            _bottomShadow.InputTransparent = true;

            Children.Add(_bottomShadow);
            Children.Add(_bottomLine);
            Children.Add(_focusLine);
		}

        #region Measure and layout

        /// <summary>
        /// Measure children
        /// </summary>
        protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
        {
            return MeasureChildren(widthConstraint, heightConstraint);
        }

        /// <summary>
        /// Layout children
        /// </summary>
        protected override void LayoutChildren(double x, double y, double width, double height)
        {
            // Measure all children and save result
            SizeRequest childrenAllocatedSize = MeasureChildren(width, height);

            // Do actual layout
            foreach (ChildInfo childInfo in _children)
            {
                Rectangle location = new Rectangle(0, 0, childInfo.Size.Request.Width, childInfo.Size.Request.Height);

                if (ItemsLayoutOptions == TabBarLayoutOptions.Fill)
                {
                    location.Width = width / _children.Count;
                }

                if (childInfo.Child.VerticalOptions.Alignment == LayoutAlignment.Fill && double.IsNaN(height) == false && double.IsInfinity(height) == false)
                {
                    location.Height = height - BottomLineHeightRequest;
                }

                if (childInfo.Child.Bounds != location)
                {
                    LayoutChildIntoBoundingRegion(childInfo.Child, location);
                }
            }

            // Do TranslationX changes
            UpdateTranslation(width, height);

            if (_currentToggledButton != null)
            {
                View v = _currentToggledButton as View;
                LayoutChildIntoBoundingRegion(_focusLine, new Rectangle(v.X + v.TranslationX, height - FocusLineHeightRequest, v.Width, FocusLineHeightRequest));
			}

            LayoutChildIntoBoundingRegion(_bottomLine, new Rectangle(0, height - BottomLineHeightRequest, width, BottomLineHeightRequest));

            if (BottomShadowLenght > 0)
            {
                LayoutChildIntoBoundingRegion(_bottomShadow, new Rectangle(0, height, width, BottomShadowLenght));
            }
        }

        /// <summary>
        /// Update children translations
        /// </summary>
        /// <param name="width">Available width</param>
        /// <param name="height">Available height</param>
        private void UpdateTranslation(double width, double height)
        {
            if (ItemsLayoutOptions == TabBarLayoutOptions.Start)
            {
                double xOffset = 1;
                foreach (ChildInfo childInfo in _children)
                {
                    childInfo.Child.TranslationX = xOffset;
                    xOffset += childInfo.Size.Request.Width;
                }
            }
            else if (ItemsLayoutOptions == TabBarLayoutOptions.Fill)
            {
                double oneItemWidth = width / _children.Count;
                double xOffset = 0;

                foreach (ChildInfo childInfo in _children)
                {
                    childInfo.Child.TranslationX = xOffset;
                    xOffset += oneItemWidth;
                }
            }
            else
            {
                double xOffset = (width - _children.Sum(c => c.Size.Request.Width)) / 2;
                foreach (ChildInfo childInfo in _children)
                {
                    childInfo.Child.TranslationX = xOffset;
                    xOffset += childInfo.Size.Request.Width;
                }
            }

            if (_previousPanChangedArgs != null)
            {
                OnPanChanged(this, _previousPanChangedArgs);
            }
        }

        /// <summary>
        /// Do actual children measure
        /// </summary>
        private SizeRequest MeasureChildren(double width, double height)
        {
            Size childrenTotalSize = new Size();

            foreach (ChildInfo childInfo in _children)
            {
                childInfo.Size = childInfo.Child.Measure(width, height, MeasureFlags.IncludeMargins);

                childrenTotalSize.Width += childInfo.Size.Request.Width;
                childrenTotalSize.Height = Math.Max(childInfo.Size.Request.Height, childrenTotalSize.Height);
            }

            if (ItemsLayoutOptions == TabBarLayoutOptions.Fill)
            {
                return new SizeRequest(new Size(width, Math.Max(MinHeightRequest, childrenTotalSize.Height)), 
                                       new Size(width, Math.Max(MinHeightRequest, childrenTotalSize.Height)));
            }
            else
            {
                return new SizeRequest(new Size(childrenTotalSize.Width, Math.Max(MinHeightRequest, childrenTotalSize.Height)),
                                       new Size(childrenTotalSize.Width, Math.Max(MinHeightRequest, childrenTotalSize.Height)));
            }
        }

        #endregion

        #region Items

        /// <summary>
        /// Create item container based on ItemTemplate or ItemTemplateSelector
        /// </summary>
        private View CreateItemView(object model)
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

            item.BindingContext = model;
            return item;
        }

        /// <summary>
        /// Create tab header item container
        /// </summary>
        private View CreateItemFromTabView(TabItem tabItem)
		{
			if (tabItem.HeaderTemplate == null && tabItem.Header == null)
			{
				throw new Exception("TabBar.CreateItemContainer: TabItem HeaderTemplate is null and Header is null or not type of View");
			}

            IToggable item = null;

            // Create container from TabItem.HeaderTemplate
            if (tabItem.Header != null && tabItem.Header is View)
            {
                item = tabItem.Header as IToggable;
            }
            else
            {
                View header = tabItem.HeaderTemplate.CreateContent() as View;

                Binding bind = new Binding("Header");
                bind.Source = tabItem;
                header.SetBinding(View.BindingContextProperty, bind);

                item = header as IToggable;
            }

            if (item == null)
			{
				throw new Exception("TabBar.CreateItem: ItemTemplate is not subclass of ToggleButton");
			}

			item.IsToggledChanged -= OnIsToggledChanged;
			item.IsToggledChanged += OnIsToggledChanged;

			return item as View;
		}

		/// <summary>
		/// Called when TabView source changes
		/// </summary>
		private void OnSourceChanged(TabView oldTabView, TabView newTabView)
		{
			if (oldTabView != null)
			{
				oldTabView.CurrentItemIndexChanged -= OnCurrentItemIndexChanged;
				oldTabView.PanChanged -= OnPanChanged;

                if (oldTabView.ItemsSource != null && oldTabView.ItemsSource is INotifyCollectionChanged)
                {
                    (oldTabView.ItemsSource as INotifyCollectionChanged).CollectionChanged -= OnItemsSourceCollectionChanged;
                }
			}

			if (newTabView != null)
			{
				newTabView.CurrentItemIndexChanged += OnCurrentItemIndexChanged;
				newTabView.PanChanged += OnPanChanged;

                if (newTabView.ItemsSource != null && newTabView.ItemsSource is INotifyCollectionChanged)
                {
                    (newTabView.ItemsSource as INotifyCollectionChanged).CollectionChanged += OnItemsSourceCollectionChanged;
                }
			}

            OnItemsSourceChanged(oldTabView != null ? oldTabView.ItemsSource : null, newTabView != null ? newTabView.ItemsSource : null);

            OnCurrentItemIndexChanged(newTabView, newTabView.CurrentItemIndex);

        }

        /// <summary>
        /// 
        /// </summary>
        private void OnItemsSourceChanged(IList oldSource, IList newSource)
        {
            if (oldSource != null && oldSource.Count > 0)
            {
                OnItemsSourceCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset, null));
            }

            if (newSource != null && newSource.Count > 0)
            {
                OnItemsSourceCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newSource));
            }
        }

        /// <summary>
        /// Called when items source collection changes (added or removed tabitems)
        /// </summary>
        private void OnItemsSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                AddItems(e.NewItems, e.NewStartingIndex);
            }
            else if (e.Action == NotifyCollectionChangedAction.Move)
            {
                MoveItems(e.OldItems, e.OldStartingIndex, e.NewStartingIndex);
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                RemoveItems(e.OldItems, e.OldStartingIndex);
            }
            else if (e.Action == NotifyCollectionChangedAction.Replace)
            {
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                RemoveItems(e.OldItems, e.OldStartingIndex);
            }
            else
            {
                throw new Exception("TabBar: Invalid action!");
            }
        }

        private void AddItems(IList newItems, int newStartIndex)
        {
            newStartIndex = (newStartIndex == -1) ? 0 : newStartIndex;

            foreach (object item in newItems)
            {
                View itemContainer = null;

                if (item is TabItem)
                {
                    itemContainer = CreateItemFromTabView(item as TabItem);
                }
                else
                {
                    itemContainer = CreateItemView(item);
                }
                
                ChildInfo childInfo = new ChildInfo(itemContainer, newStartIndex);
                _children.Insert(newStartIndex, childInfo);

                Children.Insert(Children.IndexOf(_bottomLine), itemContainer);

                newStartIndex++;
            }
        }

        private void RemoveItems(IList items, int oldStartIndex)
        {
            if (items != null)
            {
                foreach (object item in items)
                {
                    View itemToRemove = _children[oldStartIndex].Child;
                    _children.RemoveAt(oldStartIndex);
                    Children.Remove(itemToRemove);
                }
            }
            else
            {
                foreach (ChildInfo item in _children)
                {
                    Children.Remove(item.Child);
                }
                _children.Clear();
            }
        }

        private void MoveItems(IEnumerable items, int oldStartIndex, int newStartIndex)
        {
            if (items != null)
            {
                List<ChildInfo> itemToMove = new List<ChildInfo>();

                // TODO: Children?

                int i = oldStartIndex;
                foreach (object item in items)
                {
                    ChildInfo v = _children[i];
                    _children.RemoveAt(i);
                    itemToMove.Add(v);
                    i++;
                }

                i = newStartIndex;
                foreach (ChildInfo item in itemToMove)
                {
                    _children.Insert(i, item);
                    i++;
                }
            }
        }

        #endregion

        #region Focus

        /// <summary>
        /// TabView is panned. Update horizontal line location during pan.
        /// </summary>
        private void OnPanChanged(object sender, PanChangedArgs args)
        {
            _previousPanChangedArgs = args;

            View leftChild = null;
            View rightChild = null;

            int leftIndex = (int)Math.Floor(args.Location);
            leftIndex = leftIndex < 0 ? _children.Count - 1 : leftIndex;

            int rightIndex = (int)Math.Ceiling(args.Location);
            rightIndex = rightIndex > _children.Count - 1 || leftIndex == _children.Count - 1 ? 0 : rightIndex;

            leftChild = _children[leftIndex].Child;
            rightChild = _children[rightIndex].Child;

            double panWidth = leftChild.Width + ((rightChild.Width - leftChild.Width) * (args.Location - leftIndex));
            double panXDelta = ((rightChild.X + rightChild.TranslationX) - (leftChild.X + leftChild.TranslationX)) * (args.Location - leftIndex);

            double panX = leftChild.X + leftChild.TranslationX + panXDelta;

            ChildInfo lastChild = _children.Last();
            ChildInfo firstChild = _children.First();

            if ((panX + (panWidth / 2) > Width / 2 && lastChild.Size.Request.Width + lastChild.Child.TranslationX > Width) || 
                (panX + (panWidth / 2) <= Width / 2 && firstChild.Child.TranslationX < 0))
            {
                double newPanX = (Width - panWidth) / 2;

                double childrenPanDelta = panX - newPanX;

                View firstIndexChild = _children.First(c => c.Index == 0).Child;
                if (firstIndexChild.TranslationX - childrenPanDelta > 0)
                {
                    childrenPanDelta += (firstIndexChild.TranslationX - childrenPanDelta);
                }

                View lastIndexChild = _children.First(c => c.Index == _children.Count - 1).Child;
                if (lastIndexChild.TranslationX + lastIndexChild.Bounds.Width - childrenPanDelta < Width)
                {
                    childrenPanDelta = -(Width - (lastIndexChild.TranslationX + lastIndexChild.Bounds.Width));
                }
                    
                foreach (ChildInfo child in _children)
                {
                    child.Child.TranslationX -= childrenPanDelta;
                }

                panX = newPanX;
            }

            LayoutChildIntoBoundingRegion(_focusLine, new Rectangle(panX, Height - FocusLineHeightRequest, panWidth, FocusLineHeightRequest));
        }

        /// <summary>
        /// Event handler when focused index changes in Source. Update togglebutton.
        /// </summary>
        private void OnCurrentItemIndexChanged(object sender, int index)
		{
            IToggable focusedToggleButton = _children[index].Child as IToggable;

            if (_currentToggledButton != null && _currentToggledButton != focusedToggleButton)
            {
                UnToggle(_currentToggledButton);
            }

            if (focusedToggleButton != _currentToggledButton)
            {
                _currentToggledButton = focusedToggleButton;
                Toggle(focusedToggleButton);
            }
		}

		/// <summary>
		/// Tab toggled changed
		/// </summary>
		private void OnIsToggledChanged(object sender, bool isToggled)
		{
			// Set focus if toggled
			if (isToggled)
			{
                Source.CurrentItemIndex = _children.First(i => i.Child == sender).Index;
			}
            // If untoggled same button which was toggled, keep it toggled.
            else if (Source != null)
            {
                ToggleButton untoggledButton = sender as ToggleButton;

                int senderIndex = _children.IndexOf(_children.Find(i => i.Child == sender));
                if (senderIndex == Source.CurrentItemIndex)
                {
                    Toggle(untoggledButton);
                }
            }
		}

        /// <summary>
        /// Untoggle without event raise
        /// </summary>
        private void UnToggle(IToggable toggleButton)
		{
			if (toggleButton.IsToggled)
			{
				toggleButton.IsToggledChanged -= OnIsToggledChanged;
				toggleButton.IsToggled = false;
				toggleButton.IsToggledChanged += OnIsToggledChanged;
			}
		}

		/// <summary>
		/// Toggle without event raise
		/// </summary>
        private void Toggle(IToggable toggleButton)
		{
			if (toggleButton.IsToggled == false)
			{
				toggleButton.IsToggledChanged -= OnIsToggledChanged;
				toggleButton.IsToggled = true;
				toggleButton.IsToggledChanged += OnIsToggledChanged;
			}
		}

		#endregion

		private void OnBottomLineColorChanged(Color oldValue, Color newValue)
		{
            if (_bottomLine != null)
            {
				_bottomLine.Color = newValue;
			}
		}

        private void OnBottomLineHeightRequestChanged(double oldValue, double newValue)
        {
            if (_bottomLine != null)
            {
                _bottomLine.HeightRequest = BottomLineHeightRequest;
            }
        }

		private void OnFocusLineColorChanged(Color oldValue, Color newValue)
		{
            if (_focusLine != null)
            {
                _focusLine.Color = newValue;
            }
		}

		private void OnFocusLineHeightRequestChanged(double oldValue, double newValue)
		{
            if (_focusLine != null)
            {
                _focusLine.HeightRequest = newValue;
            }
		}


        private void OnBottomShadowLenghtChanged(double oldValue, double newValue)
        {
            if (_bottomShadow != null)
            {
                _bottomShadow.HeightRequest = newValue;

                if (newValue == 0)
                {
                    _bottomShadow.InputTransparent = true;
                    _bottomShadow.Opacity = 0;
                }
                else
                {
                    _bottomShadow.InputTransparent = false;
                    _bottomShadow.Opacity = 1;
                }
            }
        }
    }
}
