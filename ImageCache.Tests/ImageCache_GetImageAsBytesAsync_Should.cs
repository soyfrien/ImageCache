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
public class ImageCache_GetImageAsBytesAsync_Should
{
	// The URI of _imageUri will generate the following Guid filename: 11250d6b-4460-eda2-bfea-9022169a5e88
	private readonly Uri _imageUri = new ("https://ppdac.ltd/wp-content/uploads/2020/10/logo@2x-1.png");
	private const string _deployedFileName = "11250d6b-4460-eda2-bfea-9022169a5e88";

	public ImageCache_GetImageAsBytesAsync_Should() =>
		ImageCache.ImageCachePath = @"..\Out\CachedItems\GetImageAsBytesAsync";


	[TestInitialize]
	/// <summary>
	/// Ensure cache folder exists and is empty.
	///	</summary>
	public void TestInitialize()
	{
		// The cache folder should exist
		if (!Directory.Exists(ImageCache.ImageCachePath))
			Directory.CreateDirectory(ImageCache.ImageCachePath);

		// But it should be empty
		if (File.Exists(Path.Combine(ImageCache.ImageCachePath, _deployedFileName)))
			File.Delete(Path.Combine(ImageCache.ImageCachePath, _deployedFileName));
	}


	[TestMethod("Return a byte array of the image from the web.")]
	public void GetImageAsBytesAsync_InputIsUri_ReturnByteArray_FromWeb()
	{
		//Arrange
		long expectedFilesize = new FileInfo(_deployedFileName).Length;
		Assert.IsFalse(File.Exists(Path.Combine(ImageCache.ImageCachePath, _deployedFileName)));
		

		//Act
		byte[]? imageBytes = ImageCache.GetImageAsBytesAsync(_imageUri).Result;


		//Assert
		Assert.IsFalse(File.Exists(Path.Combine(ImageCache.ImageCachePath, _deployedFileName)));
		Assert.IsNotNull(imageBytes);
		Assert.AreEqual(expectedFilesize, imageBytes.Length);
	}

	[TestMethod("Return a byte array of the image from the cache.")]
	public async Task GetImageAsBytesAsync_InputIsUri_ReturnByteArray_FromCache()
	{
		//Arrange
		Assert.IsFalse(File.Exists(Path.Combine(ImageCache.ImageCachePath, _deployedFileName)));
		await ImageCache.KeepAsync(_imageUri);
		Assert.IsTrue(File.Exists(Path.Combine(ImageCache.ImageCachePath, _deployedFileName)));

		long expectedFilesize = new FileInfo(_deployedFileName).Length;

		//Act
		byte[]? imageBytes = await ImageCache.GetImageAsBytesAsync(_imageUri);

		//Assert
		Assert.IsNotNull(imageBytes);
		Assert.AreEqual(expectedFilesize, imageBytes.Length);
	}


	[TestMethod("Return a byte array of the image from the web using string url.")]
	public async Task GetImageAsBytesAsync_InputIsString_ReturnByteArray_FromWeb()
	{
		//Arrange
		long expectedFilesize = new FileInfo(_deployedFileName).Length;
		Assert.IsFalse(File.Exists(Path.Combine(ImageCache.ImageCachePath, _deployedFileName)));


		//Act
		byte[]? imageBytes = await ImageCache.GetImageAsBytesAsync(_imageUri.AbsoluteUri);


		//Assert
		Assert.IsFalse(File.Exists(Path.Combine(ImageCache.ImageCachePath, _deployedFileName)));
		Assert.IsNotNull(imageBytes);
		Assert.AreEqual(expectedFilesize, imageBytes.Length);
	}

	[TestMethod("Return a byte array of the image from the cache using string url.")]
	public async Task GetImageAsBytesAsync_InputIsString_ReturnByteArray_FromCache()
	{
		//Arrange
		Assert.IsFalse(File.Exists(Path.Combine(ImageCache.ImageCachePath, _deployedFileName)));
		await ImageCache.KeepAsync(_imageUri);
		Assert.IsTrue(File.Exists(Path.Combine(ImageCache.ImageCachePath, _deployedFileName)));

		long expectedFilesize = new FileInfo(_deployedFileName).Length;

		//Act
		byte[]? imageBytes = await ImageCache.GetImageAsBytesAsync(_imageUri.AbsoluteUri);
		
		//Assert
		Assert.IsNotNull(imageBytes);
		Assert.AreEqual(expectedFilesize, imageBytes.Length);
	}


	[TestMethod("Throws ArgumentNullException if input is null.")]
	public void GetImageAsBytesAsync_InputIsNull_ThrowArgumentNullException() =>
		Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await ImageCache.GetImageAsBytesAsync(new Uri(null!)));


	[TestMethod("Throws ArgumentNullException for empty string")]
	public void GetImageAsBytesAsync_InputIsEmptyString_ThrowArgumentNullException() =>
		Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await ImageCache.GetImageAsBytesAsync(string.Empty));
}