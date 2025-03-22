using System;

namespace GameProject {
    public class Test {
        public static void TestAll() {
            Console.WriteLine("Running all tests:");
            Test.Test0();
            Test.Test1();
            Test.Test2();
            Test.Test3();
            Test.Test4();
            Test.Test5();
            Test.Test6();
            Console.WriteLine("Done running tests.\n");
        }
        public static void Test0() {
            var a = new AposNumber(100, 0, 0);
            var b = new AposNumber(50, 1, 0);

            Assert($"{a} <  {b} : {a < b}", a < b, false);
            Assert($"{a} >  {b} : {a > b}", a > b, false);
            Assert($"{a} == {b} : {a == b}", a == b, true);
            Console.WriteLine();
        }
        public static void Test1() {
            var a = new AposNumber(200, 0, 0);
            var b = new AposNumber(100, 1, 0);

            Assert($"{a} <  {b} : {a < b}", a < b, false);
            Assert($"{a} >  {b} : {a > b}", a > b, false);
            Assert($"{a} == {b} : {a == b}", a == b, true);
            Console.WriteLine();
        }
        public static void Test2() {
            var a = new AposNumber(100, 0, 0);
            var b = new AposNumber(100, 1, 0);

            Assert($"{a} <  {b} : {a < b}", a < b, true);
            Assert($"{a} >  {b} : {a > b}", a > b, false);
            Assert($"{a} == {b} : {a == b}", a == b, false);
            Console.WriteLine();
        }
        public static void Test3() {
            var a = new AposNumber(300, 0, 0);
            var b = new AposNumber(100, 1, 0);

            Assert($"{a} <  {b} : {a < b}", a < b, false);
            Assert($"{a} >  {b} : {a > b}", a > b, true);
            Assert($"{a} == {b} : {a == b}", a == b, false);
            Console.WriteLine();
        }
        public static void Test4() {
            var a = new AposNumber(100, 0, 1);
            var b = new AposNumber(100, 1, 0);

            Assert($"{a} <  {b} : {a < b}", a < b, false);
            Assert($"{a} >  {b} : {a > b}", a > b, true);
            Assert($"{a} == {b} : {a == b}", a == b, false);
            Console.WriteLine();
        }
        public static void Test5() {
            var a = new AposNumber(100, 0, 0);
            var b = new AposNumber(100, 1, 1);

            Assert($"{a} <  {b} : {a < b}", a < b, true);
            Assert($"{a} >  {b} : {a > b}", a > b, false);
            Assert($"{a} == {b} : {a == b}", a == b, false);
            Console.WriteLine();
        }
        public static void Test6() {
            var a = new AposNumber(100, 0, 0);
            var b = new AposNumber(100, 999999999, 0);

            Assert($"{a} <  {b} : {a < b}", a < b, true);
            Assert($"{a} >  {b} : {a > b}", a > b, false);
            Assert($"{a} == {b} : {a == b}", a == b, false);
            Console.WriteLine();
        }

        public static void Assert(string text, bool result, bool expected) {
            var oldColor = Console.ForegroundColor;
            if (result == expected) {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Success: {text}");
            } else {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine($"Failed:  {text}");
            }
            Console.ForegroundColor = oldColor;
        }
    }
}
