using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

// Xamarin
using Xamarin.Forms;

namespace XamKit
{
	public interface IItemPanMenuLayout
	{
        void PanUpdated(double horizontalPan);
	}

    public class ItemContainer : Layout<View>
    {
        private ItemMenuLayout m_rightMenuLayout = null;
        private ItemMenuLayout m_leftMenuLayout = null;
        // private CheckBox m_checkBox = null;

        private SizeRequest m_rightMenuSize = new SizeRequest();
        private SizeRequest m_leftMenuSize = new SizeRequest();
        // private SizeRequest m_checkBoxSize = new SizeRequest();

        private double m_horizontalPan = 0;
        private double m_horizontalPanStart = 0;

        private const uint c_animationRatio = 16;
        private const uint c_animationDuration = 250;

        private string C_panBackAnimationName = "panBackAnimation";

        #region Properties

        /// <summary>
        /// If CheckBox is visible
        /// </summary>
        public static readonly BindableProperty IsCheckBoxVisibleProperty =
            BindableProperty.Create("IsCheckBoxVisible", typeof(bool), typeof(ItemContainer), false);

        public bool IsCheckBoxVisible
        {
            get { return (bool)GetValue(IsCheckBoxVisibleProperty); }
            set { SetValue(IsCheckBoxVisibleProperty, value); }
        }

        #endregion

        #region Separator

        /// <summary>
        /// 
        /// </summary>
        public static readonly BindableProperty IsSeparatorVisibleProperty =
            BindableProperty.Create("IsSeparatorVisible", typeof(bool), typeof(ItemContainer), false);
        
        public bool IsSeparatorVisible
        {
            get { return (bool)GetValue(IsSeparatorVisibleProperty); }
            set { SetValue(IsSeparatorVisibleProperty, value); }
        }

        /// <summary>
        /// Separator line thickness
        /// </summary>
        public static readonly BindableProperty SeparatorThicknessProperty =
            BindableProperty.Create("SeparatorThickness", typeof(double), typeof(ItemContainer), 1.0);
        
        public double SeparatorThickness
        {
            get { return (double)GetValue(SeparatorThicknessProperty); }
            set { SetValue(SeparatorThicknessProperty, value); }
        }

        /// <summary>
        /// Separator line color
        /// </summary>
        public static readonly BindableProperty SeparatorColorProperty =
            BindableProperty.Create("SeparatorColor", typeof(Color), typeof(ItemContainer), Color.Black);
        
        public Color SeparatorColor
        {
            get { return (Color)GetValue(SeparatorColorProperty); }
            set { SetValue(SeparatorColorProperty, value); }
        }

        #endregion

        #region Left and right menu

        /// <summary>
        /// Is right menu open with pan
        /// </summary>
        public static readonly BindableProperty IsRightMenuOpenProperty =
            BindableProperty.Create("IsRightMenuOpen", typeof(bool), typeof(ItemContainer), false);

        public bool IsRightMenuOpen
        {
            get { return (bool)GetValue(IsRightMenuOpenProperty); }
            protected set { SetValue(IsRightMenuOpenProperty, value); }
        }

        /// <summary>
        /// Is left menu open with pan
        /// </summary>
        public static readonly BindableProperty IsLeftMenuOpenProperty =
            BindableProperty.Create("IsLeftMenuOpen", typeof(bool), typeof(ItemContainer), false);

        public bool IsLeftMenuOpen
        {
            get { return (bool)GetValue(IsLeftMenuOpenProperty); }
            protected set { SetValue(IsLeftMenuOpenProperty, value); }
        }

        /// <summary>
        /// Right menu items data source
        /// </summary>
        public static readonly BindableProperty RightMenuItemsSourceProperty =
            BindableProperty.Create("RightMenuItemsSource", typeof(IList), typeof(ItemContainer), null, propertyChanged: OnRightMenuItemsSourceChanged);

        private static void OnRightMenuItemsSourceChanged(BindableObject bindable, object oldValue, object newValue)
        {
            ItemContainer item = bindable as ItemContainer;

            if (oldValue != null && oldValue is INotifyCollectionChanged)
            {
                (oldValue as INotifyCollectionChanged).CollectionChanged -= item.OnRightMenuItemsSourceCollectionChangedInternal;
            }

            if (newValue != null && newValue is INotifyCollectionChanged)
            {
                (newValue as INotifyCollectionChanged).CollectionChanged += item.OnRightMenuItemsSourceCollectionChangedInternal;
            }

            item.OnRightMenuItemsSourceChanged(oldValue as IList, newValue as IList);
        }

