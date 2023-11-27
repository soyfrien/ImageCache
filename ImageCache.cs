using System.Security.Cryptography;
using System.Text;
#if WINDOWS
using Microsoft.Maui.Graphics.Win2D;
using Microsoft.Maui.Storage;
#elif IOS || ANDROID || MACCATALYST
using Microsoft.Maui.Graphics.Platform;
#endif

namespace Ppdac.Maui.Cache;

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
/// <permission cref="Ppdac.Maui.Cache">This class is public.</permission>
public class ImageCache
{
	/// <summary>
	/// This property sets the location of the cache.
	/// The default is a folder named "ppdac" in the <see cref="FileSystem.CacheDirectory"/>.
	/// </summary>
	public static string ImageCachePath { get; set; } = $"{FileSystem.Current.CacheDirectory}\\ppdac";
	private static readonly HashAlgorithm s_sha256 = SHA256.Create();
	private static readonly List<string> s_cachedUris = [];


	/// <summary>
	/// Gets the number of items in the cache.
	/// </summary>
	public static int Count => s_cachedUris.Count;


	/// <summary>
	/// Using the first sixteen bytes of the  <see cref="Uri"/>, computes the deterministic filename as a <see cref="Guid"/> <see cref="string"/>.
	/// </summary>
	/// <param name="uri">The <see cref="Uri"/> whose filename to lookup.</param>
	/// <returns>A <see cref="Guid"/> as a <see cref="string"/>.</returns>
	internal static string GetFilename(Uri uri)
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


	/// <summary>
	/// Gets image from URI and returns it as an <see cref="ImageSource"/>.
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


	/// <summary>
	/// A Func that returns a MemoryStream which can be used to set an ImageSource
	/// on objects like the Microsoft.Maui.Controls.Image or the Microsoft.Maui.Graphics.IImage,
	/// for example in conjunction with ImageSource.FromStream(Func‹stream› stream):
	/// <code>Microsoft.Maui.Controls.Image _img;
	/// ImageCache _imageCache = new ImageCache();
	/// _img.Source = ImageSource.FromStream(_imageCache.AsFuncStream(url))
	/// </code>
	/// </summary>
	/// <example>
	/// Microsoft.Maui.Controls.Image _img;
	/// ImageCache _imageCache = new ImageCache();
	/// 
	/// _img.Source = ImageSource.FromStream(_imageCache.AsFuncStream(url));
	/// </example>
	/// <param name="uri">A <see cref="Uri"/> whose .AbsoluteUri property is the location of the image.</param>
	/// <returns>A <see cref="MemoryStream"/> of the byte array of the image.</returns>
	public async Task<Func<Stream>> GetAsFuncStreamAsync(Uri uri)
	{
		ArgumentNullException.ThrowIfNull(uri);

		if (!s_cachedUris.Contains(uri.AbsoluteUri))
			await KeepAsync(uri);

		//byte[] buffer = await GetFromStorageAsBytesAsync(uri);

		string pathToCachedFile = $"{ImageCachePath}\\{GetFilename(uri)}";
		byte[] buffer = File.ReadAllBytes(pathToCachedFile);//.ConfigureAwait(false);

		return () => new MemoryStream(buffer);
	}


	/// <summary>
	/// Looks for a key equal to the URL and returns a stream of byte[], which can be used to set an ImageSource
	/// on objects like the Microsoft.Maui.Controls.Image or the Microsoft.Maui.Graphics.IImage.
	/// 
	/// If not found, the image is cached and then as a MemoryStream of its buffer.
	/// </summary>
	/// <param name="uri">The URI is both the key and the location of the image on the Internet.</param>
	/// <returns>A MemoryStream of the byte array of the image.</returns>
	public async Task<Stream> GetAsStreamAsync(Uri uri)
	{
		ArgumentNullException.ThrowIfNull(uri);
		if (Directory.Exists(ImageCachePath) is false)
			Directory.CreateDirectory(ImageCachePath);

		string pathToCachedFile = $"{ImageCachePath}\\{GetFilename(uri)}";
		if (File.Exists(pathToCachedFile) is false)
			await KeepAsync(uri);

		byte[] buffer = await File.ReadAllBytesAsync(pathToCachedFile)
			.ConfigureAwait(false);

		return new MemoryStream(buffer);
	}


