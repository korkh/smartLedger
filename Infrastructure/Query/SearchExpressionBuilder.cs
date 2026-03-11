using System.Linq.Expressions;
using Domain.Interfaces;

namespace Infrastructure.Services
{
    public class SearchExpressionBuilder : ISearchExpressionBuilder
    {
        public Expression<Func<T, bool>> BuildSearchExpression<T>(string searchText)
            where T : class
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return x => true; // Возвращаем всё, если поиск пустой

            var parameter = Expression.Parameter(typeof(T), "e");
            var propertyChecks = new List<Expression>();

            // Очищаем и разделяем поисковые слова
            var searchTerms = searchText
                .ToLower()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);

            foreach (var property in typeof(T).GetProperties())
            {
                // Ищем только в строковых свойствах
                if (property.PropertyType == typeof(string))
                {
                    var propertyAccess = Expression.Property(parameter, property);

                    // 1. Проверка на null: e.Property != null
                    var notNullCheck = Expression.NotEqual(
                        propertyAccess,
                        Expression.Constant(null)
                    );

                    foreach (var term in searchTerms)
                    {
                        // 2. e.Property.ToLower()
                        var toLower = Expression.Call(propertyAccess, "ToLower", Type.EmptyTypes);

                        // 3. .Contains("term")
                        var containsMethod = Expression.Call(
                            toLower,
                            "Contains",
                            Type.EmptyTypes,
                            Expression.Constant(term)
                        );

                        // Соединяем: e.Property != null && e.Property.ToLower().Contains("term")
                        var fullCheck = Expression.AndAlso(notNullCheck, containsMethod);
                        propertyChecks.Add(fullCheck);
                    }
                }
            }

            if (!propertyChecks.Any())
                return x => true;

            // Объединяем все проверки через OR
            var orExpression = propertyChecks.Aggregate<Expression>(
                (accumulate, next) => Expression.OrElse(accumulate, next)
            );

            return Expression.Lambda<Func<T, bool>>(orExpression, parameter);
        }
    }
}
