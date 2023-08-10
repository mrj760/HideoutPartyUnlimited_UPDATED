using System;
using System.Collections.Generic;
using System.Linq;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;

namespace HideoutPartyUnlimited
{
    public class HideoutCampaignBehavior : CampaignBehaviorBase
    {
        public static TroopRoster PrevTroopRoster { get; set; } = null;

        public static string LoadFileName { get; set; } = "";

        public void OnGameLoaded(CampaignGameStarter campaignGameStarter)
        {
            this.AddGameMenus(campaignGameStarter);
        }

        public override void RegisterEvents()
        {
            CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(this.OnGameLoaded));
            CampaignEvents.OnGameLoadFinishedEvent.AddNonSerializedListener(this, new Action(this.OnLoadFinish));
        }

        public override void SyncData(IDataStore dataStore)
        {
        }

        protected void AddGameMenus(CampaignGameStarter campaignGameStarter)
        {
            this.GameMenuOptionRewrite(campaignGameStarter, "hideout_place", "attack", new GameMenuOption.OnConsequenceDelegate(this.game_menu_encounter_attack_on_consequence));
            this.GameMenuOptionRewrite(campaignGameStarter, "hideout_after_wait", "attack", new GameMenuOption.OnConsequenceDelegate(this.game_menu_encounter_attack_on_consequence));
        }

        private void GameMenuOptionRewrite(CampaignGameStarter gameStarter, string menuId, string idString, GameMenuOption.OnConsequenceDelegate onCons)
        {
            foreach (GameMenuOption gameMenuOption in (Helper.ReflectionInvokeMethod_Instance(Helper.ReflectionGetField_Instance(gameStarter, "_gameMenuManager") as GameMenuManager, "GetGameMenu", new object[]
            {
                menuId
            }) as GameMenu).MenuOptions)
            {
                if (gameMenuOption.IdString == idString)
                {
                    gameMenuOption.OnConsequence = onCons;
                }
            }
        }

        private void game_menu_encounter_attack_on_consequence(MenuCallbackArgs args)
        {
            int playerMaximumTroopCountForHideoutMission = Campaign.Current.Models.BanditDensityModel.GetPlayerMaximumTroopCountForHideoutMission(MobileParty.MainParty);
            TroopRoster troopRoster = MobilePartyHelper.GetStrongestAndPriorTroops(MobileParty.MainParty, playerMaximumTroopCountForHideoutMission, true);
            if (args.MenuContext.Handler == null)
            {
                return;
            }
            if (HideoutCampaignBehavior.PrevTroopRoster != null)
            {
                TroopRoster prevTroopRoster = HideoutCampaignBehavior.PrevTroopRoster;
                FlattenedTroopRoster availableTroops = this.GetAvailableTroops(prevTroopRoster, playerMaximumTroopCountForHideoutMission, true);
                prevTroopRoster.Clear();
                prevTroopRoster.Add(availableTroops);
                troopRoster = prevTroopRoster;
            }
            TaleWorlds.CampaignSystem.CampaignBehaviors.HideoutCampaignBehavior obj = new TaleWorlds.CampaignSystem.CampaignBehaviors.HideoutCampaignBehavior();
            Campaign campaign = Campaign.Current;
            int num = (campaign != null) ? campaign.Models.BanditDensityModel.GetPlayerMaximumTroopCountForHideoutMission(MobileParty.MainParty) : 0;
            args.MenuContext.OpenTroopSelection(MobileParty.MainParty.MemberRoster, troopRoster, Helper.ReflectionCreateDelegate(obj, "CanChangeStatusOfTroop", typeof(Func<CharacterObject, bool>)) as Func<CharacterObject, bool>, new Action<TroopRoster>(this.OnTroopRosterManageDone), num);
        }

        public void Attack_on_consequence(MenuCallbackArgs args)
        {
            int playerMaximumTroopCountForHideoutMission = Campaign.Current.Models.BanditDensityModel.GetPlayerMaximumTroopCountForHideoutMission(MobileParty.MainParty);
            TroopRoster strongestAndPriorTroops = MobilePartyHelper.GetStrongestAndPriorTroops(MobileParty.MainParty, playerMaximumTroopCountForHideoutMission, true);
            if (args.MenuContext.Handler == null)
            {
                return;
            }
            this.SetPrevTroops(playerMaximumTroopCountForHideoutMission, ref strongestAndPriorTroops);
            this.OpenTroopSelection(args, strongestAndPriorTroops, new Action<TroopRoster>(this.OnTroopRosterManageDone));
        }

