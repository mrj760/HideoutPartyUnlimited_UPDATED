using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Newtonsoft.Json;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;

namespace HideoutPartyUnlimited
{
    public class HPUHideoutTroopRoster
    {
        protected string TroopRosterFolder { get; set; } = "../../Modules/HideoutPartyUnlimited/TroopRoster";

        protected string TroopRosterSaveFileName { get; set; } = "../../Modules/HideoutPartyUnlimited/TroopRoster/HideoutTroopRoster.json";

        public Dictionary<string, HPUHideoutTroopRoster.HPUTroopRoster> TroopRosterDictionary { get; set; } = new Dictionary<string, HPUHideoutTroopRoster.HPUTroopRoster>();

        protected void AddTroopRoster(TroopRoster hideoutTroops, string savName)
        {
            if (savName == null)
            {
                savName = "HideoutTroopRosterTemp";
            }
            HPUHideoutTroopRoster.HPUTroopRoster hputroopRoster = new HPUHideoutTroopRoster.HPUTroopRoster(savName);
            foreach (TroopRosterElement troopRosterElement in hideoutTroops.GetTroopRoster())
            {
                HPUHideoutTroopRoster.HPUCharacterInfo item = new HPUHideoutTroopRoster.HPUCharacterInfo(troopRosterElement.Character.StringId, troopRosterElement.Number);
                if (troopRosterElement.Number > 0)
                {
                    hputroopRoster.HPUCharacterInfoList.Add(item);
                }
            }
            this.TroopRosterDictionary[savName] = hputroopRoster;
        }

        public void SaveTroopRoster(TroopRoster hideoutTroops, string savName)
        {
            this.CheckFolder(this.TroopRosterFolder);
            HPUHideoutTroopRoster hpuhideoutTroopRoster = new HPUHideoutTroopRoster();
            if (File.Exists(this.TroopRosterSaveFileName))
            {
                hpuhideoutTroopRoster = JsonConvert.DeserializeObject<HPUHideoutTroopRoster>(File.ReadAllText(this.TroopRosterSaveFileName));
            }
            hpuhideoutTroopRoster.AddTroopRoster(hideoutTroops, savName);
            string contents = JsonConvert.SerializeObject(hpuhideoutTroopRoster, (Newtonsoft.Json.Formatting)1);
            File.WriteAllText(this.TroopRosterSaveFileName, contents);
        }

        protected TroopRoster RestoreTroopRoster(HPUHideoutTroopRoster myHPUtroop)
        {
            string text = HideoutCampaignBehavior.LoadFileName;
            if (text == null)
            {
                text = "HideoutTroopRosterTemp";
            }
            else
            {
                text += ".sav";
            }
            HPUHideoutTroopRoster.HPUTroopRoster hputroopRoster;
            myHPUtroop.TroopRosterDictionary.TryGetValue(text, out hputroopRoster);
            TroopRoster troopRoster = null;
            if (hputroopRoster != null)
            {
                Campaign.Current.Models.BanditDensityModel.GetPlayerMaximumTroopCountForHideoutMission(MobileParty.MainParty);
                troopRoster = TroopRoster.CreateDummyTroopRoster();
                troopRoster.Add(MobileParty.MainParty.MemberRoster.ToFlattenedRoster());
                foreach (TroopRosterElement troopRosterElement in troopRoster.GetTroopRoster())
                {
                    troopRoster.SetElementNumber(troopRoster.FindIndexOfTroop(troopRosterElement.Character), 0);
                    foreach (HPUHideoutTroopRoster.HPUCharacterInfo hpucharacterInfo in hputroopRoster.HPUCharacterInfoList)
                    {
                        if (troopRosterElement.Character.StringId == hpucharacterInfo.StringId)
                        {
                            troopRoster.SetElementNumber(troopRoster.FindIndexOfTroop(troopRosterElement.Character), hpucharacterInfo.Number);
                            break;
                        }
                    }
                    if (troopRosterElement.Character.IsPlayerCharacter)
                    {
                        troopRoster.SetElementNumber(troopRoster.FindIndexOfTroop(troopRosterElement.Character), 1);
                    }
                }
            }
            return troopRoster;
        }

        public TroopRoster LoadTroopRoster()
        {
            this.CheckFolder(this.TroopRosterFolder);
            if (File.Exists(this.TroopRosterSaveFileName))
            {
                HPUHideoutTroopRoster myHPUtroop = JsonConvert.DeserializeObject<HPUHideoutTroopRoster>(File.ReadAllText(this.TroopRosterSaveFileName));
                return this.RestoreTroopRoster(myHPUtroop);
            }
            return null;
        }

        public bool CheckFolder(string path)
        {
            bool result;
            if (Directory.Exists(path))
            {
                result = true;
            }
            else if (Directory.CreateDirectory(path).Exists)
            {
                result = true;
            }
            else
            {
                result = false;
                MessageBox.Show("HideoutPartyUnlimited Error: Unable to create folder.\r\n" + path);
            }
            return result;
        }

        public class HPUTroopRoster
        {
            public string SaveFileName { get; set; }

            public List<HPUHideoutTroopRoster.HPUCharacterInfo> HPUCharacterInfoList { get; set; } = new List<HPUHideoutTroopRoster.HPUCharacterInfo>();

            public HPUTroopRoster(string saveName)
            {
                this.SaveFileName = saveName;
            }
        }

        public class HPUCharacterInfo
        {
            public string StringId { get; set; }

            public int Number { get; set; }

            public HPUCharacterInfo(string id, int number)
            {
                this.StringId = id;
                this.Number = number;
            }
        }
    }
}
