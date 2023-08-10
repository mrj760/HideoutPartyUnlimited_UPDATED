using System;
using System.Windows.Forms;
using HarmonyLib;
using TaleWorlds.Core;
using TaleWorlds.SaveSystem;

namespace HideoutPartyUnlimited
{
    [HarmonyPatch(typeof(Game), "Save")]
    internal class HPUGetSaveFileNamePatch
    {
        private static void Prefix(MetaData metaData, string saveName, ISaveDriver driver)
        {
            if (HideoutCampaignBehavior.PrevTroopRoster != null)
            {
                new HPUHideoutTroopRoster().SaveTroopRoster(HideoutCampaignBehavior.PrevTroopRoster, saveName + ".sav");
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
