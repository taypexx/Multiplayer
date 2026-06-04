using System;
using System.Collections.Generic;
using System.Collections;
using Il2CppAssets.Scripts.GameCore.HostComponent;
using Il2CppFormulaBase;
using MelonLoader;
using UnityEngine;

namespace Multiplayer.Managers
{
    internal static class AccuracyManager
    {
        private const float Precision = 0.0001f;
        private static readonly HashSet<float> SpecialValues = new() { 0.6f, 0.7f, 0.8f, 0.9f, 1f };

        private static readonly HashSet<short> PlayedNoteIds = new();
        private static readonly HashSet<short> MissedNoteIds = new();

        private static int TotalMusic;
        private static int TotalEnergy;
        private static int TotalHittable;
        private static int TotalBlock;

        private static int CurrentPerfect;
        private static int CurrentGreat;
        private static int CurrentBlock;
        private static int CurrentMusic;
        private static int CurrentEnergy;
        private static int CurrentRedPoint;

        private static int MissMonster;
        private static int MissLong;
        private static int MissLongPair;
        private static int MissGhost;
        private static int MissEnergy;
        private static int MissMusic;
        private static int MissRedPoint;
        private static int MissBlock;
        private static int MissMul;

        private static TaskStageTarget TaskStageTarget;
        private static StageBattleComponent StageBattleComponent;

