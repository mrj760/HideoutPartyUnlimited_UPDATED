using System;
using System.Windows.Forms;
using HarmonyLib;
using TaleWorlds.Core;

namespace HideoutPartyUnlimited
{
    [HarmonyPatch(typeof(MBSaveLoad), "LoadSaveGameData")]
    internal class HPUGetLoadFileNamePatch
    {
        private static void Prefix(string saveName)
        {
            HideoutCampaignBehavior.LoadFileName = saveName;
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
