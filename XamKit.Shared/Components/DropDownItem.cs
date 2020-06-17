using System;
using System.Collections.Generic;
using System.Text;

namespace XamKit
{
    public class DropDownItem : MenuButton
    {
        // Override for internal use only
        public new bool IsCheckBoxVisible
        {
            get
            {
                return base.IsCheckBoxVisible;
            }
            internal set
            {
                base.IsCheckBoxVisible = value;
            }
        }

        /// <summary>
        /// Set left based on DropDown icon width
        /// </summary>
        internal void SetLeftPadding(double padding)
        {
        }
    }
}
