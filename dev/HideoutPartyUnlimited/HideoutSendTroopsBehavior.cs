using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;
using HarmonyLib;
using SandBox;
using SandBox.ViewModelCollection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.Inventory;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Overlay;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.ViewModelCollection.Inventory;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace HideoutPartyUnlimited
{
    public class HideoutSendTroopsBehavior : CampaignBehaviorBase
    {
        public static HideoutSendTroopsBehavior Instance { get; set; } = new HideoutSendTroopsBehavior();

        private HideoutSendTroopsBehavior.HideoutOptinManager _HideoutOptionManager { get; set; }

        public override void RegisterEvents()
        {
            CampaignEvents.OnNewGameCreatedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(this.OnNewGameCreated));
            CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(this.OnGameLoaded));
            CampaignEvents.OnMissionEndedEvent.AddNonSerializedListener(this, new Action<IMission>(this.OnMissionEnded));
        }

        public override void SyncData(IDataStore dataStore)
        {
        }

        private void OnNewGameCreated(CampaignGameStarter campaignGameStarter)
        {
            this.AddGameMenus(campaignGameStarter);
            HideoutSendTroopsBehavior.HideoutState.CurrentState = HideoutSendTroopsBehavior.HideoutState.State.None;
        }

        private void OnGameLoaded(CampaignGameStarter campaignGameStarter)
        {
            this.AddGameMenus(campaignGameStarter);
            HideoutSendTroopsBehavior.HideoutState.CurrentState = HideoutSendTroopsBehavior.HideoutState.State.None;
        }

        private void AddGameMenus(CampaignGameStarter campaignGameStarter)
        {
            HideoutSendTroopsBehavior.HideoutConfig.LoadFile();
            this._HideoutOptionManager = new HideoutSendTroopsBehavior.HideoutOptinManager("hideout_menu_option", "{HIDEOUT_OPTION_TEXT}", new OnInitDelegate(this.game_menu_hideout_option_on_init), 0, 0, null);
            this._HideoutOptionManager.RegisterAddGameMenuOptionRelated("hideout_place", "CallBandits", "Attack (Nearby bandits enter the fray)", "attack", new GameMenuOption.OnConditionDelegate(this.game_menu_attack_hideout_call_bandits_on_condition), new GameMenuOption.OnConsequenceDelegate(this.game_menu_encounter_order_attack_call_bandits_on_consequence), false, -1, false);
            this._HideoutOptionManager.RegisterAddGameMenuOptionRelated("hideout_after_wait", "CallBandits", "Attack (Nearby bandits enter the fray)", "attack", new GameMenuOption.OnConditionDelegate(this.game_menu_attack_hideout_call_bandits_on_condition), new GameMenuOption.OnConsequenceDelegate(this.game_menu_encounter_order_attack_call_bandits_on_consequence), false, -1, false);
            this._HideoutOptionManager.RegisterAddGameMenuOptionRelated("hideout_place", "SendTroops", "{=QfMeoKOm}Send troops.", "CallBandits", new GameMenuOption.OnConditionDelegate(this.game_menu_attack_hideout_parties_on_condition), new GameMenuOption.OnConsequenceDelegate(this.game_menu_encounter_order_attack_on_consequence), false, -1, false);
            this._HideoutOptionManager.RegisterAddGameMenuOptionRelated("hideout_after_wait", "SendTroops", "{=QfMeoKOm}Send troops.", "CallBandits", new GameMenuOption.OnConditionDelegate(this.game_menu_attack_hideout_parties_on_condition), new GameMenuOption.OnConsequenceDelegate(this.game_menu_encounter_order_attack_on_consequence), false, -1, false);
            this._HideoutOptionManager.RegisterAddGameMenuOptionRelated("hideout_place", "_Space_", "", "leave", (MenuCallbackArgs x) => true, null, false, -1, false);
            this._HideoutOptionManager.RegisterAddGameMenuOptionRelated("hideout_after_wait", "_Space_", "", "leave", (MenuCallbackArgs x) => true, null, false, -1, false);
            foreach (string text in HideoutSendTroopsBehavior.HideoutState.DesignContest2.Keys)
            {
                if (Directory.Exists(BasePath.Name + "Modules/HideoutPartyUnlimited/SceneObj/" + HideoutSendTroopsBehavior.HideoutState.DesignContest2[text]))
                {
                    HideoutSendTroopsBehavior.HideoutState.DesignContest2Exist++;
                    this._HideoutOptionManager.RegisterAddGameMenuOptionRelated("hideout_place", text, text, "", new GameMenuOption.OnConditionDelegate(this.game_menu_attack_hideout_Contest2_on_condition), new GameMenuOption.OnConsequenceDelegate(this.game_menu_encounter_order_attack_Contest2_on_consequence), false, -1, false);
                    this._HideoutOptionManager.RegisterAddGameMenuOptionRelated("hideout_after_wait", text, text, "", new GameMenuOption.OnConditionDelegate(this.game_menu_attack_hideout_Contest2_on_condition), new GameMenuOption.OnConsequenceDelegate(this.game_menu_encounter_order_attack_Contest2_on_consequence), false, -1, false);
                }
            }
            this._HideoutOptionManager.RegisterAddGameMenuOptionRelated("hideout_place", "_Space2_", "", "", new GameMenuOption.OnConditionDelegate(this.game_menu_attack_hideout_Contest2_space_on_condition), null, false, -1, false);
            this._HideoutOptionManager.RegisterAddGameMenuOptionRelated("hideout_after_wait", "_Space2_", "", "", new GameMenuOption.OnConditionDelegate(this.game_menu_attack_hideout_Contest2_space_on_condition), null, false, -1, false);
            this._HideoutOptionManager.RegisterAddGameMenuOptionRelated("hideout_place", "Option", "Options", "", new GameMenuOption.OnConditionDelegate(this.game_menu_hideout_option_condition), new GameMenuOption.OnConsequenceDelegate(this.game_menu_hideout_option_consequence), false, -1, false);
            this._HideoutOptionManager.RegisterAddGameMenuOptionRelated("hideout_after_wait", "Option", "Options", "", new GameMenuOption.OnConditionDelegate(this.game_menu_hideout_option_condition), new GameMenuOption.OnConsequenceDelegate(this.game_menu_hideout_option_consequence), false, -1, false);
            this._HideoutOptionManager.RegisterAddGameMenuOption("hideout_menu_option", "SendTroopsOption", "Send troops : {SENDTROOPS_ON_OFF}", "", new GameMenuOption.OnConditionDelegate(this.SendTroops_condition), new GameMenuOption.OnConsequenceDelegate(this.SendTroops_consequence), false, -1, false);
            this._HideoutOptionManager.RegisterAddGameMenuOption("hideout_menu_option", "DayTimeAssault", "Daytime Assault : {DAYTIME_ON_OFF}", "", new GameMenuOption.OnConditionDelegate(this.DaytimeAssault_condition), new GameMenuOption.OnConsequenceDelegate(this.DaytimeAssault_consequence), false, -1, false);
            this._HideoutOptionManager.RegisterAddGameMenuOption("hideout_menu_option", "LeftInventoryMemory", "Memorize item sort(Left Inventory) : {LEFT_INVENTORY_SORT_ON_OFF", "", new GameMenuOption.OnConditionDelegate(this.LeftInventorySort_condition), new GameMenuOption.OnConsequenceDelegate(this.LeftInventorySort_consequence), false, -1, false);
            this._HideoutOptionManager.RegisterAddGameMenuOption("hideout_menu_option", "FightingManyBandits", "Fighting many bandits : {MANY_BANDITS_ON_OFF}", "", new GameMenuOption.OnConditionDelegate(this.ManyBandits_condition), new GameMenuOption.OnConsequenceDelegate(this.ManyBandits_consequence), false, -1, false);
            this._HideoutOptionManager.RegisterAddGameMenuOption("hideout_menu_option", "SceneDesignContest2", "Scene Design Contest 2 : {CONTEST2_ON_OFF}", "", new GameMenuOption.OnConditionDelegate(this.Contest2_condition), new GameMenuOption.OnConsequenceDelegate(this.Contest2_consequence), false, -1, false);
            this._HideoutOptionManager.InteractionOption("SendTroopsFlg", "SendTroopsOption", "hideout_place", "SendTroops");
            this._HideoutOptionManager.InteractionOption("SendTroopsFlg", "SendTroopsOption", "hideout_after_wait", "SendTroops");
            this._HideoutOptionManager.ProcessAddGameMenuOption();
        }

        private void OnMissionEnded(IMission obj)
        {
            if (Settlement.CurrentSettlement != null && Settlement.CurrentSettlement.IsHideout && Settlement.CurrentSettlement == MobileParty.MainParty.CurrentSettlement)
            {
                if (HideoutSendTroopsBehavior.HideoutState.OriginalSceneName != "")
                {
                    Traverse.Create(Settlement.CurrentSettlement.Hideout).Property("SceneName", null).SetValue(HideoutSendTroopsBehavior.HideoutState.OriginalSceneName);
                }
                HideoutSendTroopsBehavior.HideoutState.OriginalSceneName = "";
            }
        }

        private void game_menu_hideout_option_on_init(MenuCallbackArgs args)
        {
            if (Settlement.CurrentSettlement != null && Settlement.CurrentSettlement.IsHideout)
            {
                GameTexts.SetVariable("HIDEOUT_OPTION_TEXT", "Hideout Party Unlimited - Options");
                if (HideoutSendTroopsBehavior.HideoutConfig.Config == null)
                {
                    HideoutSendTroopsBehavior.HideoutConfig.LoadFile();
                }
            }
        }

        private bool game_menu_hideout_option_condition(MenuCallbackArgs args)
        {
            args.optionLeaveType = (GameMenuOption.LeaveType)18;
            return true;
        }

        private void game_menu_hideout_option_consequence(MenuCallbackArgs args)
        {
            this._HideoutOptionManager.MemoMenuStringID = args.MenuContext.GameMenu.StringId;
            GameMenu.SwitchToMenu("hideout_menu_option");
        }

        private bool SendTroops_condition(MenuCallbackArgs args)
        {
            args.optionLeaveType = (GameMenuOption.LeaveType)10;
            args.Tooltip = new TextObject("Add an automatic combat option.", null);
            this.TextSetSwitch("SENDTROOPS_ON_OFF", HideoutSendTroopsBehavior.HideoutConfig.Config.SendTroopsFlg);
            return true;
        }

        private void SendTroops_consequence(MenuCallbackArgs args)
        {
            HideoutSendTroopsBehavior.HideoutConfig.HideoutOption config = HideoutSendTroopsBehavior.HideoutConfig.Config;
            config.SendTroopsFlg = !config.SendTroopsFlg;
            GameMenu.SwitchToMenu("hideout_menu_option");
        }

        private void TextSetSwitch(string textID, bool flg)
        {
            if (flg)
            {
                GameTexts.SetVariable(textID, "ON");
                return;
            }
            GameTexts.SetVariable(textID, "OFF");
        }

        private bool DaytimeAssault_condition(MenuCallbackArgs args)
        {
            args.optionLeaveType = (GameMenuOption.LeaveType)12;
            args.Tooltip = new TextObject("ON : Attack at any time\nOFF: Attack at night", null);
            this.TextSetSwitch("DAYTIME_ON_OFF", HideoutSendTroopsBehavior.HideoutConfig.Config.DayTimeHideoutAssaultFlg);
            return true;
        }

        private void DaytimeAssault_consequence(MenuCallbackArgs args)
        {
            HideoutSendTroopsBehavior.HideoutConfig.HideoutOption config = HideoutSendTroopsBehavior.HideoutConfig.Config;
            config.DayTimeHideoutAssaultFlg = !config.DayTimeHideoutAssaultFlg;
            GameMenu.SwitchToMenu("hideout_menu_option");
        }

        private bool LeftInventorySort_condition(MenuCallbackArgs args)
        {
            args.optionLeaveType = (GameMenuOption.LeaveType)14;
            this.TextSetSwitch("LEFT_INVENTORY_SORT_ON_OFF", HideoutSendTroopsBehavior.HideoutConfig.Config.LeftInventorySortMemorizeFlg);
            return true;
        }

        private void LeftInventorySort_consequence(MenuCallbackArgs args)
        {
            HideoutSendTroopsBehavior.HideoutConfig.HideoutOption config = HideoutSendTroopsBehavior.HideoutConfig.Config;
            config.LeftInventorySortMemorizeFlg = !config.LeftInventorySortMemorizeFlg;
            GameMenu.SwitchToMenu("hideout_menu_option");
        }

        private bool ManyBandits_condition(MenuCallbackArgs args)
        {
            args.optionLeaveType = (GameMenuOption.LeaveType)12;
            this.TextSetSwitch("MANY_BANDITS_ON_OFF", HideoutSendTroopsBehavior.HideoutConfig.Config.FightingManyBanditsFlg);
            return true;
        }

        private void ManyBandits_consequence(MenuCallbackArgs args)
        {
            HideoutSendTroopsBehavior.HideoutConfig.HideoutOption config = HideoutSendTroopsBehavior.HideoutConfig.Config;
            config.FightingManyBanditsFlg = !config.FightingManyBanditsFlg;
            GameMenu.SwitchToMenu("hideout_menu_option");
        }

        private bool Contest2_condition(MenuCallbackArgs args)
        {
            args.optionLeaveType = (GameMenuOption.LeaveType)14;
            this.TextSetSwitch("CONTEST2_ON_OFF", HideoutSendTroopsBehavior.HideoutConfig.Config.SceneDesignContest2Flg);
            return true;
        }

        private void Contest2_consequence(MenuCallbackArgs args)
        {
            if (HideoutSendTroopsBehavior.HideoutState.DesignContest2Exist > 0)
            {
                HideoutSendTroopsBehavior.HideoutConfig.HideoutOption config = HideoutSendTroopsBehavior.HideoutConfig.Config;
                config.SceneDesignContest2Flg = !config.SceneDesignContest2Flg;
            }
            else
            {
                HideoutSendTroopsBehavior.HideoutConfig.Config.SceneDesignContest2Flg = false;
                InformationManager.ShowInquiry(new InquiryData("Information", "Contest data are not available.\nPlease download the pack from the following page.\n\nBannerlord Creative Competition 2 Winners\nhttps://www.taleworlds.com/en/News/519", true, false, "OK", "", null, null, "", 0f, null), false, false);
            }
            GameMenu.SwitchToMenu("hideout_menu_option");
        }

        private bool game_menu_attack_hideout_parties_on_condition(MenuCallbackArgs args)
        {
            args.optionLeaveType = (GameMenuOption.LeaveType)10;
            Hideout hideout = Settlement.CurrentSettlement.Hideout;
            if (!Hero.MainHero.IsWounded && Settlement.CurrentSettlement.MapFaction != PartyBase.MainParty.MapFaction)
            {
                if (Settlement.CurrentSettlement.Parties.Any((MobileParty x) => x.IsBandit) && hideout.NextPossibleAttackTime.IsPast)
                {
                    return (bool)Helper.ReflectionInvokeMethod_Instance((from x in Campaign.Current.SandBoxManager.GameStarter.CampaignBehaviors
                                                                         where x is TaleWorlds.CampaignSystem.CampaignBehaviors.HideoutCampaignBehavior
                                                                         select x).FirstOrDefault() as TaleWorlds.CampaignSystem.CampaignBehaviors.HideoutCampaignBehavior, "IsHideoutAttackableNow", new object[0]);
                }
            }
            return false;
        }

        private void game_menu_encounter_order_attack_on_consequence(MenuCallbackArgs args)
        {
            Helper.ReflectionInvokeMethod_Instance((from x in Campaign.Current.SandBoxManager.GameStarter.CampaignBehaviors
                                                    where x is TaleWorlds.CampaignSystem.CampaignBehaviors.HideoutCampaignBehavior
                                                    select x).FirstOrDefault() as TaleWorlds.CampaignSystem.CampaignBehaviors.HideoutCampaignBehavior, "ArrangeHideoutTroopCountsForMission", new object[0]);
            Settlement.CurrentSettlement.Hideout.UpdateNextPossibleAttackTime();
            if (PlayerEncounter.IsActive)
            {
                PlayerEncounter.LeaveEncounter = false;
            }
            else
            {
                PlayerEncounter.Start();
                PlayerEncounter.Current.SetupFields(PartyBase.MainParty, Settlement.CurrentSettlement.Party);
            }
            if (PlayerEncounter.Battle != null)
            {
                Traverse.Create(PlayerEncounter.Current).Field("_mapEvent").SetValue(null);
                Traverse.Create(PlayerEncounter.Current).Property("EncounterState", null).SetValue(0);
            }
            if (PlayerEncounter.Battle == null)
            {
                PlayerEncounter.InitSimulation(PartyBase.MainParty.MemberRoster.ToFlattenedRoster(), Settlement.CurrentSettlement.Party.MemberRoster.ToFlattenedRoster());
                if (PlayerEncounter.Current != null && PlayerEncounter.Current.BattleSimulation != null)
                {
                    MenuContext menuContext = args.MenuContext;
                    ((MapState)Game.Current.GameStateManager.ActiveState).StartBattleSimulation();
                }
            }
        }

        private bool game_menu_attack_hideout_call_bandits_on_condition(MenuCallbackArgs args)
        {
            if (HideoutSendTroopsBehavior.HideoutConfig.Config.FightingManyBanditsFlg)
            {
                args.optionLeaveType = (GameMenuOption.LeaveType)12;
                Hideout hideout = Settlement.CurrentSettlement.Hideout;
                if (!Hero.MainHero.IsWounded && Settlement.CurrentSettlement.MapFaction != PartyBase.MainParty.MapFaction)
                {
                    if (Settlement.CurrentSettlement.Parties.Any((MobileParty x) => x.IsBandit) && hideout.NextPossibleAttackTime.IsPast)
                    {
                        return (bool)Helper.ReflectionInvokeMethod_Instance((from x in Campaign.Current.SandBoxManager.GameStarter.CampaignBehaviors
                                                                             where x is TaleWorlds.CampaignSystem.CampaignBehaviors.HideoutCampaignBehavior
                                                                             select x).FirstOrDefault() as TaleWorlds.CampaignSystem.CampaignBehaviors.HideoutCampaignBehavior, "IsHideoutAttackableNow", new object[0]);
                    }
                }
            }
            return false;
        }

        private void game_menu_encounter_order_attack_call_bandits_on_consequence(MenuCallbackArgs args)
        {
            HideoutCampaignBehavior campaignBehavior = Campaign.Current.GetCampaignBehavior<HideoutCampaignBehavior>();
            if (HideoutSendTroopsBehavior.HideoutConfig.Config.FightingManyBanditsFlg)
            {
                HideoutSendTroopsBehavior.HideoutState.CurrentState = HideoutSendTroopsBehavior.HideoutState.State.CallBandits;
            }
            campaignBehavior.Attack_on_consequence(args);
        }

        private bool game_menu_attack_hideout_Contest2_on_condition(MenuCallbackArgs args)
        {
            if (HideoutSendTroopsBehavior.HideoutConfig.Config.SceneDesignContest2Flg)
            {
                args.optionLeaveType = (GameMenuOption.LeaveType)12;
                Hideout hideout = Settlement.CurrentSettlement.Hideout;
                if (!Hero.MainHero.IsWounded && Settlement.CurrentSettlement.MapFaction != PartyBase.MainParty.MapFaction)
                {
                    if (Settlement.CurrentSettlement.Parties.Any((MobileParty x) => x.IsBandit) && hideout.NextPossibleAttackTime.IsPast)
                    {
                        return (bool)Helper.ReflectionInvokeMethod_Instance((from x in Campaign.Current.SandBoxManager.GameStarter.CampaignBehaviors
                                                                             where x is TaleWorlds.CampaignSystem.CampaignBehaviors.HideoutCampaignBehavior
                                                                             select x).FirstOrDefault() as TaleWorlds.CampaignSystem.CampaignBehaviors.HideoutCampaignBehavior, "IsHideoutAttackableNow", new object[0]);
                    }
                }
            }
            return false;
        }

        private void game_menu_encounter_order_attack_Contest2_on_consequence(MenuCallbackArgs args)
        {
            HideoutCampaignBehavior campaignBehavior = Campaign.Current.GetCampaignBehavior<HideoutCampaignBehavior>();
            if (HideoutSendTroopsBehavior.HideoutConfig.Config.FightingManyBanditsFlg)
            {
                HideoutSendTroopsBehavior.HideoutState.CurrentState = HideoutSendTroopsBehavior.HideoutState.State.CallBandits;
            }
            HideoutSendTroopsBehavior.HideoutState.OriginalSceneName = Traverse.Create(Settlement.CurrentSettlement.Hideout).Property("SceneName", null).GetValue<string>();
            string value;
            if (HideoutSendTroopsBehavior.HideoutState.DesignContest2.TryGetValue(args.Text.ToString(), out value))
            {
                Traverse.Create(Settlement.CurrentSettlement.Hideout).Property("SceneName", null).SetValue(value);
            }
            campaignBehavior.Attack_on_consequence(args);
        }

        private bool game_menu_attack_hideout_Contest2_space_on_condition(MenuCallbackArgs args)
        {
            if (HideoutSendTroopsBehavior.HideoutConfig.Config.SceneDesignContest2Flg)
            {
                Hideout hideout = Settlement.CurrentSettlement.Hideout;
                if (!Hero.MainHero.IsWounded && Settlement.CurrentSettlement.MapFaction != PartyBase.MainParty.MapFaction)
                {
                    if (Settlement.CurrentSettlement.Parties.Any((MobileParty x) => x.IsBandit) && hideout.NextPossibleAttackTime.IsPast)
                    {
                        return (bool)Helper.ReflectionInvokeMethod_Instance((from x in Campaign.Current.SandBoxManager.GameStarter.CampaignBehaviors
                                                                             where x is TaleWorlds.CampaignSystem.CampaignBehaviors.HideoutCampaignBehavior
                                                                             select x).FirstOrDefault() as TaleWorlds.CampaignSystem.CampaignBehaviors.HideoutCampaignBehavior, "IsHideoutAttackableNow", new object[0]);
                    }
                }
            }
            return false;
        }

        [HarmonyPatch(typeof(SandBoxMissions), "GetPriorityListForHideoutMission")]
        private class HPUGetPriorityListForHideoutMissionPatch
        {
            private static void Prefix(MapEvent playerMapEvent, BattleSideEnum side, ref int firstPhaseTroopCount)
            {
                if (HideoutSendTroopsBehavior.HideoutConfig.Config.FightingManyBanditsFlg && HideoutSendTroopsBehavior.HideoutState.CurrentState == HideoutSendTroopsBehavior.HideoutState.State.CallBandits)
                {
                    HideoutSendTroopsBehavior.HideoutState.CurrentState = HideoutSendTroopsBehavior.HideoutState.State.None;
                    
                    foreach (MobileParty mobileParty in Campaign.Current.BanditParties)
                    //MobilePartiesAroundPositionList(32).GetPartiesAroundPosition(playerMapEvent.Position, 65f))
                    {
                        //if (mobileParty.IsBandit && !mobileParty.IsBanditBossParty && !mobileParty.IsCurrentlyUsedByAQuest)
                        var dist = mobileParty.Position2D.Distance(Campaign.Current.MainParty.Position2D);
                        if (dist < 65.01f && !mobileParty.IsCurrentlyUsedByAQuest && !mobileParty.IsBanditBossParty)
                        {
                            mobileParty.MapEventSide = playerMapEvent.DefenderSide;
                        }
                    }
                }
            }

            private static void Finalizer(Exception __exception)
            {
                if (__exception != null)
                {
                    MessageBox.Show(__exception.FlattenException());
                }
            }
        }

        [HarmonyPatch(typeof(BattleSimulation), "OnReturn")]
        private class HPUBattleSimulationPatch
        {
            private static bool Prefix(BattleSimulation __instance)
            {
                if (Settlement.CurrentSettlement != null && Settlement.CurrentSettlement.IsHideout)
                {
                    foreach (PartyBase partyBase in Traverse.Create(__instance).Field("_mapEvent").GetValue<MapEvent>().InvolvedParties)
                    {
                        partyBase.MemberRoster.RemoveZeroCounts();
                    }
                    PlayerEncounter.Update();
                    BattleState battleState = HideoutSendTroopsBehavior.HPUBattleSimulationPatch.ConvertToBattleState(__instance);
                    Helper.ReflectionSetFieldPropertyValue_Instance(PlayerEncounter.Battle, "BattleState", battleState);
                    CampaignEventDispatcher.Instance.OnMissionEnded(Mission.Current);
                    GameMenu.SwitchToMenu("hideout_place");
                    return false;
                }
                return true;
            }

            private static BattleState ConvertToBattleState(BattleSimulation battleSimulation)
            {
                if (battleSimulation.BattleObserver is SPScoreboardVM)
                {
                    switch ((battleSimulation.BattleObserver as SPScoreboardVM).BattleResultIndex)
                    {
                        case -1:
                            return 0;
                        case 0:
                            if (PlayerEncounter.Battle.PlayerSide == (BattleSideEnum)1)
                            {
                                return (BattleState)1;
                            }
                            if (PlayerEncounter.Battle.PlayerSide == null)
                            {
                                return (BattleState)2;
                            }
                            return 0;
                        case 1:
                            if (PlayerEncounter.Battle.PlayerSide == (BattleSideEnum)1)
                            {
                                return (BattleState)2;
                            }
                            if (PlayerEncounter.Battle.PlayerSide == null)
                            {
                                return (BattleState)1;
                            }
                            return (BattleState)0;
                        case 2:
                            return (BattleState)0;
                    }
                }
                return 0;
            }

            private static void Finalizer(Exception __exception)
            {
                if (__exception != null)
                {
                    MessageBox.Show(__exception.FlattenException());
                }
            }
        }

        public class HideoutOptinManager
        {
            private HideoutSendTroopsBehavior.HideoutOptinManager.Menu OneMenu { get; set; }

            private List<HideoutSendTroopsBehavior.HideoutOptinManager.Option> OptionList { get; set; } = new List<HideoutSendTroopsBehavior.HideoutOptinManager.Option>();

            private List<HideoutSendTroopsBehavior.HideoutOptinManager.Option> RelatedOptionList { get; set; } = new List<HideoutSendTroopsBehavior.HideoutOptinManager.Option>();

            private List<Tuple<string, HideoutSendTroopsBehavior.HideoutOptinManager.Option, HideoutSendTroopsBehavior.HideoutOptinManager.Option>> InteractionOptionList { get; set; } = new List<Tuple<string, HideoutSendTroopsBehavior.HideoutOptinManager.Option, HideoutSendTroopsBehavior.HideoutOptinManager.Option>>();

            public string MemoMenuStringID { get; set; }

            public HideoutOptinManager(string menuId, string menuText, OnInitDelegate initDelegate, GameOverlays.MenuOverlayType overlay = 0, GameMenu.MenuFlags menuFlags = 0, object relatedObject = null)
            {
                this.OneMenu = new HideoutSendTroopsBehavior.HideoutOptinManager.Menu(menuId, menuText, initDelegate, overlay, menuFlags, relatedObject);
            }

            public void RegisterAddGameMenuOption(string menuId, string optionId, string optionText, string insertTargetId, GameMenuOption.OnConditionDelegate condition, GameMenuOption.OnConsequenceDelegate consequence, bool isLeave = false, int index = -1, bool isRepeatable = false)
            {
                this.OptionList.Add(new HideoutSendTroopsBehavior.HideoutOptinManager.Option(menuId, optionId, optionText, insertTargetId, condition, consequence, isLeave, index, isRepeatable));
            }

            public void RegisterAddGameMenuOptionRelated(string menuId, string optionId, string optionText, string insertTargetId, GameMenuOption.OnConditionDelegate condition, GameMenuOption.OnConsequenceDelegate consequence, bool isLeave = false, int index = -1, bool isRepeatable = false)
            {
                this.RelatedOptionList.Add(new HideoutSendTroopsBehavior.HideoutOptinManager.Option(menuId, optionId, optionText, insertTargetId, condition, consequence, isLeave, index, isRepeatable));
            }

            public void InteractionOption(string hideoutOption, string optionId, string otherMenuId, string otherOptionId)
            {
                HideoutSendTroopsBehavior.HideoutOptinManager.Option item = this.OptionList.Find((HideoutSendTroopsBehavior.HideoutOptinManager.Option x) => x.OptionId == optionId);
                HideoutSendTroopsBehavior.HideoutOptinManager.Option item2 = this.RelatedOptionList.Find((HideoutSendTroopsBehavior.HideoutOptinManager.Option x) => x.MenuId == otherMenuId && x.OptionId == otherOptionId);
                this.InteractionOptionList.Add(new Tuple<string, HideoutSendTroopsBehavior.HideoutOptinManager.Option, HideoutSendTroopsBehavior.HideoutOptinManager.Option>(hideoutOption, item, item2));
            }

            public void ProcessAddGameMenuOption()
            {
                PropertyInfo[] properties = typeof(HideoutSendTroopsBehavior.HideoutConfig.HideoutOption).GetProperties();
                CampaignGameStarter gameStarter = Campaign.Current.SandBoxManager.GameStarter;
                PropertyInfo[] array = properties;
                for (int i = 0; i < array.Length; i++)
                {
                    PropertyInfo info = array[i];
                    if (info.PropertyType.Name == typeof(bool).Name)
                    {
                        List<Tuple<string, HideoutSendTroopsBehavior.HideoutOptinManager.Option, HideoutSendTroopsBehavior.HideoutOptinManager.Option>> list = this.InteractionOptionList.FindAll((Tuple<string, HideoutSendTroopsBehavior.HideoutOptinManager.Option, HideoutSendTroopsBehavior.HideoutOptinManager.Option> x) => x.Item1 == info.Name);
                        if (list != null && list.Count > 0 && (bool)info.GetValue(HideoutSendTroopsBehavior.HideoutConfig.Config))
                        {
                            foreach (Tuple<string, HideoutSendTroopsBehavior.HideoutOptinManager.Option, HideoutSendTroopsBehavior.HideoutOptinManager.Option> tuple in list)
                            {
                                if (!this.CheckGameMenuOption(tuple.Item3.MenuId, tuple.Item3.OptionId))
                                {
                                    HideoutSendTroopsBehavior.HideoutOptinManager.Option item = tuple.Item3;
                                    gameStarter.AddGameMenuOption(item.MenuId, item.OptionId, item.OptionText, item.Condition, item.Consequence, item.IsLeave, item.Index, item.IsRepeatable);
                                }
                            }
                        }
                    }
                }
                using (List<HideoutSendTroopsBehavior.HideoutOptinManager.Option>.Enumerator enumerator2 = this.RelatedOptionList.GetEnumerator())
                {
                    while (enumerator2.MoveNext())
                    {
                        HideoutSendTroopsBehavior.HideoutOptinManager.Option otherOption = enumerator2.Current;
                        if (this.InteractionOptionList.FindIndex((Tuple<string, HideoutSendTroopsBehavior.HideoutOptinManager.Option, HideoutSendTroopsBehavior.HideoutOptinManager.Option> x) => x.Item3.MenuId == otherOption.MenuId && x.Item3.OptionId == otherOption.OptionId) < 0)
                        {
                            gameStarter.AddGameMenuOption(otherOption.MenuId, otherOption.OptionId, otherOption.OptionText, otherOption.Condition, otherOption.Consequence, otherOption.IsLeave, otherOption.Index, otherOption.IsRepeatable);
                        }
                    }
                }
                foreach (HideoutSendTroopsBehavior.HideoutOptinManager.Option option in this.RelatedOptionList)
                {
                    this.InsertAfterGameMenuOptionSort(option.MenuId, option.InsertTargetId, option.OptionId);
                }
                gameStarter.AddGameMenu(this.OneMenu.MenuId, this.OneMenu.MenuText, this.OneMenu.InitDelegate, this.OneMenu.Overlay, this.OneMenu.MenuFlags, this.OneMenu.RelatedObject);
                foreach (HideoutSendTroopsBehavior.HideoutOptinManager.Option option2 in this.OptionList)
                {
                    gameStarter.AddGameMenuOption(option2.MenuId, option2.OptionId, option2.OptionText, option2.Condition, option2.Consequence, option2.IsLeave, option2.Index, option2.IsRepeatable);
                }
                foreach (HideoutSendTroopsBehavior.HideoutOptinManager.Option option3 in this.OptionList)
                {
                    this.InsertAfterGameMenuOptionSort(option3.MenuId, option3.InsertTargetId, option3.OptionId);
                }
                gameStarter.AddGameMenuOption(this.OneMenu.MenuId, "OptionBack", "Back", new GameMenuOption.OnConditionDelegate(this.OptionBack_condition), new GameMenuOption.OnConsequenceDelegate(this.ProcessBack), false, -1, false);
            }

            private bool OptionBack_condition(MenuCallbackArgs args)
            {
                args.optionLeaveType = (GameMenuOption.LeaveType)16;
                return true;
            }

            public void ProcessBack(MenuCallbackArgs args)
            {
                HideoutSendTroopsBehavior.HideoutConfig.SaveFile();
                PropertyInfo[] properties = typeof(HideoutSendTroopsBehavior.HideoutConfig.HideoutOption).GetProperties();
                for (int i = 0; i < properties.Length; i++)
                {
                    PropertyInfo info = properties[i];
                    if (info.PropertyType.Name == typeof(bool).Name)
                    {
                        List<Tuple<string, HideoutSendTroopsBehavior.HideoutOptinManager.Option, HideoutSendTroopsBehavior.HideoutOptinManager.Option>> list = this.InteractionOptionList.FindAll((Tuple<string, HideoutSendTroopsBehavior.HideoutOptinManager.Option, HideoutSendTroopsBehavior.HideoutOptinManager.Option> x) => x.Item1 == info.Name);
                        if (list != null && list.Count > 0)
                        {
                            if ((bool)info.GetValue(HideoutSendTroopsBehavior.HideoutConfig.Config))
                            {
                                foreach (Tuple<string, HideoutSendTroopsBehavior.HideoutOptinManager.Option, HideoutSendTroopsBehavior.HideoutOptinManager.Option> tuple in list)
                                {
                                    if (!this.CheckGameMenuOption(tuple.Item3.MenuId, tuple.Item3.OptionId))
                                    {
                                        CampaignGameStarter gameStarter = Campaign.Current.SandBoxManager.GameStarter;
                                        HideoutSendTroopsBehavior.HideoutOptinManager.Option item = tuple.Item3;
                                        gameStarter.AddGameMenuOption(item.MenuId, item.OptionId, item.OptionText, item.Condition, item.Consequence, item.IsLeave, item.Index, item.IsRepeatable);
                                    }
                                }
                                using (List<Tuple<string, HideoutSendTroopsBehavior.HideoutOptinManager.Option, HideoutSendTroopsBehavior.HideoutOptinManager.Option>>.Enumerator enumerator = list.GetEnumerator())
                                {
                                    while (enumerator.MoveNext())
                                    {
                                        Tuple<string, HideoutSendTroopsBehavior.HideoutOptinManager.Option, HideoutSendTroopsBehavior.HideoutOptinManager.Option> tuple2 = enumerator.Current;
                                        HideoutSendTroopsBehavior.HideoutOptinManager.Option item2 = tuple2.Item3;
                                        this.InsertAfterGameMenuOptionSort(item2.MenuId, item2.InsertTargetId, item2.OptionId);
                                    }
                                    goto IL_1CD;
                                }
                            }
                            foreach (Tuple<string, HideoutSendTroopsBehavior.HideoutOptinManager.Option, HideoutSendTroopsBehavior.HideoutOptinManager.Option> tuple3 in list)
                            {
                                HideoutSendTroopsBehavior.HideoutOptinManager.Option item3 = tuple3.Item3;
                                this.DeleteGameMenuOption(item3.MenuId, item3.OptionId);
                            }
                        }
                    }
                IL_1CD:;
                }
                GameMenu.SwitchToMenu(this.MemoMenuStringID);
            }

            private void InsertAfterGameMenuOptionSort(string menuId, string targetIdString, string moveIdString)
            {
                List<GameMenuOption> list = Helper.ReflectionGetField_Instance(Helper.ReflectionInvokeMethod_Instance(Helper.ReflectionGetField_Instance(Campaign.Current.SandBoxManager.GameStarter, "_gameMenuManager") as GameMenuManager, "GetGameMenu", new object[]
                {
                    menuId
                }) as GameMenu, "_menuItems") as List<GameMenuOption>;
                int num = list.FindIndex((GameMenuOption x) => x.IdString == targetIdString);
                int num2 = list.FindIndex((GameMenuOption x) => x.IdString == moveIdString);
                if (num >= 0 && num2 >= 0)
                {
                    GameMenuOption item = list[num2];
                    list.Remove(item);
                    if (num + 1 < list.Count)
                    {
                        list.Insert(num + 1, item);
                        return;
                    }
                    list.Add(item);
                }
            }

            private void DeleteGameMenuOption(string menuId, string targetIdString)
            {
                List<GameMenuOption> list = Helper.ReflectionGetField_Instance(Helper.ReflectionInvokeMethod_Instance(Campaign.Current.GameMenuManager, "GetGameMenu", new object[]
                {
                    menuId
                }) as GameMenu, "_menuItems") as List<GameMenuOption>;
                int num = list.FindIndex((GameMenuOption x) => x.IdString == targetIdString);
                if (num >= 0)
                {
                    GameMenuOption item = list[num];
                    list.Remove(item);
                }
            }

            private bool CheckGameMenuOption(string menuId, string targetIdString)
            {
                return (Helper.ReflectionGetField_Instance(Helper.ReflectionInvokeMethod_Instance(Campaign.Current.GameMenuManager, "GetGameMenu", new object[]
                {
                    menuId
                }) as GameMenu, "_menuItems") as List<GameMenuOption>).FindIndex((GameMenuOption x) => x.IdString == targetIdString) >= 0;
            }

            private class Option
            {
                public string MenuId { get; set; }

                public string OptionId { get; set; }

                public string OptionText { get; set; }

                public string InsertTargetId { get; set; }

                public GameMenuOption.OnConditionDelegate Condition { get; set; }

                public GameMenuOption.OnConsequenceDelegate Consequence { get; set; }

                public bool IsLeave { get; set; }

                public int Index { get; set; }

                public bool IsRepeatable { get; set; }

                public Option(string menuId, string optionId, string optionText, string insertTargetId, GameMenuOption.OnConditionDelegate condition, GameMenuOption.OnConsequenceDelegate consequence, bool isLeave = false, int index = -1, bool isRepeatable = false)
                {
                    this.MenuId = menuId;
                    this.OptionId = optionId;
                    this.OptionText = optionText;
                    this.InsertTargetId = insertTargetId;
                    this.Condition = condition;
                    this.Consequence = consequence;
                    this.IsLeave = isLeave;
                    this.Index = index;
                    this.IsRepeatable = isRepeatable;
                }
            }

            private class Menu
            {
                public string MenuId { get; set; }

                public string MenuText { get; set; }

                public OnInitDelegate InitDelegate { get; set; }

                public GameOverlays.MenuOverlayType Overlay { get; set; }

                public GameMenu.MenuFlags MenuFlags { get; set; }

                public object RelatedObject { get; set; }

                public Menu(string menuId, string menuText, OnInitDelegate initDelegate, GameOverlays.MenuOverlayType overlay = 0, GameMenu.MenuFlags menuFlags = 0, object relatedObject = null)
                {
                    this.MenuId = menuId;
                    this.MenuText = menuText;
                    this.InitDelegate = initDelegate;
                    this.Overlay = overlay;
                    this.MenuFlags = menuFlags;
                    this.RelatedObject = relatedObject;
                }
            }
        }

        public static class HideoutConfig
        {
            private static string SendTroopsConfigPath { get; set; } = BasePath.Name + "Modules/HideoutPartyUnlimited/Config.xml";

            public static HideoutSendTroopsBehavior.HideoutConfig.HideoutOption Config { get; set; } = null;

            public static void LoadFile()
            {
                HideoutSendTroopsBehavior.HideoutConfig.Config = HideoutSendTroopsBehavior.HideoutConfig.XMLLoad(HideoutSendTroopsBehavior.HideoutConfig.SendTroopsConfigPath);
            }

            public static void SaveFile()
            {
                HideoutSendTroopsBehavior.HideoutConfig.XMLSave(HideoutSendTroopsBehavior.HideoutConfig.SendTroopsConfigPath, HideoutSendTroopsBehavior.HideoutConfig.Config);
            }

            private static void XMLSave(string path, HideoutSendTroopsBehavior.HideoutConfig.HideoutOption option)
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(HideoutSendTroopsBehavior.HideoutConfig.HideoutOption));
                using (StreamWriter streamWriter = new StreamWriter(path, false, Encoding.UTF8))
                {
                    xmlSerializer.Serialize(streamWriter, option);
                }
            }
            
            private static HideoutSendTroopsBehavior.HideoutConfig.HideoutOption XMLLoad(string path)
            {
                HideoutSendTroopsBehavior.HideoutConfig.FileExistCheck(path);
                HideoutSendTroopsBehavior.HideoutConfig.HideoutOption result = null;
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(HideoutSendTroopsBehavior.HideoutConfig.HideoutOption));
                using (StreamReader streamReader = new StreamReader(path, Encoding.UTF8))
                {
                    result = (xmlSerializer.Deserialize(streamReader) as HideoutSendTroopsBehavior.HideoutConfig.HideoutOption);
                }
                return result;
            }

            private static void FileExistCheck(string path)
            {
                if (!File.Exists(path))
                {
                    HideoutSendTroopsBehavior.HideoutConfig.XMLSave(path, new HideoutSendTroopsBehavior.HideoutConfig.HideoutOption());
                }
            }

            public class HideoutOption
            {
                public bool SendTroopsFlg { get; set; }

                public bool DayTimeHideoutAssaultFlg { get; set; }

                public bool LeftInventorySortMemorizeFlg { get; set; }

                public bool FightingManyBanditsFlg { get; set; }

                public bool SceneDesignContest2Flg { get; set; }
            }
        }

        public static class HideoutState
        {
            public static HideoutSendTroopsBehavior.HideoutState.State CurrentState { get; set; } = HideoutSendTroopsBehavior.HideoutState.State.None;

            public static int DesignContest2Exist { get; set; } = 0;

            public static Dictionary<string, string> DesignContest2 { get; set; } = new Dictionary<string, string>
            {
                {
                    "Valeria Lookout",
                    "Scene_Levente"
                },
                {
                    "Battanian Burial Mound",
                    "battanian_burial_mound"
                },
                {
                    "Abandoned Mine",
                    "patatest"
                },
                {
                    "OldVlandiaTown",
                    "OldVlandiaTown"
                },
                {
                    "Urban Hideout",
                    "scn_hideout_urban_vlandia"
                },
                {
                    "Canyon",
                    "Canyon"
                },
                {
                    "Smuggler's Descent",
                    "scn_smugglersdescent1"
                },
                {
                    "Ruined Castle Hideout",
                    "forest_complete"
                },
                {
                    "Brigands Keep",
                    "Scene_Ceftadzime"
                },
                {
                    "Shipwreck Cove",
                    "contest_shipwreck_cove"
                },
                {
                    "SwampMountain Dwelling",
                    "SwampMountainDwelling"
                },
                {
                    "Ruins of Erandel",
                    "Ruins_of_Erandel"
                },
                {
                    "RuinSeaside",
                    "RuinSeaside"
                },
                {
                    "Forest Ruins",
                    "Myscenehideout"
                }
            };

            public static string OriginalSceneName { get; set; } = "";

            public enum State
            {
                None,
                Attack,
                SendTroops,
                CallBandits
            }
        }

        [HarmonyPatch(typeof(Hideout), "NextPossibleAttackTime", MethodType.Getter)]
        private class HideoutNextPossibleAttackTimePatch
        {
            private static CampaignTime SaveNextPossibleAttackTime { get; set; } = CampaignTime.Zero;

            private static void Postfix(ref CampaignTime __result)
            {
                if (HideoutSendTroopsBehavior.HideoutConfig.Config.DayTimeHideoutAssaultFlg)
                {
                    Settlement currentSettlement = Settlement.CurrentSettlement;
                    Hideout hideout = (currentSettlement != null) ? currentSettlement.Hideout : null;
                    if (hideout != null)
                    {
                        Traverse traverse = Traverse.Create(hideout).Field("_nextPossibleAttackTime");
                        if (traverse.GetValue<CampaignTime>() != CampaignTime.Now - CampaignTime.Hours(1f))
                        {
                            HideoutSendTroopsBehavior.HideoutNextPossibleAttackTimePatch.SaveNextPossibleAttackTime = traverse.GetValue<CampaignTime>();
                            traverse.SetValue(CampaignTime.Now - CampaignTime.Hours(1f));
                            __result = traverse.GetValue<CampaignTime>();
                            return;
                        }
                    }
                }
                else if (HideoutSendTroopsBehavior.HideoutNextPossibleAttackTimePatch.SaveNextPossibleAttackTime != CampaignTime.Zero)
                {
                    Settlement currentSettlement2 = Settlement.CurrentSettlement;
                    Hideout hideout2 = (currentSettlement2 != null) ? currentSettlement2.Hideout : null;
                    if (hideout2 != null)
                    {
                        Traverse traverse2 = Traverse.Create(hideout2).Field("_nextPossibleAttackTime");
                        if (traverse2.GetValue<CampaignTime>() < HideoutSendTroopsBehavior.HideoutNextPossibleAttackTimePatch.SaveNextPossibleAttackTime)
                        {
                            traverse2.SetValue(HideoutSendTroopsBehavior.HideoutNextPossibleAttackTimePatch.SaveNextPossibleAttackTime);
                            __result = traverse2.GetValue<CampaignTime>();
                        }
                    }
                }
            }

            private static void Finalizer(Exception __exception)
            {
                if (__exception != null)
                {
                    MessageBox.Show(__exception.FlattenException());
                }
            }
        }

        [HarmonyPatch(typeof(TaleWorlds.CampaignSystem.CampaignBehaviors.HideoutCampaignBehavior), "IsHideoutAttackableNow")]
        private class HideoutCampaignBehaviorIsHideoutAttackableNowPatch
        {
            private static int SaveStart { get; set; }

            private static int SaveEnd { get; set; }

            private static void Prefix(TaleWorlds.CampaignSystem.CampaignBehaviors.HideoutCampaignBehavior __instance)
            {
                if (HideoutSendTroopsBehavior.HideoutConfig.Config.DayTimeHideoutAssaultFlg)
                {
                    Traverse traverse = Traverse.Create(__instance).Field("CanAttackHideoutStart");
                    Traverse traverse2 = Traverse.Create(__instance).Field("CanAttackHideoutEnd");
                    if (traverse.GetValue<int>() != 0 && traverse2.GetValue<int>() != 24)
                    {
                        HideoutSendTroopsBehavior.HideoutCampaignBehaviorIsHideoutAttackableNowPatch.SaveStart = traverse.GetValue<int>();
                        HideoutSendTroopsBehavior.HideoutCampaignBehaviorIsHideoutAttackableNowPatch.SaveEnd = traverse2.GetValue<int>();
                        traverse.SetValue(0);
                        traverse2.SetValue(24);
                        return;
                    }
                }
                else if (HideoutSendTroopsBehavior.HideoutCampaignBehaviorIsHideoutAttackableNowPatch.SaveStart != 0 && HideoutSendTroopsBehavior.HideoutCampaignBehaviorIsHideoutAttackableNowPatch.SaveEnd != 0)
                {
                    Traverse.Create(__instance).Field("CanAttackHideoutStart").SetValue(HideoutSendTroopsBehavior.HideoutCampaignBehaviorIsHideoutAttackableNowPatch.SaveStart);
                    Traverse.Create(__instance).Field("CanAttackHideoutEnd").SetValue(HideoutSendTroopsBehavior.HideoutCampaignBehaviorIsHideoutAttackableNowPatch.SaveEnd);
                }
            }

            private static void Finalizer(Exception __exception)
            {
                if (__exception != null)
                {
                    MessageBox.Show(__exception.FlattenException());
                }
            }
        }

        [HarmonyPatch(typeof(SPInventoryVM), MethodType.Constructor, new Type[]
        {
            typeof(InventoryLogic),
            typeof(bool),
            typeof(Func<WeaponComponentData, ItemObject.ItemUsageSetFlags>),
            typeof(string),
            typeof(string)
        })]
        private class HideoutSPInventorySortTypePatch
        {
            private static SPInventorySortControllerVM SaveController { get; set; }

            private static void Postfix(SPInventoryVM __instance)
            {
                if (HideoutSendTroopsBehavior.HideoutConfig.Config.LeftInventorySortMemorizeFlg)
                {
                    if (HideoutSendTroopsBehavior.HideoutSPInventorySortTypePatch.SaveController == null)
                    {
                        HideoutSendTroopsBehavior.HideoutSPInventorySortTypePatch.SaveController = __instance.OtherInventorySortController;
                        return;
                    }
                    Traverse.Create(HideoutSendTroopsBehavior.HideoutSPInventorySortTypePatch.SaveController).Field("_listToControl").SetValue(Traverse.Create(__instance.OtherInventorySortController).Field("_listToControl").GetValue());
                    __instance.OtherInventorySortController = HideoutSendTroopsBehavior.HideoutSPInventorySortTypePatch.SaveController;
                    __instance.OtherInventorySortController.SortByCurrentState();
                    __instance.OtherInventorySortController.RefreshValues();
                }
            }

            private static void Finalizer(Exception __exception)
            {
                if (__exception != null)
                {
                    MessageBox.Show(__exception.FlattenException());
                }
            }
        }
    }
}
