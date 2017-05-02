using System;
using System.Linq.Expressions;

namespace LambdaInvoker
{
    public class MyCaller : ICaller
    {
        // The instance of my ReplacingMethodsExpressionVisitor class.
        protected ExpressionVisitor Visitor { get; } = new ReplacingMethodsExpressionVisitor();

        public int Call(Expression<Func<int>> expression)
        {
            if(expression.NodeType != ExpressionType.Lambda)
                throw new Exception("The expression argument isn't a lambda expression");

            // Invoking the expression body using the visitor. 
            var invokedExpression = (LambdaExpression)Visitor.Visit(expression);

            // Final invoking of the reformed lambda expression.
            var result = (int)invokedExpression.Compile().DynamicInvoke();
            return result;
        }
    }
}