        private void SetPrevTroops(int playerMaximumTroopCountForHideoutMission, ref TroopRoster troopRoster)
        {
            if (HideoutCampaignBehavior.PrevTroopRoster != null)
            {
                TroopRoster prevTroopRoster = HideoutCampaignBehavior.PrevTroopRoster;
                FlattenedTroopRoster availableTroops = this.GetAvailableTroops(prevTroopRoster, playerMaximumTroopCountForHideoutMission, true);
                prevTroopRoster.Clear();
                prevTroopRoster.Add(availableTroops);
                troopRoster = prevTroopRoster;
            }
        }

        private void OpenTroopSelection(MenuCallbackArgs args, TroopRoster troopRoster, Action<TroopRoster> onTroopRosterManageDone)
        {
            TaleWorlds.CampaignSystem.CampaignBehaviors.HideoutCampaignBehavior obj = new TaleWorlds.CampaignSystem.CampaignBehaviors.HideoutCampaignBehavior();
            Campaign campaign = Campaign.Current;
            int num = (campaign != null) ? campaign.Models.BanditDensityModel.GetPlayerMaximumTroopCountForHideoutMission(MobileParty.MainParty) : 0;
            args.MenuContext.OpenTroopSelection(MobileParty.MainParty.MemberRoster, troopRoster, Helper.ReflectionCreateDelegate(obj, "CanChangeStatusOfTroop", typeof(Func<CharacterObject, bool>)) as Func<CharacterObject, bool>, onTroopRosterManageDone, num);
        }

        private FlattenedTroopRoster GetAvailableTroops(TroopRoster prevRoster, int maxTroopCount, bool includePlayer)
        {
            TroopRoster troopRoster = TroopRoster.CreateDummyTroopRoster();
            FlattenedTroopRoster flattenedTroopRoster = MobileParty.MainParty.MemberRoster.ToFlattenedRoster();
            flattenedTroopRoster.RemoveIf((FlattenedTroopRosterElement x) => x.IsWounded);
            List<CharacterObject> list = (from x in flattenedTroopRoster
                                          select x.Troop into x
                                          orderby x.Level descending
                                          select x).ToList<CharacterObject>();
            if (list.Any((CharacterObject x) => x.IsPlayerCharacter))
            {
                list.Remove(CharacterObject.PlayerCharacter);
                if (includePlayer)
                {
                    troopRoster.AddToCounts(CharacterObject.PlayerCharacter, 1, false, 0, 0, true, -1);
                    maxTroopCount--;
                }
            }
            Dictionary<string, int> dictionary = new Dictionary<string, int>();
            foreach (TroopRosterElement troopRosterElement in prevRoster.GetTroopRoster())
            {
                if (troopRosterElement.Character != null)
                {
                    dictionary[troopRosterElement.Character.StringId] = troopRosterElement.Number;
                }
            }
            int num = 0;
            Dictionary<string, int> dictionary2 = new Dictionary<string, int>();
            foreach (CharacterObject characterObject in list)
            {
                int num2;
                if (dictionary.TryGetValue(characterObject.StringId, out num2))
                {
                    if (!dictionary2.ContainsKey(characterObject.StringId))
                    {
                        dictionary2[characterObject.StringId] = 0;
                    }
                    if (dictionary2[characterObject.StringId] < num2 && num < maxTroopCount)
                    {
                        troopRoster.AddToCounts(characterObject, 1, false, 0, 0, true, -1);
                        Dictionary<string, int> dictionary3 = dictionary2;
                        string stringId = characterObject.StringId;
                        dictionary3[stringId]++;
                        num++;
                    }
                }
            }
            return troopRoster.ToFlattenedRoster();
        }

        private void OnTroopRosterManageDone(TroopRoster hideoutTroops)
        {
            HideoutCampaignBehavior.PrevTroopRoster = hideoutTroops;
            Helper.ReflectionInvokeMethod_Instance(new TaleWorlds.CampaignSystem.CampaignBehaviors.HideoutCampaignBehavior(), "OnTroopRosterManageDone", new object[]
            {
                hideoutTroops
            });
        }

        public void OnLoadFinish()
        {
            HideoutCampaignBehavior.PrevTroopRoster = new HPUHideoutTroopRoster().LoadTroopRoster();
        }
    }
}
