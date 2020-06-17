using System;
using System.Collections.Generic;

// Xamarin
using Xamarin.Forms;

namespace XamKit
{
	public static class Resource
	{
        /// <summary>
        /// Get all resources fron source and add to target
        /// </summary>
        public static void Update(ResourceDictionary targetResources, ResourceDictionary sourceResources)
        {
            LoadResourcesRecursive(targetResources, sourceResources);
        }

        /// <summary>
        /// Get resources recursive from dictionary and merged dictionaries
        /// </summary>
        private static void LoadResourcesRecursive(ResourceDictionary targetResources, ResourceDictionary sourceResources)
		{
			if (sourceResources.MergedDictionaries != null)
			{
                foreach (ResourceDictionary resourceDictionary in sourceResources.MergedDictionaries)
                {
                    LoadResourcesRecursive(targetResources, resourceDictionary);
                }
            }

			foreach (var resource in sourceResources)
			{
				if (targetResources.ContainsKey(resource.Key))
				{
                    targetResources.Remove(resource.Key);
				}

                targetResources.Add(resource.Key, resource.Value);
			}
		}
	}
}