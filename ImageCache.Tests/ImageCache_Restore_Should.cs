using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ppdac.Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ppdac.Cache.Tests;

[TestClass()]
public class ImageCache_Restore_Should
{
	private readonly ImageCache _imageCache;
	private readonly List<Uri> _imageUris = [
		new Uri("https://ppdac.ltd/wp-content/uploads/2020/04/logo1-dark@2x-1.png"),
		new Uri("https://ppdac.ltd/wp-content/uploads/2020/10/logo@2x-1.png"),
		new Uri("https://ppdac.ltd/wp-content/uploads/2021/09/cloud-storage-applications-internet-6621829-e1651375167197-1024x678.jpg")
	];




	public ImageCache_Restore_Should()
	{
		_imageCache = new ImageCache();
		ImageCache.ImageCachePath = @"..\Out\CachedItems\Restore";
	}


	[TestInitialize]
	public void Initialize()
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


	[TestMethod("Restores cached items to tracked Uris.")]
	public void Restore_ReturnTask()
	{
		// Arrange: Add an item to cache by getting it as an ImageSource
		ImageSource imageSource = _imageCache.GetAsImageSourceAsync(_imageUris[0]).Result;
		Assert.IsTrue(ImageCache.Count > 0);
		_imageCache.Clear();
		Assert.AreEqual(0, ImageCache.Count);


		// Act
		Task<string> result = _imageCache.Restore();


		// Assert: Ensure the item is restored
		Assert.AreEqual(1, ImageCache.Count);
		Assert.IsTrue(result.IsCompletedSuccessfully);


		// Rearrange: Add a second item to cache by getting it as a Stream
		Stream? imageSream = _imageCache.GetImageAsStreamAsync(_imageUris[1]).Result;
		_imageCache.Clear();
		Assert.AreEqual(0, ImageCache.Count);


		// Act
		result = _imageCache.Restore();


		// Assert: Ensure both items are restored
		Assert.AreEqual(2, ImageCache.Count);
		Assert.IsTrue(result.IsCompletedSuccessfully);

		// Rearrange: Add a third item to cache 
		_imageCache.Clear();
		Assert.AreEqual(0, ImageCache.Count);
		byte[]? imageBytes = ImageCache.GetAsBytesAsync(_imageUris[2]).Result;
		_imageCache.Clear();
		Assert.AreEqual(0, ImageCache.Count);


		// Act
		result = _imageCache.Restore();


		// Assert: Ensure all three items are restored
		Assert.AreEqual(3, ImageCache.Count);
		Assert.IsTrue(result.IsCompletedSuccessfully);



		// Rearrange: Purge the cache and add all three items to cache using a different method
		_imageCache.Purge().Wait();
		foreach (Uri uri in _imageUris)
			ImageCache.KeepAsync(uri).Wait();
		_imageCache.Clear();
		Assert.AreEqual(0, ImageCache.Count);
		result = _imageCache.Restore();
		Assert.AreEqual(3, ImageCache.Count);
		Assert.IsTrue(result.IsCompletedSuccessfully);
	}


	[TestMethod("Returns gracefully if the cache is empty.")]
	public void Restore_ReturnsGracefully() =>
		Assert.AreEqual(0, ImageCache.Count);
}