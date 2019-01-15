using ICities;

/* By Strad, 01/2019 */

/* Version BETA 1.0.0 */

namespace RoundaboutBuilder
{
    namespace RoundaboutBuilder
    {
        public class RoundAboutBuilder : IUserMod
        {
            public static readonly string VERSION = "BETA 1.0.0";

            public string Name
            {
                get { return "Roundabout Builder"; }
            }

            public string Description
            {
                get { return "Automatically builds roundabouts. [" + VERSION + "]"; }
            }
        }
    }
}
