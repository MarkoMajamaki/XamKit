using System;
using System.Collections;
using System.Collections.ObjectModel;
using SkiaSharp;
using SkiaSharp.Views.Forms;

// Xamarin
using Xamarin.Forms;

namespace XamKit
{
    [ContentProperty("ItemsSource")]
    public class MenuButton : PopupButtonBase
	{
        private SkiaSharp.Extended.Svg.SKSvg m_subMenuIconSvg = null;

        private CheckBox _checkBox = null;
        private MenuItemsView _menuItemsView = null;
        private Size _checkBoxSize = new Size();
        private Size _subMenuIconSize = new Size();

        /// <summary>
        /// Event to raise when any subitem tapped
        /// </summary>
        public event EventHandler SubMenuItemTapped;

        #region Properties

        /// <summary>
        /// Menu items
        /// </summary>
        public static readonly BindableProperty ItemsSourceProperty =
			BindableProperty.Create("ItemsSource", typeof(IList), typeof(MenuButton), null);

		public IList ItemsSource
		{
			get { return (IList)GetValue(ItemsSourceProperty); }
			set { SetValue(ItemsSourceProperty, value); }
		}

        /// <summary>
        /// Item DataTemplate
        /// </summary>
        public static readonly BindableProperty ItemTemplateProperty =
            BindableProperty.Create("ItemTemplate", typeof(DataTemplate), typeof(MenuButton), null);

        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }

        /// <summary>
        /// Items DataTemplate selector
        /// </summary>
        public static readonly BindableProperty ItemTemplateSelectorProperty =
            BindableProperty.Create("ItemTemplateSelector", typeof(DataTemplateSelector), typeof(MenuButton), null);

        public DataTemplateSelector ItemTemplateSelector
        {
            get { return (DataTemplateSelector)GetValue(ItemTemplateSelectorProperty); }
            set { SetValue(ItemTemplateSelectorProperty, value); }
        }

        /// <summary>
        /// Items panel template. First child should be type of Panel
        /// </summary>
        public static readonly BindableProperty ItemsLayoutTemplateProperty =
            BindableProperty.Create("ItemsLayoutTemplate", typeof(DataTemplate), typeof(MenuButton), null);
        
        public DataTemplate ItemsLayoutTemplate
        {
            get { return (DataTemplate)GetValue(ItemsLayoutTemplateProperty); }
            set { SetValue(ItemsLayoutTemplateProperty, value); }
        }

        /// <summary>
        /// Is menu item toggable. Not if has submenu items.
        /// </summary>
        public static readonly BindableProperty IsToggableProperty =
            BindableProperty.Create("IsToggable", typeof(bool), typeof(MenuButton), false);

        public bool IsToggable
        {
            get { return (bool)GetValue(IsToggableProperty); }
            set { SetValue(IsToggableProperty, value); }
        }

        /// <summary>
        /// Is checkbox visible
        /// </summary>
        public static readonly BindableProperty IsCheckBoxVisibleProperty =
            BindableProperty.Create("IsCheckBoxVisible", typeof(bool), typeof(MenuButton), false);

        public bool IsCheckBoxVisible
        {
            get { return (bool)GetValue(IsCheckBoxVisibleProperty); }
            set { SetValue(IsCheckBoxVisibleProperty, value); }
        }

        /// <summary>
        /// Command which is executed when any submenu button is tapped
        /// </summary>
        public static readonly BindableProperty ItemCommandProperty =
            BindableProperty.Create("ItemCommand", typeof(IComparable), typeof(MenuButton), default(IComparable));

        public IComparable ItemCommand
        {
            get { return (IComparable)GetValue(ItemCommandProperty); }
            set { SetValue(ItemCommandProperty, value); }
        }

        #endregion

        #region CheckBox

        public static readonly BindableProperty CheckBoxStyleProperty =
            BindableProperty.Create("CheckBoxStyle", typeof(Style), typeof(MenuButton), null);

        public Style CheckBoxStyle
        {
            get { return (Style)GetValue(CheckBoxStyleProperty); }
            set { SetValue(CheckBoxStyleProperty, value); }
        }

        #endregion

        #region SubMenuIcon

        public static readonly BindableProperty IsSubMenuIconVisibleProperty =
            BindableProperty.Create("IsSubMenuIconVisible", typeof(bool), typeof(MenuButton), true);