        public IList RightMenuItemsSource
        {
            get { return (IList)GetValue(RightMenuItemsSourceProperty); }
            set { SetValue(RightMenuItemsSourceProperty, value); }
        }

        /// <summary>
        /// Left menu items data source
        /// </summary>
        public static readonly BindableProperty LeftMenuItemsSourceProperty =
            BindableProperty.Create("LeftMenuItemsSource", typeof(IList), typeof(ItemContainer), null, propertyChanged: OnLeftMenuItemsSourceChanged);

        private static void OnLeftMenuItemsSourceChanged(BindableObject bindable, object oldValue, object newValue)
        {
            ItemContainer item = bindable as ItemContainer;

            if (oldValue != null && oldValue is INotifyCollectionChanged)
            {
                (oldValue as INotifyCollectionChanged).CollectionChanged -= item.OnLeftMenuItemsSourceCollectionChangedInternal;
            }

            if (newValue != null && newValue is INotifyCollectionChanged)
            {
                (newValue as INotifyCollectionChanged).CollectionChanged += item.OnLeftMenuItemsSourceCollectionChangedInternal;
            }

            item.OnLeftMenuItemsSourceChanged(oldValue as IList, newValue as IList);
        }

        public IList LeftMenuItemsSource
        {
            get { return (IList)GetValue(LeftMenuItemsSourceProperty); }
            set { SetValue(LeftMenuItemsSourceProperty, value); }
        }

        /// <summary>
        /// Item datatemplate
        /// </summary>
        public static readonly BindableProperty ItemTemplateProperty =
            BindableProperty.Create("ItemTemplate", typeof(DataTemplate), typeof(ItemContainer), null);

        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }

        /// <summary>
        /// Items datatemplate selector
        /// </summary>
        public static readonly BindableProperty ItemTemplateSelectorProperty =
            BindableProperty.Create("ItemTemplateSelector", typeof(DataTemplateSelector), typeof(ItemContainer), null);

        public DataTemplateSelector ItemTemplateSelector
        {
            get { return (DataTemplateSelector)GetValue(ItemTemplateSelectorProperty); }
            set { SetValue(ItemTemplateSelectorProperty, value); }
        }

        /// <summary>
        /// Items container which content contains ItemTemplate view
        /// </summary>
        public static readonly BindableProperty ItemContainerTemplateProperty =
            BindableProperty.Create("ItemContainerTemplate", typeof(DataTemplate), typeof(ItemContainer), null);

        public DataTemplate ItemContainerTemplate
        {
            get { return (DataTemplate)GetValue(ItemContainerTemplateProperty); }
            set { SetValue(ItemContainerTemplateProperty, value); }
        }

        /// <summary>
        /// Items container datatemplate selector
        /// </summary>
        public static readonly BindableProperty ItemContainerTemplateSelectorProperty =
            BindableProperty.Create("ItemContainerTemplateSelector", typeof(DataTemplateSelector), typeof(ItemContainer), null);

        public DataTemplateSelector ItemContainerTemplateSelector
        {
            get { return (DataTemplateSelector)GetValue(ItemContainerTemplateSelectorProperty); }
            set { SetValue(ItemContainerTemplateSelectorProperty, value); }
        }

        /// <summary>
        /// Is panning
        /// </summary>
        public static readonly BindableProperty IsPanningProperty =
            BindableProperty.Create("IsPanning", typeof(bool), typeof(ItemContainer), false);

        public bool IsPanning
        {
            get { return (bool)GetValue(IsPanningProperty); }
            private set { SetValue(IsPanningProperty, value); }
        }

        #endregion

        public ItemContainer()
        {
            RightMenuItemsSource = new ObservableCollection<object>();
            LeftMenuItemsSource = new ObservableCollection<object>();

            PanGestureRecognizer pan = new PanGestureRecognizer();
            pan.PanUpdated += OnPanUpdated;
            GestureRecognizers.Add(pan);

            IsClippedToBounds = true;
        }

