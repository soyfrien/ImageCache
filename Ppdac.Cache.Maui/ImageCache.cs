namespace Ppdac.Cache.Maui;

/// <summary>
/// Use this class to cache images from the Internet.
/// Its functions receive a URI and turn the resource into an ImageSource, byte array, Stream, or Func‹Stream›.
/// Where ever you would give a control a URL, a stream, or byte[], do so as normal, but have ImageCache sit in the middle. For example, in .NET MAUI:
/// <code>
/// // MauiProgram.cs:
/// ...
/// builder.Services.AddSingleton‹ImageCache›();
/// ...
/// </code>
/// Then use Dependency Injection to gain access to the class of a view, viewmodel, page or control:
/// <code>
/// Using Ppdac.ImageCache.Maui;
/// ImageCache _imageCache;
/// ...
/// Page(ViewModel viewModel, ImageCache imageCache)
/// {
///		_imageCache = imageCache;
///		foreach (Image image in viewModel.Images)
///			Image.Source = _imageCache.GetImageAsImageSource(image.Url);
///		
///		Stream imageStream = await _imageCache.GetImageAsStreamAsync(image.Url);
///		Bitmap bitmap = new(imageStream);
///		
///		byte[] imageBytes = await _imageCache.GetImageAsBytesAsync(image.Url);
///		...
/// </code>
/// See the demo project on the project's GitHub page for more examples.
/// </summary>
/// <permission cref="Ppdac.Cache">This class is public.</permission>
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
		byte[] buffer = await File.ReadAllBytesAsync(pathToCachedFile).ConfigureAwait(false);
		
		return ImageSource.FromStream(() =>
		{
			return new MemoryStream(buffer);
		});
	}
}
