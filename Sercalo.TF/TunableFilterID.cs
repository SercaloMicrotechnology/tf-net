using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Sercalo.TF
{
    public class TunableFilterID
    {
        /// <summary>
        /// Gets the name of the product.
        /// </summary>
        /// <value>
        /// The name of the product.
        /// </value>
        public string ProductName { get; private set; }
        /// <summary>
        /// Gets the serial number.
        /// </summary>
        /// <value>
        /// The serial number.
        /// </value>
        public string SerialNumber { get; private set; }
        /// <summary>
        /// Gets the firmware version.
        /// </summary>
        /// <value>
        /// The firmware version.
        /// </value>
        public string FirmwareVersion { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TunableFilterID"/> class.
        /// </summary>
        /// <param name="pn">The pn.</param>
        /// <param name="sn">The sn.</param>
        /// <param name="ver">The ver.</param>
        private TunableFilterID(string pn, string sn, string ver)
        {
            ProductName = pn;
            SerialNumber = sn;
            FirmwareVersion = ver;
        }

        /// <summary>
        /// Create the tunable filter id structure from the identifier string
        /// </summary>
        /// <param name="str">The identifier string.</param>
        /// <returns></returns>
        public static TunableFilterID FromIDString(string str)
        {
            Match match = Regex.Match(str, @"(?<pn>\S+)\|(?<sn>\S*)\|(?<ver>\S*)");

            if (!match.Success)
                throw new SercaloException($"Cannot find a suitable match from expresion '{str}'.");

            return new TunableFilterID(match.Groups["pn"].Value, match.Groups["sn"].Value, match.Groups["ver"].Value);
        }
    }
}