        public bool IsSubMenuIconVisible
        {
            get { return (bool)GetValue(IsSubMenuIconVisibleProperty); }
            set { SetValue(IsSubMenuIconVisibleProperty, value); }
        }

        public static readonly BindableProperty SubMenuIconResourceKeyProperty =
            BindableProperty.Create("SubMenuIconResourceKey", typeof(string), typeof(MenuButton), null);

        public string SubMenuIconResourceKey
        {
            get { return (string)GetValue(SubMenuIconResourceKeyProperty); }
            set { SetValue(SubMenuIconResourceKeyProperty, value); }
        }

        public static readonly BindableProperty SubMenuIconAssemblyNameProperty =
            BindableProperty.Create("SubMenuIconAssemblyName", typeof(string), typeof(MenuButton), null);

        public string SubMenuIconAssemblyName
        {
            get { return (string)GetValue(SubMenuIconAssemblyNameProperty); }
            set { SetValue(SubMenuIconAssemblyNameProperty, value); }
        }

        public static readonly BindableProperty SubMenuIconHeightRequestProperty =
            BindableProperty.Create("SubMenuIconHeightRequest", typeof(double), typeof(MenuButton), 20.0);

        public double SubMenuIconHeightRequest
        {
            get { return (double)GetValue(SubMenuIconHeightRequestProperty); }
            set { SetValue(SubMenuIconHeightRequestProperty, value); }
        }

        public static readonly BindableProperty SubMenuIconWidthRequestProperty =
            BindableProperty.Create("SubMenuIconWidthRequest", typeof(double), typeof(MenuButton), 20.0);

        public double SubMenuIconWidthRequest
        {
            get { return (double)GetValue(SubMenuIconWidthRequestProperty); }
            set { SetValue(SubMenuIconWidthRequestProperty, value); }
        }

        public static readonly BindableProperty SubMenuIconMarginProperty =
            BindableProperty.Create("SubMenuIconMargin", typeof(Thickness), typeof(MenuButton), new Thickness(0));

        public Thickness SubMenuIconMargin
        {
            get { return (Thickness)GetValue(SubMenuIconMarginProperty); }
            set { SetValue(SubMenuIconMarginProperty, value); }
        }

        #endregion

        #region Colors

        // SubMenuIcon

        public static readonly BindableProperty SubMenuIconColorProperty =
            BindableProperty.Create("SubMenuIconColor", typeof(Color), typeof(MenuButton), Color.Transparent);

        public Color SubMenuIconColor
        {
            get { return (Color)GetValue(SubMenuIconColorProperty); }
            set { SetValue(SubMenuIconColorProperty, value); }
        }

        public static readonly BindableProperty SubMenuIconHoverColorProperty =
            BindableProperty.Create("SubMenuIconHoverColor", typeof(Color), typeof(MenuButton), Color.Transparent);

        public Color SubMenuIconHoverColor
        {
            get { return (Color)GetValue(SubMenuIconHoverColorProperty); }
            set { SetValue(SubMenuIconHoverColorProperty, value); }
        }

        public static readonly BindableProperty SubMenuIconPressedColorProperty =
            BindableProperty.Create("SubMenuIconPressedColor", typeof(Color), typeof(MenuButton), Color.Transparent);

        public Color SubMenuIconPressedColor
        {
            get { return (Color)GetValue(SubMenuIconPressedColorProperty); }
            set { SetValue(SubMenuIconPressedColorProperty, value); }
        }

        public static readonly BindableProperty SubMenuIconDisabledColorProperty =
            BindableProperty.Create("SubMenuIconDisabledColor", typeof(Color), typeof(MenuButton), Color.Transparent);

        public Color SubMenuIconDisabledColor
        {
            get { return (Color)GetValue(SubMenuIconDisabledColorProperty); }
            set { SetValue(SubMenuIconDisabledColorProperty, value); }
        }

        // SubMenuIcon toggled

        public static readonly BindableProperty ToggledSubMenuIconColorProperty =
            BindableProperty.Create("ToggledSubMenuIconColor", typeof(Color), typeof(MenuButton), Color.Transparent);

        public Color ToggledSubMenuIconColor
        {
            get { return (Color)GetValue(ToggledSubMenuIconColorProperty); }
            set { SetValue(ToggledSubMenuIconColorProperty, value); }
        }

