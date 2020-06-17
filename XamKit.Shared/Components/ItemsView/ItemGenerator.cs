using System;
using System.Collections.Generic;
using System.Linq;

// Xamarin
using Xamarin.Forms;

namespace XamKit
{
    public enum ItemStateTypes { None, Adding, Removing }

    public class ItemsGenerator
    {
        private class ItemInfo
        {
            /// <summary>
            /// Item view element
            /// </summary>
            public View ItemView { get; set; }

            /// <summary>
            /// Item datacontext model
            /// </summary>
            public object Item { get; set; }

            /// <summary>
            /// Template where ItemView was generated
            /// </summary>
            public DataTemplate ItemTemplate { get; set; }

            /// <summary>
            /// Item current state
            /// </summary>
            public ItemStateTypes State { get; set; }
        }

        // List of all items
        private List<ItemInfo> _items = null;
        private List<ItemInfo> _virtualizedItems = null;
        private List<ItemInfo> _realizedItems = null;

        /// <summary>
        /// Items generator host
        /// </summary>
        public IGeneratorHost GeneratorHost { get; private set; }

        /// <summary>
        /// Get items total count (including NOT generated)
        /// </summary>
        public int TotalItemsCount
        {
            get
            {
                return _items.Count;
            }
        }

        /// <summary>
        /// Get generated items count
        /// </summary>
        public int GeneratedItemsCount
        {
            get
            {
                return _items.Count(i => i.ItemView != null);
            }
        }

        /// <summary>
        /// Is items recycle enabled
        /// </summary>
        public bool IsRecycleEnabled { get; set; } = true;

        public ItemsGenerator(IGeneratorHost generatorHost)
        {
            _items = new List<ItemInfo>();
            GeneratorHost = generatorHost;

            _virtualizedItems = new List<ItemInfo>();
            _realizedItems = new List<ItemInfo>();
        }

        public void SetVirtualized(int index)
        {
            ItemInfo childInfo = _items.ElementAt(index);

            _realizedItems.Remove(childInfo);

            if (_virtualizedItems.Contains(childInfo) == false)
            {
                // childInfo.ItemView.BindingContext = null;
                _virtualizedItems.Add(childInfo);
            }
        }

        public void SetRealized(int index)
        {
            ItemInfo childInfo = _items.ElementAt(index);

            _virtualizedItems.Remove(childInfo);

            if (_realizedItems.Contains(childInfo) == false)
            {
                _realizedItems.Add(childInfo);
            }
        }

        /// <summary>
        /// Set item state by index
        /// </summary>
        public void SetState(int index, ItemStateTypes state)
        {
            _items[index].State = state;
        }

        /// <summary>
        /// Get item state from index
        /// </summary>
        public ItemStateTypes GetState(int index)
        {
            return _items[index].State;
        }

        /// <summary>
        /// Insert item and item view to index
        /// </summary>
        /// <param name="index">Item index</param>
        /// <param name="item">Item model</param>
        /// <param name="itemView">Item container</param>
        /// <param name="state">Item state</param>
        public void Insert(int index, object item, View itemView, ItemStateTypes state = ItemStateTypes.None)
        {
            ItemInfo info = new ItemInfo();
            info.Item = item;
            info.ItemView = itemView;
            info.State = state;

            if (_items.Count == index - 1)
            {
                _items.Add(info);
            }
            else
            {
                _items.Insert(index, info);
            }
        }

        /// <summary>
        /// Get single item view by index
        /// </summary>
        /// <param name="index">Item index</param>
        /// <returns>Return null if generator host has any item for index</returns>
        public View GetItemViewFromIndex(int index)
        {
            if (_items.Count > index && _items.ElementAt(index).ItemView != null)
            {
                return _items.ElementAt(index).ItemView;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Remove item by index
        /// </summary>
        /// <param name="index">Item index</param>
        public void Remove(int index)
        {
            if (index < _items.Count)
            {
                _items.RemoveAt(index);
            }
        }

        /// <summary>
        /// Clear/remove all items from cache
        /// </summary>
        public void RemoveAll()
        {
            _items.Clear();
            _virtualizedItems.Clear();
            _realizedItems.Clear();
        }

        /// <summary>
        /// Get item view index
        /// </summary>
        /// <param name="itemContainer"></param>
        /// <returns>-1 if item is not generated</returns>
        public int GetIndex(View itemContainer)
        {
            int i = 0;
            foreach (ItemInfo itemInfo in _items)
            {
                if (itemInfo.ItemView == itemContainer)
                {
                    return i;
                }
                i++;
            }

            return -1;
        }

        /// <summary>
        /// Change item view index
        /// </summary>
        public void Move(int startIndex, int toIndex)
        {
            ItemInfo itemInfo = _items.ElementAt(startIndex);
            _items.RemoveAt(startIndex);
            _items.Insert(toIndex, itemInfo);
        }

        /// <summary>
        /// Is item view generated in giving index
        /// </summary>
        public bool HasItemViewGenerated(int index)
        {
            return _items.Count > index && index >= 0 && _items.ElementAt(index).ItemView != null;
        }

        /// <summary>
        /// Generater item view from index and save it to cache
        /// </summary>
        /// <param name="index">Item index</param>
        /// <returns>Generated item view</returns>
        public View GenerateItemView(int index)
        {
            ItemInfo itemInfo = null;

            if (_items.Count > index)
            {
                itemInfo = _items.ElementAt(index); 
            }
            else
            {
                itemInfo = new ItemInfo();
                _items.Add(itemInfo);
            }

            if (itemInfo.ItemView == null)
            {
                if (GeneratorHost.IsItemItsOwnContainer(itemInfo.Item))
                {
                    itemInfo.ItemView = itemInfo.Item as View;
                }
                else if (IsRecycleEnabled)
                {
                    if (itemInfo.ItemTemplate == null)
                    {
                        itemInfo.ItemTemplate = GeneratorHost.CreateItemTemplate(itemInfo.Item);
                    }

                    View itemView = TryeGetRecycledItem(itemInfo);

                    if (itemView == null)
                    {
                        itemView = itemInfo.ItemTemplate.CreateContent() as View;
                    }

                    itemInfo.ItemView = itemView;
                }
                else
                {
                    itemInfo.ItemTemplate = GeneratorHost.CreateItemTemplate(itemInfo.Item);
                    itemInfo.ItemView = itemInfo.ItemTemplate.CreateContent() as View;
                }

                GeneratorHost.PrepareItemView(itemInfo.ItemView, itemInfo.Item);
            }

            return itemInfo.ItemView;
        }

        private View TryeGetRecycledItem(ItemInfo itemInfo)
        {
            View itemView = null;

            if (_virtualizedItems.Count > 0)
            {
                foreach (ItemInfo item in _virtualizedItems)
                {
                    if (item.ItemView != null && item.ItemTemplate == itemInfo.ItemTemplate)
                    {
                        itemView = item.ItemView;
                        item.ItemView = null;
                        break;
                    }
                }
            }

            return itemView;
        }
    }
}
