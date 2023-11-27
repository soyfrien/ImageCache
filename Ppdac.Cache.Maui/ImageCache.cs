using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ppdac.Cache.Maui;

public class ImageCache : Cache.ImageCache
{
	/// <summary>
	/// Gets image from <see cref="Uri"/> and returns it as an <see cref="ImageSource"/>.
	/// </summary>
	/// <param name="uri"><see cref="Uri"/> of image to cache.</param>
	/// <returns>The image is returned as an <see cref="ImageSource"/>.</returns>
	public async Task<ImageSource> GetAsImageSourceAsync(Uri uri)
	{
		if (string.IsNullOrEmpty(uri.AbsoluteUri))
			return null!;
		if (Directory.Exists(ImageCachePath) is false)
			Directory.CreateDirectory(ImageCachePath);
		string pathToCachedFile = $"{ImageCachePath}\\{GetFilename(uri)}";
		if (File.Exists(pathToCachedFile) is false || s_cachedUris.Contains(uri.AbsoluteUri) is false)
			await KeepAsync(uri).ConfigureAwait(false);
		byte[] buffer = File.ReadAllBytes(pathToCachedFile);
		
		return ImageSource.FromStream(() =>
		{
			return new MemoryStream(buffer);
		});
	}
}