        public static readonly BindableProperty ToggledSubMenuIconHoverColorProperty =
            BindableProperty.Create("ToggledSubMenuIconHoverColor", typeof(Color), typeof(MenuButton), Color.Transparent);

        public Color ToggledSubMenuIconHoverColor
        {
            get { return (Color)GetValue(ToggledSubMenuIconHoverColorProperty); }
            set { SetValue(ToggledSubMenuIconHoverColorProperty, value); }
        }

        public static readonly BindableProperty ToggledSubMenuIconPressedColorProperty =
            BindableProperty.Create("ToggledSubMenuIconPressedColor", typeof(Color), typeof(MenuButton), Color.Transparent);

        public Color ToggledSubMenuIconPressedColor
        {
            get { return (Color)GetValue(ToggledSubMenuIconPressedColorProperty); }
            set { SetValue(ToggledSubMenuIconPressedColorProperty, value); }
        }

        public static readonly BindableProperty ToggledSubMenuIconDisabledColorProperty =
            BindableProperty.Create("ToggledSubMenuIconDisabledColor", typeof(Color), typeof(MenuButton), Color.Transparent);

        public Color ToggledSubMenuIconDisabledColor
        {
            get { return (Color)GetValue(ToggledSubMenuIconDisabledColorProperty); }
            set { SetValue(ToggledSubMenuIconDisabledColorProperty, value); }
        }

        #endregion

        public MenuButton()
		{
            ItemsSource = new ObservableCollection<object>();

            if (IsCheckBoxVisible && IsToggable)
            {
                if (_checkBox != null)
                {
                    _checkBox = CreateCheckBox();
                }
                if (_checkBox != null && Children.Contains(_checkBox) == false)
                {
                    Children.Add(_checkBox);
                }
            }
        }

        /// <summary>
        /// Handle properties changes
        /// </summary>
        protected override void OnPropertyChanged(string propertyName = null)
        {
            if (propertyName == IsCheckBoxVisibleProperty.PropertyName || 
                propertyName == IsToggableProperty.PropertyName)
            {
                if (IsCheckBoxVisible && IsToggable)
                {
                    if (_checkBox == null)
                    {
                        _checkBox = CreateCheckBox();
                    }

                    if (_checkBox != null && Children.Contains(_checkBox) == false)
                    {
                        Children.Add(_checkBox);
                    }
                }
                else if (_checkBox != null && Children.Contains(_checkBox))
                {
                    Children.Remove(_checkBox);
                }

                if (_menuItemsView != null)
                {
                    _menuItemsView.SelectionMode = IsCheckBoxVisible && IsToggable ? SelectionModes.Multiple : SelectionModes.Single;
                }
            }
            else if (propertyName == CheckBoxStyleProperty.PropertyName)
            {
                if (_checkBox != null)
                {
                    _checkBox.Style = CheckBoxStyle;
                }
            }
            else
            {
                base.OnPropertyChanged(propertyName);
            }
        }

        #region Measure

        /// <summary>
        /// Measure total size
        /// </summary>
        protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
        {
            MeasureChildren(widthConstraint, heightConstraint);

            Size actualSubMenuIconSize = new Size();
            Thickness actualSubMenuIconMargin = new Thickness();
            if (ItemsSource != null && ItemsSource.Count > 0 && IsSubMenuIconVisible && string.IsNullOrEmpty(SubMenuIconAssemblyName) == false && string.IsNullOrEmpty(SubMenuIconResourceKey) == false)
            {
                actualSubMenuIconSize = _subMenuIconSize;
                actualSubMenuIconMargin = SubMenuIconMargin;
            }

            SizeRequest size = base.OnMeasure(widthConstraint - _checkBoxSize.Width - actualSubMenuIconSize.Width, heightConstraint);

            Size s = new Size();
            s.Width = size.Request.Width + _checkBoxSize.Width + actualSubMenuIconSize.Width + actualSubMenuIconMargin.HorizontalThickness;
            s.Height = size.Request.Height;

            return new SizeRequest(s, s);
        }

