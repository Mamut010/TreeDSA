using TreeDSA.Core;
using TreeDSA.Core.AvlTree;
using TreeDSA.Core.BPlusTree;
using TreeDSA.Core.BTree;
using TreeDSA.Core.SplayTree;
using TreeDSA.Utils;

static void TestTree<T>(ITree<T> tree, IEnumerable<T> items, params T[] toRemovedItems) where T : IComparable<T>
{
    foreach (var item in items)
    {
        tree.Add(item);
    }

    Console.WriteLine("*Initial tree*");
    tree.Display();
    Console.WriteLine($"Count: {tree.Count}");
    Console.WriteLine($"Enumerated items: {EnumerableUtil.EnumerableToString(tree)}");
    Console.WriteLine("--------------------------------------------");

    foreach (var toRemovedItem in toRemovedItems)
    {
        Console.WriteLine($"Removing: {toRemovedItem}");
        tree.Remove(toRemovedItem);
        tree.Display();
        Console.WriteLine($"Count: {tree.Count}");
        Console.WriteLine("=======================");
    }

    Console.WriteLine($"*Tree after removing {EnumerableUtil.EnumerableToString(toRemovedItems)}*");
    tree.Display();
    Console.WriteLine($"Count: {tree.Count}");

    Console.WriteLine("--------------------------------------------");
    Console.WriteLine($"Enumerated items: {EnumerableUtil.EnumerableToString(tree)}");
}

static void TestAvlTree<T>(IEnumerable<T> items, params T[] toRemovedItems) where T : IComparable<T>
{
    TestTree(new AvlTree<T>(), items, toRemovedItems);
}

static void TestSplayTree<T>(IEnumerable<T> items, params T[] toRemovedItems) where T : IComparable<T>
{
    TestTree(new SplayTree<T>(), items, toRemovedItems);
}

static void TestBTree<T>(int minimumDegree, IEnumerable<T> items, params T[] toRemovedItems) where T : IComparable<T>
{
    TestTree(new BTree<T>(minimumDegree), items, toRemovedItems);
}

static void TestBPlusTree<T>(int order, IEnumerable<T> items, params T[] toRemovedItems) where T : IComparable<T>
{
    var tree = new BPlusTree<T>(order);

    foreach (var item in items)
    {
        Console.WriteLine($"Inserting: {item}");
        tree.Add(item);
        tree.Display();
        Console.WriteLine($"Count: {tree.Count}");
        Console.WriteLine("=======================");
    }

    Console.WriteLine("--------------------------------------------");
    Console.WriteLine($"Enumerated items: {EnumerableUtil.EnumerableToString(tree.ToArray())}");
}

static void TestAvlTreeWithPredefinedItems()
{
    var items = new int[] { 9, 5, 10, 0, 6, 11, -1, 1, 2 };
    var toRemovedItems = new int[] { 8, 9, 10 };

    TestAvlTree(items, toRemovedItems);
}

static void TestSplayTreeWithPredefinedItems()
{
    //var items = new int[] { 100, 50, 200, 40, 60, 200 };
    //var toRemovedItems = new int[] { 50 };
    var items = new int[] { 9, 5, 10, 0, 6, 11, -1, 1, 2 };
    var toRemovedItems = new int[] { 8, 9, 10 };

    TestSplayTree(items, toRemovedItems);
}

static void TestBTreeWithPredefinedItems()
{
    //var items = new int[] { 10, 20, 30, 40, 50, 60, 70, 80, 90 };
    //var toRemovedItems = new int[] { };
    var items = Enumerable.Range(1, 9).Select(item => item * 10);
    var toRemovedItems = new int[] { 10, 30, 50, 70, 50, 30, 30 };

    var minimumDegree = 3;
    TestBTree(minimumDegree, items.Concat([30, 30]), toRemovedItems);
}

static void TestBTreeWithPredefinedItems2()
{
    var items = Enumerable.Range(0, 26).Select(item => (char)(item + 'A'));
    var toRemovedItems = new char[] { 'F', 'M', 'G' };

    var minimumDegree = 3;
    TestBTree(minimumDegree, items, toRemovedItems);
}

static void TestBPlusTreeWithPredefinedItems()
{
    var items = Enumerable.Range(1, 8).Select(item => item * 10);
    var toRemovedItems = Array.Empty<int>();

    var order = 4;
    TestBPlusTree(order, items.Concat([30, 30, 40, 40]), toRemovedItems);
}

//TestBTreeWithPredefinedItems2();
var t = 2;
var tree = new BTree<char>(t);
var chars = new char[] { 'F', 'S', 'Q', 'K', 'C', 'L', 'H', 'T', 'V', 'W', 'M', 'R', 'N', 'P', 'A', 'B', 'X', 'Y', 'D', 'Z', 'E' };
foreach (var item in chars)
{
    Console.WriteLine($"Inserting {item}...");
    tree.Add(item);
    tree.Display();
    Console.WriteLine();
}