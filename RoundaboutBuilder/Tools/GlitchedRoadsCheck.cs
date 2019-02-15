using ColossalFramework.UI;
using System;
using UnityEngine;

/* By Strad, 01/2019 */

/* Version BETA 1.2.0 */

/* I put a lot of warnings there because very bad things happened when I ran earlier builds of this class, but now everything should be resolved and work alright. */

namespace RoundaboutBuilder.Tools
{
    static class GlitchedRoadsCheck
    {
        private static int count = 0;
        private static int unhandledExceptions = 0;

        /* Inspired by Move It */
        public static void RemoveGlitchedRoads()
        {
            if (!ModLoadingExtension.LevelLoaded)
            {
                ExceptionPanel notLoaded = UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel");
                notLoaded.SetMessage("Not In-Game", "Use this button when in-game to remove glitched roads", false);
                return;
            }

            ExceptionPanel panel = UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel");
            string message;
            count = 0;
            unhandledExceptions = 0;

            for (ushort segmentId = 0; segmentId < NetManager.instance.m_segments.m_buffer.Length; segmentId++)
            {
                NetSegment segment = NetManager.instance.m_segments.m_buffer[segmentId];

                if ((segment.m_flags & NetSegment.Flags.Created) == NetSegment.Flags.None) continue;

                try
                {
                    if(segment.m_startNode == 0 || segment.m_endNode == 0)
                    {
                        Release(segmentId);
                        continue;
                    }

                    try
                    {
                        NetNode node1 = NetManager.instance.m_nodes.m_buffer[segment.m_startNode];
                        NetNode node2 = NetManager.instance.m_nodes.m_buffer[segment.m_endNode];

                        if((node1.m_flags & NetNode.Flags.Created) == NetNode.Flags.None || (node2.m_flags & NetNode.Flags.Created) == NetNode.Flags.None)
                        {
                            Release(segmentId);
                            continue;
                        }

                        // Teasing methods
                        node1.CountSegments();
                        node2.CountSegments();
                    }
                    catch(Exception e)
                    {
                        Debug.Log("Invalid node. Has thrown exception: ");
                        Debug.Log(e);
                        Release(segmentId);
                    }

                    
                }
                catch(Exception e)
                {
                    unhandledExceptions++;
                    Debug.LogError(e);
                }
            }
            if (unhandledExceptions > 0)
            {
                message = "ERROR: Unhandled exceptions: " + unhandledExceptions + ". Maybe something went VERY VERY wrong. You should reload last save. (Segments removed: " + count + ")";
            }
            else if (count > 0)
            {
                message = $"Removed {count} segment{(count == 1 ? "" : "s")}! Please check that nothing unpleasant has happened to your game. Save it in a new file.";
            }
            else
            {
                message = "No glitched segments found.";
            }
            
            panel.SetMessage("Removing glitched segments", message, false);
        }

        private static void Release(ushort segmentId)
        {
            NetManager.instance.ReleaseSegment(segmentId, true);
            count++;
        }
    }
}
