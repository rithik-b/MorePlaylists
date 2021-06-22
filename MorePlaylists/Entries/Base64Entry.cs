using MorePlaylists.Utilities;
using System;
using System.IO;

namespace MorePlaylists.Entries
{
    public abstract class Base64Entry : GenericEntry
    {
        protected virtual string CoverString
        {
            get
            {
                if (CoverData == null)
                    return string.Empty;
                return Utils.ByteArrayToBase64(CoverData);
            }
            set
            {
                if (value == null || value.Length == 0)
                {
                    CoverData = Array.Empty<byte>();
                    return;
                }
                try
                {
                    CoverData = Utils.Base64ToByteArray(value);
                }
                catch (FormatException)
                {
                    CoverData = Array.Empty<byte>();
                }
            }
        }

        protected byte[] CoverData { get; set; }

        public override Stream GetCoverStream()
        {
            if (CoverData != null)
                return new MemoryStream(CoverData);
            else
                return new MemoryStream(Array.Empty<byte>());
        }
    }
}
