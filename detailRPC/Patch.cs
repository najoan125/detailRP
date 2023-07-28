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
        public static bool isdeath, isoverload, isclear, auto, discord, isStart;
        [HarmonyPatch(typeof(DiscordController), "UpdatePresence")]
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
                if (___discord == null)
                    return true;
                if (scrController.instance == null)
                    return true;

                // 레벨을 플레이 중일 때만 RP 변경(플레이 중이지 않을 경우, 기본 RP 출력)
                if (!scrController.instance.gameworld)
                {
                    return true;
                }
                if (ADOBase.sceneName == GCNS.sceneLevelSelect)
                    return true;
                if (ADOBase.sceneName == "scnCLS")
                    return true;


                String text = String.Empty;
                String text2 = String.Empty;
                String text3 = String.Empty;

                if (ADOBase.sceneName == GCNS.sceneLevelSelect)
                {
                    text2 = RDString.Get("discord.inLevelSelect", null);
                    int overallProgressStage = Persistence.GetOverallProgressStage();
                    if (overallProgressStage >= 9)
                    {
                        text3 = RDString.Get("levelSelect.GameCompleteFull", null);
                    }
                    else if (overallProgressStage >= 5)
                    {
                        text3 = RDString.Get("levelSelect.GameComplete", null);
                    }
                }
                else if (scrController.instance != null && ADOBase.customLevel && !ADOBase.isOfficialLevel)
                {
                    string text4 = ADOBase.customLevel.levelData.fullCaption;
                    if (!ADOBase.isLevelEditor)
                    {
                        text2 = RDString.Get("discord.playing", null);
                        if (!scrMisc.ApproximatelyFloor((double)(GCS.speedTrialMode ? GCS.currentSpeedTrial : (ADOBase.isLevelEditor ? ADOBase.editor.playbackSpeed : 1f)), 1.0))
                        {
                            string str = RDString.Get("levelSelect.multiplier", new Dictionary<string, object>
                            {
                                {
                                    "multiplier",
                                    ADOBase.conductor.song.pitch.ToString("0.0#")
                                }
                            });
                            text4 = text4 + " (" + str + ")";
                        }
                        text3 = text4;
                    }
                    else
                    {
                        text2 = RDString.Get("discord.inLevelEditor", null);
                        if (!ADOBase.editor.customLevel.levelPath.IsNullOrEmpty())
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
                else if (ADOBase.sceneName == "scnCLS")
                {
                    text2 = RDString.Get("discord.inCustomLevelSelect", null);
                }
                else if (scrController.instance != null && scrController.instance.gameworld)
                {
                    string levelName = scrController.instance.levelName;
                    string text5 = ADOBase.GetLocalizedLevelName(ADOBase.isInternalLevel ? GCS.internalLevelName : levelName).RemoveRichTags();
                    if (!scrMisc.ApproximatelyFloor((double)(GCS.speedTrialMode ? GCS.currentSpeedTrial : (ADOBase.isLevelEditor ? ADOBase.editor.playbackSpeed : 1f)), 1.0))
                    {
                        string str2 = RDString.Get("levelSelect.multiplier", new Dictionary<string, object>
                        {
                            {
                                "multiplier",
                                ADOBase.conductor.song.pitch.ToString("0.0#")
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

                // edited
                if (text2.IsNullOrEmpty())
                {
                    return true;
                }

                // Activity.Detail에서 판정텍스트 앞에 들어갈 현재 상태
                // 아래를 거치지 않아야 하는 경우: "레벨 에디터"
                if (text2 == "공전 중:" && isStart)
                {
                    text2 = "공전 중";
                }
                if (!isStart && !Patch.isdeath && !Patch.isoverload && !ADOBase.isLevelEditor)
                {
                    text2 = RDString.language == UnityEngine.SystemLanguage.Korean ? "준비 중" : "Ready";
                }
                // 아래를 거치지 않아야 하는 경우: "실패방지", "준비 중 상태"
                else if ((Patch.isdeath || Patch.isoverload) && !scrController.instance.noFail)
                {
                    if (Patch.isdeath)
                        text2 = RDString.language == UnityEngine.SystemLanguage.Korean ? "죽음" : "Fail";
                    else
                        text2 = RDString.language == UnityEngine.SystemLanguage.Korean ? "과부하!" : "Overload!";
                }

                // 일시정지 상태가 아님 && 오토 플레이 아님 && (죽지 않았거나 실패방지 상태일 때)
                // Activity.Detail에 판정 상태를 띄우기 위한 if문
                if (!scrController.instance.paused && !RDC.auto && (!(Patch.isdeath || Patch.isoverload) || scrController.instance.noFail))
                {
                    if (!isclear)
                    {
                        if (isStart)
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
                                if (!ADOBase.isLevelEditor)
                                {
                                    text3 = RDString.Get("discord.playing", null) + (RDString.language == UnityEngine.SystemLanguage.Korean ? " " : ": ") + text3;
                                }
                            }
                        } // if (isStart)
                        else
                        {
                            activity.Details = text2;
                        }
                    } // if (!isClear)
                    else if (!scrController.instance.mistakesManager.IsAllPurePerfect())
                    {
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
                            if (!ADOBase.isLevelEditor)
                            {
                                text3 = RDString.Get("discord.playing", null) + (RDString.language == UnityEngine.SystemLanguage.Korean ? " " : ": ") + text3;
                            }
                        }
                    }
                    else if (scrController.instance.mistakesManager.IsAllPurePerfect())
                    {
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
                            if (!ADOBase.isLevelEditor)
                            {
                                text3 = RDString.Get("discord.playing", null) + (RDString.language == UnityEngine.SystemLanguage.Korean ? " " : ": ") + text3;
                            }
                        }
                    }
                } // 큰 if문 End

                else if (scrController.instance.paused)
                {
                    if (!ADOBase.isLevelEditor)
                    {
                        if (!scrController.instance.noFail || !isStart)
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
                else if (Patch.isdeath || Patch.isoverload)
                {
                    activity.Details = text2 + " (" + Main.Progress() + "%)";
                }
                if (!RDC.auto)
                    auto = false;
                // edited

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
        }
    }

    // when Restarting
    [HarmonyPatch(typeof(scrController), "Restart")]
    public static class RestartPatch
    {
        public static void Prefix()
        {
            Patch.isdeath = false;
            Patch.isoverload = false;
            Patch.isclear = false;
            Patch.isStart = false;
        }
    }

    // when player failed
    [HarmonyPatch(typeof(scrController), "PlayerControl_Update")]
    internal static class Death
    {
        [HarmonyPatch(typeof(scrController), "FailAction")]
        private static void Prefix(bool overload = false)
        {
            if (Patch.discord && !scrController.instance.noFail)
            {
                if (!overload)
                    Patch.isdeath = true;
                else
                    Patch.isoverload = true;

                if (!ADOBase.isLevelEditor)
                    Patch.isStart = false;
                DiscordController.shouldUpdatePresence = true;
            }
        }
    }

    // when player failed Update
    [HarmonyPatch(typeof(scrController), "Fail2_Update")]
    public static class FailUpdate {
        public static void Prefix()
        {
            if (scrController.instance.ValidInputWasTriggered())
            {
                Patch.isdeath = false;
                Patch.isoverload = false;
                DiscordController.shouldUpdatePresence = true;
            }
        }
    }

    // when Pressed any key to Start
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
                Patch.isStart = true;
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
                Patch.isStart = true;
                DiscordController.shouldUpdatePresence = true;
            }
        }
    }
    
    // Level Editor Play Button
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
                Patch.isStart = true;
                DiscordController.shouldUpdatePresence = true;
            }
        }
    }

    // Pause in LevelEditor
    [HarmonyPatch(typeof(scnEditor),"TogglePause")]
    public static class EditorPausePatch
    {
        public static void Prefix()
        {
            if (Patch.discord)
            {
                Patch.isdeath = false;
                Patch.isoverload = false;
                DiscordController.shouldUpdatePresence = true;
            }
        }
    }

    // Pause in game
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

    // Toggle Auto Play in LevelEditor
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

    // After fail, go back to Checkpoint
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

    // Clear portal
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
