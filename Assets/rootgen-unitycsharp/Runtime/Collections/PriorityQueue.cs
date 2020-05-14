using System.Collections.Generic;
/* Possible optimizations for this class:
*  -Turn the class into a Dictionary of lists.
*  -Abstract out the property in Cell for the next cell
*   with the same priority and put it in this class.
*/
public class PriorityQueue<T> {
    private List<T> _list = new List<T>();
    private int _highestPriority = int.MaxValue;
    private int _clearThreshold;
    private Dictionary<int, Queue<T>> _priorityQueue = new Dictionary<int, Queue<T>>();
    public int Count { get { return _priorityQueue.Count; } }

    public PriorityQueue(int clearThreshold = 10) {
        _clearThreshold = 10;
    }

/// <summary>
///     Enqueue an item in the priority queue.
/// </summary>
/// <param name="item">The item to be enqueued.</param>
/// <param name="priority">
///     An integer representing the priority of the item in the
///     queue, where a lower value means a higher priority.
/// </param>
    public void Enqueue(T item, int priority) {
        priority = -priority;

        if (_priorityQueue.ContainsKey(priority)) {
            _priorityQueue[priority].Enqueue(item);
        }
        else {
            Queue<T> queue = new Queue<T>();
            queue.Enqueue(item);
            _priorityQueue.Add(priority, queue);
        }

        if (priority > _highestPriority)
        {
            _highestPriority = priority;
        }
    }

    public T Dequeue() {
        if (Count == 0) {
            throw new System.IndexOutOfRangeException(
                "PriorityQueue is empty."
            );
        }

        RefreshHighestPriority();

        T result = _priorityQueue[_highestPriority].Dequeue();

        return result;
    }

    public void Clear() {
        _priorityQueue.Clear();
        _highestPriority = int.MaxValue;
    }

    private void ClearEmptyQueues() {
        foreach (int key in _priorityQueue.Keys) {
            if (_priorityQueue[key].Count == 0) {
                _priorityQueue.Remove(key);
            }
        }
    }
    
    private void RefreshHighestPriority() {
        ClearEmptyQueues();

        if (Count == 0)
            return;

        while(
            !_priorityQueue.ContainsKey(_highestPriority) ||
            _priorityQueue[_highestPriority] == null ||
            _priorityQueue[_highestPriority].Peek() == null
        ) {
            --_highestPriority;
        }
    }
}
