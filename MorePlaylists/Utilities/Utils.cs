using IPA.Utilities;
using System;

namespace MorePlaylists.Utilities
{
    internal class Utils
    {
        public static readonly string Base64Prefix = "base64,";

        public static FieldAccessor<StandardLevelDetailViewController, LoadingControl>.Accessor LoadingControlAccessor = FieldAccessor<StandardLevelDetailViewController, LoadingControl>.GetAccessor("_loadingControl");
        public static string ByteArrayToBase64(byte[] byteArray)
        {
            if (byteArray == null || byteArray.Length == 0)
                return string.Empty;
            return Base64Prefix + Convert.ToBase64String(byteArray);
        }

        public static byte[] Base64ToByteArray(string base64Str)
        {
            if (string.IsNullOrEmpty(base64Str))
            {
                return null;
            }
            int dataIndex = GetBase64DataStartIndex(base64Str);
            if (dataIndex > 0)
                return Convert.FromBase64String(base64Str.Substring(dataIndex));
            else
                return Convert.FromBase64String(base64Str);
        }

        public static int GetBase64DataStartIndex(string base64Str)
        {
            int tagIndex = Math.Max(0, base64Str.IndexOf(',') + 1);
            return tagIndex;
        }
    }
}
