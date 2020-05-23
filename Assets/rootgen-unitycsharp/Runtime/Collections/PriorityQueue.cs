using System.Collections.Generic;
/* Possible optimizations for this class:
*  -Turn the class into a Dictionary of lists.
*  -Abstract out the property in hex for the next hex
*   with the same priority and put it in this class.
*/
public class PriorityQueue<T> {
    private int _highestPriority = int.MaxValue;
    private int _clearThreshold;
    private Dictionary<int, Queue<T>> _priorityQueue =
        new Dictionary<int, Queue<T>>();
    public int Count { 
        get { 
            return _priorityQueue.Count; 
        } 
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

        if (priority < _highestPriority) {
            _highestPriority = priority;
        }
    }

    public T Dequeue() {
        if (Count == 0) {
            throw new System.IndexOutOfRangeException(
                "PriorityQueue is empty."
            );
        }

        T result = _priorityQueue[_highestPriority].Dequeue();

        if (_priorityQueue[_highestPriority].Count == 0) {
            _priorityQueue.Remove(_highestPriority);
            foreach(KeyValuePair <int, Queue<T>> pair in _priorityQueue) {
                if (pair.Key < _highestPriority) {
                    _highestPriority = pair.Key;
                }
            }
        }

        return result;
    }

    public void Clear() {
        _priorityQueue.Clear();
        _highestPriority = int.MaxValue;
    }
}
