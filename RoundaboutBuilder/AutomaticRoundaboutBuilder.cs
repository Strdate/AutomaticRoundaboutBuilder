using ICities;

/* By Strad, 01/2019 */

/* Version BETA 1.1.0 */

namespace RoundaboutBuilder
{
        public class RoundAboutBuilder : IUserMod
        {
            public static readonly string VERSION = "BETA 1.1.0";
            public bool OldSnappingAlgorithm { get; private set; } = false;

            public string Name
            {
                get { return "Roundabout Builder"; }
            }

            public string Description
            {
            get { return "Press CTRL+O to open menu. [" + VERSION + "]"; }
            //get { return "Automatically builds roundabouts. [" + VERSION + "]"; }
        }

        }
}
