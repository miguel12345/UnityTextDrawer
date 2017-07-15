using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


//This is a home-brewed cache system
public class LRUDictionary<TK,TV> : IDictionary<TK,TV>
{
    private class LinkedListNode
    {
        public readonly TK Key;
        public readonly TV Val;
        public LinkedListNode Next;
        public LinkedListNode Previous;

        public LinkedListNode(TK k, TV v)
        {
            Key = k;
            Val = v;
        }
    }

    public int Capacity;
    private int _size;
    private LinkedListNode _mostRecent, _oldest;

    private readonly Dictionary<TK, LinkedListNode> _internalDict = new Dictionary<TK, LinkedListNode>();

    public LRUDictionary(int capacity)
    {
        if (capacity <= 0)
        {
            throw new ArgumentException("Positive capacity required.");
        }

        Capacity = capacity;
        _size = 0;
    }
    

    public IEnumerator<KeyValuePair<TK, TV>> GetEnumerator()
    {
        return _internalDict.Select(kvp => new KeyValuePair<TK, TV>(kvp.Key, kvp.Value.Val)).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Add(KeyValuePair<TK, TV> item)
    {
        throw new NotImplementedException();
    }

    public void Clear()
    {
        _internalDict.Clear();
        _mostRecent = _oldest = null;
    }

    bool ICollection<KeyValuePair<TK, TV>>.Contains(KeyValuePair<TK, TV> keyValuePair)
    {
        var key = keyValuePair.Key;
        return _internalDict.ContainsKey(key) && _internalDict[key].Val.Equals(keyValuePair.Value);
    }

    void ICollection<KeyValuePair<TK, TV>>.CopyTo(KeyValuePair<TK, TV>[] array, int arrayIndex)
    {
        throw new InvalidOperationException("LRUCache does not support CopyTo");
    }

    public bool Remove(KeyValuePair<TK, TV> item)
    {
        throw new NotImplementedException();
    }

    public int Count {
        get { return _internalDict.Count; }
    }
    public bool IsReadOnly {
        get { return false; }
    }
    public void Add(TK key, TV value)
    {
        var node = new LinkedListNode(key,value);
        AddNode(node);
        _internalDict.Add(key,node);
    }

    public bool ContainsKey(TK key)
    {
        return _internalDict.ContainsKey(key);
    }

    public bool Remove(TK key)
    {
        LinkedListNode node;
        if (_internalDict.TryGetValue(key, out node))
        {
            RemoveNode(node);
        }

        return _internalDict.Remove(key);
    }

    public bool TryGetValue(TK key, out TV value)
    {
        LinkedListNode node;
        if (_internalDict.TryGetValue(key, out node))
        {
            value = node.Val;
            return true;
        }

        value = default(TV);
        return false;
    }

    public TV this[TK key]
    {
        get
        {
            var node = _internalDict[key];
            MoveNodeToFront(node);
            return node.Val;
        }
        set
        {
            var node = new LinkedListNode(key,value);
            _internalDict[key] = node;
            AddNode(node);
        }
    }

    private void AddNode(LinkedListNode node)
    {
        if (_oldest == null) _oldest = node;
        
        var tmp = _mostRecent;
        _mostRecent = node;
        
        node.Next = tmp;
        if (tmp != null)
        {
            tmp.Previous = _mostRecent;
        }

        _size++;

        if (ExceedsCapacity(_size))
        {
            RemoveOldestNode();
        }
    }

    private void RemoveNode(LinkedListNode node)
    {
        var previousNode = node.Previous;
        if (previousNode != null)
        {
            previousNode.Next = node.Next;
        }

        var nextNode = node.Next;
        if (nextNode != null)
        {
            nextNode.Previous = previousNode;
        }
        
        node.Next = node.Previous = null;

        if (_oldest == node) _oldest = previousNode;

        _size--;
    }

    private void MoveNodeToFront(LinkedListNode node)
    {
        RemoveNode(node);
        AddNode(node);
    }

    private void RemoveOldestNode()
    {
        if (_oldest == null)
        {
            throw new InvalidOperationException();
        }

        var k = _oldest.Key;
        Remove(k);
    }

    public bool ExceedsCapacity(int size)
    {
        return size > Capacity;
    }

    public ICollection<TK> Keys {
        get { return _internalDict.Keys; }
        }
    public ICollection<TV> Values {
        get { throw new InvalidOperationException(); }
    }
}