        /// <summary>
        /// Layout checkbox
        /// </summary>
        protected override void LayoutChildren(double x, double y, double width, double height)
        {
            MeasureChildren(width, height);

            if (_checkBox != null && Children.Contains(_checkBox))
            {
                LayoutChildIntoBoundingRegion(_checkBox, new Rectangle(x, (height - _checkBoxSize.Height) / 2, _checkBoxSize.Width, _checkBoxSize.Height));
            }

            base.LayoutChildren(x, y, width, height);
        }

        /// <summary>
        /// Layout content
        /// </summary>
        protected override void LayoutContent(double x, double y, double width, double height)
        {
            base.LayoutContent(x + _checkBoxSize.Width, y, width - _subMenuIconSize.Width - _checkBoxSize.Width, height);
        }

        /// <summary>
        /// Measure parts
        /// </summary>
        private void MeasureChildren(double width, double height)
        {
            _checkBoxSize = new Size();
            if (_checkBox != null && Children.Contains(_checkBox))
            {
                _checkBoxSize = _checkBox.Measure(width, height, MeasureFlags.IncludeMargins).Request;
            }

            _subMenuIconSize = new Size();
            if (ItemsSource != null && ItemsSource.Count > 0 && IsSubMenuIconVisible)
            {
                _subMenuIconSize = MeasureSubMenuIcon(width, height);
            }
        }

        /// <summary>
        /// Measure icon size
        /// </summary>
        private Size MeasureSubMenuIcon(double widthConstraint, double heightConstraint)
        {
            if (SubMenuIconWidthRequest >= 0 && SubMenuIconHeightRequest >= 0)
            {
                return new Size(SubMenuIconWidthRequest, SubMenuIconHeightRequest);
            }
            else
            {
                if (m_subMenuIconSvg == null)
                {
                    m_subMenuIconSvg = SvgImage.GetSvgImage(SubMenuIconAssemblyName, SubMenuIconResourceKey);
                }

                float scale = SvgImage.CalculateScale(m_subMenuIconSvg.Picture.CullRect.Size, IconWidthRequest, IconHeightRequest);

                return new Size(m_subMenuIconSvg.Picture.CullRect.Width * scale, m_subMenuIconSvg.Picture.CullRect.Height * scale);
            }
        }

        #endregion

        #region Paint

        /// <summary>
        /// Add submenu icon
        /// </summary>
        protected override void OnPaintForeground(SKPaintSurfaceEventArgs e, Rectangle availableSpace)
        {
            Size actualSubMenuIconSize = new Size();

            if (ItemsSource != null && ItemsSource.Count > 0 && IsSubMenuIconVisible && string.IsNullOrEmpty(SubMenuIconAssemblyName) == false && string.IsNullOrEmpty(SubMenuIconResourceKey) == false)
            {
                if (m_subMenuIconSvg == null)
                {
                    m_subMenuIconSvg = SvgImage.GetSvgImage(SubMenuIconAssemblyName, SubMenuIconResourceKey);
                }

                Rectangle subMenuIconActualLocation = new Rectangle(
                    availableSpace.Width - _subMenuIconSize.Width - SubMenuIconMargin.Right, 
                    (availableSpace.Height - _subMenuIconSize.Height) / 2,
                    _subMenuIconSize.Height,
                    _subMenuIconSize.Width);

                float skSubMenuIconX = (float)subMenuIconActualLocation.X * DeviceScale;
                float skSubMenuIconY = (float)subMenuIconActualLocation.Y * DeviceScale;

                SKMatrix subMenuIconMatrix = new SKMatrix();
                SKPoint subMenuIconPosition = new SKPoint();

                float scale = SvgImage.CalculateScale(m_subMenuIconSvg.Picture.CullRect.Size, SubMenuIconWidthRequest, SubMenuIconHeightRequest) * DeviceScale;

                subMenuIconPosition.X = (float)skSubMenuIconX + (float)((SubMenuIconWidthRequest - _subMenuIconSize.Width) / 2) * DeviceScale;
                subMenuIconPosition.Y = (float)skSubMenuIconY + (float)((SubMenuIconHeightRequest - _subMenuIconSize.Height) / 2) * DeviceScale;
                subMenuIconMatrix.SetScaleTranslate(scale, scale, subMenuIconPosition.X, subMenuIconPosition.Y);
                
                using (var paint = new SKPaint())
                {
                    Color color = GetActualSubMenuIconColor();

                    paint.ColorFilter = SKColorFilter.CreateBlendMode(color.ToSKColor(), SKBlendMode.SrcIn);
                    paint.Style = SKPaintStyle.Fill;
                    paint.IsAntialias = true;
                    e.Surface.Canvas.DrawPicture(m_subMenuIconSvg.Picture, ref subMenuIconMatrix, paint);
                }

                actualSubMenuIconSize = _subMenuIconSize;
            }

            base.OnPaintForeground(e, new Rectangle(availableSpace.X + _checkBoxSize.Width, 
                                                    availableSpace.Y, 
                                                    availableSpace.Width - actualSubMenuIconSize.Width - _checkBoxSize.Width, 
                                                    availableSpace.Height));
        }
        
