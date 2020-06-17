using System;
using Xamarin.Forms;

namespace XamKit
{
    /// <summary>
    /// Force checkbox visibility by SelectionMode
    /// </summary>
    public class DropDownMenuItemsView : MenuItemsView
    {
        private double? _leftPadding = null;

        /// <summary>
        /// Set items left padding
        /// </summary>
        /// <param name="padding"></param>
        public void SetItemsLeftPadding(double padding)
        {
            bool wasChanged = _leftPadding != null;

            _leftPadding = padding;
            
            for (int i = 0; i < ItemsGenerator.TotalItemsCount; i++)
            {
                if (ItemsGenerator.HasItemViewGenerated(i))
                {
                    View item = ItemsGenerator.GetItemViewFromIndex(i);

                    if (_leftPadding != null)
                    {
                        SetLeftPadding(item, padding);
                    }
                    else if (wasChanged && item is Button button)
                    {
                        button.ClearValue(Button.PaddingProperty);
                    }
                }
            }
        }

        protected override void OnSelectionModeChanged(SelectionModes oldValue, SelectionModes newValue)
        {
            base.OnSelectionModeChanged(oldValue, newValue);

            for (int i = 0; i < ItemsGenerator.TotalItemsCount; i++)
            {
                if (ItemsGenerator.HasItemViewGenerated(i))
                {
                    View item = ItemsGenerator.GetItemViewFromIndex(i);
                    InitializeItem(item);

                    if (_leftPadding != null)
                    {
                        SetLeftPadding(item, _leftPadding.Value);
                    }
                }
            }
        }

        protected override void PrepareItemView(View itemView)
        {
            base.PrepareItemView(itemView);

            InitializeItem(itemView);

            if (_leftPadding != null)
            {
                SetLeftPadding(itemView, _leftPadding.Value);
            }
        }

        private void InitializeItem(View itemView)
        {
            if (itemView is MenuButton button)
            {
                if (button.ItemsSource == null || button.ItemsSource.Count > 0)
                {
                    button.IsCheckBoxVisible = false;
                    button.IsToggable = false;
                }
                else
                {
                    button.IsCheckBoxVisible = SelectionMode == SelectionModes.Multiple;
                    button.IsToggable = true;
                }
            }
        }

        private void SetLeftPadding(View item, double padding)
        {
            if (item is Button button)
            {
                if (button.IconResourceKey != null && button.IconPlacement == ButtonIconPlacements.Left || (item is MenuButton menuButton && menuButton.IsCheckBoxVisible))
                {
                    // Do nothing
                    return;
                }
                else
                {
                    button.Padding = new Thickness(padding, button.Padding.Top, button.Padding.Right, button.Padding.Bottom);
                }
            }
        }
    }
}