        /// <summary>
        /// Layout parts
        /// </summary>
        protected override void LayoutChildren(double x, double y, double width, double height)
        {
            // base.LayoutChildren(x, y, width, height);

            if (IsLeftMenuOpen && m_leftMenuLayout != null && m_leftMenuLayout.IsVisible)
            {
                LayoutChildIntoBoundingRegion(m_leftMenuLayout, new Rectangle(0, 0, m_leftMenuSize.Request.Width, height));
            }

            if (IsRightMenuOpen && m_rightMenuLayout != null && m_rightMenuLayout.IsVisible)
            {
                LayoutChildIntoBoundingRegion(m_rightMenuLayout, new Rectangle(width - m_rightMenuSize.Request.Width, 0, m_rightMenuSize.Request.Width, height));
            }
        }

        /// <summary>
        /// Measure parts
        /// </summary>
        protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
        {
            SizeRequest s = base.OnMeasure(widthConstraint, heightConstraint);

            MeasureMenu(widthConstraint, heightConstraint);

            return s;
        }

        /// <summary>
        /// Measure left and right menu
        /// </summary>
        private void MeasureMenu(double width, double height)
        {
            if (m_leftMenuLayout != null)
            {
                m_leftMenuSize = m_leftMenuLayout.Measure(width, height, MeasureFlags.IncludeMargins);
            }

            if (m_rightMenuLayout != null)
            {
                m_rightMenuSize = m_rightMenuLayout.Measure(width, height, MeasureFlags.IncludeMargins);
            }
        }

