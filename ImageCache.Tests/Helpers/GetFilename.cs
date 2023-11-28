using System.Text;
using System.Security.Cryptography;

namespace ImageCache.Tests.Helpers;
public class GetFilename
{
	private static readonly HashAlgorithm s_sha256 = SHA256.Create();

	/// <summary>
	/// Using the first sixteen bytes of the  <see cref="Uri"/>, computes the deterministic filename as a <see cref="Guid"/> <see cref="string"/>.
	/// </summary>
	/// <param name="uri">The <see cref="Uri"/> whose filename to lookup.</param>
	/// <returns>A <see cref="Guid"/> as a <see cref="string"/>.</returns>
	public static string FromUri(Uri uri)
	{
		ArgumentNullException.ThrowIfNull(uri);
		lock (s_sha256)
		{
			byte[] uriHash = s_sha256.ComputeHash(Encoding.UTF8.GetBytes(uri.AbsoluteUri));
			ReadOnlySpan<byte> filenameSeed = new(uriHash[..16]);
			Guid filename = new(filenameSeed);

			return $"{filename}";
		}
	}
}
