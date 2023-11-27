using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ppdac.Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ppdac.Cache.Tests;

[TestClass()]
public class ImageCache_Purge_Should
{
	private readonly ImageCache _imageCache;
	private readonly List<Uri> _imageUris = [
		new Uri("https://ppdac.ltd/wp-content/uploads/2020/04/logo1-dark@2x-1.png"),
		new Uri("https://ppdac.ltd/wp-content/uploads/2020/10/logo@2x-1.png"),
		new Uri("https://ppdac.ltd/wp-content/uploads/2021/09/cloud-storage-applications-internet-6621829-e1651375167197-1024x678.jpg")
	];


	public ImageCache_Purge_Should()
	{
		_imageCache = new ImageCache();
		ImageCache.ImageCachePath = @"..\Out\CachedItems\Purge";
	}

	[TestInitialize]
	public void TestInitialize()
	{
		// Arrange: Count should be zero and the cache should be empty, and non-tracking.
		if (Directory.Exists(ImageCache.ImageCachePath))
		{
			foreach (FileInfo file in new DirectoryInfo(ImageCache.ImageCachePath).GetFiles())
				file.Delete();

			Directory.Delete(ImageCache.ImageCachePath);
		}
		_imageCache.Clear();
		Assert.AreEqual(0, ImageCache.Count);
	}


	[TestMethod("Remove cached items from storage and deletes cache folder.")]
	public void Purge_ReturnTaskString()
	{
		foreach (Uri uri in _imageUris)
			ImageCache.KeepAsync(uri).Wait();
		Assert.IsTrue(new DirectoryInfo(ImageCache.ImageCachePath).GetFiles().Count() == _imageUris.Count);
		Assert.IsTrue(ImageCache.Count == _imageUris.Count);
		
		
		_imageCache.Clear();
		Assert.AreEqual(0, ImageCache.Count);

		_imageCache.Purge().Wait();
		Assert.IsFalse(Directory.Exists(ImageCache.ImageCachePath));
	}
}