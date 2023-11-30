namespace Ppdac.Cache.Tests;

[TestClass]
[DeploymentItem(@".\Deployables\335a2bf2-ddaa-43a8-3c79-acb61472aab8")]
public class ImageCache_GetByteCountAsync_Should
{
	private readonly ImageCache _imageCache;
	// The URI of _imageUri will generate the following Guid filename: 335a2bf2-ddaa-43a8-3c79-acb61472aab8
	private readonly Uri _imageUri = new("https://ppdac.ltd/wp-content/uploads/2020/10/logo@2x-1.png");
	private const string _deployedFileName = "335a2bf2-ddaa-43a8-3c79-acb61472aab8";
	private readonly long _expectedFilesize = new FileInfo(_deployedFileName).Length;

	public ImageCache_GetByteCountAsync_Should()
	{
		_imageCache = new ImageCache
		{
			ImageCachePath = @"..\Out\CachedItems\GetByteCountAsync"
		};
	}


	[TestMethod("Return filesize of web resource as an integer.")]
	public async Task GetByteCountAsync_InputIsUri_ReturnInt_FromWeb()
	{
		string cachedFilePath = Path.Combine(_imageCache.ImageCachePath, GetFilename.FromUri(_imageUri));
		if (File.Exists(cachedFilePath))
			File.Delete(cachedFilePath);
		Assert.IsFalse(File.Exists(cachedFilePath));

		// Assert: The created file should have the same filesize as the deployed file.
		Assert.AreEqual(await _imageCache.GetByteCountAsync(_imageUri), _expectedFilesize);
	}



	[TestMethod("Return filesize of cached resource as an integer.")]
	public async Task GetByteCountAsync_InputIsUri_ReturnInt_FromCache()
	{
		//Make sure the directory and file exist before the test.
		string filename = GetFilename.FromUri(_imageUri);
		string createdFilePath = Path.Combine(_imageCache.ImageCachePath, filename);
		if (!Directory.Exists(_imageCache.ImageCachePath))
			Directory.CreateDirectory(_imageCache.ImageCachePath);
		if (!File.Exists(createdFilePath))
			File.Copy(_deployedFileName, createdFilePath);
		Assert.IsTrue(File.Exists(createdFilePath));

		//Act
		await _imageCache.KeepAsync(_imageUri);

		//Make sure the directory is created if it did not exist.
		Assert.IsTrue(Directory.Exists(_imageCache.ImageCachePath));
		Assert.IsTrue(File.Exists(createdFilePath));

		//Assert: The created file should have the same filesize as the deployed file.
		Assert.AreEqual(await _imageCache.GetByteCountAsync(_imageUri), _expectedFilesize);
	}


	[TestMethod("Throw ArgumentNullException if the Uri is null.")]
	public async Task GetByteCountAsync_InputIsNull_ThrowArgumentNullException() =>
		await Assert.ThrowsExceptionAsync<ArgumentNullException>(
			  action: async () => await _imageCache.GetByteCountAsync(new Uri(null!)));


	[TestMethod("Return filesize of web resource as an integer.")]
	public async Task GetByteCountAsync_InputIsStringUrl_ReturnInt_FromWeb()
	{
		string cachedFilePath = Path.Combine(_imageCache.ImageCachePath, GetFilename.FromUri(_imageUri));
		if (File.Exists(cachedFilePath))
			File.Delete(cachedFilePath);
		Assert.IsFalse(File.Exists(cachedFilePath));

		// Assert: The created file should have the same filesize as the deployed file.
		Assert.AreEqual(await _imageCache.GetByteCountAsync(_imageUri.AbsoluteUri), _expectedFilesize);
	}



	[TestMethod("Return filesize of cached resource as an integer.")]
	public async Task GetByteCountAsync_InputIsStringUrl_ReturnInt_FromCache()
	{
		//Make sure the directory and file exist before the test.
		string filename = GetFilename.FromUri(_imageUri);
		string createdFilePath = Path.Combine(_imageCache.ImageCachePath, filename);
		if (!Directory.Exists(_imageCache.ImageCachePath))
			Directory.CreateDirectory(_imageCache.ImageCachePath);
		if (!File.Exists(createdFilePath))
			File.Copy(_deployedFileName, createdFilePath);
		Assert.IsTrue(File.Exists(createdFilePath));

		//Act
		await _imageCache.KeepAsync(_imageUri);

		//Make sure the directory is created if it did not exist.
		Assert.IsTrue(Directory.Exists(_imageCache.ImageCachePath));
		Assert.IsTrue(File.Exists(createdFilePath));

		//Assert: The created file should have the same filesize as the deployed file.
		Assert.AreEqual(await _imageCache.GetByteCountAsync(_imageUri.AbsoluteUri), _expectedFilesize);
	}


	[TestMethod("Throw ArgumentNullException for empty string.")]
	public async Task GetByteCountAsync_InputIsEmptyString_ThrowArgumentNullException() =>
		await Assert.ThrowsExceptionAsync<UriFormatException>(
			action: async () => await _imageCache.GetByteCountAsync(" "));
}