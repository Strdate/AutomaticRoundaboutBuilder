using ColossalFramework.PlatformServices;
using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace RoundaboutBuilder.UI
{
    public class UndoItAdvertising : UIPanel
    {
        private readonly string atlasName = "UndoItAd";
        private UITextureAtlas imageAtlas;

        public static PublishedFileId undoItId = new PublishedFileId(1890830956uL);

        public static UndoItAdvertising Instance;
        public UndoItAdvertising()
        {
            Instance = this;
        }

        public override void Start()
        {
            base.Start();
            int num = 269;
            int num2 = 269;

            name = "RAB_UndoItAd";
            atlas = ResourceLoader.GetAtlas("Ingame");
            backgroundSprite = "SubcategoriesPanel";
            size = new Vector2(num + 20, num2 + 20);
            absolutePosition = new Vector3(200, 20);
 
            string[] spriteNames = new string[]
            {
                "UndoItImage",
            };
            if (ResourceLoader.GetAtlas(atlasName) == UIView.GetAView().defaultAtlas)
            {
                imageAtlas = ResourceLoader.CreateTextureAtlas("undoit.png", atlasName, atlas.material, num, num2, spriteNames);
            }
            else
            {
                imageAtlas = ResourceLoader.GetAtlas(atlasName);
            }

            //
            float buffer = 269 + 20;

            UISprite sprite = AddUIComponent<UISprite>();
            sprite.relativePosition = new Vector2(10, 10);
            sprite.width = 50;
            sprite.height = 50;
            sprite.spriteName = "UndoItImage";
            sprite.atlas = imageAtlas;
            //sprite.width = num;
            //sprite.height = num2;
            //
            sprite.size = sprite.CalculateMinimumSize();
            buffer += sprite.height;

            UILabel label = AddUIComponent<UILabel>();
            label.width = width - 16;
            label.wordWrap = true;
            label.textScale = 0.9f;
            label.text = "Do you wish Cities Skylines had Undo option? Your wishes came true! This new 'Undo It!' mod adds undo/redo to the game, which applies to vanilla" +
                " road/building/prop/tree/bulldoze tools. More info & video in the Steam Workshop page\n(Sorry for spam)";
            label.width = width - 16;
            label.relativePosition = new Vector2(8, buffer);
            label.SendToBack();
            buffer += label.height + 8;

            var dontShow = UIUtil.CreateCheckBox(this);
            dontShow.name = "RAB_dontShow";
            dontShow.label.text = "Do not show again";
            dontShow.isChecked = true;
            dontShow.relativePosition = new Vector3(8, buffer);
            dontShow.width = width - 16; // width - padding
            /*dontShow.eventCheckChanged += (c, state) =>
            {
               
            };*/
            buffer += dontShow.height + 8;

            var workshopButton = UIUtil.CreateButton(this);
            workshopButton.text = "Undo It! Workshop";
            workshopButton.tooltip = "Open workshop page in steam overlay";
            workshopButton.relativePosition = new Vector2(8, buffer);
            workshopButton.width = width - 90 - 24;
            workshopButton.eventClick += delegate (UIComponent component, UIMouseEventParameter click)
            {
                if (PlatformService.IsOverlayEnabled() && undoItId != null)
                {
                    PlatformService.ActivateGameOverlayToWorkshopItem(undoItId);
                }
                workshopButton.Unfocus();
            };

            var closeButton = UIUtil.CreateButton(this);
            closeButton.text = "Close";
            closeButton.relativePosition = new Vector2(16 + workshopButton.width, buffer);
            closeButton.eventClick += (c, p) =>
            {
                RoundAboutBuilder.ShowUndoItAd.value = !dontShow.isChecked;
                isVisible = false;
                enabled = false;
            };
            buffer += closeButton.height + 8;

            height = buffer;

            isVisible = true;

            // hide on small screens
            if (Screen.height < 1000)
                enabled = false;
        }
    }
}
