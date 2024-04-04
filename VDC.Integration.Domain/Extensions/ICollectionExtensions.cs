using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace VDC.Integration.Domain.Extensions
{
    public static class ICollectionExtensions
    {
        public static void Merge<TEntity, UEntity, TProperty>(this ICollection<TEntity> collection, ICollection<UEntity> collectionToMerge,
          Expression<Func<TEntity, TProperty>> firstTypePropertyExpression, Expression<Func<UEntity, TProperty>> secondTypePropertyExpression,
          Func<UEntity, TEntity> createNewFunction, Action<TEntity, UEntity> updateAction)
        {
            if (collection == null || collectionToMerge == null)
                return;

            var firstTypePropertyFunction = firstTypePropertyExpression.Compile();

            var secondTypePropertyFunction = secondTypePropertyExpression.Compile();

            var removedItens = collection.Where(x => collectionToMerge.All(y => !secondTypePropertyFunction(y).Equals(firstTypePropertyFunction(x)))).ToList();

            removedItens.ForEach(x => collection.Remove(x));

            foreach (var item in collectionToMerge)
            {
                var itemValue = secondTypePropertyFunction(item);

                var existentItem = collection.FirstOrDefault(x => firstTypePropertyFunction(x).Equals(itemValue));

                if (existentItem == null)
                {
                    var newItem = createNewFunction(item);

                    collection.Add(newItem);
                }
                else
                {
                    updateAction(existentItem, item);
                }
            }
        }
    }
}
