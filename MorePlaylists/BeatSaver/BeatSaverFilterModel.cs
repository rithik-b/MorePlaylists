using BeatSaverSharp;

namespace MorePlaylists.BeatSaver
{
    internal class BeatSaverFilterModel
    {
        public SearchTextPlaylistFilterOptions? NullableSearchFilter { get; private set; }
        public SearchTextPlaylistFilterOptions SearchFilter => NullableSearchFilter ??= new SearchTextPlaylistFilterOptions();
        public string? UserName { get; set; }
        public FilterMode FilterMode { get; set; } = FilterMode.Search;

        public void ClearFilters()
        {
            NullableSearchFilter = null;
            UserName = null;
            FilterMode = FilterMode.Search;
        }
    }

    internal enum FilterMode
    {
        Search,
        User
    }
   
}
