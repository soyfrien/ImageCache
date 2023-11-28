namespace Ppdac.Cache.Tests;

[TestClass]
[DeploymentItem(@".\Deployables\11250d6b-4460-eda2-bfea-9022169a5e88")]
public class ImageCache_GetAsStreamAsync_Should
{
	private readonly ImageCache _imageCache;
	// The URI of _imageUri will generate the following Guid filename: 11250d6b-4460-eda2-bfea-9022169a5e88
	private readonly Uri _imageUri = new("https://ppdac.ltd/wp-content/uploads/2020/10/logo@2x-1.png");
	private const string _deployedFileName = "11250d6b-4460-eda2-bfea-9022169a5e88";
	private readonly long expectedFilesize = new FileInfo(_deployedFileName).Length;

	public ImageCache_GetAsStreamAsync_Should()
	{
		_imageCache = new ImageCache
		{
			ImageCachePath = @"..\Out\CachedItems\AsStreamAsync"
		};
	}


	[TestMethod("Return a Stream of bytes of the image from the web.")]
	public void AsStreamAsync_InputIsUri_ReturnStream_FromCache()
	{
		//Arrange
		//Make sure the directory and file exist before the test.
		string filename = GetFilename.FromUri(_imageUri);
		string createdFilePath = Path.Combine(_imageCache.ImageCachePath, filename);
		if (!Directory.Exists(_imageCache.ImageCachePath))
			Directory.CreateDirectory(_imageCache.ImageCachePath);
		Assert.IsTrue(Directory.Exists(_imageCache.ImageCachePath));
		File.Copy(_deployedFileName, createdFilePath, true);
		Assert.IsTrue(File.Exists(createdFilePath));


		//Act
		using Stream stream = _imageCache.GetAsStreamAsync(_imageUri).Result;


		//Assert
		Assert.AreEqual(new FileInfo(createdFilePath).Length, stream.Length);
	}


	[TestMethod("Return a Stream of bytes of the image from the web.")]
	public void AsStreamAsync_InputIsUri_ReturnStream_FromWeb()
	{
		//Make sure the directory exist and file doesn't
		string filename = GetFilename.FromUri(_imageUri);
		string createdFilePath = Path.Combine(_imageCache.ImageCachePath, filename);
		if (!Directory.Exists(_imageCache.ImageCachePath))
			Directory.CreateDirectory(_imageCache.ImageCachePath);
		Assert.IsTrue(Directory.Exists(_imageCache.ImageCachePath));
		if (File.Exists(createdFilePath))
			File.Delete(createdFilePath);
		Assert.IsFalse(File.Exists(createdFilePath));

		using Stream stream = _imageCache.GetAsStreamAsync(_imageUri).Result;
		Assert.AreEqual(expectedFilesize, stream.Length);
	}


	[TestMethod("Return ArgumentNullException if Uri is null.")]
	public void AsStreamAsync_InputIsNull_ThrowsArgumentNullException() =>
		Assert.ThrowsExceptionAsync<ArgumentNullException>(() => _imageCache.GetAsStreamAsync(null!));
}