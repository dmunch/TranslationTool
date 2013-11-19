using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace TranslationTool.Helpers
{
	public class LambdaComparer<T, TKey> : IEqualityComparer<T>
	{
		private readonly Expression<Func<T, TKey>> _KeyExpr;
		private readonly Func<T, TKey> _CompiledFunc;
		
		public LambdaComparer(Expression<Func<T, TKey>> getKey)
		{
			_KeyExpr = getKey;
			_CompiledFunc = _KeyExpr.Compile();
		}

		public int Compare(T obj1, T obj2)
		{
			return Comparer<TKey>.Default.Compare(_CompiledFunc(obj1), _CompiledFunc(obj2));
		}

		public bool Equals(T obj1, T obj2)
		{
			return EqualityComparer<TKey>.Default.Equals(_CompiledFunc(obj1), _CompiledFunc(obj2));
		}

		public int GetHashCode(T obj)
		{
			return EqualityComparer<TKey>.Default.GetHashCode(_CompiledFunc(obj));
		}
	}
}
