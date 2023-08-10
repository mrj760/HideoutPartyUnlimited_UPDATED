using System;
using System.Collections.Generic;
using System.Xml;
using HarmonyLib;
using SandBox;
using StoryMode;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace HideoutPartyUnlimited
{
    public class HideoutPartyUnlimited : MBSubModuleBase
    {
        protected override void OnSubModuleLoad()
        {
            try
            {
                new Harmony("2063.mods.mountandblade2bannerlord.HideoutPartyUnlimited").PatchAll();
            }
            catch (Exception ex)
            {
                InformationManager.ShowInquiry(new InquiryData("Information", "Failed hooking HideoutPartyUnlimited code\n\n" + ex.Message, true, false, "OK", "", null, null, "", 0f, null), false, false);
            }
        }

        protected override void OnSubModuleUnloaded()
        {
            new Harmony("2063.mods.mountandblade2bannerlord.HideoutPartyUnlimited").UnpatchAll(null);
        }

        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            if (game.GameType is Campaign && Convert.ToBoolean(this.GetSettingValue("/Module/SubModules/SubModule/Tags/Tag[@key='TroopRecords']/@value")[0].InnerText))
            {
                this.InsertBehavior(gameStarterObject as CampaignGameStarter, "HideoutCampaignBehavior", new HideoutCampaignBehavior());
                this.InsertBehavior(gameStarterObject as CampaignGameStarter, "HideoutCampaignBehavior", HideoutSendTroopsBehavior.Instance);
            }
        }

        private void InsertBehavior(CampaignGameStarter gameStarter, string targetBehaviorName, CampaignBehaviorBase campaignBehavior)
        {
            List<CampaignBehaviorBase> list = Helper.ReflectionGetField_Instance(gameStarter, "_campaignBehaviors") as List<CampaignBehaviorBase>;
            int index = list.FindIndex((CampaignBehaviorBase x) => x.GetType().Name == targetBehaviorName);
            list.Insert(index, campaignBehavior);
        }

        public override void OnGameLoaded(Game game, object gameStarterObject)
        {
            if (game.GameType is Campaign)
            {
                base.OnGameLoaded(game, gameStarterObject);
                this.ReplaceBanditDensityModel(game);
            }
        }

        public override void OnNewGameCreated(Game game, object initializerObject)
        {
            if (game.GameType is Campaign)
            {
                base.OnNewGameCreated(game, initializerObject);
                this.ReplaceBanditDensityModel(game);
            }
        }

        protected XmlNodeList GetSettingValue(string xpath)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(BasePath.Name + "Modules/HideoutPartyUnlimited/SubModule.xml");
            return xmlDocument.SelectNodes(xpath);
        }

        protected void ReplaceBanditDensityModel(Game game)
        {
            int count = Convert.ToInt32(this.GetSettingValue("/Module/SubModules/SubModule/Tags/Tag[@key='PlayerMaximumTroopCountForHideoutMission']/@value")[0].InnerText);
            if (game.GameManager is StoryModeGameManager)
            {
                Helper.ReflectionSetFieldPropertyValue_Instance(Campaign.Current.Models, "BanditDensityModel", new ChangeBanditDensityModel(count));
                return;
            }
            if (game.GameManager is SandBoxGameManager)
            {
                Helper.ReflectionSetFieldPropertyValue_Instance(Campaign.Current.Models, "BanditDensityModel", new SandboxChangeBanditDensityModel(count));
            }
        }
    }
}