	/// <summary>
	/// Looks for a key equal to the URL and returns a stream of <see cref="byte"/>[], which can be used to set an ImageSource
	/// </summary>
	/// <param name="uri">A <see cref="Uri"/> whose .AbsoluteUri property is the location of the image.</param>
	/// <returns>A <see cref="byte"/> array of the image from storage.</returns>
	public static async Task<byte[]> GetFromStorageAsBytesAsync(Uri uri)
	{
		ArgumentNullException.ThrowIfNull(uri);
		string pathToCachedFile = $"{ImageCachePath}\\{GetFilename(uri)}";
		if (Directory.Exists(ImageCachePath) is false)
		{
			Directory.CreateDirectory(ImageCachePath);
			await KeepAsync(uri);
		}

		return File.ReadAllBytes(pathToCachedFile);
	}


	/// <summary>
	/// A non-static version of <see cref="GetImageAsBytesAsync(Uri)"/>.
	/// </summary>
	/// <param name="uri">The URI is both the key and the location of the image on the Internet.</param>
	/// <returns>The byte array of the image if found, or null.</returns>
	public static async Task<byte[]> GetAsBytesAsync(Uri uri)
	{
		byte[]? buffer = await GetImageAsBytesAsync(uri).ConfigureAwait(false);
		return buffer!;
	}


	/// <summary>
	/// Looks for a key equal to the URI's .AbsoluteUri property and returns a the image as a byte array.
	/// If not found, null is returned.
	/// </summary>
	/// <param name="uri">The URI is both the key and the location of the image on the Internet.</param>
	/// <returns>The byte array of the image if found, or null.</returns>
	public static async Task<byte[]?> GetImageAsBytesAsync(Uri uri)
	{
		ArgumentNullException.ThrowIfNull(uri);
		string pathToCachedFile = $"{ImageCachePath}\\{GetFilename(uri)}";
		if (Directory.Exists(ImageCachePath) is false)
			Directory.CreateDirectory(ImageCachePath);
		if (File.Exists(pathToCachedFile))
			return File.ReadAllBytes(pathToCachedFile);//.ConfigureAwait(false);

		using HttpClient httpClient = new();
		HttpResponseMessage responseMessage = await httpClient.GetAsync(uri).ConfigureAwait(false);
		if (responseMessage.IsSuccessStatusCode is false)
			return null; //throw new Exception($"Failed to get image from {url.AbsoluteUri}.");

		using Stream stream = await responseMessage.Content.ReadAsStreamAsync().ConfigureAwait(false);
		byte[] bytes = new byte[stream.Length];
		_ = await stream.ReadAsync(bytes.AsMemory(0, (int)stream.Length))
			.ConfigureAwait(false);

		return bytes;
	}


	/// <summary>
	/// See <see cref="GetImageAsBytesAsync(Uri)"/>.
	/// </summary>
	/// <param name="url">The string URL</param>
	/// <returns>A byte array.</returns>
	public static async Task<byte[]?> GetImageAsBytesAsync(string url) => await GetImageAsBytesAsync(new Uri(url));


	/// <summary>
	/// Looks for a key equal to the string and returns a the size image in buffer.
	/// If not found it is added then the size is returned. If null, -1 is returned.
	/// </summary>
	/// <param name="uri">The URL is both the key and the location of the image on the Internet.</param>
	/// <returns>The size of the image if found in buffer, as long. Or null.</returns>
	public static async Task<int> GetByteCountAsync(Uri uri)
	{
		if (uri is null)
			return -1;
		if (!s_cachedUris.Contains(uri.AbsoluteUri))
		{
			s_cachedUris.Add(uri.AbsoluteUri);
			await KeepAsync(uri).ConfigureAwait(false);
		}

		//return (await GetImageAsBytesAsync(uri).ConfigureAwait(false))?.Length ?? -1;
		string pathToCachedFile = $"{ImageCachePath}\\{GetFilename(uri)}";
		if (Directory.Exists(ImageCachePath) is false)
			Directory.CreateDirectory(ImageCachePath);
		if (File.Exists(pathToCachedFile))
			return File.ReadAllBytes(pathToCachedFile).Length;


		using HttpClient httpClient = new();
		HttpResponseMessage responseMessage = await httpClient.GetAsync(uri).ConfigureAwait(false);
		if (responseMessage.IsSuccessStatusCode is false)
			return 0; //throw new Exception($"Failed to get image from {url.AbsoluteUri}.");

		using Stream stream = await responseMessage.Content.ReadAsStreamAsync().ConfigureAwait(false);

		return await stream.ReadAsync(new byte[stream.Length])
			.ConfigureAwait(false);

	}


