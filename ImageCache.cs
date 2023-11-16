﻿#if WINDOWS
using Microsoft.Maui.Graphics.Win2D;
#elif IOS || ANDROID || MACCATALYST
using Microsoft.Maui.Graphics.Platform;
#endif

namespace Ppdac.ImageCache.Maui;

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
public class ImageCache
{
	private const string _CacheFile = "ppdac.imagecache.maui.dat";
	private static readonly Dictionary<string, byte[]> s_imageStore = [];
	
	
	/// <summary>
	/// Adds an image to the store with a URL.
	/// </summary>
	/// <remarks>
	/// It is not neccessary to call this, as whenever an unknown image is requested,
	/// it is automatically added to the cache.
	/// </remarks>
	/// <param name="url">A URL for the image as a string.</param>
	public static void Add(string url)
	{
		ArgumentException.ThrowIfNullOrEmpty(url);

		if (s_imageStore.TryGetValue(key: url, value: out byte[]? value))
			s_imageStore[url] = value;
		else
			_ = s_imageStore.TryAdd(url, value: GetImageAsBytes(url)!);
	}


	/// <summary>
	/// Adds an image to the store with a URI.
	/// </summary>
	/// <remarks>
	/// It is not neccessary to call this, as whenever an unknown image is requested,
	/// it is automatically added to the cache.
	/// </remarks>
	/// <param name="uri">A URI whose .AbsoluteUri property is the location of the image.</param>
	public static void Add(Uri uri)
	{
		ArgumentNullException.ThrowIfNull(uri);

		if (s_imageStore.TryGetValue(key: uri.AbsoluteUri, value: out byte[]? value))
			s_imageStore[uri.AbsoluteUri] = value;
		else
			s_imageStore.Add(uri.AbsoluteUri, GetImageAsBytes(uri)!);
	}


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
	/// <param name="url">A URL for the image as a string.</param>
	/// <returns>A MemoryStream of the byte array of the image.</returns>
	public static Func<Stream> AsFuncStream(string url)
	{
		if (s_imageStore.TryGetValue(url, out byte[]? value))
			return () => new MemoryStream(value);
		else
		{
			Add(new Uri(url));
			return () => new MemoryStream(s_imageStore[url]);
		}
	}

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
	public static Func<Stream> AsFuncStream(Uri uri)
	{
		ArgumentNullException.ThrowIfNull(uri);

		if (s_imageStore.TryGetValue(uri.AbsoluteUri, out byte[]? value))
			return () => new MemoryStream(value);
		else
		{
			Add(uri);
			return () => new MemoryStream(s_imageStore[uri.AbsoluteUri]);
		}
	}


	/// <summary>
	/// Looks for a key equal to the URL and returns a stream of byte[], which can be used to set an ImageSource
	/// on objects like the Microsoft.Maui.Controls.Image or the Microsoft.Maui.Graphics.IImage.
	/// 
	/// If not found, the image is cached and then as a MemoryStream of its bytes.
	/// </summary>
	/// <param name="url">The URL is both the key and the location of the image on the Internet.</param>
	/// <returns>A MemoryStream of the byte array of the image.</returns>
	public static Stream AsStream(string url)
	{
		if (s_imageStore.TryGetValue(url, out byte[]? value))
			return new MemoryStream(value);
		else
		{
			Add(url);
			return new MemoryStream(s_imageStore[url]);
		}
	}


	/// <summary>
	/// Looks for a key equal to the URL and returns a stream of byte[], which can be used to set an ImageSource
	/// on objects like the Microsoft.Maui.Controls.Image or the Microsoft.Maui.Graphics.IImage.
	/// 
	/// If not found, the image is cached and then as a MemoryStream of its bytes.
	/// </summary>
	/// <param name="uri">The URI is both the key and the location of the image on the Internet.</param>
	/// <returns>A MemoryStream of the byte array of the image.</returns>
	public static Stream AsStream(Uri uri)
	{
		if (s_imageStore.TryGetValue(uri.AbsoluteUri, out byte[]? value))
			return new MemoryStream(value);
		else
		{
			Add(uri);
			return new MemoryStream(s_imageStore[uri.AbsoluteUri]);
		}
	}

