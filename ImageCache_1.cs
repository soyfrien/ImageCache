#if WINDOWS
using Microsoft.Maui.Graphics.Win2D;
#elif ANDROID || IOS || MACCATALYST
using Microsoft.Maui.Graphics.Platform;
#endif

using System.Collections;
using System.Security.Cryptography;
using System.Text;


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
public class ImageCache
{
	/// <summary>
	/// This is the path to the folder where the _ContiguousCacheFile is stored.
	/// </summary>
	private static string? _ImageCachePath;// = $"{FileSystem.Current.CacheDirectory}\\ppdac";

	/// <summary>
	/// This is the name of the file that is saved to the filesystem.
	/// </summary>
	private static string? _ContiguousCacheFile;// = "ppdac.imagecache.data";

	private static readonly HashAlgorithm s_sha256 = SHA256.Create();
	private static readonly Dictionary<string, byte[]> s_imageStore = [];


	/// <summary>
	/// Default constructor settings:
	/// <code>
	/// _ImageCachePath = $"{FileSystem.Current.CacheDirectory}\\Images";
	/// _ContiguousCacheFile = "ppdac.imagecache.data";
	/// </code>
	/// </summary>
	public ImageCache()
	{
		_ImageCachePath = $"{FileSystem.Current.CacheDirectory}\\Image";
		_ContiguousCacheFile = "ppdac.imagecache.data";
	}


	/// <summary>
	/// Use the default constructor or pass null for the imageCachePath to use its default while overriding the cacheFile.
	/// </summary>
	/// <param name="imageCachePath">Path of folder to save the cache file in.</param>
	/// <param name="cacheFile">Filename for the cache data file.</param>
	public ImageCache(string imageCachePath, string cacheFile = "ppdac.imagecache.data") : this()
	{

		if (string.IsNullOrEmpty(imageCachePath) is false)
			_ImageCachePath = imageCachePath;

		if (string.IsNullOrEmpty(imageCachePath) is false)
			_ContiguousCacheFile = cacheFile;

		// TODO: Log this non-default behavior for user.
	}


	/// <summary>
	/// Gets image from URI and returns it as an <see cref="ImageSource"/>.
	/// </summary>
	/// <param name="uri">URI of image to cache.</param>
	/// <returns>The image is returned as an <see cref="ImageSource"/>.</returns>
	public static ImageSource GetAsImageSource(Uri uri)
	{
		if (string.IsNullOrEmpty(uri.AbsoluteUri))
			return null;

		using HttpClient httpClient = new();
		HttpResponseMessage responseMessage = httpClient.GetAsync(uri).Result;
		if (responseMessage.IsSuccessStatusCode is false)
			return null; //throw new Exception($"Failed to get image from {uri.AbsoluteUri}.");

		Stream stream = responseMessage.Content.ReadAsStreamAsync().Result;
#if WINDOWS
		Microsoft.Maui.Graphics.IImage img = new W2DImageLoadingService().FromStream(stream);
#elif IOS || ANDROID || MACCATALYST
		Microsoft.Maui.Graphics.IImage img = PlatformImage.FromStream(stream);
#endif
		byte[] bytes = img.AsBytes();

		return ImageSource.FromStream(() => new MemoryStream(bytes));
	}

	/// <summary>
	/// Clears the Dictionary.
	/// </summary>
	public static void FreeMemory()
	{
		s_imageStore.Clear();
	}


	/// <inheritdoc/>
	public static Func<Stream> GetImageAsFuncStream(Uri uri)
	{
		if (s_imageStore.ContainsKey(uri.AbsoluteUri))//TryGetValue(uri.AbsoluteUri, out byte[]? value))
			return () => new MemoryStream(GetImageAsBytes(uri));//(value);
		else
		{
			Add(uri);

			lock (s_sha256)
			{
				byte[] uriHash = s_sha256.ComputeHash(Encoding.UTF8.GetBytes(uri.AbsoluteUri));

				ReadOnlySpan<byte> filenameSeed = new(uriHash[..16]);

				Guid filename = new(filenameSeed);
				byte[]? bytes = s_imageStore[filename.ToString()
															.Replace('{', '"')
															.Replace('}', '"')] as byte[];

				return () => new MemoryStream(bytes);
			}
		}

		//string filename = GetFilename(uri);
		//if (s_imageStore.ContainsKey(filename))//TryGetValue(uri.AbsoluteUri, out byte[]? value))
		//	return () => new MemoryStream(GetImageAsBytes(uri));//(value);
		//else
		//{
		//	Add(new Uri(uri.AbsoluteUri));
		//	byte[]? bytes = s_imageStore[filename] as byte[];
		//	return () => new MemoryStream(bytes!);
		//}
	}
	/// <summary>
	/// Alias for <see cref="GetImageAsFuncStream(Uri)"/>.
	/// </summary>
	public static Func<Stream> AsFuncStream(Uri uri) => GetImageAsFuncStream(uri);