        /// <summary>
        /// Get submenu icon color
        /// </summary>
        private Color GetActualSubMenuIconColor()
        {
            return GetColor(
                SubMenuIconColor,
                SubMenuIconHoverColor,
                SubMenuIconPressedColor,
                ToggledSubMenuIconColor,
                ToggledSubMenuIconHoverColor,
                ToggledSubMenuIconPressedColor,
                SubMenuIconDisabledColor);
        }

        /// <summary>
        /// Get background color
        /// </summary>
        protected override Color GetActualBackgroundColor()
        {
            if (AnimationStyle == AnimationStyles.Ellipse)
            {
                if (IsEnabled)
                {
                    if (IsOpen)
                    {
                        return AnimationUtils.ColorTransform(_toggledAnimationProcess, BackgroundColor, ToggledBackgroundColor);
                    }
                    else
                    {
                        Color def = AnimationUtils.ColorTransform(_toggledAnimationProcess, BackgroundColor, ToggledBackgroundColor);
                        Color hover = AnimationUtils.ColorTransform(_toggledAnimationProcess, BackgroundHoverColor, ToggledBackgroundHoverColor);
                        return AnimationUtils.ColorTransform(_hoverAnimationProcess, def, hover);
                    }
                }
                else
                {
                    return BackgroundDisabledColor;
                }
            }
            else
            {
                return GetColor(
                    BackgroundColor,
                    BackgroundHoverColor,
                    BackgroundPressedColor,
                    ToggledBackgroundColor,
                    ToggledBackgroundHoverColor,
                    ToggledBackgroundPressedColor,
                    BackgroundDisabledColor);
            }
        }

        /// <summary>
        /// Get color based on IsPressed, IsToggled and animation process.
        /// </summary>
        protected override Color GetColor(
            Color defaultColor,
            Color hoverColor,
            Color pressedColor,
            Color toggledColor,
            Color toggledHoverColor,
            Color toggledPressedColor,
            Color disabledColor)
        {
            if (_pressedAnimationProcess < 1 || _hoverAnimationProcess < 1 || (_toggledAnimationProcess < 1 && _toggledAnimationProcess > 0))
            {
                return base.GetColor(defaultColor, hoverColor, pressedColor, toggledColor, toggledHoverColor, toggledPressedColor, disabledColor);
            }
            else
            {
                if (IsPressed)
                {
                    if (IsToggled || IsOpen)
                    {
                        return toggledPressedColor;
                    }
                    else
                    {
                        return pressedColor;
                    }
                }
                else if (IsMouseOver)
                {
                    if (IsToggled || IsOpen)
                    {
                        return toggledHoverColor;
                    }
                    else
                    {
                        return hoverColor;
                    }
                }
                else
                {
                    if (IsToggled || IsOpen)
                    {
                        return toggledColor;
                    }
                    else
                    {
                        return defaultColor;
                    }
                }
            }
        }
        
        #endregion

