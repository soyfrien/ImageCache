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
public class ImageCache_AsFuncStreamAsync_Should
{
	private readonly ImageCache _imageCache;
	private readonly Uri _imageUri = new ("https://ppdac.ltd/wp-content/uploads/2020/10/logo@2x-1.png");
	private const string _deployedFileName = "11250d6b-4460-eda2-bfea-9022169a5e88";
	private readonly long expectedFilesize = new FileInfo(_deployedFileName).Length;

	public ImageCache_AsFuncStreamAsync_Should()
	{
		_imageCache = new ImageCache();
		ImageCache.ImageCachePath = @"..\Out\CachedItems\AsFuncStreamAsync";
	}


	[TestMethod("Return a Func<Stream> of byte[] from a Uri input.")]
	public void AsFuncStreamAsync_InputIsUri_ReturnsFuncOfStream_FromCache()
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
		Func<Stream> funcStream = _imageCache.AsFuncStreamAsync(_imageUri).Result;
		byte[] bytes = new byte[funcStream().Length];
		funcStream().Read(bytes, 0, bytes.Length);


		// Assert
		Assert.AreEqual(expectedFilesize, bytes.Length);
	}


	[TestMethod("Return a Func<Stream> of byte[] from a Uri input.")]
	public void AsFuncStreamAsync_InputIsUri_ReturnsFuncOfStream_FromWeb()
	{
		// Arrange: Ensure cached file does not exist.
		_imageCache.Clear();
		string filename = ImageCache.GetFilename(_imageUri);
		string createdFilePath = Path.Combine(ImageCache.ImageCachePath, filename);
		//Create the directory if it does not exist.
		if (!Directory.Exists(ImageCache.ImageCachePath))
			Directory.CreateDirectory(ImageCache.ImageCachePath);
		// Delete the file if it exists.
		if (File.Exists(createdFilePath))
			File.Delete(createdFilePath);
		Assert.IsFalse(File.Exists(createdFilePath));


		// Act
		Func<Stream> funcStream = _imageCache.AsFuncStreamAsync(_imageUri).Result;
		byte[] bytes = new byte[funcStream().Length];
		funcStream().Read(bytes, 0, bytes.Length);


		// Assert
		Assert.AreEqual(expectedFilesize, bytes.Length);
	}

	
	[TestMethod("Return ArgumentNullException if Uri is null.")]
	public void AsFuncStreamAsync_InputIsNull_ReturnsNull() =>
		Assert.ThrowsExceptionAsync<ArgumentNullException>(() => _imageCache.AsFuncStreamAsync(null!));
}