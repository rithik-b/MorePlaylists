using System;
using HMUI;
using IPA.Utilities;
using UnityEngine;

namespace MorePlaylists.Utilities
{
    internal class Utils
    {
        private const string Base64Prefix = "base64,";

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
            return Convert.FromBase64String(dataIndex > 0 ? base64Str.Substring(dataIndex) : base64Str);
        }

        public static int GetBase64DataStartIndex(string base64Str)
        {
            return Math.Max(0, base64Str.IndexOf(',') + 1);
        }

        public static void TransferScrollBar(ScrollView sender, ScrollView reciever)
        {
            Accessors.PageUpAccessor(ref reciever) = Accessors.PageUpAccessor(ref sender);
            Accessors.PageDownAccessor(ref reciever) = Accessors.PageDownAccessor(ref sender);
            Accessors.ScrollIndicatorAccessor(ref reciever) = Accessors.ScrollIndicatorAccessor(ref sender);

            RectTransform scrollBar = sender.transform.Find("ScrollBar").GetComponent<RectTransform>();
            scrollBar.SetParent(sender.transform.parent);
            GameObject.Destroy(sender.gameObject);
            scrollBar.sizeDelta = new Vector2(8f, scrollBar.sizeDelta.y);
        }
    }
}
