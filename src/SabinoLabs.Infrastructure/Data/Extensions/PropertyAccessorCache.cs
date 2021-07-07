using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace SabinoLabs.Infrastructure.Data.Extensions
{
    public static class PropertyAccessorCache<T> where T : class
    {
        private static readonly IDictionary<string, LambdaExpression> _cache;

        static PropertyAccessorCache()
        {
            Dictionary<string, LambdaExpression> storage = new Dictionary<string, LambdaExpression>();

            Type t = typeof(T);
            ParameterExpression parameter = Expression.Parameter(t, "p");
            foreach (PropertyInfo property in t.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                MemberExpression propertyAccess = Expression.MakeMemberAccess(parameter, property);
                LambdaExpression lambdaExpression = Expression.Lambda(propertyAccess, parameter);
                storage[property.Name] = lambdaExpression;
            }

            _cache = storage;
        }

        public static LambdaExpression Get(string propertyName)
        {
            LambdaExpression result;
            return _cache.TryGetValue(propertyName, out result) ? result : null;
        }
    }
}
