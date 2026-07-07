using System;
using Terraria.ModLoader.IO;

namespace SariaMod.Items.zTalking
{
    public class PendingCutscene
    {
        public string ID { get; set; }
        public string TargetNodeID { get; set; }
        public string ButtonText { get; set; }
        public double RemainingTime { get; set; } // In ticks (60 ticks = 1 second)
        public string ConditionID { get; set; }
        public bool IsTriggered { get; set; }

        public PendingCutscene() { }

        public PendingCutscene(string id, string targetNodeID, string buttonText, double durationMinutes, string conditionID)
        {
            ID = id;
            TargetNodeID = targetNodeID;
            ButtonText = buttonText;
            RemainingTime = durationMinutes * 60 * 60; // Minutes * 60s * 60ticks
            ConditionID = conditionID;
            IsTriggered = false;
        }

        public TagCompound Save()
        {
            return new TagCompound
            {
                ["ID"] = ID,
                ["TargetNodeID"] = TargetNodeID,
                ["ButtonText"] = ButtonText,
                ["RemainingTime"] = RemainingTime,
                ["ConditionID"] = ConditionID,
                ["IsTriggered"] = IsTriggered
            };
        }

        public static PendingCutscene Load(TagCompound tag)
        {
            return new PendingCutscene
            {
                ID = tag.GetString("ID"),
                TargetNodeID = tag.GetString("TargetNodeID"),
                ButtonText = tag.GetString("ButtonText"),
                RemainingTime = tag.GetDouble("RemainingTime"),
                ConditionID = tag.GetString("ConditionID"),
                IsTriggered = tag.GetBool("IsTriggered")
            };
        }
    }
}
