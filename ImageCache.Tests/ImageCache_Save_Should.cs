namespace Ppdac.Cache.Tests;

[TestClass]
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
		_imageCache.ImageCachePath = @"..\Out\CachedItems\Save";
	}


	[TestMethod()]
	public void Save_WriteTrackedUrisToCache()
	{
		foreach (Uri uri in _imageUris)
			_imageCache.KeepAsync(uri).Wait();

		Assert.AreEqual(_imageUris.Count, ImageCache.Count);

		// Check the three files are in the cache folder.
		foreach (Uri uri in _imageUris)
		{
			string filename = GetFilename.FromUri(uri);
			string createdFilePath = Path.Combine(_imageCache.ImageCachePath, filename);
			Assert.IsTrue(File.Exists(createdFilePath));
		}

		// Purge the cache.
		_imageCache.Purge();

		// Check the cache is empty.
		Assert.AreEqual(3, ImageCache.Count);
		foreach (Uri uri in _imageUris)
		{
			string filename = GetFilename.FromUri(uri);
			string createdFilePath = Path.Combine(_imageCache.ImageCachePath, filename);
			Assert.IsFalse(File.Exists(createdFilePath));
		}


		Task result = _imageCache.Save();
		result.Wait();
		Assert.IsTrue(result.IsCompleted);
		Assert.IsTrue(result.IsCompletedSuccessfully);



		// Check the cache is not empty.
		Assert.AreEqual(_imageUris.Count, ImageCache.Count);

		foreach (Uri uri in _imageUris)
		{
			string filename = GetFilename.FromUri(uri);
			string createdFilePath = Path.Combine(_imageCache.ImageCachePath, filename);
			Assert.IsTrue(File.Exists(createdFilePath));
		}

	}
}