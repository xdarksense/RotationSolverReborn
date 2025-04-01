using FFXIVClientStructs.FFXIV.Client.Game.UI;

namespace RotationSolver.Basic.Helpers
{
    /// <summary>
    /// Enum representing different head markers.
    /// </summary>
    internal enum HeadMarker : byte
    {
        Attack1,
        Attack2,
        Attack3,
        Attack4,
        Attack5,
        Bind1,
        Bind2,
        Bind3,
        Stop1,
        Stop2,
        Square,
        Circle,
        Cross,
        Triangle,
        Attack6,
        Attack7,
        Attack8,
    }

    /// <summary>
    /// Helper class for managing head markers.
    /// </summary>
    internal class MarkingHelper
    {
        private static readonly object _lock = new object();

        /// <summary>
        /// Gets the marker for the specified head marker index.
        /// </summary>
        /// <param name="index">The head marker index.</param>
        /// <returns>The object ID of the marker.</returns>
        internal unsafe static long GetMarker(HeadMarker index)
        {
            var instance = MarkingController.Instance();
            if (instance == null || instance->Markers.Length == 0) return 0;
            return instance->Markers[(int)index].ObjectId;
        }

        /// <summary>
        /// Gets a value indicating whether there are any attack characters.
        /// </summary>
        internal static bool HaveAttackChara => GetAttackSignTargets().Any(id => id != 0);

        /// <summary>
        /// Gets the attack sign targets.
        /// </summary>
        internal static long[] GetAttackSignTargets()
        {
            return new long[]
            {
                GetMarker(HeadMarker.Attack1),
                GetMarker(HeadMarker.Attack2),
                GetMarker(HeadMarker.Attack3),
                GetMarker(HeadMarker.Attack4),
                GetMarker(HeadMarker.Attack5),
                GetMarker(HeadMarker.Attack6),
                GetMarker(HeadMarker.Attack7),
                GetMarker(HeadMarker.Attack8),
            };
        }

        /// <summary>
        /// Gets the stop targets.
        /// </summary>
        internal static long[] GetStopTargets()
        {
            return new long[]
            {
                GetMarker(HeadMarker.Stop1),
                GetMarker(HeadMarker.Stop2),
            };
        }

        /// <summary>
        /// Filters out characters that have stop markers.
        /// </summary>
        /// <param name="charas">The characters to filter.</param>
        /// <returns>The filtered characters.</returns>
        internal unsafe static IEnumerable<IBattleChara> FilterStopCharacters(IEnumerable<IBattleChara> charas)
        {
            var ids = new HashSet<long>(GetStopTargets().Where(id => id != 0));
            return charas.Where(b => !ids.Contains((long)b.GameObjectId));
        }
    }
}