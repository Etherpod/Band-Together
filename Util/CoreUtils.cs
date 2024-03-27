using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace BandTogether.Util;

public static class CoreUtils
{
	#region lists

	public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T> action)
	{
		var listSource = source.ToList();
		foreach (var element in listSource)
		{
			action(element);
		}

		return listSource;
	}

	public static IEnumerable<T> AddAll<T>(this IEnumerable<T> source, ICollection<T> target) =>
		source.ForEach(target.Add);

	public static IEnumerable<U> AddAll<T, U>(this IEnumerable<T> source, ICollection<U> target, Func<T, U> transform) =>
		source
			.Select(transform)
			.ForEach(target.Add);

	#endregion

	#region dictionaries

	public static IEnumerable<(K, V)> AddAll<K, V>(this IEnumerable<(K key, V value)> source, IDictionary<K, V> target) =>
		source.ForEach(entry => target.Add(entry.key, entry.value));

	#endregion

	#region pairs

	public static IEnumerable<(T, U)> SelectPair<T, U>(this IEnumerable<T> source, Func<T, U> transform) =>
		source.Select(element => (element, transform(element)));

	public static IEnumerable<(U, T)> Flip<T, U>(this IEnumerable<(T first, U second)> source) =>
		source.Select(pair => (pair.second, pair.first));

	#endregion
}