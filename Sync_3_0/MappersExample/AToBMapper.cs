using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MappersExample
{
    public class AToBMapper: Mapper<A, B>
    {
        public Dictionary<string, string> MappingProperties => new Dictionary<string, string>()
        {
            { nameof(A.x), nameof(B.x) },
            { nameof(A.y), nameof(B.y) },
        };

        protected override View GetSourceView()
        {
            // Например, SyncView, как в предыдущем синхронизаторе.
            return A.Views.SyncView;
        }

        public override B Map(A source, B dest)
        {
            dest.Guid = PKHelper.GetGuidByObject(source).Value;
            dest.x = source.x;
            dest.y = source.y + 4;

            return dest;
        }
    }
}
