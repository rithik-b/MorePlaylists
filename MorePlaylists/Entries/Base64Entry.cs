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
                return CoverData == null ? string.Empty : Utils.ByteArrayToBase64(CoverData);
            }
            set
            {
                if (string.IsNullOrEmpty(value))
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
            return new MemoryStream(CoverData != null ? CoverData : Array.Empty<byte>());
        }
    }
}