        #region BindingContext

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            // Do automatic bindings if BindingContext is implementing IMenuItem
            if (BindingContext is IMenuItem)
            {
                Binding bind = new Binding("BindingContext.ItemsSource");
                bind.Source = this;
                this.SetBinding(MenuButton.ItemsSourceProperty, bind);

                bind = new Binding("BindingContext.Command");
                bind.Source = this;
                this.SetBinding(MenuButton.CommandProperty, bind);

                bind = new Binding("BindingContext.Icon");
                bind.Source = this;
                this.SetBinding(MenuButton.IconResourceKeyProperty, bind);

                bind = new Binding("BindingContext.IconAssemblyName");
                bind.Source = this;
                this.SetBinding(MenuButton.IconAssemblyNameProperty, bind);

                bind = new Binding("BindingContext.Text");
                bind.Source = this;
                this.SetBinding(MenuButton.TextProperty, bind);

                bind = new Binding("BindingContext.IsToggable");
                bind.Source = this;
                this.SetBinding(MenuButton.IsToggableProperty, bind);
            }
        }

        #endregion

        #region CheckBox

        private CheckBox CreateCheckBox()
        {
            CheckBox checkBox = new CheckBox();
            checkBox.Style = CheckBoxStyle;
            checkBox.IsToggledChanged += OnCheckBoxToggledChanged;
            return checkBox;
        }

        protected override void OnIsToggledChanged(bool toggled)
        {
            if (_checkBox != null)
            {
                _checkBox.IsToggled = toggled;
            }
        }

        private void OnCheckBoxToggledChanged(object sender, bool isToggled)
        {
            IsToggled = isToggled;
        }

        #endregion

        #region Interaction

        protected override View CreatePopupContent()
        {
            _menuItemsView = new MenuItemsView();
            _menuItemsView.SubMenuItemTapped += OnSubMenuItemTapped;
            _menuItemsView.ItemsLayout = new StackLayout() { Spacing = 0 };
            _menuItemsView.Margin = new Thickness(0, 8, 0, 8);
            _menuItemsView.SelectionMode = SelectionModes.None;
            _menuItemsView.HasItemsChanged += OnHasSubMenuItemsChanged;

            OnActualPopupPlacementChanged(PopupPlacement);

            Binding bind = new Binding(MenuButton.ItemsSourceProperty.PropertyName);
            bind.Source = this;
            _menuItemsView.SetBinding(MenuItemsView.ItemsSourceProperty, bind);

            bind = new Binding(MenuButton.ItemTemplateProperty.PropertyName);
            bind.Source = this;
            _menuItemsView.SetBinding(MenuItemsView.ItemTemplateProperty, bind);

            bind = new Binding(MenuButton.ItemTemplateSelectorProperty.PropertyName);
            bind.Source = this;
            _menuItemsView.SetBinding(MenuItemsView.ItemTemplateSelectorProperty, bind);

            bind = new Binding(MenuButton.ItemsLayoutTemplateProperty.PropertyName);
            bind.Source = this;
            _menuItemsView.SetBinding(MenuItemsView.ItemsLayoutTemplateProperty, bind);

            bind = new Binding(MenuButton.ItemCommandProperty.PropertyName);
            bind.Source = this;
            _menuItemsView.SetBinding(MenuItemsView.ItemCommandProperty, bind);

            return _menuItemsView;
        }

        private void OnHasSubMenuItemsChanged(ItemsView sender, bool hasItems)
        {
            // MenuButton is not toggable if has submenu items
            IsToggable = !hasItems;
        }

        private void OnSubMenuItemTapped(object sender, EventArgs e)
        {
            SubMenuItemTapped?.Invoke(sender, e);
        }

        /// <summary>
        /// Override tapped event. MenuButton is NOT toggable if has submenu items.
        /// </summary>
        protected override void OnTapped()
        {
            if (ItemsSource != null && ItemsSource.Count > 0)
            {
                // Popup opened on release event
            }
            else
            {
                if (Command != null)
                {
                    // Execute command with parameter                        
                    Command.Execute(CommandParameter);                        
                }

                RaiseTapped();
            }
        }

        #endregion

        #region Menu

        protected override void OnReleased()
        {
            // MenuButton is toggable or it has submenu

            if (IsToggable)
            {
                base.OnReleased();
            }
            else
            {
                if (m_popup == null)
                {
                    m_popup = CreatePopup();
                }

                IsOpen = true;

                DoToggleAnimation(true);
            }
        }

        protected override void OnPopupOpened()
        {
            base.OnPopupOpened();

            if (_pressedAnimationProcess.Equals(0))
            {
                _toggledAnimationProcess = 1;
                _skiaCanvas.InvalidateSurface();
            }
        }

        protected override void OnPopupClosed()
        {
            base.OnPopupClosed();
            DoToggleAnimation(false);

            if (_menuItemsView != null)
            {
                _menuItemsView.CloseSubMenus();
            }
        }

        #endregion
    }
}
