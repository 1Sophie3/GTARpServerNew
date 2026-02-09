using RPCore.Database;
using RPCore.Notifications;
using GTANetworkAPI;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace RPCore.CharCreator
{
    class CharCreatorEvents : Script
    {

        [RemoteEvent("SetPlayerGender")]
        public void OnSetPlayerGender(Player player, string gender)
        {
            if (gender.ToLower() == "männlich")
            {
                NAPI.Player.SetPlayerSkin(player, PedHash.FreemodeMale01);
            }
            else
            {
                NAPI.Player.SetPlayerSkin(player, PedHash.FreemodeFemale01);
            }
        }

        [RemoteEvent("CharacterCreated")]
        public static void OnCharacterCreated(Player player, string character, bool created)
        {
            Accounts account = player.GetData<Accounts>(Accounts.Account_Key);

            JObject obj = JObject.Parse(character);

            //Hair
            NAPI.Player.SetPlayerClothes(player, 2, (int)obj["hair"][0], 0);
            NAPI.Player.SetPlayerHairColor(player, (byte)obj["hair"][1], (byte)obj["hair"][1]);

            //Eyecolor
            NAPI.Player.SetPlayerEyeColor(player, (byte)obj["eyeColor"][0]);

            //Facefeatures
            for (int i = 0; i < 19; i++)
            {
                NAPI.Player.SetPlayerFaceFeature(player, i, (int)obj["faceFeatures"][i]);
            }

            //Headblend
            HeadBlend headblend = new HeadBlend();
            headblend.ShapeFirst = (byte)obj["blendData"][0];
            headblend.ShapeSecond = (byte)obj["blendData"][1];
            headblend.ShapeThird = 0;
            headblend.SkinFirst = (byte)obj["blendData"][2];
            headblend.SkinSecond = (byte)obj["blendData"][3];
            headblend.SkinThird = 0;
            headblend.ShapeMix = (byte)obj["blendData"][4];
            headblend.SkinMix = (byte)obj["blendData"][5];
            headblend.ThirdMix = 0;
            NAPI.Player.SetPlayerHeadBlend(player, headblend);

            //Clothing
            NAPI.Player.SetPlayerClothes(player, 11, (int)obj["clothing"][0], 0);
            NAPI.Player.SetPlayerClothes(player, 3, (int)obj["clothing"][1], 0);
            NAPI.Player.SetPlayerClothes(player, 8, (int)obj["clothing"][2], 0);
            NAPI.Player.SetPlayerClothes(player, 6, (int)obj["clothing"][3], 0);

            //Headoverlay
            for (int i = 0; i < 12; i++)
            {
                HeadOverlay headOverlay = new HeadOverlay();
                int headOverlayCheck = (int)obj["headOverlays"][i];
                if (headOverlayCheck == -1)
                {
                    headOverlayCheck = 255;
                }
                if (i != 1)
                {
                    headOverlay.Index = (byte)headOverlayCheck;
                    headOverlay.Opacity = 1.0f;
                    headOverlay.Color = (byte)obj["headOverlaysColors"][i];
                    headOverlay.SecondaryColor = (byte)obj["headOverlaysColors"][i];
                }
                else
                {
                    headOverlayCheck = (int)obj["beard"][0];
                    headOverlay.Index = (byte)headOverlayCheck;
                    headOverlay.Opacity = 1.0f;
                    headOverlay.Color = (byte)obj["beard"][1];
                    headOverlay.SecondaryColor = (byte)obj["headOverlaysColors"][2];
                }
                NAPI.Player.SetPlayerHeadOverlay(player, i, headOverlay);
            }
            account.CharacterData = character;
            player.Name = $"{obj["firstname"]}_{obj["lastname"]}";

            if (created == true)
            {
                //player.TriggerEvent("showHideMoneyHud");
                player.TriggerEvent("charcreator-hide");
                //NAPI.ClientEvent.TriggerClientEvent(player, "PlayerFreeze", false);
                Utils.sendNotification(player, "Charakter erfolgreich erstellt, warte auf Einreise ...", "fas fa-user");
                //player.TriggerEvent("showHideMoneyHud");
            }
        }
    }
}
