using System;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;

namespace HideoutPartyUnlimited
{
    public class SandboxChangeBanditDensityModel : DefaultBanditDensityModel
    {
        public int TroopCount { get; set; }

        public SandboxChangeBanditDensityModel()
        {
            this.TroopCount = 9999;
        }

        public SandboxChangeBanditDensityModel(int count)
        {
            this.TroopCount = count;
        }

        public override int GetPlayerMaximumTroopCountForHideoutMission(MobileParty party)
        {
            return this.TroopCount;
        }

        public override int NumberOfMaximumTroopCountForFirstFightInHideout
        {
            get
            {
                if (HideoutSendTroopsBehavior.HideoutConfig.Config.FightingManyBanditsFlg)
                {
                    return 9999;
                }
                return base.NumberOfMaximumTroopCountForFirstFightInHideout;
            }
        }
    }
}
