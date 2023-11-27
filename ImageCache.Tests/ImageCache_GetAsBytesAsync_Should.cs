using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ppdac.Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Ppdac.Cache.Tests;


[TestClass]
[DeploymentItem(@".\Deployables\11250d6b-4460-eda2-bfea-9022169a5e88")]
public class ImageCache_GetAsBytesAsync_Should
{
	private readonly ImageCache _imageCache;// = new ();
	// The URI of _imageUri will generate the following Guid filename: 11250d6b-4460-eda2-bfea-9022169a5e88
	private readonly Uri _imageUri = new ("https://ppdac.ltd/wp-content/uploads/2020/10/logo@2x-1.png");
	private const string _deployedFileName = "11250d6b-4460-eda2-bfea-9022169a5e88";
	private readonly long _expectedFilesize = new FileInfo(_deployedFileName).Length;


	public ImageCache_GetAsBytesAsync_Should()
	{
		_imageCache = new ();
		ImageCache.ImageCachePath = @"..\Out\CachedItems\GetBytesAsync";
	}


	[TestMethod("Return the number of bytes of a cached file.")]
	public void GetAsBytesAsync_InpuptIsUri_ReturnNumberOfBytes_FromStorage()
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
		byte[] bytes = ImageCache.GetAsBytesAsync(_imageUri).Result;


		// Assert
		Assert.AreEqual(_expectedFilesize, bytes.Length);
	}


	[TestMethod("Return the number of bytes of a web resource.")]
	public void GetAsBytesAsync_InpuptIsUri_ReturnNumberOfBytes_FromWeb()
	{
		// Arrange: Ensure cached file does not exist.
		string filename = ImageCache.GetFilename(_imageUri);
		string createdFilePath = Path.Combine(ImageCache.ImageCachePath, filename);
		if (File.Exists(createdFilePath))
			File.Delete(createdFilePath);
		Assert.IsFalse(File.Exists(createdFilePath));


		// Act
		byte[] bytes = ImageCache.GetAsBytesAsync(_imageUri).Result;


		// Assert
		Assert.AreEqual(_expectedFilesize, bytes.Length);
	}


	[TestMethod("Return ArgumentNullException if Uri is null.")]
	public void GetAsBytesAsync_InputIsNull_ThrowsArgumentNullException() =>
		Assert.ThrowsExceptionAsync<ArgumentNullException>(() => ImageCache.GetImageAsBytesAsync(new Uri(null!)));


	[TestMethod("Throw ArgumentNullException for empty string.")]
	public async Task GetNumberOfBytesAsync_InputIsEmptyString_ThrowArgumentNullException() =>
		await Assert.ThrowsExceptionAsync<UriFormatException>(
			async () => await ImageCache.GetImageAsBytesAsync(" "));


	[TestMethod("Return ArgumentNullException for empty string.")]
	public void GetAsBytesAsync_InputIsEmptyString_ThrowsArgumentNullException() =>
		Assert.ThrowsExceptionAsync<ArgumentNullException>(() => ImageCache.GetImageAsBytesAsync(string.Empty));
}