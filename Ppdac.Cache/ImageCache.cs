#if NET7_0_OR_GREATER
using Microsoft.Maui.Storage;
#endif

using System.Security.Cryptography;
using System.Text;

namespace Ppdac.Cache;

/// <summary>
/// Use this class to cache images from the Internet. Where ever you would give a control a URL, a stream, or byte[],
/// as normal, but have ImageCache sit in the middle. For example, in .NET MAUI:
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
/// <permission cref="Ppdac.ImageCache">This class is public.</permission>
public class ImageCache
{
	// Release value for ImageCachePath when Microsoft.Maui.Storage is available: Path.Combine(FileSystem.Current.CacheDirectory, "ppdac");
	// BenchmarkDotNet value: "C:\\Users\\Louis\\source\\repos\\ImageCache.Benchmarks\\bin\\Debug\\Images";
	/// <summary>
	/// The path of the cache folder.
	/// </summary>
	/// <remarks>
	/// Defaults to the App's CacheDirectory on Windows, Android, iOS, and Mac Catalyst, or when running on .NET 7.0 or newer,
	/// or to the user's AppData folder on Windows when running on .NET 5.0 or older.
	/// </remarks>
#if NET7_0_OR_GREATER
	public string ImageCachePath { get; set; } = Path.Combine(FileSystem.Current.CacheDirectory, "ppdac");
#elif NET6_0
	public string ImageCachePath { get; set; } = 
		Path.Combine(Environment.GetFolderPath(
			folder: Environment.SpecialFolder.ApplicationData),
			"ppdac.cache.imagecache");
#endif
	private static readonly HashAlgorithm s_sha256 = SHA256.Create();
	/// <summary>
	/// Provides a quicker and more energy efficient lookup of the known (and thus cached) <see cref="Uri"/>s when compared to scanning the filesystem.
	/// </summary>
	/// <remarks>
	/// This field isn't intended to be directly manipulated by a consumer, and was originally created to ease development.
	/// It's of type <see cref="string"/> instead of <see cref="Uri"/> based on an unproven assumption that this will use slightly less memory.
	/// For reasons like these it may be removed in a future version.</remarks>
	protected static readonly List<string> s_cachedUris = new(); //[];


	/// <summary>
	/// Gets the number of items in the cache.
	/// </summary>
	public static int Count => s_cachedUris.Count;


	/// <summary>
	/// Retrieves the resource at the specified <see cref="Uri"/> as a <see cref="byte"/>[] array.
	/// </summary>
	/// <param name="uri">The URI is both the key and the location of the image on the Internet.</param>
	/// <returns><see cref="byte"/>[] of the image if found, or an empty byte array.</returns>
	public async Task<byte[]> GetAsBytesAsync(Uri uri)
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
			return Array.Empty<byte>(); //throw new Exception($"Failed to get image from {url.AbsoluteUri}.");

		using Stream stream = await responseMessage.Content.ReadAsStreamAsync().ConfigureAwait(false);
		byte[] buffer = new byte[stream.Length];
		_ = await stream.ReadAsync(buffer.AsMemory(0, (int)stream.Length))
			.ConfigureAwait(false);

		return buffer;
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
	/// See <see cref="GetImageAsBytesAsync(Uri)"/>.
	/// </summary>
	/// <param name="url">The string URL</param>
	/// <returns>A byte array.</returns>
	internal async Task<byte[]?> GetAsBytesAsync(string url) => await GetAsBytesAsync(new Uri(url));


	/// <summary>
	/// Looks for a key equal to the URL and returns a stream of <see cref="byte"/>[], which can be used to set an ImageSource
	/// </summary>
	/// <param name="uri">A <see cref="Uri"/> whose .AbsoluteUri property is the location of the image.</param>
	/// <returns>A <see cref="byte"/> array of the image from storage.</returns>
	internal async Task<byte[]> GetFromStorageAsBytesAsync(Uri uri)
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
	/// Looks for a key equal to the string and returns a the size image in buffer.
	/// If not found it is added then the size is returned. If null, -1 is returned.
	/// </summary>
	/// <remarks>This is currently limited to <see cref="Int32.MaxValue"/> as it uses <see cref="Stream.ReadAsync"/> which returns a <see cref="ValueTask"/>‹int›.</remarks>
	/// <param name="uri">The URL is both the key and the location of the image on the Internet.</param>
	/// <returns>The size of the image if found in buffer, as long. Or -1, if null.</returns>
	public async Task<int> GetByteCountAsync(Uri uri)
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
			return -404; //throw new Exception($"Failed to get image from {url.AbsoluteUri}.");

