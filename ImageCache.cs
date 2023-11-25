using System.Security.Cryptography;
using System.Text;
#if WINDOWS
using Microsoft.Maui.Graphics.Win2D;
using Microsoft.Maui.Storage;
#elif IOS || ANDROID || MACCATALYST
using Microsoft.Maui.Graphics.Platform;
#endif

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
/// <permission cref="Ppdac.Cache">This class is public.</permission>
public class ImageCache
{
	private static string _imageCachePath = $"{FileSystem.Current.CacheDirectory}\\ppdac";
	private static string _cacheFile = "ppdac.imagecache.dat";
	public static string ImageCachePath
	{
		get => _imageCachePath;
		set => _imageCachePath = value;
	}
	public static string CacheFile
	{
		get => _cacheFile;
		set => _cacheFile = value;
	}
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
	public static string GetFilename(Uri uri)
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


	public async Task<Stream> GetImageAsStreamAsync(Uri uri)
	{
		ArgumentNullException.ThrowIfNull(uri);

		if (Directory.Exists(ImageCachePath) is false)
			Directory.CreateDirectory(ImageCachePath);

		string pathToCachedFile = $"{ImageCachePath}\\{GetFilename(uri)}";
		if (File.Exists(pathToCachedFile) is false)
			await KeepAsync(uri).ConfigureAwait(true);

		//byte[] buffer = await File.ReadAllBytesAsync(pathToCachedFile).ConfigureAwait(false);

		byte[] buffer = await GetFromStorageAsBytesAsync(uri);
		return new MemoryStream(buffer);
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
	public async Task<Func<Stream>> AsFuncStreamAsync(Uri uri)
	{
		ArgumentNullException.ThrowIfNull(uri);

		if (!s_cachedUris.Contains(uri.AbsoluteUri))
		{
			s_cachedUris.Add(uri.AbsoluteUri);
			await KeepAsync(uri);
		}
		byte[] buffer = await GetFromStorageAsBytesAsync(uri);

		//string pathToCachedFile = $"{ImageCachePath}\\{GetFilename(uri)}";
		//byte[] buffer = File.ReadAllBytes(pathToCachedFile);//.ConfigureAwait(false);

		return () => new MemoryStream(buffer);
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
	/// Looks for a key equal to the URL and returns a stream of byte[], which can be used to set an ImageSource
	/// on objects like the Microsoft.Maui.Controls.Image or the Microsoft.Maui.Graphics.IImage.
	/// 
	/// If not found, the image is cached and then as a MemoryStream of its buffer.
	/// </summary>
	/// <param name="uri">The URI is both the key and the location of the image on the Internet.</param>
	/// <returns>A MemoryStream of the byte array of the image.</returns>
	public async Task<Stream> AsStreamAsync(Uri uri)
	{

		if (!s_cachedUris.Contains(uri.AbsoluteUri))
		{
			if (Directory.Exists(ImageCachePath) is false)
				Directory.CreateDirectory(ImageCachePath);

			await KeepAsync(uri);
			byte[]? buffer = await GetImageAsBytesAsync(uri);
			return new MemoryStream(buffer!);
		}
		else
		{
			//string pathToCachedFile = $"{ImageCachePath}\\{GetFilename(uri)}";
			//byte[] buffer = File.ReadAllBytes(pathToCachedFile);//.ConfigureAwait(false);
			byte[] buffer = await GetFromStorageAsBytesAsync(uri);

			return new MemoryStream(buffer);

			//return new MemoryStream(await GetFromStorageAsBytesAsync(uri));
		}
	}


	//public async static Task<byte[]> GetAsBytesAsync(Uri uri)
	//{
	//	ArgumentNullException.ThrowIfNull(uri);

	//	if (Directory.Exists(ImageCachePath) is false)
	//		Directory.CreateDirectory(ImageCachePath);

	//	string pathToCachedFile = $"{ImageCachePath}\\{GetFilename(uri)}";
	//	byte[] bytes;
	//	if (File.Exists(pathToCachedFile))
	//		bytes = await File.ReadAllBytesAsync(pathToCachedFile).ConfigureAwait(false);
	//	else
	//	{
	//		using HttpClient httpClient = new();
	//		HttpResponseMessage responseMessage = await httpClient.GetAsync(uri).ConfigureAwait(false);

	//		if (responseMessage.IsSuccessStatusCode is false)
	//			throw new HttpRequestException($"Failed to get image from {uri.AbsoluteUri}.");

	//		Stream stream = await responseMessage.Content.ReadAsStreamAsync().ConfigureAwait(false);


	//		// load stream into byte array
	//		bytes = new byte[stream.Length];
	//		//await File.WriteAllBytesAsync($"{filePath}", buffer);
	//		// File.WriteAllBytes($"{filePath}", buffer);
	//		//await stream.ReadAsync(buffer.AsMemory(0, (int)stream.Length));
	//		stream.Dispose();
	//	}

	//	return bytes;
	//}


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
		{
			byte[] buffer = await GetFromStorageAsBytesAsync(uri);
			return buffer;
			//return File.ReadAllBytes(pathToCachedFile);//.ConfigureAwait(false);
		}
			


		using HttpClient httpClient = new();
		HttpResponseMessage responseMessage = await httpClient.GetAsync(uri).ConfigureAwait(false);
		if (responseMessage.IsSuccessStatusCode is false)
			return null; //throw new Exception($"Failed to get image from {url.AbsoluteUri}.");

		using Stream stream = await responseMessage.Content.ReadAsStreamAsync().ConfigureAwait(false);
		byte[] bytes = new byte[stream.Length];
		await stream.ReadAsync(bytes.AsMemory(0, (int)stream.Length)).ConfigureAwait(false);

		return bytes;
	}


	/// <summary>
	/// Looks for a key equal to the string and returns a the size image in buffer.
	/// If not found it is added then the size is returned. If null, -1 is returned.
	/// </summary>
	/// <param name="uri">The URL is both the key and the location of the image on the Internet.</param>
	/// <returns>The size of the image if found in buffer, as long. Or null.</returns>
	public async Task<int> GetNumberOfBytesAsync(Uri uri)
	{
		//ArgumentNullException.ThrowIfNull(uri);
		if (uri is null)
			return -1;

		if (!s_cachedUris.Contains(uri.AbsoluteUri))
		{
			s_cachedUris.Add(uri.AbsoluteUri);
			await KeepAsync(uri).ConfigureAwait(false);
		}
		int length = (await GetImageAsBytesAsync(uri).ConfigureAwait(false))?.Length ?? -1;

		return length;
	}


	/// <summary>
	/// Looks for a key equal to the string and returns a the size image in buffer.
	/// If not found it is added then the size is returned. If null, -1 is returned.
	/// </summary>
	/// <param name="url">The URL is both the key and the location of the image on the Internet.</param>
	/// <returns>The size of the image if found in buffer, as long. Or null.</returns>
	//public async Task<int> GetNumberOfBytesAsync(string url)
	//{
	//	ArgumentNullException.ThrowIfNull(url);

	//	return await GetNumberOfBytesAsync(new Uri(url));

	//	//string pathToCachedFile = $"{ImageCachePath}\\{GetFilename(new (url))}";
	//	//if (!s_cachedUris.Contains(pathToCachedFile))
	//	//{
	//	//	s_cachedUris.Add(pathToCachedFile);
	//	//	await KeepAsync(new (url));
	//	//}
	//	//int length = (await GetImageAsBytesAsync(url))?.Length ?? -1;

	//	//return length;
	//}



	/// <summary>
	/// Saves the cache to the filesystem.
	/// </summary>
	/// <code>StatusLabel.Text = _imageStore.Save().Result;</code>
	/// <remarks>Files are now saved automatically. You should still use <see cref="Restore"/>."</remarks>
	/// <returns>A result string from a TResult. </returns>
	public async Task<string> Save()
	{
		foreach (string url in s_cachedUris)
			await KeepAsync(new Uri(url)).ConfigureAwait(false);

		return "Tracked URIs saved to cache.";
	}


	/// <summary>
	/// Restores the cache from the filesystem.
	/// </summary>
	/// <code>StatusLabel.Text = _imageStore.Restore().Result;</code>
	/// <remarks>For example, when used after using Dependency Injection to access the ImageCache</remarks>
	/// <returns>A string as TResult.</returns>
	public Task<string> Restore()
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
	/// Removes the cache from the filesystem, but does not clear the tracked URIs in memory.
	/// </summary>
	/// <code>StatusLabel.Text = _imageStore.Purge().Result;</code>
	/// <remarks>You can, for example, Purge the images from disk and use <see cref="Save"/> to write them back to the cache.</remarks>
	/// <returns>A string from a TResult.</returns>
	public Task<string> Purge()
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
	/// Clears the Dictionary, but does not delete the files from the filesystem.
	/// </summary>
	public Task Clear()
	{
		s_cachedUris.Clear();

		return Task.CompletedTask;
	}
}