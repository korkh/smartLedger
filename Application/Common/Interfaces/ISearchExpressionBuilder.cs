using System.Linq.Expressions;

namespace Application.Common.Interfaces
{
    public interface ISearchExpressionBuilder
    {
        Expression<Func<T, bool>> BuildSearchExpression<T>(string searchText)
            where T : class;
    }
}
