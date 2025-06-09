using ECommons.GameFunctions;
using ECommons.Logging;

namespace RotationSolver.Basic.Helpers
{
    /// <summary>
    /// Target helper to get calculated targets within range with additional caching within a data refresh interval.
    /// </summary>
    public static class TargetHelper
    {
        /// <summary>
        /// Retrieves a collection of valid battle characters that can be targeted based on the specified criteria.
        /// </summary>
        /// <param name="range">The range to check for targets.</param>
        /// <param name="getFriendly">If set to <c>true</c>, gets party members; if set to <c>false</c> gets hostiles. Set to null gets all.</param>
        /// <returns>
        /// A <see cref="List{IBattleChara}"/> containing the valid targets.
        /// </returns>
        public static List<IBattleChara> GetTargetsByRange(float range, bool? getFriendly = null)
        {
            if (DataCenter.AllTargets == null)
            {
                return [];
            }

            List<IBattleChara> targets = [];
            float searchRange = range;
            if (getFriendly == true)
            {
                range += 1000; // Store party with flag 1000
            }
            else if (getFriendly == false)
            {
                range += 2000; // Store hostiles with flag 2000
            }

            if (DataCenter.TargetsByRange.TryGetValue(float.Round(range, 0), out List<IBattleChara>? cachedTargets))
            {
                targets = cachedTargets;
            }
            else
            {
                if (getFriendly == true)
                {
                    foreach (IBattleChara target in DataCenter.PartyMembers.GetObjectInRadius(searchRange))
                    {
                        if (!ValidityCheck(target))
                        {
                            continue;
                        }

                        targets.Add(target);
                    }
                }
                else if (getFriendly == false)
                {
                    foreach (IBattleChara target in DataCenter.AllHostileTargets.GetObjectInRadius(searchRange))
                    {
                        if (!ValidityCheck(target))
                        {
                            continue;
                        }

                        targets.Add(target);
                    }
                }
                else
                {
                    foreach (IBattleChara target in DataCenter.AllTargets.GetObjectInRadius(searchRange))
                    {
                        if (!ValidityCheck(target))
                        {
                            continue;
                        }

                        targets.Add(target);
                    }
                }

                DataCenter.TargetsByRange[float.Round(range, 0)] = targets;
            }

            return targets;
        }

        /// <summary>
        /// Performs a general check on the specified battle character to determine if it meets the criteria for targeting.
        /// </summary>
        /// <param name="battleChara">The battle character to check.</param>
        /// <returns>
        /// <c>true</c> if the battle character meets the criteria for targeting; otherwise, <c>false</c>.
        /// </returns>
        private static bool ValidityCheck(IBattleChara battleChara)
        {
            if (battleChara == null)
            {
                return false;
            }

            unsafe
            {
                if (battleChara.Struct() == null)
                {
                    return false;
                }
            }

            if (!battleChara.IsTargetable)
            {
                return false;
            }

            try
            {
                if (battleChara.StatusList == null)
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                PluginLog.Error($"Exception accessing StatusList for {battleChara?.NameId}: {ex.Message}");
                return false;
            }

            bool isBlacklisted = false;
            foreach (uint id in DataCenter.BlacklistedNameIds)
            {
                if (id == battleChara.NameId)
                {
                    isBlacklisted = true;
                    break;
                }
            }
            if (isBlacklisted)
            {
                return false;
            }

            if (battleChara.IsEnemy() && !battleChara.IsAttackable())
            {
                return false;
            }

            bool isStopTarget = false;
            foreach (long stopId in MarkingHelper.GetStopTargets())
            {
                if (stopId == (long)battleChara.GameObjectId)
                {
                    isStopTarget = true;
                    break;
                }
            }
            return !isStopTarget || !Service.Config.FilterStopMark;
        }
    }
}
