namespace TRuDI.Backend.Utils
{
    using Microsoft.AspNetCore.Html;
    using System.Security.Cryptography;
    using System.Text;

    public static class HtmlStringExtensions
    {
        public static HtmlString AddLineBreak(this string source, int lineLength)
        {
            var sb = new StringBuilder();

            for (int i = 0; i < source.Length; i++)
            {
                sb.Append(source[i]);
                if ((i + 1) % lineLength == 0)
                {
                    sb.AppendLine("<br/>");
                }
            }

            return new HtmlString(sb.ToString());
        }

        public static string OidToFriendlyName(this string oid)
        {
            var o = new Oid(oid);
            return o.FriendlyName;
        }
    }
}