		using Stream stream = await responseMessage.Content.ReadAsStreamAsync().ConfigureAwait(false);

		return await stream.ReadAsync(new byte[stream.Length])
			.ConfigureAwait(false);
	}


	/// <summary>
	/// See <see cref="GetByteCountAsync(Uri)"/>.
	/// </summary>
	/// <remarks>Using a <see cref="Uri"/> instead of a <see cref="string"/> URL is recommended.</remarks>
	/// <param name="url">The URL is both the key and the location of the image on the Internet.</param>
	/// <returns>The size of the image if found in buffer. Or -1, if null.</returns>
	public async Task<int> GetByteCountAsync(string url) => await GetByteCountAsync(new Uri(url));


	/// <summary>
	/// Using the first sixteen buffer of the  <see cref="Uri"/>, computes the deterministic filename as a <see cref="Guid"/> <see cref="string"/>.
	/// </summary>
	/// <param name="uri">The <see cref="Uri"/> whose filename to lookup.</param>
	/// <returns>A <see cref="Guid"/> as a <see cref="string"/>.</returns>
	protected internal static string GetFilename(Uri uri)
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
	/// Clears the tracked Uris from memory, but does not delete the files from the filesystem, and will still hit the cache before the Internet.
	/// </summary>
	public static Task Clear()
	{
		s_cachedUris.Clear();

		return Task.CompletedTask;
	}


	/// <summary>
	/// Saves the resource to the <see cref="FileSystem.CacheDirectory"/> and becomes aware of its <see cref="Uri"/>.
	/// </summary>
	/// <remarks>You can use this to pre-cache items.</remarks>
	/// <param name="uri">The URI of the image about to be kept in storage.</param>
	public async Task KeepAsync(Uri uri)
	{
		ArgumentNullException.ThrowIfNull(uri);

		if (Directory.Exists(ImageCachePath) is false)
			Directory.CreateDirectory(ImageCachePath);

		string pathToCachedFile = $"{ImageCachePath}\\{GetFilename(uri)}";
		if (File.Exists(pathToCachedFile))
			return;

		byte[]? bytes = await GetAsBytesAsync(uri).ConfigureAwait(false);
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
	/// Gives an item count and size of the cache, in mebibytes.
	/// </summary>
	/// <returns>A Task‹string› giving the item count and amount of storage used.</returns>
	public Task<string> Report()
	{
		if (!Directory.Exists(ImageCachePath))
			Directory.CreateDirectory(ImageCachePath);

		int count = Directory.GetFiles(ImageCachePath).ToList().Count;

		long size = 0;
		foreach (string file in Directory.GetFiles(ImageCachePath).ToList())
			size += new FileInfo(file).Length;

		if ((size / 1024 / 1024 / 1024 >= 1))
			return Task.FromResult($"{count} items in cache ({size / 1024 / 1024 / 1024} GiB).");
		else if ((size / 1024 / 1024 >= 1))
			return Task.FromResult($"{count} items in cache ({size / 1024 / 1024} MiB).");
		else if ((size / 1024) >= 1)
			return Task.FromResult($"{count} items in cache ({size / 1024} KiB).");
		else
			return Task.FromResult($"{count} items in cache ({size} bytes).");
	}


	/// <summary>
	/// Saves the cache to the filesystem.
	/// </summary>
	/// <code>StatusLabel.Text = _imageStore.Save().Result;</code>
	/// <remarks>Files are now saved automatically. <see cref="Restore"/> is also no longer neccessary.</remarks>
	/// <returns>A result string from a TResult. </returns>
	public async Task<string> Save()
	{
		foreach (string url in s_cachedUris)
			await KeepAsync(new Uri(url)).ConfigureAwait(false);

		return "Tracked URIs saved to cache.";
	}
}