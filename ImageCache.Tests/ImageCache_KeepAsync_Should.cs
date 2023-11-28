namespace Ppdac.Cache.Tests;


[TestClass]
[DeploymentItem(@".\Deployables\11250d6b-4460-eda2-bfea-9022169a5e88")]
public class ImageCache_KeepAsync_Should
{
	private readonly ImageCache _imageCache;
	// The URI of _imageUri will generate the following Guid filename: 11250d6b-4460-eda2-bfea-9022169a5e88
	private readonly Uri _imageUri = new("https://ppdac.ltd/wp-content/uploads/2020/10/logo@2x-1.png");
	private const string _deployedFileName = "11250d6b-4460-eda2-bfea-9022169a5e88";
	public ImageCache_KeepAsync_Should() => _imageCache = new ImageCache();

	[TestMethod("Cache a file from a Uri to storage, with a Guid for its name (should be the same number of bytes as the deployed file).")]
	public async Task KeepAsync_InputIsUri_CreateGuidNamedFileAsync()
	{
		//Make sure the directory and file don't exist before the test.
		string filename = GetFilename.FromUri(_imageUri);
		_imageCache.ImageCachePath = @"..\Out\CachedItems\KeepAsync";
		string createdFilePath = Path.Combine(_imageCache.ImageCachePath, filename);
		Assert.IsFalse(Directory.Exists(_imageCache.ImageCachePath));
		Assert.IsFalse(File.Exists(createdFilePath));


		//Act
		await _imageCache.KeepAsync(_imageUri);

		//Make sure the directory is created if it did not exist.
		Assert.IsTrue(Directory.Exists(_imageCache.ImageCachePath));
		Assert.IsTrue(File.Exists(createdFilePath));
		//Get the filesize of the created file and the expected filesize from the deployed file.
		long filesize = new FileInfo(createdFilePath).Length;
		long expectedFilesize = new FileInfo(_deployedFileName).Length;

		//Assert: The created file should have the same filesize as the deployed file.
		Assert.AreEqual(expectedFilesize, filesize);
	}


	[TestMethod("Throws ArgumentNullException if input is null.")]
	public async Task KeepAsync_InputIsNull_ThrowsArgumentNullExceptionAsync()
	{
		//Arrange
		Uri nullUri = null!;

		//Act
		Func<Task> act = async () => await _imageCache.KeepAsync(nullUri);

		//Assert
		await Assert.ThrowsExceptionAsync<ArgumentNullException>(act);
	}
}
