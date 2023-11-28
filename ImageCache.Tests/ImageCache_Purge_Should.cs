namespace Ppdac.Cache.Tests;

[TestClass]
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
		_imageCache = new ImageCache
		{
			ImageCachePath = @"..\Out\CachedItems\Purge"
		};
	}

	[TestInitialize]
	public void TestInitialize()
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


	[TestMethod("Remove cached items from storage and deletes cache folder.")]
	public void Purge_ReturnTaskString()
	{
		foreach (Uri uri in _imageUris)
			_imageCache.KeepAsync(uri).Wait();
		Assert.IsTrue(new DirectoryInfo(_imageCache.ImageCachePath).GetFiles().Count() == _imageUris.Count);
		Assert.IsTrue(ImageCache.Count == _imageUris.Count);


		ImageCache.Clear();
		Assert.AreEqual(0, ImageCache.Count);

		_imageCache.Purge().Wait();
		Assert.IsFalse(Directory.Exists(_imageCache.ImageCachePath));
	}
}