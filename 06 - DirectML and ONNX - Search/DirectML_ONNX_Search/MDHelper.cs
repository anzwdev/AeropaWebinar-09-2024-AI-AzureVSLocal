using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace DirectML_ONNX_Search
{
    internal class MDHelper
    {

        public static string RemoveTags(string content)
        {
            var result = content;

            // Remove the front matter
            /*
            var frontMatterIndex = content.IndexOf("---", StringComparison.Ordinal);
            if (frontMatterIndex > 0)
            {
                var frontMatterEndIndex = content.IndexOf("---", frontMatterIndex + 3, StringComparison.Ordinal);
                if (frontMatterEndIndex > 0)
                {
                    result = content[(frontMatterEndIndex + 3)..];
                }
            }
            */

            // Remove the code blocks
            //result = Regex.Replace(result, @"```.*?```", "", RegexOptions.Singleline);

            // Remove the links
            result = Regex.Replace(result, @"\[.*?\]\(.*?\)", "");

            // Remove the images
            result = Regex.Replace(result, @"!\[.*?\]\(.*?\)", "");

            // Remove the HTML tags
            result = Regex.Replace(result, @"<.*?>", "");

            // Remove the markdown tags
            result = Regex.Replace(result, @"[*_]", "");

            // Remove the extra spaces
            result = Regex.Replace(result, @"\s+", " ");

            return result;
        }

    }
}