	/// <summary>
	/// Returns a filename based on the SHA256 hash of the URI.
	/// </summary>
	/// <param name="uri"></param>
	/// <returns></returns>
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


	/// <inheritdoc/>
	public static void Add(Uri uri)
	{
		ArgumentNullException.ThrowIfNull(uri);
		lock (s_sha256)
		{
			byte[] uriHash = s_sha256.ComputeHash(Encoding.UTF8.GetBytes(uri.AbsoluteUri));

			ReadOnlySpan<byte> filenameSeed = new(uriHash[..16]);
			Guid guid = new(filenameSeed);
			

			
			string filename = $"{guid}".Replace('{', '"').Replace('}', '"');

			if (s_imageStore.ContainsKey(key: filename))//, value: out byte[]? value))
				s_imageStore[filename] = GetImageAsBytes(uri)!;//value;
			else
				s_imageStore.Add(filename, GetImageAsBytes(uri)!);
		}
	}


	/// <inheritdoc/>
	public static byte[]? GetImageAsBytes(string url)
	{
		if (string.IsNullOrEmpty(url))
			return null;

		if (s_imageStore.ContainsKey(url))//TryGetValue(url, out byte[]? value) ? value
			return s_imageStore[url] as byte[];
		else
		{
			using HttpClient httpClient = new();
			HttpResponseMessage responseMessage = httpClient.GetAsync(url).Result;
			if (responseMessage.IsSuccessStatusCode is false)
				return null; //throw new Exception($"Failed to get image from {uri.AbsoluteUri}.");

			Stream stream = responseMessage.Content.ReadAsStreamAsync().Result;
#if WINDOWS
			Microsoft.Maui.Graphics.IImage img = new W2DImageLoadingService().FromStream(stream);
#elif IOS || ANDROID || MACCATALYST
		Microsoft.Maui.Graphics.IImage img = PlatformImage.FromStream(stream);
#endif
			byte[] bytes = img.AsBytes();

			stream.Dispose();
			img.Dispose();

			return bytes;
		}
	}


	/// <inheritdoc/>
	public static byte[] GetImageAsBytes(Uri uri)
	{
		if (string.IsNullOrEmpty(uri.AbsoluteUri))
			return null;

		using HttpClient httpClient = new();
		HttpResponseMessage responseMessage = httpClient.GetAsync(uri).Result;
		if (responseMessage.IsSuccessStatusCode is false)
			return null; //throw new Exception($"Failed to get image from {uri.AbsoluteUri}.");

		Stream stream = responseMessage.Content.ReadAsStreamAsync().Result;
#if WINDOWS
		Microsoft.Maui.Graphics.IImage img = new W2DImageLoadingService().FromStream(stream);
#elif IOS || ANDROID || MACCATALYST
		Microsoft.Maui.Graphics.IImage img = PlatformImage.FromStream(stream);
#endif
		byte[] bytes = img.AsBytes();

		stream.Dispose();
		img.Dispose();

		return bytes;
	}


	/// <inheritdoc/>
	public static int GetNumberOfBytes(Uri uri)
	{
		if (string.IsNullOrEmpty(uri.AbsoluteUri))
			return -1;

		lock (s_sha256)
		{
			byte[] uriHash = s_sha256.ComputeHash(Encoding.UTF8.GetBytes(uri.AbsoluteUri));
			ReadOnlySpan<byte> filenameSeed = new(uriHash[..16]);
			Guid guid = new(filenameSeed);
			string filename = $"{guid}".Replace('{', '"').Replace('}', '"');

			if (s_imageStore.ContainsKey(filename))//TryGetValue(uri.AbsoluteUri, out byte[]? value))
			{
				byte[]? bytes = s_imageStore[filename] as byte[];
				return bytes.Length;
				//return value.Length; 
			}
			else
			{
				Add(new Uri(uri.AbsoluteUri));
				byte[] bytes = (byte[])s_imageStore[filename]!;
				return bytes.Length;
				//return s_imageStore[uri.AbsoluteUri].Length;
			}
		}
	}