        /// <summary>
        /// When item panned
        /// </summary>
		private void OnPanUpdated(object sender, PanUpdatedEventArgs e)
        {
            if (e.StatusType == GestureStatus.Started)
            {
                IsPanning = true;

                MeasureMenu(Width, Height);

                m_horizontalPanStart = m_horizontalPan;
            }
            else if (e.StatusType == GestureStatus.Running)
            {
                double actualPan = 0;

                if (e.TotalX > 0 && IsRightMenuOpen)
                {
                    if (e.TotalX + m_horizontalPanStart > 0)
                    {
                        double bounceX = e.TotalX + m_horizontalPanStart;
                        actualPan = bounceX / Math.Sqrt(Math.Abs(bounceX / 10));
                        (m_rightMenuLayout as IItemPanMenuLayout).PanUpdated(0);
                    }
                    else
                    {
                        actualPan = e.TotalX + m_horizontalPanStart;
                        (m_rightMenuLayout as IItemPanMenuLayout).PanUpdated(actualPan);
                    }

                    UpdateTranslation(actualPan);
                }
                else if (e.TotalX < 0 && IsLeftMenuOpen)
                {
                    if (e.TotalX + m_horizontalPanStart < 0)
                    {
                        double bounceX = e.TotalX + m_horizontalPanStart;
                        actualPan = bounceX / Math.Sqrt(Math.Abs(bounceX / 10));
                        (m_leftMenuLayout as IItemPanMenuLayout).PanUpdated(0);
                    }
                    else
                    {
                        actualPan = e.TotalX + m_horizontalPanStart;
                        (m_leftMenuLayout as IItemPanMenuLayout).PanUpdated(actualPan);
                    }

                    UpdateTranslation(actualPan);
                }
                else if (e.TotalX > 0 && LeftMenuItemsSource.Count > 0)
                {
                    IsLeftMenuOpen = true;

                    if (m_leftMenuSize.Request.Width < e.TotalX)
                    {
                        double bounceX = e.TotalX + m_horizontalPanStart - m_leftMenuSize.Request.Width;
                        actualPan = m_leftMenuSize.Request.Width + (bounceX / Math.Sqrt(Math.Abs(bounceX / 10)));
                    }
                    else
                    {
                        actualPan = e.TotalX + m_horizontalPanStart;
                    }

                    if (m_leftMenuLayout.Width < 0)
                    {
                        LayoutChildIntoBoundingRegion(m_leftMenuLayout, new Rectangle(0, 0, m_leftMenuSize.Request.Width, Height));
                    }

                    (m_leftMenuLayout as IItemPanMenuLayout).PanUpdated(actualPan);
                    UpdateTranslation(actualPan);
                }
                else if (e.TotalX < 0 && RightMenuItemsSource.Count > 0)
                {
                    IsRightMenuOpen = true;

                    if (m_rightMenuSize.Request.Width < Math.Abs(e.TotalX + m_horizontalPanStart))
                    {
                        double bounceX = e.TotalX + m_horizontalPanStart + m_rightMenuSize.Request.Width;
                        actualPan = -m_rightMenuSize.Request.Width + (bounceX / Math.Sqrt(Math.Abs(bounceX / 10)));
                    }
                    else
                    {
                        actualPan = e.TotalX + m_horizontalPanStart;
                    }

                    if (m_rightMenuLayout.Width < 0)
                    {
                        LayoutChildIntoBoundingRegion(m_rightMenuLayout, new Rectangle(Width - m_rightMenuSize.Request.Width, 0, m_rightMenuSize.Request.Width, Height));
                    }

                    (m_rightMenuLayout as IItemPanMenuLayout).PanUpdated(actualPan);
                    UpdateTranslation(actualPan);
                }

                m_horizontalPan = actualPan;
            }
            else
            {
                Animation anim = null;

                if (IsRightMenuOpen && m_horizontalPan < -m_rightMenuSize.Request.Width / 2)
                {
                    anim = new Animation((double d) =>
                    {
                        m_horizontalPan = d;
                        UpdateTranslation(d);
                        (m_rightMenuLayout as IItemPanMenuLayout).PanUpdated(d);

                    }, m_horizontalPan, -m_rightMenuSize.Request.Width);

                    IsRightMenuOpen = true;

                    anim.Commit(this, C_panBackAnimationName, c_animationRatio, c_animationDuration, Easing.SpringOut);
                }
                else if (IsRightMenuOpen && m_horizontalPan > -m_rightMenuSize.Request.Width / 2)
                {
                    double previousMenuPan = m_horizontalPan;
                    anim = new Animation((double d) =>
                    {
                        m_horizontalPan = d;
                        UpdateTranslation(d);

                        if (d > previousMenuPan)
                        {
                            (m_rightMenuLayout as IItemPanMenuLayout).PanUpdated(Math.Min(0, d));
                            previousMenuPan = d;
                        }

                    }, m_horizontalPan, 0);

                    IsRightMenuOpen = false;

                    anim.Commit(this, C_panBackAnimationName, c_animationRatio, c_animationDuration, Easing.SpringOut /*AnimationUtils.EaseOutQuint*/);
                }
                else if (IsLeftMenuOpen && m_horizontalPan >= m_leftMenuSize.Request.Width / 2)
                {
                    anim = new Animation((double d) =>
                    {
                        m_horizontalPan = d;
                        UpdateTranslation(d);
                        (m_leftMenuLayout as IItemPanMenuLayout).PanUpdated(d);

                    }, m_horizontalPan, m_leftMenuSize.Request.Width);

                    IsLeftMenuOpen = true;

                    anim.Commit(this, C_panBackAnimationName, c_animationRatio, c_animationDuration, Easing.SpringOut);
                }
                else if (IsLeftMenuOpen && m_horizontalPan < m_leftMenuSize.Request.Width / 2)
                {
                    double previousMenuPan = m_horizontalPan;
                    anim = new Animation((double d) =>
                    {
                        m_horizontalPan = d;
                        UpdateTranslation(d);

                        if (d < previousMenuPan)
                        {
                            (m_leftMenuLayout as IItemPanMenuLayout).PanUpdated(Math.Max(0, d));
                            previousMenuPan = d;
                        }

                    }, m_horizontalPan, 0);

                    IsLeftMenuOpen = false;

                    anim.Commit(this, C_panBackAnimationName, c_animationRatio, c_animationDuration, Easing.SpringOut/*AnimationUtils.EaseOutQuint*/);
                }

                IsPanning = false;
            }
        }

        /// <summary>
        /// Update translation X to each component
        /// </summary>
        private void UpdateTranslation(double translationX)
        {
            /*if (Content != null)
            {
                Content.TranslationX = translationX;
            }
            else
            {
                InvalidatePaint();
            }*/
        }

        /// <summary>
        /// Called when all right menu items changed
        /// </summary>
        private void OnRightMenuItemsSourceChanged(IList oldSource, IList newSource)
        {
            if (m_rightMenuLayout == null)
            {
                m_rightMenuLayout = new ItemMenuLayout();
                Children.Add(m_rightMenuLayout);
            }

            if (oldSource != null && oldSource.Count > 0)
            {
                OnMenuItemsSourceCollectionChangedInternal(m_rightMenuLayout, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset, null));
            }