	/// <summary>
	/// Looks for a key equal to the string and returns a the size image in buffer.
	/// If not found it is added then the size is returned. If null, -1 is returned.
	/// </summary>
	/// <param name="url">The URL is both the key and the location of the image on the Internet.</param>
	/// <returns>The size of the image if found in buffer, as long. Or null.</returns>
	public async Task<int> GetByteCountAsync(string url) => await GetByteCountAsync(new Uri(url));


	/// <summary>
	/// Writes the buffer to the filesystem, a Guid from the first 16 buffer has a hash of the Uri to make a filename.
	/// </summary>
	/// <param name="uri">The URI of the image about to be kept in storage.</param>
	public static async Task KeepAsync(Uri uri)
	{
		ArgumentNullException.ThrowIfNull(uri);

		if (Directory.Exists(ImageCachePath) is false)
			Directory.CreateDirectory(ImageCachePath);

		string pathToCachedFile = $"{ImageCachePath}\\{GetFilename(uri)}";
		if (File.Exists(pathToCachedFile))
			return;

		byte[]? bytes = await GetImageAsBytesAsync(uri).ConfigureAwait(false);
		await File.WriteAllBytesAsync($"{pathToCachedFile}", bytes!).ConfigureAwait(false);

		if (s_cachedUris.Contains(uri.AbsoluteUri) is false)
			s_cachedUris.Add(uri.AbsoluteUri);
	}


	/// <summary>
	/// Removes the cache from the filesystem, but does not clear the tracked URIs in memory.
	/// </summary>
	/// <code>StatusLabel.Text = _imageStore.Purge().Result;</code>
	/// <remarks>You can, for example, Purge the images from disk and use <see cref="Save"/> to write them back to the cache.</remarks>
	/// <returns>A string from a TResult.</returns>
	public static Task<string> Purge()
	{

		if (Directory.Exists(ImageCachePath) is false)
			return Task.FromResult($"{ImageCachePath} wasn't there.");


		if (Directory.GetFiles(ImageCachePath).ToList().Count > 0)
			foreach (string file in Directory.GetFiles(ImageCachePath).ToList())
				File.Delete(file);

		Directory.Delete($"{ImageCachePath}");

		return Task.FromResult($"Image cache purged from {ImageCachePath}.");
	}


	/// <summary>
	/// Restores the cache from the filesystem.
	/// </summary>
	/// <code>StatusLabel.Text = _imageStore.Restore().Result;</code>
	/// <remarks>For example, when used after using Dependency Injection to access the ImageCache</remarks>
	/// <returns>A string as TResult.</returns>
	public static Task<string> Restore()
	{
		//string cachePath = $"{FileSystem.Current.CacheDirectory}\\{_ContiguousCacheFile}";

		//if (File.Exists(cachePath) is false)
		//	

		//using FileStream fs = new(cachePath, FileMode.Open);
		//using BinaryReader br = new(fs);

		//int count = br.ReadInt32();
		//for (int i = 0; i < count; i++)
		//{
		//	string key = br.ReadString();
		//	int length = br.ReadInt32();
		//	byte[] value = br.ReadBytes(length);
		//	s_imageStore.TryAdd(key, value);
		//}
		//return Task.FromResult($"Image cache restored from {cachePath}.");

		if (!Directory.Exists(ImageCachePath))
			Directory.CreateDirectory(ImageCachePath);

		int count = Directory.GetFiles(ImageCachePath).ToList().Count;

		return Task.FromResult($"{count} items in cache.");
	}


	/// <summary>
	/// Saves the cache to the filesystem.
	/// </summary>
	/// <code>StatusLabel.Text = _imageStore.Save().Result;</code>
	/// <remarks>Files are now saved automatically. You should still use <see cref="Restore"/>."</remarks>
	/// <returns>A result string from a TResult. </returns>
	public static async Task<string> Save()
	{
		foreach (string url in s_cachedUris)
			await KeepAsync(new Uri(url)).ConfigureAwait(false);

		return "Tracked URIs saved to cache.";
	}


	/// <summary>
	/// Clears the Dictionary, but does not delete the files from the filesystem.
	/// </summary>
	public Task Clear()
	{
		s_cachedUris.Clear();

		return Task.CompletedTask;
	}
}