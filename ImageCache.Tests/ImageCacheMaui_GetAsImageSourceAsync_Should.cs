namespace Ppdac.Cache.Maui.Tests;

[TestClass]
[DeploymentItem(@".\Deployables\335a2bf2-ddaa-43a8-3c79-acb61472aab8")]
public class ImageCacheMaui_GetAsImageSourceAsync_Should
{
	private readonly ImageCache _imageCache;
	private readonly Uri _imageUri = new ("https://ppdac.ltd/wp-content/uploads/2020/10/logo@2x-1.png");
	private const string _deployedFileName = "335a2bf2-ddaa-43a8-3c79-acb61472aab8";
	readonly long expectedFilesize = new FileInfo(_deployedFileName).Length;

	public ImageCacheMaui_GetAsImageSourceAsync_Should()
	{
		_imageCache = new ImageCache
		{
			ImageCachePath = @"Out\CachedItems\GetImageSourceAsync"
		};
	}


	[TestMethod("Return an ImageSource from a Uri input")]
	public void GetAsImageSourceAsync_InputIsUri_ReturnImageSource_FromWeb()
	{
		// Act
		ImageSource? imageSource = _imageCache.GetAsImageSourceAsync(_imageUri).Result;


		// Assert: Returned ImageSource
		Assert.IsNotNull(imageSource);

		// Assert: Can create an Image from the ImageSource
		Image image = new()
		{
			Source = imageSource
		};
		Assert.IsNotNull(image);

		// Assert: Cached file is expected size
		FileInfo cachedFileInfo = new (Path.Combine(_imageCache.ImageCachePath, GetFilename.FromUri(_imageUri)));
		Assert.IsTrue(cachedFileInfo.Exists);
		Assert.AreEqual(expectedFilesize, cachedFileInfo.Length);
	}


	[TestMethod("Return an ImageSource from already cached Uri input")]
	public void GetAsImageSourceAsync_InputIsUri_ReturnImageSource_FromCache()
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
		ImageSource? imageSource = _imageCache.GetAsImageSourceAsync(_imageUri).Result;

		// Assert: Returned ImageSource
		Assert.IsNotNull(imageSource);

		// Assert: Can create an Image from the ImageSource
		Image image = new()
		{
			Source = imageSource	
		};
		Assert.IsNotNull(image);

		// Assert: Cached file is expected size
		FileInfo cachedFileInfo = new (Path.Combine(_imageCache.ImageCachePath, GetFilename.FromUri(_imageUri)));
		Assert.IsTrue(cachedFileInfo.Exists);
		Assert.AreEqual(expectedFilesize, cachedFileInfo.Length);
	}


	[TestMethod("Return ArgumentNullException if Uri is null.")]
	public void GetAsImageSourceAsync_InputIsNull_ThrowsArgumentNullException() =>
		Assert.ThrowsExceptionAsync<ArgumentNullException>(() => _imageCache.GetAsImageSourceAsync(null!));
}