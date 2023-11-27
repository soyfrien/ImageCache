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
public class ImageCache_GetFromStorageAsBytesAsync_Should
{
	private readonly ImageCache _imageCache;
	// The URI of _imageUri will generate the following Guid filename: 11250d6b-4460-eda2-bfea-9022169a5e88
	private readonly Uri _imageUri = new Uri("https://ppdac.ltd/wp-content/uploads/2020/10/logo@2x-1.png");
	private const string _deployedFileName = "11250d6b-4460-eda2-bfea-9022169a5e88";

	public ImageCache_GetFromStorageAsBytesAsync_Should()
	{
		_imageCache = new ImageCache();
		ImageCache.ImageCachePath = @"..\Out\CachedItems\GetFromStorageAsBytesAsync";
	}

	[TestMethod("Return a byte[] from the cache storage.")]
	public void GetFromStorageAsBytesAsync_InputIsUri_ReturnsByteArray()
	{
		//Arrange
		//Make sure the directory and file exist before the test.
		string filename = ImageCache.GetFilename(_imageUri);
		string createdFilePath = Path.Combine(ImageCache.ImageCachePath, filename);

		if (!Directory.Exists(ImageCache.ImageCachePath))
			Directory.CreateDirectory(ImageCache.ImageCachePath);
		Assert.IsTrue(Directory.Exists(ImageCache.ImageCachePath));

		File.Copy(_deployedFileName, createdFilePath, true);
		Assert.IsTrue(File.Exists(createdFilePath));

		//Act
		byte[] bytes = ImageCache.GetFromStorageAsBytesAsync(_imageUri).Result;

		//Assert
		Assert.AreEqual(bytes.Length, new FileInfo(createdFilePath).Length);
	}


	[TestMethod("Throws ArgumentNullException if input is null.")]
	public void GetFromStorageAsBytesAsync_InputIsNull_ThrowArgumentNullException() =>
		Assert.ThrowsExceptionAsync<ArgumentNullException>(() => ImageCache.GetFromStorageAsBytesAsync(null!));
}