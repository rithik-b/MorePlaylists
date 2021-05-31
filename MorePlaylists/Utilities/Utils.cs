using System;

namespace MorePlaylists.Utilities
{
    public class Utils
    {
        public static readonly string Base64Prefix = "base64,";
        internal static string ByteArrayToBase64(byte[] byteArray)
        {
            if (byteArray == null || byteArray.Length == 0)
                return string.Empty;
            return Base64Prefix + Convert.ToBase64String(byteArray);
        }

        internal static byte[] Base64ToByteArray(string base64Str)
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
