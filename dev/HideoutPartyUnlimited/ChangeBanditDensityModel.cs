using StoryMode.GameComponents;
using TaleWorlds.CampaignSystem.Party;

namespace HideoutPartyUnlimited
{
    public class ChangeBanditDensityModel : StoryModeBanditDensityModel
    {
        public int TroopCount { get; set; }

        public ChangeBanditDensityModel()
        {
            this.TroopCount = 9999;
        }

        public ChangeBanditDensityModel(int count)
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
