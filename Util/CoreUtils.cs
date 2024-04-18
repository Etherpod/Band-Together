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

	public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T> source) =>
		source.Where(value => value != null);

	public static IEnumerable<T> AddAll<T>(this ICollection<T> source, IEnumerable<T> elementsToAdd)
	{
		elementsToAdd.ForEach(source.Add);
		return source;
	}

	public static IEnumerable<T> AddAllTo<T>(this IEnumerable<T> source, ICollection<T> target) =>
		source.ForEach(target.Add);

	public static IEnumerable<U> AddAllTo<T, U>(this IEnumerable<T> source, ICollection<U> target, Func<T, U> transform) =>
		source
			.Select(transform)
			.ForEach(target.Add);

	#endregion

	#region dictionaries

	public static IEnumerable<(K, V)> AddAllTo<K, V>(this IEnumerable<(K key, V value)> source, IDictionary<K, V> target) =>
		source.ForEach(entry => target.Add(entry.key, entry.value));

	public static V GetOrInit<K, V>(this IDictionary<K, V> source, K key, V initial)
	{
		if (source.TryGetValue(key, out var foundValue)) return foundValue;

		source[key] = initial;
		return initial;
	}

	#endregion

	#region pairs

	public static IEnumerable<(T, U)> SelectPair<T, U>(this IEnumerable<T> source, Func<T, U> transform) =>
		source.Select(element => (element, transform(element)));

	public static IEnumerable<(U first, T second)> Flip<T, U>(this IEnumerable<(T first, U second)> source) =>
		source.Select(pair => (pair.second, pair.first));

	public static IEnumerable<(K key, V value)> ToPairs<K, V>(this IEnumerable<KeyValuePair<K, V>> source) =>
		source.Select(kvp => (kvp.Key, kvp.Value));

	public static IDictionary<K, V> ToDict<K, V>(this IEnumerable<(K k, V v)> source) =>
		source
			.GroupBy(entry => entry.k)
			.Select(group => (k: group.Key, v: group.Last().v))
			.ToDictionary(entry => entry.k, entry => entry.v);

	#endregion

	#region strings

	public static string Join(this IEnumerable<string> source, string separator = null) =>
		string.Join(separator, source);

	#endregion
}