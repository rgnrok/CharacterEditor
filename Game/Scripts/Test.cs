using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CharacterEditor
{
    public enum TestEnum 
    {
        A,
        B,
        C
    }


    public struct TestStruct
    {
        public readonly int x;
        public readonly int y;

        public TestStruct(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    public class StructComparer : IEqualityComparer<TestStruct>
    {
        public bool Equals(TestStruct obj1, TestStruct obj2)
        {
            return obj1.x == obj2.x && obj1.y == obj2.y;
        }

        public int GetHashCode(TestStruct obj)
        {
            return obj.x ^ obj.y;
        }
    }

    public class StructComparer2 : IEqualityComparer<TestStruct2>
    {
        public bool Equals(TestStruct2 obj1, TestStruct2 obj2)
        {
            return obj1.x == obj2.x && obj1.y == obj2.y;
        }

        public int GetHashCode(TestStruct2 obj)
        {
            return obj.x ^ obj.y;
        }
    }

    public struct TestStruct2 : IEquatable<TestStruct2>
    {
        public readonly int x;
        public readonly int y;

        public TestStruct2(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is TestStruct2)) return false;
            var st2 = (TestStruct2) obj;
            return Equals(st2);
        }

        public override int GetHashCode()
        {
            return x ^ y;
        }

        public bool Equals(TestStruct2 other)
        {
            return x == other.x && y == other.y;
        }
    }

    public static class TestProg
    {
        public static void Do()
        {
            var t1 = new TestStruct(1, 2);
            var t2 = new TestStruct(2, 1);


            var capacity = 10000000;
            var dic = new Dictionary<TestStruct, int>(capacity);
            Logger.LogWarning("Start Struct add");
            var start = DateTime.Now;
            for (var i = 0; i < capacity; i++) dic.Add(new TestStruct(i, i * 2), 0);
            Logger.LogWarning($"End Struct add {(DateTime.Now - start).Ticks/10000000f}");

            Logger.LogWarning("Start Struct get");
            start = DateTime.Now;
            for (var i = 0; i < capacity; i++)
                if (!dic.ContainsKey(new TestStruct(i, i * 2)))
                    continue;
            Logger.LogWarning($"End Struct get {(DateTime.Now - start).Ticks / 10000000f}");

            Logger.LogWarning("==============================");


            var dic2 = new Dictionary<TestStruct, int>(capacity, new StructComparer());
            Logger.LogWarning("Start StructComparer add");
            start = DateTime.Now;
            for (var i = 0; i < capacity; i++) dic2.Add(new TestStruct(i, i * 2), 0);
            Logger.LogWarning($"End StructComparer add {(DateTime.Now - start).Ticks / 10000000f}");

            Logger.LogWarning("Start StructComparer get");
            start = DateTime.Now;
            for (var i = 0; i < capacity; i++)
                if (!dic2.ContainsKey(new TestStruct(i, i * 2)))
                    continue;
            Logger.LogWarning($"End StructComparer get {(DateTime.Now - start).Ticks / 10000000f}");

            Logger.LogWarning("==============================");

            var dic3 = new Dictionary<TestStruct2, int>(capacity);
            Logger.LogWarning("Start Struct2 add");
            start = DateTime.Now;
            for (var i = 0; i < capacity; i++) dic3.Add(new TestStruct2(i, i * 2), 0);
            Logger.LogWarning($"End Struct2 add {(DateTime.Now - start).Ticks / 10000000f}");

            Logger.LogWarning("Start Struct2 get");
            start = DateTime.Now;
            for (var i = 0; i < capacity; i++)
                if (!dic3.ContainsKey(new TestStruct2(i, i * 2)))
                    continue;
            Logger.LogWarning($"End Struct2 get {(DateTime.Now - start).Ticks / 10000000f}");

            Logger.LogWarning("==============================");

            var dic4 = new Dictionary<TestStruct2, int>(capacity, new StructComparer2());
            Logger.LogWarning("Start Struct2 StructComparer2 add");
            start = DateTime.Now;
            for (var i = 0; i < capacity; i++) dic4.Add(new TestStruct2(i, i * 2), 0);
            Logger.LogWarning($"End Struct2 StructComparer2 add {(DateTime.Now - start).Ticks / 10000000f}");

            Logger.LogWarning("Start Struct2 StructComparer2 get");
            start = DateTime.Now;
            for (var i = 0; i < capacity; i++)
                if (!dic4.ContainsKey(new TestStruct2(i, i * 2)))
                    continue;
            Logger.LogWarning($"End Struct2 StructComparer2 get {(DateTime.Now - start).Ticks / 10000000f}");
        }
    }
}