using System;
using System.Text;

namespace MorePlaylists.Utilities
{
    internal static class Utils
    {
        public static LevelSelectionFlowCoordinator.State GetStateForPlaylist(IBeatmapLevelPack beatmapLevelPack)
        {
            var state = new LevelSelectionFlowCoordinator.State(beatmapLevelPack);
            Accessors.LevelCategoryAccessor(ref state) = SelectLevelCategoryViewController.LevelCategory.CustomSongs;
            return state;
        }
        
        public static string RemoveSpecialCharacters(this string str)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in str)
            {
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || (c == ' '))
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }
    }
}
