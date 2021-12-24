using System;

namespace MorePlaylists.Utilities
{
    internal class Utils
    {
        public static byte[] Base64ToByteArray(string base64Str)
        {
            if (string.IsNullOrEmpty(base64Str))
            {
                return null;
            }

            int dataIndex = Math.Max(0, base64Str.IndexOf(',') + 1);
            return Convert.FromBase64String(dataIndex > 0 ? base64Str.Substring(dataIndex) : base64Str);
        }

        public static LevelSelectionFlowCoordinator.State GetStateForPlaylist(IBeatmapLevelPack beatmapLevelPack)
        {
            LevelSelectionFlowCoordinator.State state = new LevelSelectionFlowCoordinator.State(beatmapLevelPack);
            Accessors.LevelCategoryAccessor(ref state) = SelectLevelCategoryViewController.LevelCategory.CustomSongs;
            return state;
        }
    }
}
