using System.Collections.Generic;
using NUnit.Framework;
using SeesawHelper;

public class FixedSizedQueueTests
{
    [Test]
    public void Enqueue_UnderLimit_KeepsAllElements()
    {
        var queue = new FixedSizedQueue<int> { Limit = 3 };

        queue.Enqueue(1);
        queue.Enqueue(2);

        Assert.AreEqual(new[] { 1, 2 }, ToList(queue));
    }

    [Test]
    public void Enqueue_AtLimit_KeepsAllElements()
    {
        var queue = new FixedSizedQueue<int> { Limit = 3 };

        queue.Enqueue(1);
        queue.Enqueue(2);
        queue.Enqueue(3);

        Assert.AreEqual(new[] { 1, 2, 3 }, ToList(queue));
    }

    [Test]
    public void Enqueue_OverLimit_DropsOldestElements()
    {
        var queue = new FixedSizedQueue<int> { Limit = 3 };

        queue.Enqueue(1);
        queue.Enqueue(2);
        queue.Enqueue(3);
        queue.Enqueue(4);

        Assert.AreEqual(new[] { 2, 3, 4 }, ToList(queue));
    }

    [Test]
    public void Enqueue_FarOverLimit_RetainsOnlyMostRecent()
    {
        var queue = new FixedSizedQueue<int> { Limit = 2 };

        for (int i = 1; i <= 10; i++)
            queue.Enqueue(i);

        Assert.AreEqual(new[] { 9, 10 }, ToList(queue));
    }

    [Test]
    public void Enqueue_PreservesInsertionOrder()
    {
        var queue = new FixedSizedQueue<string> { Limit = 4 };

        queue.Enqueue("a");
        queue.Enqueue("b");
        queue.Enqueue("c");

        Assert.AreEqual(new[] { "a", "b", "c" }, ToList(queue));
    }

    [Test]
    public void NewQueue_IsEmpty()
    {
        var queue = new FixedSizedQueue<int> { Limit = 5 };

        Assert.IsEmpty(ToList(queue));
    }

    [Test]
    public void Limit_CanBeChangedAfterConstruction()
    {
        var queue = new FixedSizedQueue<int> { Limit = 5 };
        queue.Enqueue(1);
        queue.Enqueue(2);
        queue.Enqueue(3);

        // Shrinking the limit only takes effect on the next Enqueue
        queue.Limit = 2;
        queue.Enqueue(4);

        Assert.AreEqual(new[] { 3, 4 }, ToList(queue));
    }

    private static List<T> ToList<T>(FixedSizedQueue<T> queue)
    {
        var list = new List<T>();
        var enumerator = queue.GetEnumerator();
        while (enumerator.MoveNext())
            list.Add(enumerator.Current);
        return list;
    }
}
