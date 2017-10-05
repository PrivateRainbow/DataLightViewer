using DataLightViewer.ViewModels;
using System;
using System.Collections.Generic;

namespace DataLightViewer.Filters
{
    internal enum SearchFilterType
    {
        ByName = 0,
        ByAttributes = 1,
        CombinedSearch = 2
    }

    internal delegate bool SearchFilter(NodeViewModel vm, string pattern);

    internal static class NodeSearchFilters
    {        
        private static readonly Dictionary<SearchFilterType, SearchFilter> _filters = new Dictionary<SearchFilterType, SearchFilter>
        {
            {SearchFilterType.ByName, SearchByContent }
        };

        public static SearchFilter GetFilterBySearchType(SearchFilterType type) => _filters[type];

        #region Filters

        private static bool SearchByName(NodeViewModel source, string pattern) => source.Name.IndexOf(pattern, StringComparison.InvariantCultureIgnoreCase) > -1;
        private static bool SearchByContent(NodeViewModel source, string pattern) => source.Content.IndexOf(pattern, StringComparison.InvariantCultureIgnoreCase) > -1;

        #endregion       
    }
}
