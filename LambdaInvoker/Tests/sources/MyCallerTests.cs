using System;
using System.Linq.Expressions;
using LambdaInvoker;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LambdaInvokerTests
{
    [TestClass]
    public class MyCallerTests
    {
        private readonly ICaller _caller = new MyCaller();

        [TestMethod]
        public void Call_InvokeCorrectLambda_Returned150()
        {
            Expression<Func<int>> exp = () => A(1) + B(2 + C(), 3)*B(4, -D(5));
            var result = _caller.Call(exp);
            Assert.AreEqual(150, result);
        }

        [TestMethod]
        public void Call_InvokeLambdaWithMultipleNegation_ReturnedMinus2()
        {
            Expression<Func<int>> exp = () => -(-(-(-(-C()))));
            var result = _caller.Call(exp);
            Assert.AreEqual(-2, result);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void Call_InvokeLambdaWIthUnallowedOperation_ExceptionThrowed()
        {
            Expression<Func<int>> exp = () => 2 + C()^2;
            _caller.Call(exp);
        }

        [TestMethod]
        public void Call_CheckCachingUsingB_CountOfBIs1()
        {
            CountOfB = 0;
            Expression<Func<int>> exp = () => B(2 + C(), 3) * B(4, -D(5)) / B(4,3) + B(81/A(22) + 1, 6*C()/4);
            _caller.Call(exp);
            Assert.AreEqual(1,CountOfB);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void Call_InvokeMethodWithDoubleArgument_ExceptionThrowed()
        {
            Expression<Func<int>> exp = () => E(5.78, 2);
            _caller.Call(exp);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void Call_InvokeMethodWithSixArguments_ExceptionThrowed()
        {
            Expression<Func<int>> exp = () => Big(1, 2, 3, 4, 5, 6);
            _caller.Call(exp);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void Call_InvokeLambdaWIthConvertation_ExceptionThrowed()
        {
            Expression<Func<int>> exp = () => (int)F(5);
            _caller.Call(exp);
        }

        public static int CountOfB;

        public static int A(int a)
        {
            return 5 + a;
        }

        public static int C()
        {
            return 2;
        }

        public static int B(int a, int b)
        {
            CountOfB++;
            return b / a + b * a;
        }

        public static int D(int a)
        {
            return a - 8;
        }

        public static int E(double a, int b)
        {
            return (int)(a/b);
        }

        public static double F(int a)
        {
            return 0.7*a;
        }

        public static int Big(int a, int b, int c, int d, int e, int f)
        {
            return a + b - c + d - e + f;
        }
    }
}
