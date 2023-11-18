namespace Ppdac.Cache;


/// <summary>
/// Use this class to cache images from the Internet. Where ever you would give a control a URL, a stream, or byte[],
/// do that as normal, but have ImageCache sit in the middle. For example, in .NET MAUI:
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
///		...
///		_imageCache = imageCache;
///		_imageCache.Restore();
///		...
/// </code>
/// </summary>
/// <permission cref="ImageCache">This class is public.</permission>
public interface ICache
{
	/// <summary>
	/// Looks for a key equal to the URL and returns a stream of byte[], which can be used to set an ImageSource
	/// on objects like the Microsoft.Maui.Controls.Image or the Microsoft.Maui.Graphics.IImage.
	/// 
	/// If not found, the image is cached and then as a MemoryStream of its bytes.
	/// </summary>
	/// <param name="uri">The URI is both the key and the location of the image on the Internet.</param>
	/// <returns>A MemoryStream of the byte array of the image.</returns>
	public abstract static Stream GetImageAsStream(Uri uri);

	/// <summary>
	/// A Func that returns a stream of byte[], which can be used to set an ImageSource
	/// on objects like the Microsoft.Maui.Controls.Image or the Microsoft.Maui.Graphics.IImage,
	/// for example in conjunction with ImageSource.FromStream(Func‹stream› stream):
	/// <code>Microsoft.Maui.Controls.Image _img;
	/// ImageCache _imageCache = new ImageCache();
	/// _img.Source = ImageSource.FromStream(_imageCache.AsFuncStream(uri))
	/// </code>
	/// </summary>
	/// <example>
	/// Microsoft.Maui.Controls.Image _img;
	/// ImageCache _imageCache = new ImageCache();
	/// 
	/// _img.Source = ImageSource.FromStream(_imageCache.AsFuncStream(uri));
	/// </example>
	/// <param name="uri">A URI whose .AbsoluteUri property is the location of the image.</param>
	/// <returns>A MemoryStream of the byte array of the image.</returns>
	public abstract static Func<Stream> GetImageAsFuncStream(Uri uri);

	/// <summary>
	/// Looks for a key equal to the string and returns a stream of byte[], which can be used to set an ImageSource
	/// on objects like the Microsoft.Maui.Controls.Image or the Microsoft.Maui.Graphics.IImage.
	/// 
	/// If not found, null is returned.
	/// </summary>
	/// <param name="uri">The URL is both the key and the location of the image on the Internet.</param>
	/// <returns>A MemoryStream of the byte array of the image or null.</returns>
	public abstract static byte[] GetImageAsBytes(Uri uri);
	/// <summary>
	/// Looks for a key equal to the string and returns a stream of byte[], which can be used to set an ImageSource
	/// on objects like the Microsoft.Maui.Controls.Image or the Microsoft.Maui.Graphics.IImage.
	/// 
	/// If not found, null is returned.
	/// </summary>
	/// <param name="key">The URL is both the key and the location of the image on the Internet.</param>
	/// <returns>A MemoryStream of the byte array of the image or null.</returns>
	public abstract static byte[]? GetImageAsBytes(string key);

	// <summary>
	/// Saves the cache to the filesystem.
	/// </summary>
	/// <code>StatusLabel.Text = _imageStore.Save().Result;</code>
	/// <returns>A result string from a TResult. </returns>
	public abstract static Task<string> Save();

	/// <summary>
	/// Restores the cache from the filesystem, 
	/// after using Dependency Injection to access the ImageCache, for example.
	/// </summary>
	/// <code>StatusLabel.Text = _imageStore.Restore().Result;</code>
	/// <returns>A string as TResult.</returns>
	public abstract static Task<string> Restore();

	/// <summary>
	/// Removes the cache from the filesystem.
	/// </summary>
	/// <code>StatusLabel.Text = _imageStore.Purge().Result;</code>
	/// <returns>A string from a TResult.</returns>
	public abstract static Task<string> Purge();



	#region maybe not
	//public abstract static void Add(string url);
	//public abstract static Stream AsStream(string url);
	//public abstract static Func<Stream> AsFuncStream(string url);
	//public abstract static int GetNumberOfBytes(string url);
	#endregion

}