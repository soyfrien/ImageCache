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
public class ImageCache_GetNumberOfBytesAsync_Should
{
	private readonly ImageCache _imageCache;
	// The URI of _imageUri will generate the following Guid filename: 11250d6b-4460-eda2-bfea-9022169a5e88
	private readonly Uri _imageUri = new ("https://ppdac.ltd/wp-content/uploads/2020/10/logo@2x-1.png");
	private const string _deployedFileName = "11250d6b-4460-eda2-bfea-9022169a5e88";
	private readonly long _expectedFilesize = new FileInfo(_deployedFileName).Length;

	public ImageCache_GetNumberOfBytesAsync_Should()
	{
		_imageCache = new ImageCache();
		ImageCache.ImageCachePath = @"..\Out\CachedItems\GetNumberOfBytesAsync";
	}


	[TestMethod("Return filesize of web resource as an integer.")]
	public async Task GetNumberOfBytesAsync_InputIsUri_ReturnInt_FromWeb()
	{
		string cachedFilePath = Path.Combine(ImageCache.ImageCachePath, ImageCache.GetFilename(_imageUri));
		if (File.Exists(cachedFilePath))
			File.Delete(cachedFilePath);
		Assert.IsFalse(File.Exists(cachedFilePath));

		// Assert: The created file should have the same filesize as the deployed file.
		Assert.AreEqual(await _imageCache.GetNumberOfBytesAsync(_imageUri), _expectedFilesize);
	}



	[TestMethod("Return filesize of cached resource as an integer.")]
	public async Task GetNumberOfBytesAsync_InputIsUri_ReturnInt_FromCache()
	{
		//Make sure the directory and file exist before the test.
		string filename = ImageCache.GetFilename(_imageUri);
		string createdFilePath = Path.Combine(ImageCache.ImageCachePath, filename);
		if (!Directory.Exists(ImageCache.ImageCachePath))
			Directory.CreateDirectory(ImageCache.ImageCachePath);
		if (!File.Exists(createdFilePath))
			File.Copy(_deployedFileName, createdFilePath);
		Assert.IsTrue(File.Exists(createdFilePath));

		//Act
		await ImageCache.KeepAsync(_imageUri);

		//Make sure the directory is created if it did not exist.
		Assert.IsTrue(Directory.Exists(ImageCache.ImageCachePath));
		Assert.IsTrue(File.Exists(createdFilePath));

		//Assert: The created file should have the same filesize as the deployed file.
		Assert.AreEqual(await _imageCache.GetNumberOfBytesAsync(_imageUri), _expectedFilesize);
	}


	[TestMethod("Throw ArgumentNullException if the Uri is null.")]
	public async Task GetNumberOfBytesAsync_InputIsNull_ThrowArgumentNullException() =>
		await Assert.ThrowsExceptionAsync<ArgumentNullException>(
			  action: async () => await _imageCache.GetNumberOfBytesAsync(new Uri(null!)));


	[TestMethod("Return filesize of web resource as an integer.")]
	public async Task GetNumberOfBytesAsync_InputIsStringUrl_ReturnInt_FromWeb()
	{
		string cachedFilePath = Path.Combine(ImageCache.ImageCachePath, ImageCache.GetFilename(_imageUri));
		if (File.Exists(cachedFilePath))
			File.Delete(cachedFilePath);
		Assert.IsFalse(File.Exists(cachedFilePath));

		// Assert: The created file should have the same filesize as the deployed file.
		Assert.AreEqual(await _imageCache.GetNumberOfBytesAsync(_imageUri.AbsoluteUri), _expectedFilesize);
	}



	[TestMethod("Return filesize of cached resource as an integer.")]
	public async Task GetNumberOfBytesAsync_InputIsStringUrl_ReturnInt_FromCache()
	{
		//Make sure the directory and file exist before the test.
		string filename = ImageCache.GetFilename(_imageUri);
		string createdFilePath = Path.Combine(ImageCache.ImageCachePath, filename);
		if (!Directory.Exists(ImageCache.ImageCachePath))
			Directory.CreateDirectory(ImageCache.ImageCachePath);
		if (!File.Exists(createdFilePath))
			File.Copy(_deployedFileName, createdFilePath);
		Assert.IsTrue(File.Exists(createdFilePath));

		//Act
		await ImageCache.KeepAsync(_imageUri);

		//Make sure the directory is created if it did not exist.
		Assert.IsTrue(Directory.Exists(ImageCache.ImageCachePath));
		Assert.IsTrue(File.Exists(createdFilePath));

		//Assert: The created file should have the same filesize as the deployed file.
		Assert.AreEqual(await _imageCache.GetNumberOfBytesAsync(_imageUri.AbsoluteUri), _expectedFilesize);
	}


	[TestMethod("Throw ArgumentNullException for empty string.")]
	public async Task GetNumberOfBytesAsync_InputIsEmptyString_ThrowArgumentNullException() =>
		await Assert.ThrowsExceptionAsync<UriFormatException>(
			action: async () => await _imageCache.GetNumberOfBytesAsync(" "));
}