	/// <summary>
	/// Looks for a key equal to the string and returns a stream of byte[], which can be used to set an ImageSource
	/// on objects like the Microsoft.Maui.Controls.Image or the Microsoft.Maui.Graphics.IImage.
	/// 
	/// If not found, null is returned.
	/// </summary>
	/// <param name="key">The URL is both the key and the location of the image on the Internet.</param>
	/// <returns>A MemoryStream of the byte array of the image or null.</returns>
	public static byte[]? AsBytes(string key) => 
		s_imageStore.TryGetValue(key, out byte[]? value) ? value 
															   : null;


	/// <summary>
	/// Looks for a key equal to the URL and returns a the image as a byte array.
	/// If not found, null is returned.
	/// </summary>
	/// <param name="url">The URL is both the key and the location of the image on the Internet.</param>
	/// <returns>The byte array of the image if found, or null.</returns>
	private static byte[]? GetImageAsBytes(string url)
	{
		if (string.IsNullOrEmpty(url))
			return null;

		using HttpClient httpClient = new();
		HttpResponseMessage responseMessage = httpClient.GetAsync(new Uri(url)).Result;
		Stream stream = responseMessage.Content.ReadAsStreamAsync().Result;
#if WINDOWS
		Microsoft.Maui.Graphics.IImage img = new W2DImageLoadingService().FromStream(stream);
#elif IOS || ANDROID || MACCATALYST
		Microsoft.Maui.Graphics.IImage img = PlatformImage.FromStream(stream);
#endif
		byte[] bytes = img.AsBytes();

		stream.Dispose();
		img.Dispose();

		int filesize = bytes.Length;

		return bytes;
	}

	/// <summary>
	/// Looks for a key equal to the URI's .AbsoluteUri property and returns a the image as a byte array.
	/// If not found, null is returned.
	/// </summary>
	/// <param name="uri">The URI is both the key and the location of the image on the Internet.</param>
	/// <returns>The byte array of the image if found, or null.</returns>
	private static byte[]? GetImageAsBytes(Uri uri)
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


	/// <summary>
	/// Looks for a key equal to the string and returns a the size image in bytes.
	/// If not found it is added then the size is returned. If null, -1 is returned.
	/// </summary>
	/// <param name="url">The URL is both the key and the location of the image on the Internet.</param>
	/// <returns>The size of the image if found in bytes, as long. Or null.</returns>
	public static int GetNumberOfBytes(string url)
	{
		if (string.IsNullOrEmpty(url))
			return -1;

		if (s_imageStore.TryGetValue(url, out byte[]? value))
			return value.Length;
		else
		{
			Add(new Uri(url));
			return s_imageStore[url].Length;
		}
	}


	/// <summary>
	/// Looks for a key equal to the string and returns a the size image in bytes.
	/// If not found it is added then the size is returned. If null, -1 is returned.
	/// </summary>
	/// <param name="uri">The URL is both the key and the location of the image on the Internet.</param>
	/// <returns>The size of the image if found in bytes, as long. Or null.</returns>
	public static int GetNumberOfBytes(Uri uri)
	{
		ArgumentNullException.ThrowIfNull(uri);

		if (s_imageStore.TryGetValue(uri.AbsoluteUri, out byte[]? value))
			return value.Length;
		else
		{
			Add(uri);
			return s_imageStore[uri.AbsoluteUri].Length;
		}
	}


	/// <summary>
	/// Saves the cache to the filesystem.
	/// </summary>
	/// <code>StatusLabel.Text = _imageStore.Save().Result;</code>
	/// <returns>A result string from a TResult. </returns>
	public static Task<string> Save()
	{
		string cachePath = $"{FileSystem.Current.CacheDirectory}\\{_CacheFile}";

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
		string cachePath = $"{FileSystem.Current.CacheDirectory}\\{_CacheFile}";

		if (File.Exists(cachePath) is false)
			return Task.FromResult($"Image cache was not at {cachePath}.");

		using FileStream fs = new(cachePath, FileMode.Open);
		using BinaryReader br = new(fs);

		int count = br.ReadInt32();
		for (int i = 0; i < count; i++)
		{
			string key = br.ReadString();
			int length = br.ReadInt32();
			byte[] value = br.ReadBytes(length);
			s_imageStore.TryAdd(key, value);
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
		string cachePath = $"{FileSystem.Current.CacheDirectory}\\{_CacheFile}";

		if (File.Exists(cachePath))
		{
			File.Delete(cachePath);
			return Task.FromResult($"Deleted cache at {cachePath}.");
		}
		else
			return Task.FromResult($"Image cache not found at {cachePath}.");
	}
}