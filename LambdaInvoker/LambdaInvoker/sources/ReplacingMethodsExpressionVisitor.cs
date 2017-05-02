using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace LambdaInvoker
{
    // The visitor class that consequentially visits branches of an expression tree and invokes methods with caching. Subclass of ExpressionVisitor.
    internal class ReplacingMethodsExpressionVisitor : ExpressionVisitor
    {
        // Types of expressions are allowed according to the internship task.
        protected ExpressionType[] AllowedExpressionTypes { get; } = {ExpressionType.Lambda, ExpressionType.Constant, ExpressionType.Call,
            ExpressionType.Add, ExpressionType.AddChecked, ExpressionType.Subtract, ExpressionType.SubtractChecked,  ExpressionType.Divide,
            ExpressionType.Multiply, ExpressionType.MultiplyChecked, ExpressionType.Modulo, ExpressionType.Negate, ExpressionType.NegateChecked};

        // Cache dictionary where keys are strings with pattern "Metod_name,arg1,arg2,..." and values are ConstantExpression objects.
        protected Dictionary<string, ConstantExpression> Cache { get; set; } = new Dictionary<string, ConstantExpression>();

        public override Expression Visit(Expression node)
        {
            // Looking through allowed expression types.
            if(AllowedExpressionTypes.Contains(node.NodeType))
                return base.Visit(node);

            throw new Exception($"Unsupported expression type: '{node.NodeType}'");
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            // Visiting the operand of an unary expression to get the constant operand.
            var operand = (ConstantExpression)Visit(node.Operand);
            
            // The only allowed unary expression is negation, so it just negates the operand.
            return Expression.Constant(-(int)operand.Value, typeof(int));
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            return node.Update(Visit(node.Body), node.Parameters);
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            // Looking through allowed operation types.
            if (!AllowedExpressionTypes.Contains(node.NodeType))
                throw new Exception($"Unsupported operation: '{node.NodeType}'");

            // Visiting both right and left part of a binary expression, then invoking arithmetic operation with constants.
            var result = Expression.Lambda(node.Update(Visit(node.Left), node.Conversion, Visit(node.Right))).Compile().DynamicInvoke();
            return Expression.Constant(result, typeof(int));
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            if(node.Type != typeof(int))
                throw new Exception("Only int values are allowed");
            return node;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if(node.Arguments.Count > 5)
                throw new Exception("The count of arguments more than 5");

            // In case of method call it firstly visits all method arguments to get constants representing returned values.
            var arguments = node.Arguments.Select(Visit).ToArray();
            var updatedNode = node.Update(node.Object, arguments);

            // Then it gets a cache key to find out whether there is the stored value in the cache or not.
            var cacheKey = GetCacheKey(updatedNode);
            if (Cache.ContainsKey(cacheKey)) { return Cache[cacheKey]; }

            // If not, it invokes the method using compiled from lambda delegate and puts the result in the cache. 
            // (Maybe pure reflection would be better, but compiled-expression approach is convenient and useful in this small task)
            var expressionResult = Expression.Constant(Expression.Lambda(updatedNode).Compile().DynamicInvoke(), typeof(int));
            Cache.Add(cacheKey, expressionResult);

            return expressionResult;
        }

        protected static string GetCacheKey(MethodCallExpression expression)
        {
            // Creating string with pattern "Metod_name,arg1,arg2,...". If method hasn't any arguments, only "Method_name".
            if (!expression.Arguments.Any())
                return expression.Method.Name;
            var arguments = string.Join(",", expression.Arguments.Select(e => e.ToString()).ToArray());
            return $"{expression.Method.Name},{arguments}";
        }
    }
}