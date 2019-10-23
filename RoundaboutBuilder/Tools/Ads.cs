using ColossalFramework.PlatformServices;
using ColossalFramework.UI;
using System;
using System.Reflection;
using UnityEngine;

namespace RoundaboutBuilder.Tools
{
    public class Ads : MonoBehaviour
    {
        private static Ads _instance;
        private static bool firstPass = true;
        private static bool executed = false;

        private static int position;
        private float timer = 0.0f;
        private WorkshopAdPanel panel;

        internal static void InitAd()
        {
            try
            {
                if (!CheckDate())
                    return;
                //AddAdvertising();
                System.Random rnd = new System.Random();
                position = rnd.Next(4);

                if (!_instance && firstPass)
                {
                    firstPass = false;
                    _instance = new GameObject("Ads").AddComponent<Ads>();
                }
            }
            catch
            {

            }
        }

        /*public static void Destroy()
        {
            if (_instance)
            {
                Destroy(_instance);
            }
        }*/

        private void Update()
        {
            try
            {
                //Debug.Log("Update!");
                timer += Time.deltaTime;
                if (timer > 0.2f)
                {
                    timer = 0.0f;
                    if (panel == null)
                    {
                        panel = GameObject.FindObjectOfType<WorkshopAdPanel>();
                    } else
                    {
                        if(!executed && GetContainer(panel).childCount > position)
                        {
                            try
                            {
                                AddAdvertising(panel);
                                //Debug.Log("Added advertising");
                            }
                            catch (Exception e)
                            { Debug.LogWarning(e); }
                            executed = true;
                            Destroy(this);
                        }
                    }
                }
            } catch
            {
                Destroy(this);
            }
            
        }

        private static UIScrollablePanel GetContainer(WorkshopAdPanel panel)
        {
            return (UIScrollablePanel)panel.GetType().GetField("m_ScrollContainer", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(panel);
        }

        /*private static bool CheckPanelContent(WorkshopAdPanel panel)
        {
            try
            {
                Debug.Log("Container contents:");
                UIScrollablePanel container = GetContainer(panel);
                Debug.Log("Container: " + container != null);
                foreach (var component in container.components)
                {
                    UILabel uilabel = container.Find<UILabel>("Title");
                    Debug.Log(uilabel == null ? "null" : uilabel.text);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
                return false;
            }


            return true;
        }*/

        private static bool CheckDate()
        {
            DateTime dt1 = new DateTime(2019, 10, 27);
            DateTime dt2 = DateTime.Now;
            return dt2 < dt1;
        }

        private static void AddAdvertising(object adpanel)
        {
            UGCDetails item = default(UGCDetails);
            item.result = Result.OK;
            item.publishedFileId = new PublishedFileId(1890830956);
            item.creatorID = new UserID(76561198073673201);
            item.score = 0.7f;
            item.upVotes = 1000;
            item.downVotes = 0;
            item.timeCreated = 1571229311;
            item.timeUpdated = 1571229311;
            item.title = "Undo It! [BETA]";
            item.description = "This mod adds Undo/Redo options to the game!";
            item.tags = "Mod";
            item.imageURL = "https://steamuserimages-a.akamaihd.net/ugc/760471717844041999/C303C5BED538E11E1021E380672DE404F3B7AB12/?imw=268&imh=268&ima=fit&impolicy=Letterbox&imcolor=%23000000&letterbox=true";

            typeof(WorkshopAdPanel).GetMethod("OnQueryCompleted", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(adpanel, new object[]
                { item, true });
        }
    }
}
