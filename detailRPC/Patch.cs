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
        public static bool isdeath, isoverload = false;
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
                if (Main.isplaying && ___discord != null)
                {
                    String text = String.Empty;
                    String text2 = String.Empty;
                    String text3 = String.Empty;

                    if (scrController.instance != null && scrController.instance.isLevelEditor)
                    {
                        string text4 = scrController.instance.editor.levelData.fullCaption;
                        if (GCS.standaloneLevelMode)
                        {
                            text2 = RDString.Get("discord.playing", null);
                            if (!scrMisc.ApproximatelyFloor((double)(GCS.speedTrialMode ? GCS.currentSpeedTrial : (scrController.instance.isEditingLevel ? scrController.instance.editor.playbackSpeed : 1f)), 1.0))
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
                            if (!scrController.instance.editor.customLevel.levelPath.IsNullOrEmpty())
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
                        if (!scrMisc.ApproximatelyFloor((double)(GCS.speedTrialMode ? GCS.currentSpeedTrial : (scrController.instance.isEditingLevel ? scrController.instance.editor.playbackSpeed : 1f)), 1.0))
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
                    activity.State = text3;
                    if (!scrController.instance.paused && !RDC.auto && !(Patch.isdeath || Patch.isoverload))
                    {
                        if (GCS.difficulty == Difficulty.Lenient)
                            activity.Details = text2 + " / " + Main.Progress() + "% (" + (RDString.language == UnityEngine.SystemLanguage.Korean ? "느슨" : "Lenient") + ")";
                        else if (GCS.difficulty == Difficulty.Normal)
                            activity.Details = text2 + " / " + Main.Progress() + "% (" + (RDString.language == UnityEngine.SystemLanguage.Korean ? "보통" : "Normal") + ")";
                        else if (GCS.difficulty == Difficulty.Strict)
                            activity.Details = text2 + " / " + Main.Progress() + "% (" + (RDString.language == UnityEngine.SystemLanguage.Korean ? "엄격" : "Strict") + ")";
                    }
                    else if (scrController.instance.paused)
                        activity.Details = text2;
                    else if (RDC.auto)
                        activity.Details = text2 + " / " + Main.Progress() + "% (Auto)";
                    else if (Patch.isdeath)
                        activity.Details = text2 + " / " + Main.Progress() + "% (" + (RDString.language == UnityEngine.SystemLanguage.Korean ? "죽음" : "Death") + ")";
                    else if (Patch.isoverload)
                        activity.Details = text2 + " / " + Main.Progress() + "% (" + (RDString.language == UnityEngine.SystemLanguage.Korean ? "과부하" : "Overload") + ")";

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
                    DiscordController.shouldUpdatePresence = true;
                    return false;
                }
                return true;
            }
            public static void Postfix()
            {
                DiscordController.shouldUpdatePresence = true;
            }
        }
    }

    [HarmonyPatch(typeof(scrController),"PlayerControl_Update")]
    internal static class Death
    {
        [HarmonyPatch(typeof(scrController),"FailAction")]
        private static void Prefix(bool overload = false)
        {
            if (!overload)
                Patch.isdeath = true;
            else
                Patch.isoverload = true;
        }
    }

    [HarmonyPatch(typeof(scrController),"Countdown_Update")]
    public static class StartLoadingPatcher
    {
        public static void Postfix()
        {
            Patch.isdeath = false;
            Patch.isoverload= false;
        }
    }

    [HarmonyPatch(typeof(scrController),"Checkpoint_Enter")]
    public static class CheckpointEnter
    {
        public static void Postfix()
        {
            Patch.isdeath = false;
            Patch.isoverload = false;
        }
    }
}