	/// <inheritdoc/>
	//public static Func<Stream> GetImageAsFuncStream(Uri uri)
	//{
	//	string filename = GetFilename(uri);
	//	if (s_imageStore.ContainsKey(filename))//TryGetValue(uri.AbsoluteUri, out byte[]? value))
	//		return () => new MemoryStream(GetImageAsBytes(uri));//(value);
	//	else
	//	{
	//		Add(new Uri(uri.AbsoluteUri));
	//		byte[]? bytes = s_imageStore[filename] as byte[];
	//		return () => new MemoryStream(bytes!);
	//	}
	//}


	/// <inheritdoc/>
	public static Stream GetImageAsStream(Uri uri)
	{
		lock (s_sha256)
		{
			byte[] uriHash = s_sha256.ComputeHash(Encoding.UTF8.GetBytes(uri.AbsoluteUri));
			ReadOnlySpan<byte> filenameSeed = new(uriHash[..16]);
			Guid guid = new(filenameSeed);
			string filename = $"{guid}".Replace('{', '"').Replace('}', '"');

			if (s_imageStore.ContainsKey(filename))//.TryGetValue(uri.AbsoluteUri, out byte[]? value))
				return new MemoryStream(GetImageAsBytes(uri));//(value);
			else
			{
				Add(new Uri(uri.AbsoluteUri));
				byte[]? bytes = s_imageStore[filename] as byte[];
				return new MemoryStream(bytes!);
				//return new MemoryStream(s_imageStore[uri.AbsoluteUri]);
			}
		}
	}


	/// <summary>
	/// Saves the cache to the filesystem.
	/// </summary>
	/// <code>StatusLabel.Text = _imageStore.Save().Result;</code>
	/// <returns>A result string from a TResult. </returns>
	public static Task<string> Save()
	{
		string cachePath = $"{FileSystem.Current.CacheDirectory}\\{_ContiguousCacheFile}";

		using FileStream fs = new(cachePath, FileMode.OpenOrCreate);
		using BinaryWriter bw = new(fs);
		bw.Write(s_imageStore.Count);
		foreach (KeyValuePair<string, byte[]> kvp in s_imageStore)
		{
			bw.Write(kvp.Key);
			bw.Write(kvp.Value.Length);
			bw.Write(kvp.Value);
		}
		return Task.FromResult($"Image cache saved to {cachePath}.");
	}


	
	/// <summary>
	/// Restores the cache from the filesystem, 
	/// after using Dependency Injection to access the ImageCache, for example.
	/// </summary>
	/// <code>StatusLabel.Text = _imageStore.Restore().Result;</code>
	/// <returns>A string as TResult.</returns>
	public static Task<string> Restore()
	{
		string cachePath = $"{FileSystem.Current.CacheDirectory}\\{_ContiguousCacheFile}";

		if (File.Exists(cachePath) is false)
			return Task.FromResult($"{cachePath} does not exist.");

		using FileStream fs = new(cachePath, FileMode.Open);
		using BinaryReader br = new(fs);

		int count = br.ReadInt32();
		for (int i = 0; i < count; i++)
		{
			string key = br.ReadString();
			int length = br.ReadInt32();
			byte[] value = br.ReadBytes(length);
			s_imageStore.TryAdd(key, value);//.TryAdd(url, value);
		}
		return Task.FromResult($"Image cache restored from {cachePath}.");
	}


	/// <summary>
	/// Removes the cache from the filesystem.
	/// </summary>
	/// <code>StatusLabel.Text = _imageStore.Purge().Result;</code>
	/// <returns>A string from a TResult.</returns>
	public static Task<string> Purge()
	{
		string cachePath = $"{FileSystem.Current.CacheDirectory}\\{_ContiguousCacheFile}";

		if (File.Exists(cachePath))
		{
			File.Delete(cachePath);
			return Task.FromResult($"Deleted cache at {cachePath}.");
		}
		else
			return Task.FromResult($"Image cache not found at {cachePath}.");
	}
}