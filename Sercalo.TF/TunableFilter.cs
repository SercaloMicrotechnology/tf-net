using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sercalo.TF
{
    internal class TunableFilter : Sercalo.Serial.SerialDevice, ITunableFilter
    {
        public async Task<TunableFilterID> GetIDAsync()
        {
            string str = await QueryAsync("ID");
            return TunableFilterID.FromIDString(str);
        }
    }
}
