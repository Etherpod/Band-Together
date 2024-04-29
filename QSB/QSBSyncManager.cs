using BandTogether.TheDoor;
using BandTogether.Util;
using QSB.API;
using QSB.WorldSync;
using System.Collections.Generic;

namespace BandTogether.QSB;

public static class QSBSyncManager
{
    public static void InitSync()
    {
        //QSBWorldSync.Init<QSBShrubberyItem, Shrubbery>();
        //QSBWorldSync.Init<QSBShardItem, KeyFragment>();
    }

    /*public static void UpdateDialogueConditions()
    {
        Dictionary<string, bool> dialogueConditions = new Dictionary<string, bool>();

        DialogueConditionManager.SharedInstance._dictConditions.ForEach((pair) =>
        {
            if (pair.Key.StartsWith("BT_") && pair.Value)
            {
                dialogueConditions.Add(pair.Key, pair.Value);
            }
        });

        if (ModMain.qsbAPI.GetIsHost())
        {
            ModMain.qsbAPI.GetPlayerIDs().ForEach((id) =>
            {
                if (id != ModMain.qsbAPI.GetLocalPlayerID())
                {
                    ModMain.qsbAPI.SendMessage("BT_Conditions", dialogueConditions, id);
                }
            });
        }
    }*/
}
