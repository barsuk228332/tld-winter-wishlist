using System;
using Il2Cpp;

namespace WinterWishlist.Cheats
{
    internal static class SkillService
    {
        internal static readonly SkillType[] All =
        {
            SkillType.Firestarting,
            SkillType.CarcassHarvesting,
            SkillType.Cooking,
            SkillType.IceFishing,
            SkillType.Rifle,
            SkillType.Archery,
            SkillType.ClothingRepair,
            SkillType.ToolRepair,
            SkillType.Revolver,
            SkillType.Gunsmithing,
        };

        internal static SkillsManager Manager
        {
            get
            {
                try
                {
                    return GameManager.GetSkillsManager();
                }
                catch
                {
                    return null;
                }
            }
        }

        internal static Skill Get(SkillType type)
        {
            try
            {
                return Manager?.GetSkill(type);
            }
            catch (Exception e)
            {
                WinterWishlistMod.Log.Warning("Скилл " + type + ": " + e.Message);
                return null;
            }
        }

        internal static string Name(SkillType type)
        {
            try
            {
                var skill = Get(type);
                if (skill != null && !string.IsNullOrEmpty(skill.m_DisplayName))
                    return skill.m_DisplayName;
            }
            catch
            {
                // ниже вернём техническое имя
            }
            return type.ToString();
        }

        internal static int Level(SkillType type)
        {
            try
            {
                return Get(type)?.GetCurrentTierNumber() ?? -1;
            }
            catch
            {
                return -1;
            }
        }

        internal static string TierName(SkillType type)
        {
            try
            {
                var skill = Get(type);
                return skill?.GetCurrentTierName() ?? "—";
            }
            catch
            {
                return "—";
            }
        }

        internal static bool MaxOne(SkillType type)
        {
            try
            {
                var skill = Get(type);
                if (skill == null)
                    return false;

                skill.SetTier((int)SkillTiers.Master, SkillsManager.PointAssignmentMode.AssignInAnyMode);
                skill.SetPoints(skill.GetMaxPoints(), SkillsManager.PointAssignmentMode.AssignInAnyMode);
                return true;
            }
            catch (Exception e)
            {
                WinterWishlistMod.Log.Warning("Максимум скилла " + type + ": " + e.Message);
                return false;
            }
        }

        internal static int MaxAll()
        {
            int done = 0;
            foreach (var type in All)
            {
                if (MaxOne(type))
                    done++;
            }
            return done;
        }
    }
}
