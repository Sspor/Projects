using System;

namespace LambdaInvoker
{
    public interface ICaller
    {
        int Call(System.Linq.Expressions.Expression<Func<int>> expression);
    }
}