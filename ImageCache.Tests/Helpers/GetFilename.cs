using System.Text;
using System.Security.Cryptography;

namespace ImageCache.Tests.Helpers;
public class GetFilename
{
	private static readonly HashAlgorithm s_md5 = MD5.Create();

	/// <summary>
	/// Using the first sixteen bytes of the  <see cref="Uri"/>, computes the deterministic filename as a <see cref="Guid"/> <see cref="string"/>.
	/// </summary>
	/// <param name="uri">The <see cref="Uri"/> whose filename to lookup.</param>
	/// <returns>A <see cref="Guid"/> as a <see cref="string"/>.</returns>
	public static string FromUri(Uri uri)
	{
		ArgumentNullException.ThrowIfNull(uri);
		lock (s_md5)
		{
			byte[] uriHash = s_md5.ComputeHash(Encoding.UTF8.GetBytes(uri.AbsoluteUri));
			Guid filename = new(uriHash);

			return $"{filename}";
		}
	}
}
