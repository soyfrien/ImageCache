using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ppdac.Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ppdac.Cache.Tests;

[TestClass()]
[DeploymentItem(@".\Deployables\11250d6b-4460-eda2-bfea-9022169a5e88")]
public class ImageCache_GetAsImageSourceAsync_Should
{
	private readonly ImageCache _imageCache;
	private readonly Uri _imageUri = new ("https://ppdac.ltd/wp-content/uploads/2020/10/logo@2x-1.png");
	private const string _deployedFileName = "11250d6b-4460-eda2-bfea-9022169a5e88";
	readonly long expectedFilesize = new FileInfo(_deployedFileName).Length;

	public ImageCache_GetAsImageSourceAsync_Should()
	{
		_imageCache = new ImageCache();
		ImageCache.ImageCachePath = @"..\Out\CachedItems\GetImageSourceAsync";
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
		FileInfo cachedFileInfo = new (Path.Combine(ImageCache.ImageCachePath, ImageCache.GetFilename(_imageUri)));
		Assert.IsTrue(cachedFileInfo.Exists);
		Assert.AreEqual(expectedFilesize, cachedFileInfo.Length);
	}


	[TestMethod("Return an ImageSource from already cached Uri input")]
	public void GetAsImageSourceAsync_InputIsUri_ReturnImageSource_FromCache()
	{
		// Arrange: Ensure cached file exists, and get its number of bytes.
		Assert.IsTrue(new FileInfo(_deployedFileName).Exists);
		string filename = ImageCache.GetFilename(_imageUri);
		string createdFilePath = Path.Combine(ImageCache.ImageCachePath, filename);

		//Create the directory if it does not exist.
		if (!Directory.Exists(ImageCache.ImageCachePath))
			Directory.CreateDirectory(ImageCache.ImageCachePath);

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
		FileInfo cachedFileInfo = new (Path.Combine(ImageCache.ImageCachePath, ImageCache.GetFilename(_imageUri)));
		Assert.IsTrue(cachedFileInfo.Exists);
		Assert.AreEqual(expectedFilesize, cachedFileInfo.Length);
	}


	[TestMethod("Return ArgumentNullException if Uri is null.")]
	public void GetAsImageSourceAsync_InputIsNull_ThrowsArgumentNullException() =>
		Assert.ThrowsExceptionAsync<ArgumentNullException>(() => _imageCache.GetAsImageSourceAsync(null!));
}