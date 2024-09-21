using System.Collections;

namespace RotationSolver.Basic.Data;

/// <summary>
/// A class to delay the object list checking.
/// </summary>
/// <typeparam name="T">The type of objects in the list.</typeparam>
public class ObjectListDelay<T> : IEnumerable<T> where T : IGameObject
{
    private IEnumerable<T> _list = new List<T>();
    private readonly Func<(float min, float max)> _getRange;
    private SortedList<ulong, DateTime> _revealTime = new();
    private readonly Random _ran = new(DateTime.UtcNow.Millisecond);

    /// <summary>
    /// Initializes a new instance of the <see cref="ObjectListDelay{T}"/> class.
    /// </summary>
    /// <param name="getRange">The function to get the range of delay times.</param>
    public ObjectListDelay(Func<(float min, float max)> getRange)
    {
        _getRange = getRange;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ObjectListDelay{T}"/> class.
    /// </summary>
    /// <param name="getRange">The function to get the range of delay times as a <see cref="Vector2"/>.</param>
    public ObjectListDelay(Func<Vector2> getRange)
        : this(() =>
        {
            var vec = getRange();
            return (vec.X, vec.Y);
        })
    {
    }

    /// <summary>
    /// Delays the list of objects.
    /// </summary>
    /// <param name="originData">The original list of objects.</param>
    public void Delay(IEnumerable<T> originData)
    {
        var outList = new List<T>(originData.Count());
        var revealTime = new SortedList<ulong, DateTime>();
        var now = DateTime.UtcNow;

        foreach (var item in originData)
        {
            if (!_revealTime.TryGetValue(item.GameObjectId, out var time))
            {
                var (min, max) = _getRange();
                var delaySecond = min + (float)_ran.NextDouble() * (max - min);
                time = now + new TimeSpan(0, 0, 0, 0, (int)(delaySecond * 1000));
            }
            revealTime[item.GameObjectId] = time;

            if (now > time)
            {
                outList.Add(item);
            }
        }

        _list = outList;
        _revealTime = revealTime;
    }

    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>An enumerator that can be used to iterate through the collection.</returns>
    public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => _list.GetEnumerator();
}