namespace Ppdac.Cache.Tests;

[TestClass()]
public class ImageCache_FilesOlderThanCacheExpiryInSeconds_Should
{
	private readonly ImageCache _imageCache = new();
	private readonly Uri _imageUri = new("https://ppdac.ltd/wp-content/uploads/2020/10/logo@2x-1.png");

	public ImageCache_FilesOlderThanCacheExpiryInSeconds_Should()
	{
		_imageCache.ImageCachePath = @"..\Out\CachedItems\CacheExpiryInSeconds";
		_imageCache.CacheExpiryInSeconds = 5;
	}

	[TestMethod("Files older than CacheExpiryInSeconds are removed.")]
	public void ImageCache_StaleFilesAreDeleted()
	{
		_imageCache.KeepAsync(_imageUri).Wait();
		string pathToFile = Path.Combine(_imageCache.ImageCachePath, GetFilename.FromUri(_imageUri));
		Assert.IsTrue(File.Exists(pathToFile));

		DateTime lastWriteTime = new FileInfo(pathToFile).LastWriteTime;

		// Get the file again	
		_imageCache.KeepAsync(_imageUri).Wait();
		DateTime newLastWriteTime = new FileInfo(pathToFile).LastWriteTime;

		Assert.AreEqual(lastWriteTime, newLastWriteTime);

		// Wait for the file to expire and get it again
		Thread.Sleep(5000);
		_imageCache.KeepAsync(_imageUri).Wait();

		newLastWriteTime = new FileInfo(pathToFile).LastWriteTime;
		// Check that the last write time has changed
		Assert.AreNotEqual(lastWriteTime, newLastWriteTime);
	}

	[TestMethod("Files younger than CacheExpiryInSeconds are not removed.")]
	public void ImageCache_FreshFilesAreNotDeleted()
	{
		_imageCache.KeepAsync(_imageUri).Wait();
		string pathToFile = Path.Combine(_imageCache.ImageCachePath, GetFilename.FromUri(_imageUri));
		Assert.IsTrue(File.Exists(pathToFile));

		DateTime lastWriteTime = new FileInfo(pathToFile).LastWriteTime;
		Thread.Sleep(4000);
		_imageCache.KeepAsync(_imageUri).Wait();
	    DateTime newLastWriteTime = new FileInfo(pathToFile).LastWriteTime;

		Assert.AreEqual(lastWriteTime, newLastWriteTime);
	}
}