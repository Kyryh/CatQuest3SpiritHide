using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using HutongGames.PlayMaker;
using ProjectStar.Data;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CatQuest3SpiritHide;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;

    public static GameObject spirit;

    private void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;

        new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll();

        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
    }

}


[HarmonyPatch(typeof(PlayerSpiritBehaviour))]
class ProloguePatch {
    [HarmonyPatch(nameof(PlayerSpiritBehaviour.Start))]
    [HarmonyPostfix]
    static void StartPostfix(PlayerSpiritBehaviour __instance) {
        Plugin.spirit = __instance.transform.GetChild(0).gameObject;
        Plugin.spirit.SetActive(false);
    }
}


[HarmonyPatch(typeof(FsmStateAction))]
class FsmStateActionPatch {

    static bool IsSpiritDialogue(FsmStateAction action) {
        return action is BaseDialogueAction dialogueAction
            && dialogueAction.positionType == SpeakerPositionType.Spirit;
    }

    [HarmonyPatch(nameof(FsmStateAction.OnEnter))]
    [HarmonyPostfix]
    static void OnEnterPatch(FsmStateAction __instance) {
        if (IsSpiritDialogue(__instance))
            Plugin.spirit.SetActive(true);
    }

    [HarmonyPatch(nameof(FsmStateAction.OnExit))]
    [HarmonyPostfix]
    public static void OnExitPatch(FsmStateAction __instance) {
        if (IsSpiritDialogue(__instance))
            Plugin.spirit.SetActive(false);
    }
}

[HarmonyPatch(typeof(AddDialogueFocus))]
class AddDialogueFocusPatch {
    [HarmonyPatch(nameof(AddDialogueFocus.OnExit))]
    [HarmonyPostfix]
    public static void OnExitPatch(FsmStateAction __instance) => FsmStateActionPatch.OnExitPatch(__instance);
}