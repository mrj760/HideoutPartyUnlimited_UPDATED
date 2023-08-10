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
    // Token: 0x0200000B RID: 11
    public class HideoutSendTroopsBehavior : CampaignBehaviorBase
    {
        // Token: 0x1700000A RID: 10
        // (get) Token: 0x06000042 RID: 66 RVA: 0x00002D41 File Offset: 0x00000F41
        // (set) Token: 0x06000043 RID: 67 RVA: 0x00002D48 File Offset: 0x00000F48
        public static HideoutSendTroopsBehavior Instance { get; set; } = new HideoutSendTroopsBehavior();

        // Token: 0x1700000B RID: 11
        // (get) Token: 0x06000044 RID: 68 RVA: 0x00002D50 File Offset: 0x00000F50
        // (set) Token: 0x06000045 RID: 69 RVA: 0x00002D58 File Offset: 0x00000F58
        private HideoutSendTroopsBehavior.HideoutOptinManager _HideoutOptionManager { get; set; }

        // Token: 0x06000046 RID: 70 RVA: 0x00002D64 File Offset: 0x00000F64
        public override void RegisterEvents()
        {
            CampaignEvents.OnNewGameCreatedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(this.OnNewGameCreated));
            CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(this.OnGameLoaded));
            CampaignEvents.OnMissionEndedEvent.AddNonSerializedListener(this, new Action<IMission>(this.OnMissionEnded));
        }

        // Token: 0x06000047 RID: 71 RVA: 0x00002DB6 File Offset: 0x00000FB6
        public override void SyncData(IDataStore dataStore)
        {
        }

        // Token: 0x06000048 RID: 72 RVA: 0x00002DB8 File Offset: 0x00000FB8
        private void OnNewGameCreated(CampaignGameStarter campaignGameStarter)
        {
            this.AddGameMenus(campaignGameStarter);
            HideoutSendTroopsBehavior.HideoutState.CurrentState = HideoutSendTroopsBehavior.HideoutState.State.None;
        }

        // Token: 0x06000049 RID: 73 RVA: 0x00002DC7 File Offset: 0x00000FC7
        private void OnGameLoaded(CampaignGameStarter campaignGameStarter)
        {
            this.AddGameMenus(campaignGameStarter);
            HideoutSendTroopsBehavior.HideoutState.CurrentState = HideoutSendTroopsBehavior.HideoutState.State.None;
        }

        // Token: 0x0600004A RID: 74 RVA: 0x00002DD8 File Offset: 0x00000FD8
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

        // Token: 0x0600004B RID: 75 RVA: 0x00003298 File Offset: 0x00001498
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

        // Token: 0x0600004C RID: 76 RVA: 0x00003309 File Offset: 0x00001509
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

        // Token: 0x0600004D RID: 77 RVA: 0x00003339 File Offset: 0x00001539
        private bool game_menu_hideout_option_condition(MenuCallbackArgs args)
        {
            args.optionLeaveType = (GameMenuOption.LeaveType)18;
            return true;
        }

        // Token: 0x0600004E RID: 78 RVA: 0x00003344 File Offset: 0x00001544
        private void game_menu_hideout_option_consequence(MenuCallbackArgs args)
        {
            this._HideoutOptionManager.MemoMenuStringID = args.MenuContext.GameMenu.StringId;
            GameMenu.SwitchToMenu("hideout_menu_option");
        }

        // Token: 0x0600004F RID: 79 RVA: 0x0000336B File Offset: 0x0000156B
        private bool SendTroops_condition(MenuCallbackArgs args)
        {
            args.optionLeaveType = (GameMenuOption.LeaveType)10;
            args.Tooltip = new TextObject("Add an automatic combat option.", null);
            this.TextSetSwitch("SENDTROOPS_ON_OFF", HideoutSendTroopsBehavior.HideoutConfig.Config.SendTroopsFlg);
            return true;
        }

        // Token: 0x06000050 RID: 80 RVA: 0x0000339C File Offset: 0x0000159C
        private void SendTroops_consequence(MenuCallbackArgs args)
        {
            HideoutSendTroopsBehavior.HideoutConfig.HideoutOption config = HideoutSendTroopsBehavior.HideoutConfig.Config;
            config.SendTroopsFlg = !config.SendTroopsFlg;
            GameMenu.SwitchToMenu("hideout_menu_option");
        }

        // Token: 0x06000051 RID: 81 RVA: 0x000033BB File Offset: 0x000015BB
        private void TextSetSwitch(string textID, bool flg)
        {
            if (flg)
            {
                GameTexts.SetVariable(textID, "ON");
                return;
            }
            GameTexts.SetVariable(textID, "OFF");
        }

        // Token: 0x06000052 RID: 82 RVA: 0x000033D7 File Offset: 0x000015D7
        private bool DaytimeAssault_condition(MenuCallbackArgs args)
        {
            args.optionLeaveType = (GameMenuOption.LeaveType)12;
            args.Tooltip = new TextObject("ON : Attack at any time\nOFF: Attack at night", null);
            this.TextSetSwitch("DAYTIME_ON_OFF", HideoutSendTroopsBehavior.HideoutConfig.Config.DayTimeHideoutAssaultFlg);
            return true;
        }

        // Token: 0x06000053 RID: 83 RVA: 0x00003408 File Offset: 0x00001608
        private void DaytimeAssault_consequence(MenuCallbackArgs args)
        {
            HideoutSendTroopsBehavior.HideoutConfig.HideoutOption config = HideoutSendTroopsBehavior.HideoutConfig.Config;
            config.DayTimeHideoutAssaultFlg = !config.DayTimeHideoutAssaultFlg;
            GameMenu.SwitchToMenu("hideout_menu_option");
        }

        // Token: 0x06000054 RID: 84 RVA: 0x00003427 File Offset: 0x00001627
        private bool LeftInventorySort_condition(MenuCallbackArgs args)
        {
            args.optionLeaveType = (GameMenuOption.LeaveType)14;
            this.TextSetSwitch("LEFT_INVENTORY_SORT_ON_OFF", HideoutSendTroopsBehavior.HideoutConfig.Config.LeftInventorySortMemorizeFlg);
            return true;
        }

        // Token: 0x06000055 RID: 85 RVA: 0x00003447 File Offset: 0x00001647
        private void LeftInventorySort_consequence(MenuCallbackArgs args)
        {
            HideoutSendTroopsBehavior.HideoutConfig.HideoutOption config = HideoutSendTroopsBehavior.HideoutConfig.Config;
            config.LeftInventorySortMemorizeFlg = !config.LeftInventorySortMemorizeFlg;
            GameMenu.SwitchToMenu("hideout_menu_option");
        }

        // Token: 0x06000056 RID: 86 RVA: 0x00003466 File Offset: 0x00001666
        private bool ManyBandits_condition(MenuCallbackArgs args)
        {
            args.optionLeaveType = (GameMenuOption.LeaveType)12;
            this.TextSetSwitch("MANY_BANDITS_ON_OFF", HideoutSendTroopsBehavior.HideoutConfig.Config.FightingManyBanditsFlg);
            return true;
        }

        // Token: 0x06000057 RID: 87 RVA: 0x00003486 File Offset: 0x00001686
        private void ManyBandits_consequence(MenuCallbackArgs args)
        {
            HideoutSendTroopsBehavior.HideoutConfig.HideoutOption config = HideoutSendTroopsBehavior.HideoutConfig.Config;
            config.FightingManyBanditsFlg = !config.FightingManyBanditsFlg;
            GameMenu.SwitchToMenu("hideout_menu_option");
        }

        // Token: 0x06000058 RID: 88 RVA: 0x000034A5 File Offset: 0x000016A5
        private bool Contest2_condition(MenuCallbackArgs args)
        {
            args.optionLeaveType = (GameMenuOption.LeaveType)14;
            this.TextSetSwitch("CONTEST2_ON_OFF", HideoutSendTroopsBehavior.HideoutConfig.Config.SceneDesignContest2Flg);
            return true;
        }

        // Token: 0x06000059 RID: 89 RVA: 0x000034C8 File Offset: 0x000016C8
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

        // Token: 0x0600005A RID: 90 RVA: 0x00003538 File Offset: 0x00001738
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

        // Token: 0x0600005B RID: 91 RVA: 0x0000361C File Offset: 0x0000181C
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

        // Token: 0x0600005C RID: 92 RVA: 0x0000373C File Offset: 0x0000193C
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

        // Token: 0x0600005D RID: 93 RVA: 0x0000382C File Offset: 0x00001A2C
        private void game_menu_encounter_order_attack_call_bandits_on_consequence(MenuCallbackArgs args)
        {
            HideoutCampaignBehavior campaignBehavior = Campaign.Current.GetCampaignBehavior<HideoutCampaignBehavior>();
            if (HideoutSendTroopsBehavior.HideoutConfig.Config.FightingManyBanditsFlg)
            {
                HideoutSendTroopsBehavior.HideoutState.CurrentState = HideoutSendTroopsBehavior.HideoutState.State.CallBandits;
            }
            campaignBehavior.Attack_on_consequence(args);
        }

        // Token: 0x0600005E RID: 94 RVA: 0x00003850 File Offset: 0x00001A50
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

        // Token: 0x0600005F RID: 95 RVA: 0x00003940 File Offset: 0x00001B40
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

        // Token: 0x06000060 RID: 96 RVA: 0x000039D0 File Offset: 0x00001BD0
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

        // Token: 0x02000011 RID: 17
        [HarmonyPatch(typeof(SandBoxMissions), "GetPriorityListForHideoutMission")]
        private class HPUGetPriorityListForHideoutMissionPatch
        {
            // Token: 0x06000076 RID: 118 RVA: 0x00003BD8 File Offset: 0x00001DD8
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

            // Token: 0x06000077 RID: 119 RVA: 0x00003C74 File Offset: 0x00001E74
            private static void Finalizer(Exception __exception)
            {
                if (__exception != null)
                {
                    MessageBox.Show(__exception.FlattenException());
                }
            }
        }

        // Token: 0x02000012 RID: 18
        [HarmonyPatch(typeof(BattleSimulation), "OnReturn")]
        private class HPUBattleSimulationPatch
        {
            // Token: 0x06000079 RID: 121 RVA: 0x00003C90 File Offset: 0x00001E90
            private static bool Prefix(BattleSimulation __instance)
            {
                if (Settlement.CurrentSettlement != null && Settlement.CurrentSettlement.IsHideout)
                {
                    foreach (PartyBase partyBase in Traverse.Create(__instance).Field("_mapEvent").GetValue<MapEvent>().InvolvedParties)
                    {
                        partyBase.MemberRoster.RemoveZeroCounts();
                    }
                    PlayerEncounter.Update();
                    BattleState battleState = HideoutSendTroopsBehavior.HPUBattleSimulationPatch.ConvertToBattleStale(__instance);
                    Helper.ReflectionSetFieldPropertyValue_Instance(PlayerEncounter.Battle, "BattleState", battleState);
                    CampaignEventDispatcher.Instance.OnMissionEnded(Mission.Current);
                    GameMenu.SwitchToMenu("hideout_place");
                    return false;
                }
                return true;
            }

            private static BattleState ConvertToBattleStale(BattleSimulation battleSimulation)
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

            // Token: 0x0600007B RID: 123 RVA: 0x00003DD4 File Offset: 0x00001FD4
            private static void Finalizer(Exception __exception)
            {
                if (__exception != null)
                {
                    MessageBox.Show(__exception.FlattenException());
                }
            }
        }

        // Token: 0x02000013 RID: 19
        public class HideoutOptinManager
        {
            // Token: 0x17000010 RID: 16
            // (get) Token: 0x0600007D RID: 125 RVA: 0x00003DED File Offset: 0x00001FED
            // (set) Token: 0x0600007E RID: 126 RVA: 0x00003DF5 File Offset: 0x00001FF5
            private HideoutSendTroopsBehavior.HideoutOptinManager.Menu OneMenu { get; set; }

            // Token: 0x17000011 RID: 17
            // (get) Token: 0x0600007F RID: 127 RVA: 0x00003DFE File Offset: 0x00001FFE
            // (set) Token: 0x06000080 RID: 128 RVA: 0x00003E06 File Offset: 0x00002006
            private List<HideoutSendTroopsBehavior.HideoutOptinManager.Option> OptionList { get; set; } = new List<HideoutSendTroopsBehavior.HideoutOptinManager.Option>();

            // Token: 0x17000012 RID: 18
            // (get) Token: 0x06000081 RID: 129 RVA: 0x00003E0F File Offset: 0x0000200F
            // (set) Token: 0x06000082 RID: 130 RVA: 0x00003E17 File Offset: 0x00002017
            private List<HideoutSendTroopsBehavior.HideoutOptinManager.Option> RelatedOptionList { get; set; } = new List<HideoutSendTroopsBehavior.HideoutOptinManager.Option>();

            // Token: 0x17000013 RID: 19
            // (get) Token: 0x06000083 RID: 131 RVA: 0x00003E20 File Offset: 0x00002020
            // (set) Token: 0x06000084 RID: 132 RVA: 0x00003E28 File Offset: 0x00002028
            private List<Tuple<string, HideoutSendTroopsBehavior.HideoutOptinManager.Option, HideoutSendTroopsBehavior.HideoutOptinManager.Option>> InteractionOptionList { get; set; } = new List<Tuple<string, HideoutSendTroopsBehavior.HideoutOptinManager.Option, HideoutSendTroopsBehavior.HideoutOptinManager.Option>>();

            // Token: 0x17000014 RID: 20
            // (get) Token: 0x06000085 RID: 133 RVA: 0x00003E31 File Offset: 0x00002031
            // (set) Token: 0x06000086 RID: 134 RVA: 0x00003E39 File Offset: 0x00002039
            public string MemoMenuStringID { get; set; }

            // Token: 0x06000087 RID: 135 RVA: 0x00003E42 File Offset: 0x00002042
            public HideoutOptinManager(string menuId, string menuText, OnInitDelegate initDelegate, GameOverlays.MenuOverlayType overlay = 0, GameMenu.MenuFlags menuFlags = 0, object relatedObject = null)
            {
                this.OneMenu = new HideoutSendTroopsBehavior.HideoutOptinManager.Menu(menuId, menuText, initDelegate, overlay, menuFlags, relatedObject);
            }

            // Token: 0x06000088 RID: 136 RVA: 0x00003E80 File Offset: 0x00002080
            public void RegisterAddGameMenuOption(string menuId, string optionId, string optionText, string insertTargetId, GameMenuOption.OnConditionDelegate condition, GameMenuOption.OnConsequenceDelegate consequence, bool isLeave = false, int index = -1, bool isRepeatable = false)
            {
                this.OptionList.Add(new HideoutSendTroopsBehavior.HideoutOptinManager.Option(menuId, optionId, optionText, insertTargetId, condition, consequence, isLeave, index, isRepeatable));
            }

            // Token: 0x06000089 RID: 137 RVA: 0x00003EAC File Offset: 0x000020AC
            public void RegisterAddGameMenuOptionRelated(string menuId, string optionId, string optionText, string insertTargetId, GameMenuOption.OnConditionDelegate condition, GameMenuOption.OnConsequenceDelegate consequence, bool isLeave = false, int index = -1, bool isRepeatable = false)
            {
                this.RelatedOptionList.Add(new HideoutSendTroopsBehavior.HideoutOptinManager.Option(menuId, optionId, optionText, insertTargetId, condition, consequence, isLeave, index, isRepeatable));
            }

            // Token: 0x0600008A RID: 138 RVA: 0x00003ED8 File Offset: 0x000020D8
            public void InteractionOption(string hideoutOption, string optionId, string otherMenuId, string otherOptionId)
            {
                HideoutSendTroopsBehavior.HideoutOptinManager.Option item = this.OptionList.Find((HideoutSendTroopsBehavior.HideoutOptinManager.Option x) => x.OptionId == optionId);
                HideoutSendTroopsBehavior.HideoutOptinManager.Option item2 = this.RelatedOptionList.Find((HideoutSendTroopsBehavior.HideoutOptinManager.Option x) => x.MenuId == otherMenuId && x.OptionId == otherOptionId);
                this.InteractionOptionList.Add(new Tuple<string, HideoutSendTroopsBehavior.HideoutOptinManager.Option, HideoutSendTroopsBehavior.HideoutOptinManager.Option>(hideoutOption, item, item2));
            }

            // Token: 0x0600008B RID: 139 RVA: 0x00003F44 File Offset: 0x00002144
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

            // Token: 0x0600008C RID: 140 RVA: 0x00004324 File Offset: 0x00002524
            private bool OptionBack_condition(MenuCallbackArgs args)
            {
                args.optionLeaveType = (GameMenuOption.LeaveType)16;
                return true;
            }

            // Token: 0x0600008D RID: 141 RVA: 0x00004330 File Offset: 0x00002530
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

            // Token: 0x0600008E RID: 142 RVA: 0x0000454C File Offset: 0x0000274C
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

            // Token: 0x0600008F RID: 143 RVA: 0x00004614 File Offset: 0x00002814
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

            // Token: 0x06000090 RID: 144 RVA: 0x00004688 File Offset: 0x00002888
            private bool CheckGameMenuOption(string menuId, string targetIdString)
            {
                return (Helper.ReflectionGetField_Instance(Helper.ReflectionInvokeMethod_Instance(Campaign.Current.GameMenuManager, "GetGameMenu", new object[]
                {
                    menuId
                }) as GameMenu, "_menuItems") as List<GameMenuOption>).FindIndex((GameMenuOption x) => x.IdString == targetIdString) >= 0;
            }

            // Token: 0x0200001A RID: 26
            private class Option
            {
                // Token: 0x1700001F RID: 31
                // (get) Token: 0x060000C3 RID: 195 RVA: 0x00004CB7 File Offset: 0x00002EB7
                // (set) Token: 0x060000C4 RID: 196 RVA: 0x00004CBF File Offset: 0x00002EBF
                public string MenuId { get; set; }

                // Token: 0x17000020 RID: 32
                // (get) Token: 0x060000C5 RID: 197 RVA: 0x00004CC8 File Offset: 0x00002EC8
                // (set) Token: 0x060000C6 RID: 198 RVA: 0x00004CD0 File Offset: 0x00002ED0
                public string OptionId { get; set; }

                // Token: 0x17000021 RID: 33
                // (get) Token: 0x060000C7 RID: 199 RVA: 0x00004CD9 File Offset: 0x00002ED9
                // (set) Token: 0x060000C8 RID: 200 RVA: 0x00004CE1 File Offset: 0x00002EE1
                public string OptionText { get; set; }

                // Token: 0x17000022 RID: 34
                // (get) Token: 0x060000C9 RID: 201 RVA: 0x00004CEA File Offset: 0x00002EEA
                // (set) Token: 0x060000CA RID: 202 RVA: 0x00004CF2 File Offset: 0x00002EF2
                public string InsertTargetId { get; set; }

                // Token: 0x17000023 RID: 35
                // (get) Token: 0x060000CB RID: 203 RVA: 0x00004CFB File Offset: 0x00002EFB
                // (set) Token: 0x060000CC RID: 204 RVA: 0x00004D03 File Offset: 0x00002F03
                public GameMenuOption.OnConditionDelegate Condition { get; set; }

                // Token: 0x17000024 RID: 36
                // (get) Token: 0x060000CD RID: 205 RVA: 0x00004D0C File Offset: 0x00002F0C
                // (set) Token: 0x060000CE RID: 206 RVA: 0x00004D14 File Offset: 0x00002F14
                public GameMenuOption.OnConsequenceDelegate Consequence { get; set; }

                // Token: 0x17000025 RID: 37
                // (get) Token: 0x060000CF RID: 207 RVA: 0x00004D1D File Offset: 0x00002F1D
                // (set) Token: 0x060000D0 RID: 208 RVA: 0x00004D25 File Offset: 0x00002F25
                public bool IsLeave { get; set; }

                // Token: 0x17000026 RID: 38
                // (get) Token: 0x060000D1 RID: 209 RVA: 0x00004D2E File Offset: 0x00002F2E
                // (set) Token: 0x060000D2 RID: 210 RVA: 0x00004D36 File Offset: 0x00002F36
                public int Index { get; set; }

                // Token: 0x17000027 RID: 39
                // (get) Token: 0x060000D3 RID: 211 RVA: 0x00004D3F File Offset: 0x00002F3F
                // (set) Token: 0x060000D4 RID: 212 RVA: 0x00004D47 File Offset: 0x00002F47
                public bool IsRepeatable { get; set; }

                // Token: 0x060000D5 RID: 213 RVA: 0x00004D50 File Offset: 0x00002F50
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

            // Token: 0x0200001B RID: 27
            private class Menu
            {
                // Token: 0x17000028 RID: 40
                // (get) Token: 0x060000D6 RID: 214 RVA: 0x00004DA8 File Offset: 0x00002FA8
                // (set) Token: 0x060000D7 RID: 215 RVA: 0x00004DB0 File Offset: 0x00002FB0
                public string MenuId { get; set; }

                // Token: 0x17000029 RID: 41
                // (get) Token: 0x060000D8 RID: 216 RVA: 0x00004DB9 File Offset: 0x00002FB9
                // (set) Token: 0x060000D9 RID: 217 RVA: 0x00004DC1 File Offset: 0x00002FC1
                public string MenuText { get; set; }

                // Token: 0x1700002A RID: 42
                // (get) Token: 0x060000DA RID: 218 RVA: 0x00004DCA File Offset: 0x00002FCA
                // (set) Token: 0x060000DB RID: 219 RVA: 0x00004DD2 File Offset: 0x00002FD2
                public OnInitDelegate InitDelegate { get; set; }

                // Token: 0x1700002B RID: 43
                // (get) Token: 0x060000DC RID: 220 RVA: 0x00004DDB File Offset: 0x00002FDB
                // (set) Token: 0x060000DD RID: 221 RVA: 0x00004DE3 File Offset: 0x00002FE3
                public GameOverlays.MenuOverlayType Overlay { get; set; }

                // Token: 0x1700002C RID: 44
                // (get) Token: 0x060000DE RID: 222 RVA: 0x00004DEC File Offset: 0x00002FEC
                // (set) Token: 0x060000DF RID: 223 RVA: 0x00004DF4 File Offset: 0x00002FF4
                public GameMenu.MenuFlags MenuFlags { get; set; }

                // Token: 0x1700002D RID: 45
                // (get) Token: 0x060000E0 RID: 224 RVA: 0x00004DFD File Offset: 0x00002FFD
                // (set) Token: 0x060000E1 RID: 225 RVA: 0x00004E05 File Offset: 0x00003005
                public object RelatedObject { get; set; }

                // Token: 0x060000E2 RID: 226 RVA: 0x00004E0E File Offset: 0x0000300E
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

        // Token: 0x02000014 RID: 20
        public static class HideoutConfig
        {
            // Token: 0x17000015 RID: 21
            // (get) Token: 0x06000091 RID: 145 RVA: 0x000046EB File Offset: 0x000028EB
            // (set) Token: 0x06000092 RID: 146 RVA: 0x000046F2 File Offset: 0x000028F2
            private static string SendTroopsConfigPath { get; set; } = BasePath.Name + "Modules/HideoutPartyUnlimited/Config.xml";

            // Token: 0x17000016 RID: 22
            // (get) Token: 0x06000093 RID: 147 RVA: 0x000046FA File Offset: 0x000028FA
            // (set) Token: 0x06000094 RID: 148 RVA: 0x00004701 File Offset: 0x00002901
            public static HideoutSendTroopsBehavior.HideoutConfig.HideoutOption Config { get; set; } = null;

            // Token: 0x06000095 RID: 149 RVA: 0x00004709 File Offset: 0x00002909
            public static void LoadFile()
            {
                HideoutSendTroopsBehavior.HideoutConfig.Config = HideoutSendTroopsBehavior.HideoutConfig.XMLLoad(HideoutSendTroopsBehavior.HideoutConfig.SendTroopsConfigPath);
            }

            // Token: 0x06000096 RID: 150 RVA: 0x0000471A File Offset: 0x0000291A
            public static void SaveFile()
            {
                HideoutSendTroopsBehavior.HideoutConfig.XMLSave(HideoutSendTroopsBehavior.HideoutConfig.SendTroopsConfigPath, HideoutSendTroopsBehavior.HideoutConfig.Config);
            }

            // Token: 0x06000097 RID: 151 RVA: 0x0000472C File Offset: 0x0000292C
            private static void XMLSave(string path, HideoutSendTroopsBehavior.HideoutConfig.HideoutOption option)
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(HideoutSendTroopsBehavior.HideoutConfig.HideoutOption));
                using (StreamWriter streamWriter = new StreamWriter(path, false, Encoding.UTF8))
                {
                    xmlSerializer.Serialize(streamWriter, option);
                }
            }

            // Token: 0x06000098 RID: 152 RVA: 0x0000477C File Offset: 0x0000297C
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

            // Token: 0x06000099 RID: 153 RVA: 0x000047D8 File Offset: 0x000029D8
            private static void FileExistCheck(string path)
            {
                if (!File.Exists(path))
                {
                    HideoutSendTroopsBehavior.HideoutConfig.XMLSave(path, new HideoutSendTroopsBehavior.HideoutConfig.HideoutOption());
                }
            }

            // Token: 0x02000023 RID: 35
            public class HideoutOption
            {
                // Token: 0x1700002E RID: 46
                // (get) Token: 0x060000F3 RID: 243 RVA: 0x00004F6E File Offset: 0x0000316E
                // (set) Token: 0x060000F4 RID: 244 RVA: 0x00004F76 File Offset: 0x00003176
                public bool SendTroopsFlg { get; set; }

                // Token: 0x1700002F RID: 47
                // (get) Token: 0x060000F5 RID: 245 RVA: 0x00004F7F File Offset: 0x0000317F
                // (set) Token: 0x060000F6 RID: 246 RVA: 0x00004F87 File Offset: 0x00003187
                public bool DayTimeHideoutAssaultFlg { get; set; }

                // Token: 0x17000030 RID: 48
                // (get) Token: 0x060000F7 RID: 247 RVA: 0x00004F90 File Offset: 0x00003190
                // (set) Token: 0x060000F8 RID: 248 RVA: 0x00004F98 File Offset: 0x00003198
                public bool LeftInventorySortMemorizeFlg { get; set; }

                // Token: 0x17000031 RID: 49
                // (get) Token: 0x060000F9 RID: 249 RVA: 0x00004FA1 File Offset: 0x000031A1
                // (set) Token: 0x060000FA RID: 250 RVA: 0x00004FA9 File Offset: 0x000031A9
                public bool FightingManyBanditsFlg { get; set; }

                // Token: 0x17000032 RID: 50
                // (get) Token: 0x060000FB RID: 251 RVA: 0x00004FB2 File Offset: 0x000031B2
                // (set) Token: 0x060000FC RID: 252 RVA: 0x00004FBA File Offset: 0x000031BA
                public bool SceneDesignContest2Flg { get; set; }
            }
        }

        // Token: 0x02000015 RID: 21
        public static class HideoutState
        {
            // Token: 0x17000017 RID: 23
            // (get) Token: 0x0600009B RID: 155 RVA: 0x00004809 File Offset: 0x00002A09
            // (set) Token: 0x0600009C RID: 156 RVA: 0x00004810 File Offset: 0x00002A10
            public static HideoutSendTroopsBehavior.HideoutState.State CurrentState { get; set; } = HideoutSendTroopsBehavior.HideoutState.State.None;

            // Token: 0x17000018 RID: 24
            // (get) Token: 0x0600009D RID: 157 RVA: 0x00004818 File Offset: 0x00002A18
            // (set) Token: 0x0600009E RID: 158 RVA: 0x0000481F File Offset: 0x00002A1F
            public static int DesignContest2Exist { get; set; } = 0;

            // Token: 0x17000019 RID: 25
            // (get) Token: 0x0600009F RID: 159 RVA: 0x00004827 File Offset: 0x00002A27
            // (set) Token: 0x060000A0 RID: 160 RVA: 0x0000482E File Offset: 0x00002A2E
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

            // Token: 0x1700001A RID: 26
            // (get) Token: 0x060000A1 RID: 161 RVA: 0x00004836 File Offset: 0x00002A36
            // (set) Token: 0x060000A2 RID: 162 RVA: 0x0000483D File Offset: 0x00002A3D
            public static string OriginalSceneName { get; set; } = "";

            // Token: 0x02000024 RID: 36
            public enum State
            {
                // Token: 0x0400004E RID: 78
                None,
                // Token: 0x0400004F RID: 79
                Attack,
                // Token: 0x04000050 RID: 80
                SendTroops,
                // Token: 0x04000051 RID: 81
                CallBandits
            }
        }

        // Token: 0x02000016 RID: 22
        [HarmonyPatch(typeof(Hideout), "NextPossibleAttackTime", MethodType.Getter)]
        private class HideoutNextPossibleAttackTimePatch
        {
            // Token: 0x1700001B RID: 27
            // (get) Token: 0x060000A4 RID: 164 RVA: 0x00004955 File Offset: 0x00002B55
            // (set) Token: 0x060000A5 RID: 165 RVA: 0x0000495C File Offset: 0x00002B5C
            private static CampaignTime SaveNextPossibleAttackTime { get; set; } = CampaignTime.Zero;

            // Token: 0x060000A6 RID: 166 RVA: 0x00004964 File Offset: 0x00002B64
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

            // Token: 0x060000A7 RID: 167 RVA: 0x00004A6B File Offset: 0x00002C6B
            private static void Finalizer(Exception __exception)
            {
                if (__exception != null)
                {
                    MessageBox.Show(__exception.FlattenException());
                }
            }
        }

        // Token: 0x02000017 RID: 23
        [HarmonyPatch(typeof(TaleWorlds.CampaignSystem.CampaignBehaviors.HideoutCampaignBehavior), "IsHideoutAttackableNow")]
        private class HideoutCampaignBehaviorIsHideoutAttackableNowPatch
        {
            // Token: 0x1700001C RID: 28
            // (get) Token: 0x060000AA RID: 170 RVA: 0x00004A90 File Offset: 0x00002C90
            // (set) Token: 0x060000AB RID: 171 RVA: 0x00004A97 File Offset: 0x00002C97
            private static int SaveStart { get; set; }

            // Token: 0x1700001D RID: 29
            // (get) Token: 0x060000AC RID: 172 RVA: 0x00004A9F File Offset: 0x00002C9F
            // (set) Token: 0x060000AD RID: 173 RVA: 0x00004AA6 File Offset: 0x00002CA6
            private static int SaveEnd { get; set; }

            // Token: 0x060000AE RID: 174 RVA: 0x00004AB0 File Offset: 0x00002CB0
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

            // Token: 0x060000AF RID: 175 RVA: 0x00004B83 File Offset: 0x00002D83
            private static void Finalizer(Exception __exception)
            {
                if (__exception != null)
                {
                    MessageBox.Show(__exception.FlattenException());
                }
            }
        }

        // Token: 0x02000018 RID: 24
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
            // Token: 0x1700001E RID: 30
            // (get) Token: 0x060000B1 RID: 177 RVA: 0x00004B9C File Offset: 0x00002D9C
            // (set) Token: 0x060000B2 RID: 178 RVA: 0x00004BA3 File Offset: 0x00002DA3
            private static SPInventorySortControllerVM SaveController { get; set; }

            // Token: 0x060000B3 RID: 179 RVA: 0x00004BAC File Offset: 0x00002DAC
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

            // Token: 0x060000B4 RID: 180 RVA: 0x00004C2D File Offset: 0x00002E2D
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
