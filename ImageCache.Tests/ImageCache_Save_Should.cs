using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ppdac.Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ppdac.Cache.Tests;

[TestClass()]
public class ImageCache_Save_Should
{
	private readonly ImageCache _imageCache;
	private readonly List<Uri> _imageUris = [
		new Uri("https://ppdac.ltd/wp-content/uploads/2020/04/logo1-dark@2x-1.png"),
		new Uri("https://ppdac.ltd/wp-content/uploads/2020/10/logo@2x-1.png"),
		new Uri("https://ppdac.ltd/wp-content/uploads/2021/09/cloud-storage-applications-internet-6621829-e1651375167197-1024x678.jpg")
	];


	public ImageCache_Save_Should()
	{
		_imageCache = new ImageCache();
		ImageCache.ImageCachePath = @"..\Out\CachedItems\Save";
	}


	[TestMethod()]
	public void Save_WriteTrackedUrisToCache()
	{
		foreach (Uri uri in _imageUris)
			ImageCache.KeepAsync(uri).Wait();

		Assert.AreEqual(_imageUris.Count, ImageCache.Count);

		// Check the three files are in the cache folder.
		foreach (Uri uri in _imageUris)
		{
			string filename = ImageCache.GetFilename(uri);
			string createdFilePath = Path.Combine(ImageCache.ImageCachePath, filename);
			Assert.IsTrue(File.Exists(createdFilePath));
		}

		// Purge the cache.
		_imageCache.Purge();

		// Check the cache is empty.
		Assert.AreEqual(3, ImageCache.Count);
		foreach(Uri uri in _imageUris)
		{
			string filename = ImageCache.GetFilename(uri);
			string createdFilePath = Path.Combine(ImageCache.ImageCachePath, filename);
			Assert.IsFalse(File.Exists(createdFilePath));
		}

		// Save the tracked Uris to the cache.
		_imageCache.Save().Wait();

		// Check the cache is not empty.
		Assert.AreEqual(_imageUris.Count, ImageCache.Count);
		foreach (Uri uri in _imageUris)
		{
			string filename = ImageCache.GetFilename(uri);
			string createdFilePath = Path.Combine(ImageCache.ImageCachePath, filename);
			Assert.IsTrue(File.Exists(createdFilePath));
		}

	}
}