        // Reset all stats for the next song
        internal static void Init()
        {
            PlayedNoteIds.Clear();
            MissedNoteIds.Clear();

            TotalMusic = 0;
            TotalEnergy = 0;
            TotalHittable = 0;
            TotalBlock = 0;

            CurrentPerfect = 0;
            CurrentGreat = 0;
            CurrentBlock = 0;
            CurrentMusic = 0;
            CurrentEnergy = 0;
            CurrentRedPoint = 0;

            MissMonster = 0;
            MissLong = 0;
            MissLongPair = 0;
            MissGhost = 0;
            MissEnergy = 0;
            MissMusic = 0;
            MissRedPoint = 0;
            MissBlock = 0;
            MissMul = 0;

            TaskStageTarget = TaskStageTarget.instance;
            StageBattleComponent = StageBattleComponent.instance;

            if (StageBattleComponent == null) return;

            try
            {
                var musicData = StageBattleComponent.GetMusicData();
                if (musicData == null) return;

                foreach (var note in musicData)
                {
                    if (note == null || note.noteData == null) continue;
                    var type = note.noteData.type;

                    if (!note.isLongPressing)
                    {
                        if (note.noteData.addCombo)
                            TotalHittable++;
                    }

                    switch (type)
                    {
                        case 2: // NoteType.Block
                            TotalBlock++;
                            break;
                        case 6: // NoteType.Energy
                            TotalEnergy++;
                            break;
                        case 7: // NoteType.Music
                            TotalMusic++;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Main.Log("Failed to initialize AccuracyManager: " + ex.Message);
            }
        }

        private static int GetMissCountHittable() => MissMonster + MissLong + MissMul;
        private static int GetMissCountCollectible() => MissEnergy + MissMusic + MissRedPoint + MissGhost;
        private static int GetMissCount() => GetMissCountHittable() + GetMissCountCollectible() + MissBlock;

        private static float AccuracyCalculationTotal => TotalMusic + TotalEnergy + TotalHittable + TotalBlock;
        private static float AccuracyCalculationCounted => CurrentPerfect + CurrentGreat / 2f + CurrentBlock + CurrentMusic + CurrentEnergy + CurrentRedPoint;
        private static float AccuracyCalculationRest => Math.Max(0f, GetAccuracyRest());

        private static float GetAccuracyRest() => AccuracyCalculationTotal - CurrentPerfect - CurrentGreat - CurrentBlock - CurrentMusic - CurrentEnergy - CurrentRedPoint - GetMissCount() - MissLongPair;

        // Calculate final rounded accuracy value
        internal static float GetCalculatedAccuracy()
        {
            UpdateCurrentStats();

            float total = AccuracyCalculationTotal;
            if (total <= 0f) return 100f;

            float rest = AccuracyCalculationRest;
            float counted = AccuracyCalculationCounted;

            float acc = (counted + rest) / total;

            float rounded = MathF.Round(acc / Precision) * Precision;
            float finalAcc = (acc < rounded && SpecialValues.Contains(rounded) ? rounded - Precision : rounded) * 100f;
            return finalAcc;
        }

        // Sync current statistics from the game engine
        private static void UpdateCurrentStats()
        {
            if (TaskStageTarget == null) return;

            CurrentPerfect = TaskStageTarget.m_PerfectResult;
            CurrentGreat = TaskStageTarget.m_GreatResult;
            CurrentMusic = TaskStageTarget.m_MusicCount;
            CurrentEnergy = TaskStageTarget.m_EnergyCount;
            CurrentRedPoint = TaskStageTarget.m_RedPoint;
        }

        // Handle standard play results
        internal static void HandleSetPlayResult(int idx, byte result, bool isMulStart, bool isMulEnd, bool isLeft)
        {
            if (StageBattleComponent == null) return;
            var note = StageBattleComponent.GetMusicDataByIdx(idx);
            if (note == null || note.noteData == null) return;
            var type = note.noteData.type;
            var oid = note.objId;

            switch (result)
            {
                case 4 when type == 2: // NoteType.Block
                    CountNote(oid, CountNoteAction.Block);
                    break;
                case 1 when type == 3: // NoteType.Long
                    CountNote(oid, CountNoteAction.MissLong, -1, note.isLongPressStart);
                    break;
            }

            if (type == 8) // NoteType.Mul
                CountMul(oid, result, (float)note.configData.length);
        }

        // Handle miss results for various object types
        internal static void HandleMissCube(int idx, decimal currentTick)
        {
            if (StageBattleComponent == null) return;
            try
            {
                var result = BattleEnemyManager.instance.GetPlayResult(idx);
                var note = StageBattleComponent.GetMusicDataByIdx(idx);
                if (note == null || note.noteData == null) return;
                var type = note.noteData.type;
                var oid = note.objId;
                var isDouble = note.isDouble;

                if (result == 0 || result == 1)
                {
                    switch (type)
                    {
                        case 4: // NoteType.Ghost
                            CountNote(oid, CountNoteAction.MissGhost);
                            break;
                        case 6: // NoteType.Energy
                            CountNote(oid, CountNoteAction.MissEnergy);
                            break;
                        case 7: // NoteType.Music
                            CountNote(oid, CountNoteAction.MissMusic);
                            break;
                        case 2: // NoteType.Block
                            if (result != 0)
                                CountNote(oid, CountNoteAction.MissBlock);
                            break;
                        case 8: // NoteType.Mul
                            break;
                        default:
                            short doubleOid = -1;
                            if (isDouble)
                            {
                                var doubleNote = StageBattleComponent.GetMusicDataByIdx(note.doubleIdx);
                                if (doubleNote != null) doubleOid = doubleNote.objId;
                            }
                            CountNote(oid, CountNoteAction.MissMonster, doubleOid);
                            break;
                    }
                }

                if (type == 8) // NoteType.Mul
                    CountMul(oid, result, (float)note.configData.length);
            }
            catch (Exception ex)
            {
                Main.Log("HandleMissCube failed: " + ex.Message);
            }
        }

        // Process note counting based on action type
        private static void CountNote(short oid, CountNoteAction action, short doubleOid = -1, bool isLongStart = false, float time = 0f)
        {
            switch (action)
            {
                case CountNoteAction.Block:
                    if (PlayedNoteIds.Add(oid))
                        CurrentBlock++;
                    break;

                case CountNoteAction.MissMonster:
                    if (doubleOid == -1)
                    {
                        if (MissedNoteIds.Add(oid))
                            MissMonster++;
                    }
                    else
                    {
                        if (MissedNoteIds.Add(oid) && MissedNoteIds.Add(doubleOid))
                            MissMonster += 2;
                    }
                    break;

                case CountNoteAction.MissBlock:
                    if (MissedNoteIds.Add(oid))
                        MissBlock++;
                    if (!PlayedNoteIds.Add(oid))
                        CurrentBlock--;
                    break;

                case CountNoteAction.MissLong:
                    if (MissedNoteIds.Add(oid))
                    {
                        MissLong++;
                        if (isLongStart)
                            MissLongPair++;
                    }
                    break;

                case CountNoteAction.MissGhost:
                    if (MissedNoteIds.Add(oid))
                        MissGhost++;
                    break;

                case CountNoteAction.MissEnergy:
                    if (MissedNoteIds.Add(oid))
                        MissEnergy++;
                    break;

                case CountNoteAction.MissMusic:
                    if (MissedNoteIds.Add(oid))
                        MissMusic++;
                    break;

                case CountNoteAction.Mul:
                    if (PlayedNoteIds.Add(oid))
                    {
                        if (MissedNoteIds.Remove(oid))
                            MissMul--;
                    }
                    break;

                case CountNoteAction.MissMul:
                    if (StageBattleComponent == null) break;
                    var curTick = StageBattleComponent.realTimeTick;
                    MelonCoroutines.Start(DelayAction(() =>
                    {
                        if (StageBattleComponent == null || StageBattleComponent.realTimeTick <= curTick)
                            return;
                        if (!PlayedNoteIds.Contains(oid) && MissedNoteIds.Add(oid))
                            MissMul++;
                    }, time));
                    break;
            }
        }

        // Process multiple hit notes
        private static void CountMul(short oid, int result, float time)
        {
            switch (result)
            {
                case 0:
                case 1:
                    CountNote(oid, CountNoteAction.MissMul, time: time);
                    break;
                case 3:
                case 4:
                    CountNote(oid, CountNoteAction.Mul);
                    break;
            }
        }

        // Action wrapper for delayed execution
        private static IEnumerator DelayAction(Action action, float delay)
        {
            yield return new WaitForSeconds(delay);
            action?.Invoke();
        }

        // Note counting actions
        private enum CountNoteAction
        {
            Block,
            Mul,
            MissMonster,
            MissBlock,
            MissLong,
            MissGhost,
            MissEnergy,
            MissMusic,
            MissMul
        }
    }
}
