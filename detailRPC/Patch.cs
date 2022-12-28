using Discord;
using HarmonyLib;
using RDTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace detailRPC
{
    public static class Patch
    {
        public static bool isdeath, isoverload, isclear, auto, discord = false;
        [HarmonyPatch(typeof(DiscordController),"UpdatePresence")]
        public static class RPPatch
        {
            private static String Validate(String s)
            {
                if (s.Length <= 60)
                {
                    return s;
                }
                return s.Substring(0, 57) + "...";
            }
            public static bool Prefix(DiscordController __instance, Discord.Discord ___discord)
            {
                if (___discord != null)
                    discord = true;
                //Main.Logger.Log("RPPatch Working");
                if (Main.isplaying && ___discord != null)
                {
                    if (ADOBase.sceneName == GCNS.sceneLevelSelect)
                        return true;
                    if (ADOBase.sceneName == "scnCLS")
                        return true;
                    String text = String.Empty;
                    String text2 = String.Empty;
                    String text3 = String.Empty;

                    bool isLevelEditor = (bool)Main.isLevelEditorProperty.GetValue(null);
                    scnEditor editor = (scnEditor)Main.editorProperty.GetValue(null);
                    bool isEditingLevel = (bool)Main.isEditingLevelProperty.GetValue(null);

                    if (scrController.instance != null && isLevelEditor)
                    {
                        string text4 = editor.levelData.fullCaption;
                        if (GCS.standaloneLevelMode)
                        {
                            text2 = RDString.Get("discord.playing", null);
                            if (!scrMisc.ApproximatelyFloor((double)(GCS.speedTrialMode ? GCS.currentSpeedTrial : (isEditingLevel ? editor.playbackSpeed : 1f)), 1.0))
                            {
                                string str = RDString.Get("levelSelect.multiplier", new Dictionary<string, object>
                                {
                                    {
                                        "multiplier",
                                        scrConductor.instance.song.pitch.ToString("0.0#")
                                    }
                                });
                                text4 = text4 + " (" + str + ")";
                            }
                            text3 = text4;
                        }
                        else
                        {
                            text2 = RDString.Get("discord.inLevelEditor", null);
                            if (!editor.customLevel.levelPath.IsNullOrEmpty())
                            {
                                text3 = RDString.Get("discord.editedLevel", new Dictionary<string, object>
                                {
                                    {
                                        "level",
                                        text4
                                    }
                                });
                            }
                        }
                    }
                    else if (scrController.instance != null && scrController.instance.gameworld)
                    {
                        string text5 = ADOBase.GetLocalizedLevelName(ADOBase.sceneName);
                        if (!scrMisc.ApproximatelyFloor((double)(GCS.speedTrialMode ? GCS.currentSpeedTrial : (isEditingLevel ? editor.playbackSpeed : 1f)), 1.0))
                        {
                            string str2 = RDString.Get("levelSelect.multiplier", new Dictionary<string, object>
                            {
                                {
                                    "multiplier",
                                    scrConductor.instance.song.pitch.ToString("0.0#")
                                }
                            });
                            text5 = text5 + " (" + str2 + ")";
                        }
                        text2 = RDString.Get("discord.playing", null);
                        text3 = text5;
                        text = text5;
                    }
                    text = Validate(text);
                    text3 = Validate(text3);
                    text2 = Validate(text2);
                    Activity activity = default(Activity);
                    if (text2.IsNullOrEmpty())
                    {
                        return true;
                    }
                    if(text2 == "공전 중:")
                    {
                        text2 = "공전 중";
                    }
                    if (!scrController.instance.paused && !RDC.auto && (!(Patch.isdeath || Patch.isoverload) || scrController.instance.noFail))
                    {
                        if (!isclear)
                        {
                            if (!scrController.instance.noFail)
                            {
                                if (GCS.difficulty == Difficulty.Lenient)
                                    activity.Details = text2 + " (" + (RDString.language == UnityEngine.SystemLanguage.Korean ? "느슨" : "Lenient") + ")";
                                else if (GCS.difficulty == Difficulty.Normal)
                                    activity.Details = text2 + " (" + (RDString.language == UnityEngine.SystemLanguage.Korean ? "보통" : "Normal") + ")";
                                else if (GCS.difficulty == Difficulty.Strict)
                                    activity.Details = text2 + " (" + (RDString.language == UnityEngine.SystemLanguage.Korean ? "엄격" : "Strict") + ")";
                            }
                            else
                            {
                                if (GCS.difficulty == Difficulty.Lenient)
                                    activity.Details = "(" + (RDString.language == UnityEngine.SystemLanguage.Korean ? "느슨-실패방지" : "Lenient-noFail") + ")";
                                else if (GCS.difficulty == Difficulty.Normal)
                                    activity.Details = "(" + (RDString.language == UnityEngine.SystemLanguage.Korean ? "보통-실패방지" : "Normal-noFail") + ")";
                                else if (GCS.difficulty == Difficulty.Strict)
                                    activity.Details = "(" + (RDString.language == UnityEngine.SystemLanguage.Korean ? "엄격-실패방지" : "Strict-noFail") + ")";
                                if (!isEditingLevel)
                                {
                                    text3 = RDString.Get("discord.playing", null) + (RDString.language == UnityEngine.SystemLanguage.Korean ? " " : ": ") + text3;
                                }
                            }
                        }
                        else if (!scrController.instance.mistakesManager.IsAllPurePerfect())
                        {
                            Patch.isclear = false;
                            if (!scrController.instance.noFail)
                            {
                                if (GCS.difficulty == Difficulty.Lenient)
                                    activity.Details = text2 + " (" + (RDString.language == UnityEngine.SystemLanguage.Korean ? "느슨 클리어" : "Lenient Clear") + ")";
                                else if (GCS.difficulty == Difficulty.Normal)
                                    activity.Details = text2 + " (" + (RDString.language == UnityEngine.SystemLanguage.Korean ? "보통 클리어" : "Normal Clear") + ")";
                                else if (GCS.difficulty == Difficulty.Strict)
                                    activity.Details = text2 + " (" + (RDString.language == UnityEngine.SystemLanguage.Korean ? "엄격 클리어" : "Strict Clear") + ")";
                            }
                            else
                            {
                                if (GCS.difficulty == Difficulty.Lenient)
                                    activity.Details = "(" + (RDString.language == UnityEngine.SystemLanguage.Korean ? "느슨-실패방지 클리어" : "Lenient-noFail Clear") + ")";
                                else if (GCS.difficulty == Difficulty.Normal)
                                    activity.Details = "(" + (RDString.language == UnityEngine.SystemLanguage.Korean ? "보통-실패방지 클리어" : "Normal-noFail Clear") + ")";
                                else if (GCS.difficulty == Difficulty.Strict)
                                    activity.Details = "(" + (RDString.language == UnityEngine.SystemLanguage.Korean ? "엄격-실패방지 클리어" : "Strict-noFail Clear") + ")";
                                if (!isEditingLevel)
                                {
                                    text3 = RDString.Get("discord.playing", null) + (RDString.language == UnityEngine.SystemLanguage.Korean ? " " : ": ") + text3;
                                }
                            }
                        }
                        else if (scrController.instance.mistakesManager.IsAllPurePerfect())
                        {
                            Patch.isclear = false;
                            if (!scrController.instance.noFail)
                            {
                                if (GCS.difficulty == Difficulty.Lenient)
                                    activity.Details = text2 + " (" + (RDString.language == UnityEngine.SystemLanguage.Korean ? "느슨 완벽한 플레이!" : "Lenient Pure Perfect!") + ")";
                                else if (GCS.difficulty == Difficulty.Normal)
                                    activity.Details = text2 + " (" + (RDString.language == UnityEngine.SystemLanguage.Korean ? "보통 완벽한 플레이!" : "Normal Pure Perfect!") + ")";
                                else if (GCS.difficulty == Difficulty.Strict)
                                    activity.Details = text2 + " (" + (RDString.language == UnityEngine.SystemLanguage.Korean ? "엄격 완벽한 플레이!" : "Strict Pure Perfect!") + ")";
                            }
                            else
                            {
                                if (GCS.difficulty == Difficulty.Lenient)
                                    activity.Details = "(" + (RDString.language == UnityEngine.SystemLanguage.Korean ? "느슨-실패방지 완벽한 플레이!" : "Lenient-noFail Pure Perfect!") + ")";
                                else if (GCS.difficulty == Difficulty.Normal)
                                    activity.Details = "(" + (RDString.language == UnityEngine.SystemLanguage.Korean ? "보통-실패방지 완벽한 플레이!" : "Normal-noFail Pure Perfect!") + ")";
                                else if (GCS.difficulty == Difficulty.Strict)
                                    activity.Details = "(" + (RDString.language == UnityEngine.SystemLanguage.Korean ? "엄격-실패방지 완벽한 플레이!" : "Strict-noFail Pure Perfect!") + ")";
                                if (!isEditingLevel)
                                {
                                    text3 = RDString.Get("discord.playing", null) + (RDString.language == UnityEngine.SystemLanguage.Korean ? " " : ": ") + text3;
                                }
                            }
                        }
                    }
                    else if (scrController.instance.paused)
                    {
                        if (!isEditingLevel)
                        {
                            if (!scrController.instance.noFail)
                                activity.Details = text2 + (RDString.language == UnityEngine.SystemLanguage.Korean ? " (일시정지)" : " (Pause)");
                            else
                            {
                                activity.Details = RDString.language == UnityEngine.SystemLanguage.Korean ? "(일시정지)" : "(Pause)";
                                text3 = RDString.Get("discord.playing", null) + (RDString.language == UnityEngine.SystemLanguage.Korean ? " " : ": ") + text3;
                            }
                        }
                        else
                            activity.Details = text2;
                    }
                    else if (RDC.auto)
                    {
                        activity.Details = text2 + " (Auto)";
                        auto = true;
                    }
                    else if (Patch.isdeath)
                    {
                        Patch.isdeath = false;
                        activity.Details = text2 + " (" + (RDString.language == UnityEngine.SystemLanguage.Korean ? "죽음 - " + Main.Progress() + "%" : "Fail - " + Main.Progress() + "%") + ")";
                    }
                    else if (Patch.isoverload)
                    {
                        Patch.isoverload = false;
                        activity.Details = text2 + " (" + (RDString.language == UnityEngine.SystemLanguage.Korean ? "과부하 - " + Main.Progress() + "%" : "Overload - " + Main.Progress() + "%") + ")";
                    }
                    if (!RDC.auto)
                        auto = false;

                    activity.State = text3;
                    activity.Assets.LargeImage = "planets_icon_stars";
                    activity.Assets.LargeText = text;
                    Activity activity2 = activity;
                    ___discord.GetActivityManager().UpdateActivity(activity2, delegate (Result result)
                    {
                        if (result != Result.Ok)
                        {
                            RDBaseDll.printem(result.ToString());
                        }
                    });
                    DiscordController.shouldUpdatePresence = false;
                    return false;
                }
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(scrController),"PlayerControl_Update")]
    internal static class Death
    {
        [HarmonyPatch(typeof(scrController),"FailAction")]
        private static void Prefix(bool overload = false)
        {
            if (Patch.discord)
            {
                if (!overload)
                    Patch.isdeath = true;
                else
                    Patch.isoverload = true;
                DiscordController.shouldUpdatePresence = true;
            }
        }
    }

    [HarmonyPatch(typeof(scrCountdown), "ShowGetReady")]
    public static class StartLoadingPatcher
    {
        public static void Postfix()
        {
            if (Patch.discord)
            {
                Patch.isdeath = false;
                Patch.isoverload = false;
                Patch.isclear = false;
                DiscordController.shouldUpdatePresence = true;
            }
        }
    }

    [HarmonyPatch(typeof(scrController),"Countdown_Update")]
    public class EditorStartLoadingPatcher
    {
        public static void Prefix()
        {
            if (ADOBase.sceneName == GCNS.sceneEditor && (Patch.isdeath || Patch.isoverload) && Patch.discord)
            {
                Patch.isdeath = false;
                Patch.isoverload = false;
                Patch.isclear = false;
                DiscordController.shouldUpdatePresence = true;
            }
        }
    }
    
    [HarmonyPatch(typeof(scnEditor),"Play")]
    public static class PlayPatch
    {
        public static void Prefix()
        {
            if (Patch.discord)
            {
                Patch.isdeath = false;
                Patch.isoverload = false;
                Patch.isclear = false;
                DiscordController.shouldUpdatePresence = true;
            }
        }
    }

    [HarmonyPatch(typeof(scnEditor),"TogglePause")]
    public static class EditorPausePatch
    {
        public static void Prefix()
        {
            if (Patch.discord)
            {
                DiscordController.shouldUpdatePresence = true;
            }
        }
    }

    [HarmonyPatch(typeof(scrController),"TogglePauseGame")]
    public static class PausePatch
    {
        public static void Prefix()
        {
            if (Patch.discord)
            {
                DiscordController.shouldUpdatePresence = true;
            }
        }
    }

    [HarmonyPatch(typeof(scnEditor),"ToggleAuto")]
    public static class EditorAutoPatch
    {
        public static void Prefix()
        {
            if (Patch.discord)
            {
                DiscordController.shouldUpdatePresence = true;
            }
        }
    }

    [HarmonyPatch(typeof(scrController),"Checkpoint_Enter")]
    public static class CheckpointEnter
    {
        public static void Postfix()
        {
            if (Patch.discord)
            {
                Patch.isdeath = false;
                Patch.isoverload = false;
                DiscordController.shouldUpdatePresence = true;
            }
        }
    }

    [HarmonyPatch(typeof(scrController),"OnLandOnPortal")]
    public static class ClearPatch
    {
        public static void Postfix(scrController __instance)
        {
            if (__instance.gameworld)
            {
                Patch.isclear = true;
                DiscordController.shouldUpdatePresence = true;
            }
        }
    }
}
