using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sercalo.TF
{
    public static class Factory
    {
        /// <summary>
        /// Creates a new instance of a Tunable Filter
        /// </summary>
        /// <returns></returns>
        public static ITunableFilter Create()
            => new TunableFilter();
    }
}
