namespace Ppdac.Cache.Tests;

[TestClass]
[DeploymentItem(@".\Deployables\335a2bf2-ddaa-43a8-3c79-acb61472aab8")]
public class ImageCache_GetAsFuncStreamAsync_Should
{
	private readonly ImageCache _imageCache;
	private readonly Uri _imageUri = new("https://ppdac.ltd/wp-content/uploads/2020/10/logo@2x-1.png");
	private const string _deployedFileName = "335a2bf2-ddaa-43a8-3c79-acb61472aab8";
	private readonly long expectedFilesize = new FileInfo(_deployedFileName).Length;

	public ImageCache_GetAsFuncStreamAsync_Should()
	{
		_imageCache = new ImageCache
		{
			ImageCachePath = @"..\Out\CachedItems\GetAsFuncStreamAsync"
		};
	}


	[TestMethod("Return a Func<Stream> of byte[] from a Uri input.")]
	public void AsFuncStreamAsync_InputIsUri_ReturnsFuncOfStream_FromCache()
	{
		// Arrange: Ensure cached file exists, and get its number of bytes.
		Assert.IsTrue(new FileInfo(_deployedFileName).Exists);
		string filename = GetFilename.FromUri(_imageUri);
		string createdFilePath = Path.Combine(_imageCache.ImageCachePath, filename);

		//Create the directory if it does not exist.
		if (!Directory.Exists(_imageCache.ImageCachePath))
			Directory.CreateDirectory(_imageCache.ImageCachePath);

		// Copy the deployed file to the ImageCachePath.
		File.Copy(_deployedFileName, createdFilePath, true);
		Assert.IsTrue(File.Exists(createdFilePath));

		// Act
		Func<Stream> funcStream = _imageCache.GetAsFuncStreamAsync(_imageUri).Result;
		byte[] bytes = new byte[funcStream().Length];
		funcStream().Read(bytes, 0, bytes.Length);

		// Assert
		Assert.AreEqual(expectedFilesize, bytes.Length);
	}


	[TestMethod("Return a Func<Stream> of byte[] from a Uri input.")]
	public void AsFuncStreamAsync_InputIsUri_ReturnsFuncOfStream_FromWeb()
	{
		// Arrange: Ensure cached file does not exist.
		ImageCache.Clear();
		string filename = GetFilename.FromUri(_imageUri);
		string createdFilePath = Path.Combine(_imageCache.ImageCachePath, filename);
		//Create the directory if it does not exist.
		if (!Directory.Exists(_imageCache.ImageCachePath))
			Directory.CreateDirectory(_imageCache.ImageCachePath);
		// Delete the file if it exists.
		if (File.Exists(createdFilePath))
			File.Delete(createdFilePath);
		Assert.IsFalse(File.Exists(createdFilePath));

		// Act
		Func<Stream> funcStream = _imageCache.GetAsFuncStreamAsync(_imageUri).Result;
		byte[] bytes = new byte[funcStream().Length];
		funcStream().Read(bytes, 0, bytes.Length);

		// Assert
		Assert.AreEqual(expectedFilesize, bytes.Length);
	}


	[TestMethod("Return ArgumentNullException if Uri is null.")]
	public void AsFuncStreamAsync_InputIsNull_ReturnsNull() =>
		Assert.ThrowsExceptionAsync<ArgumentNullException>(() => _imageCache.GetAsFuncStreamAsync(null!));
}