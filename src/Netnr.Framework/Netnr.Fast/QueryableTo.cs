using System;
using System.Linq;
using System.Linq.Expressions;

namespace Netnr.Fast
{
    public class QueryableTo
    {
        /// <summary>
        /// 排序
        /// </summary>
        /// <param name="sorts">排序字段，支持多个，逗号分割</param>
        /// <param name="orders">排序类型，支持多个，逗号分割</param>
        public static IQueryable<T> OrderBy<T>(IQueryable<T> query, string sorts, string orders = "asc")
        {
            var listSort = sorts.Split(',').ToList();
            var listOrder = orders.Split(',').ToList();
            
            //倒叙
            for (int i = listSort.Count - 1; i >= 0; i--)
            {
                var sort = listSort[i];
                var order = i < listOrder.Count ? listOrder[i] : "asc";

                var property = typeof(T).GetProperties().Where(x => x.Name.ToLower() == sort.ToLower()).First();

                var parameter = Expression.Parameter(typeof(T), "p");
                var propertyAccess = Expression.MakeMemberAccess(parameter, property);
                var lambda = Expression.Lambda(propertyAccess, parameter);

                if (order.ToLower() == "desc")
                {
                    MethodCallExpression resultExp = Expression.Call(typeof(Queryable), "OrderByDescending", new Type[] { typeof(T), property.PropertyType }, query.Expression, Expression.Quote(lambda));
                    query = query.Provider.CreateQuery<T>(resultExp);
                }
                else
                {
                    MethodCallExpression resultExp = Expression.Call(typeof(Queryable), "OrderBy", new Type[] { typeof(T), property.PropertyType }, query.Expression, Expression.Quote(lambda));
                    query = query.Provider.CreateQuery<T>(resultExp);
                }
            }

            return query;
        }
    }
}