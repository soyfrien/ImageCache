using Ppdac.Cache.Maui;

namespace Ppdac.Cache.Maui.Tests;

[TestClass]
public class ImageCacheMaui_Restore_Should
{
	private readonly ImageCache _imageCache;
	private readonly List<Uri> _imageUris = [
		new Uri("https://ppdac.ltd/wp-content/uploads/2020/04/logo1-dark@2x-1.png"),
		new Uri("https://ppdac.ltd/wp-content/uploads/2020/10/logo@2x-1.png"),
		new Uri("https://ppdac.ltd/wp-content/uploads/2021/09/cloud-storage-applications-internet-6621829-e1651375167197-1024x678.jpg")
	];




	public ImageCacheMaui_Restore_Should()
	{
		_imageCache = new ImageCache
		{
			ImageCachePath = @"..\Out\CachedItems\Restore"
		};
	}


	[TestInitialize]
	public void Initialize()
	{
		// Arrange: Count should be zero and the cache should be empty, and non-tracking.
		if (Directory.Exists(_imageCache.ImageCachePath))
		{
			foreach (FileInfo file in new DirectoryInfo(_imageCache.ImageCachePath).GetFiles())
				file.Delete();

			Directory.Delete(_imageCache.ImageCachePath);
		}
		ImageCache.Clear();
		Assert.AreEqual(0, ImageCache.Count);
	}


	[TestMethod("Restores cached items to tracked Uris.")]
	public void Restore_ReturnTask()
	{
		Maui.ImageCache imageCacheMaui = new Maui.ImageCache
		{
			ImageCachePath = @"..\Out\CachedItems\Restore"
		};

		// Arrange: Add an item to cache by getting it as an ImageSource
		ImageSource imageSource = imageCacheMaui.GetAsImageSourceAsync(_imageUris[0]).Result;
		Assert.IsTrue(ImageCache.Count > 0);
		ImageCache.Clear();
		Assert.AreEqual(0, ImageCache.Count);


		// Act
		Task<string> result = _imageCache.Restore();


		// Assert: Ensure the item is restored
		Assert.AreEqual(0, ImageCache.Count);
		Assert.IsTrue(result.IsCompletedSuccessfully);


		// Rearrange: Add a second item to cache by getting it as a Stream
		Stream? imageSream = _imageCache.GetAsStreamAsync(_imageUris[1]).Result;
		ImageCache.Clear();
		Assert.AreEqual(0, ImageCache.Count);


		// Act
		result = _imageCache.Restore();


		// Assert: Ensure both items are restored
		Assert.AreEqual(0, ImageCache.Count);
		Assert.IsTrue(result.IsCompletedSuccessfully);

		// Rearrange: Add a third item to cache 
		ImageCache.Clear();
		Assert.AreEqual(0, ImageCache.Count);
		byte[]? imageBytes = _imageCache.GetAsBytesAsync(_imageUris[2]).Result;
		ImageCache.Clear();
		Assert.AreEqual(0, ImageCache.Count);


		// Act
		result = _imageCache.Restore();


		// Assert: Ensure all three items are restored
		Assert.AreEqual(0, ImageCache.Count);
		Assert.IsTrue(result.IsCompletedSuccessfully);



		// Rearrange: Purge the cache and add all three items to cache using a different method
		_imageCache.Purge().Wait();
		foreach (Uri uri in _imageUris)
			_imageCache.KeepAsync(uri).Wait();
		Assert.AreEqual(3, ImageCache.Count);
		Assert.IsTrue(result.IsCompletedSuccessfully);
	}


	[TestMethod("Returns gracefully if the cache is empty.")]
	public void Restore_ReturnsGracefully() =>
		Assert.AreEqual(0, ImageCache.Count);
}