            if (newSource != null && newSource.Count > 0)
            {
                OnMenuItemsSourceCollectionChangedInternal(m_rightMenuLayout, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newSource));
            }
        }

        /// <summary>
        /// Called when all right menu items collection changed
        /// </summary>
        private void OnRightMenuItemsSourceCollectionChangedInternal(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnMenuItemsSourceCollectionChangedInternal(m_rightMenuLayout, e);
        }

        /// <summary>
        /// Called when all left menu items changed
        /// </summary>
        private void OnLeftMenuItemsSourceChanged(IList oldSource, IList newSource)
        {
            if (m_leftMenuLayout == null)
            {
                m_leftMenuLayout = new ItemMenuLayout();
                Children.Add(m_leftMenuLayout);
            }

            if (oldSource != null && oldSource.Count > 0)
            {
                OnMenuItemsSourceCollectionChangedInternal(m_leftMenuLayout, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset, null));
            }

            if (newSource != null && newSource.Count > 0)
            {
                OnMenuItemsSourceCollectionChangedInternal(m_leftMenuLayout, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newSource));
            }
        }

        /// <summary>
        /// Called when all right menu items collection changed
        /// </summary>
        private void OnLeftMenuItemsSourceCollectionChangedInternal(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnMenuItemsSourceCollectionChangedInternal(m_leftMenuLayout, e);
        }

        /// <summary>
        /// Update layout menu items
        /// </summary>
        private void OnMenuItemsSourceCollectionChangedInternal(ItemMenuLayout layout, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                AddItems(e.NewItems, e.NewStartingIndex, layout);
            }
            else if (e.Action == NotifyCollectionChangedAction.Move)
            {
                MoveItems(e.OldItems, e.OldStartingIndex, e.NewStartingIndex, layout);
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                RemoveItems(e.OldItems, e.OldStartingIndex, layout);
            }
            else if (e.Action == NotifyCollectionChangedAction.Replace)
            {
                throw new NotImplementedException("ItemsView: NotifyCollectionChangedAction.Replace not implemented!");
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                RemoveItems(e.OldItems, e.OldStartingIndex, layout);
            }
            else
            {
                throw new Exception("ItemsView: Invalid action!");
            }
        }

        private void AddItems(IList newItems, int newStartIndex, Layout<View> layout)
        {
            int i = (newStartIndex == -1) ? 0 : newStartIndex;

            foreach (object item in newItems)
            {
                View itemContainer = null;

                if (IsContainerGenerationNeeded(item))
                {
                    itemContainer = CreateItemContainer(item);
                    CreateItem(itemContainer, item);
                }
                else
                {
                    itemContainer = item as View;
                }

                layout.Children.Insert(i, itemContainer);
                i++;
            }
        }

        private void MoveItems(IList items, int oldStartIndex, int newStartIndex, Layout<View> layout)
        {
            if (items != null)
            {
                List<View> l = new List<View>();

                int i = oldStartIndex;
                foreach (object item in items)
                {
                    View v = layout.Children[i];
                    l.Add(v);

                    layout.Children.RemoveAt(i);
                    i++;
                }

                i = newStartIndex;
                foreach (View item in l)
                {
                    layout.Children.Insert(i, item);
                    i++;
                }
            }
        }

        private void RemoveItems(IList items, int oldStartIndex, Layout<View> layout)
        {
            if (items != null)
            {
                int i = oldStartIndex;
                foreach (object item in items)
                {
                    View itemToRemove = layout.Children.ElementAt(i);
                    layout.Children.Remove(itemToRemove);
                }
            }
            else
            {
                layout.Children.Clear();
            }
        }

        /// <summary>
        /// Create menu item container
        /// </summary>
		protected virtual View CreateItemContainer(object model)
        {
            View item = null;
            if (ItemContainerTemplateSelector != null)
            {
                DataTemplate containerTemplate = ItemContainerTemplateSelector.SelectTemplate(model, null) as DataTemplate;
                item = containerTemplate.CreateContent() as View;
            }
            else if (ItemContainerTemplate != null)
            {
                item = ItemContainerTemplate.CreateContent() as View;
            }
            else
            {
                item = new ContentView();
            }

            if (item == null)
            {
                throw new Exception("ItemContainerTemplate is not subclass of ContentView");
            }

            item.BindingContext = model;
            return item;
        }

        /// <summary>
        /// Add correct item template to item container content
        /// </summary>
        protected virtual void CreateItem(View itemContainer, object model)
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
        /// Is container generation needed
        /// </summary>
        protected virtual bool IsContainerGenerationNeeded(object item)
        {
            if (item is View)
            {
                return false;
            }
            return true;
        }
    }
}
