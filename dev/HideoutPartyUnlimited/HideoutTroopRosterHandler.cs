using TaleWorlds.Core;

namespace HideoutPartyUnlimited
{
    public class HideoutTroopRosterHandler : GameHandler
    {
        public override void OnBeforeSave()
        {
        }

        public override void OnAfterSave()
        {
            if (HideoutCampaignBehavior.PrevTroopRoster != null)
            {
                new HPUHideoutTroopRoster().SaveTroopRoster(HideoutCampaignBehavior.PrevTroopRoster, "");
            }
        }
    }
}
