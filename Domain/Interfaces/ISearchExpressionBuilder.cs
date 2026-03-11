using System.Linq.Expressions;

namespace Domain.Interfaces
{
    public interface ISearchExpressionBuilder
    {
        Expression<Func<T, bool>> BuildSearchExpression<T>(string searchText)
            where T : class;
    }
}
