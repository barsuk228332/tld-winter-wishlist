using System;
using System.Collections.Generic;
using Il2Cpp;
using UnityEngine;

namespace WinterWishlist.Cheats
{
    /// <summary>Строка списка навыков для окна.</summary>
    internal sealed class SkillRow
    {
        internal Skill Skill;
        internal string Name;
        internal int Tier;
        internal string TierName;
    }

    internal static class SkillService
    {
        private static List<SkillRow> _rows = new List<SkillRow>();
        private static float _nextRefresh;

        /// <summary>
        /// Список реально существующих в этой версии игры навыков.
        /// Берём его у самого SkillsManager, а не из enum: в 2.55, например,
        /// ToolRepair больше не создаётся, и захардкоженный список ломался.
        /// </summary>
        internal static List<SkillRow> Rows()
        {
            if (Time.realtimeSinceStartup >= _nextRefresh)
                Refresh();
            return _rows;
        }

        internal static void Refresh()
        {
            _nextRefresh = Time.realtimeSinceStartup + 1f;

            var rows = new List<SkillRow>();
            try
            {
                var manager = GameManager.GetSkillsManager();
                if (manager != null)
                {
                    int count = manager.GetNumSkills();
                    for (int i = 0; i < count; i++)
                    {
                        var skill = manager.GetSkillFromIndex(i);
                        if (skill == null)
                            continue;

                        string name = null;
                        string tierName = "—";
                        int tier = -1;
                        try
                        {
                            name = skill.m_DisplayName;
                            tier = skill.GetCurrentTierNumber();
                            tierName = skill.GetCurrentTierName();
                        }
                        catch (Exception e)
                        {
                            WinterWishlistMod.Log.Warning("Чтение навыка #" + i + ": " + e.Message);
                        }

                        if (string.IsNullOrEmpty(name))
                            name = skill.m_SkillType.ToString();

                        rows.Add(new SkillRow
                        {
                            Skill = skill,
                            Name = name,
                            Tier = tier,
                            TierName = tierName,
                        });
                    }
                }
            }
            catch (Exception e)
            {
                WinterWishlistMod.Log.Warning("Список навыков: " + e.Message);
            }

            _rows = rows;
        }

        internal static bool Max(Skill skill)
        {
            try
            {
                if (skill == null)
                    return false;

                skill.SetTier((int)SkillTiers.Master, SkillsManager.PointAssignmentMode.AssignInAnyMode);
                skill.SetPoints(skill.GetMaxPoints(), SkillsManager.PointAssignmentMode.AssignInAnyMode);
                return true;
            }
            catch (Exception e)
            {
                WinterWishlistMod.Log.Warning("Максимум навыка: " + e.Message);
                return false;
            }
        }

        internal static int MaxAll()
        {
            int done = 0;
            foreach (var row in Rows())
            {
                if (Max(row.Skill))
                    done++;
            }

            Refresh();
            return done;
        }
    }
}
