using System;
using System.Collections.Generic;

namespace GryphonUtility.Bot.Web.Models.Save
{
    internal sealed class ArticleComparer : IComparer<Article>
    {
        public int Compare(Article x, Article y)
        {
            if ((x == null) || (y == null))
            {
                throw new ArgumentNullException();
            }

            int datesCompare = x.Date.CompareTo(y.Date);
            return datesCompare != 0
                ? datesCompare
                : string.Compare(x.Uri.AbsoluteUri, y.Uri.AbsoluteUri, StringComparison.Ordinal);
        }
    }
}