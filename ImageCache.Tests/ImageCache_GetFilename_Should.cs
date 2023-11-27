namespace Ppdac.Cache.Tests;

[TestClass]
public class ImageCache_GetFilename_Should
{
	private readonly ImageCache _imageCache;
	private readonly List<Uri> _goodUris = [
			new Uri("https://ppdac.ltd/wp-content/uploads/2020/04/logo1-dark@2x-1.png"),
			new Uri("https://ppdac.ltd/wp-content/uploads/2020/10/logo@2x-1.png"),
			new Uri("https://ppdac.ltd/wp-content/uploads/2021/09/cloud-storage-applications-internet-6621829-e1651375167197-1024x678.jpg")
		];


	public ImageCache_GetFilename_Should() => _imageCache = new ImageCache();


	[TestMethod("Generate a Guid from the first 16 bytes of a Uri input.")]
	public void GetFilename_InputIsUri_ReturnGuid()
	{
		foreach (Uri uri in _goodUris)
		{
			string resultingString = ImageCache.GetFilename(uri);
			Guid guid = Guid.Parse(resultingString);
			Assert.IsNotNull(guid);
		}
	}


	[TestMethod("Throws exception for malformed Uri.")]
	public void GetFilename_InputIsMalformedUri_ThrowUriFormatException() =>
		Assert.ThrowsException<UriFormatException>(() => ImageCache.GetFilename(new Uri("https:/example.com")));


	[TestMethod("Throw ArgumentNullException if input is null.")]
	public void GetFilename_InputIsNull_ThrowArgumentNullException() =>
		Assert.ThrowsException<ArgumentNullException>(() => ImageCache.GetFilename(null!));
}