using System;

namespace Mag.GraphQL.Model
{
    public static class MitreExtensions
    {
        public static Paging DefaultIfNull(this Paging paging, int skip = 0, int take = 10)
        {
            return paging ?? new Paging() { Skip = skip, Take = take };
        }
    }
}
