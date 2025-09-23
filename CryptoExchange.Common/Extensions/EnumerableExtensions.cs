namespace CryptoExchange.Common.Extensions;

public static class EnumerableExtensions
{
	/// <summary>
	/// Returns a sequence that contains only the non-null elements from the source sequence.
	/// </summary>
	/// <typeparam name="T">The reference type of the elements in the source sequence.</typeparam>
	/// <param name="source">The sequence to filter.</param>
	/// <returns>
	/// An <see cref="IEnumerable{T}"/> that contains only the non-null elements from the <paramref name="source"/> sequence.
	/// </returns>
	public static IEnumerable<T> WhereNotNull<T>( this IEnumerable<T?> source ) where T : class
	{
		return source.Where( item => item != null ).Select( item => item! );
	}
}
