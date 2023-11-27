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
public class ImageCache_GetImageAsStreamAsync_Should
{
	private Uri _imageUri = new Uri("https://ppdac.ltd/wp-content/uploads/2020/10/logo@2x-1.png");
	private const string _deployedFileName = "11250d6b-4460-eda2-bfea-9022169a5e88";
	private ImageCache _imageCache;

	public ImageCache_GetImageAsStreamAsync_Should()
	{
		_imageCache = new ImageCache();
		ImageCache.ImageCachePath = @"..\Out\CachedItems\GetImageAsStreamAsync";
	}

	[TestMethod("Get bytes of image from Uri and return a Stream of byte[]")]
	public void GetImageAsStreamAsync_InputIsUri_ReturnStream()
	{
		string cachedFilePath = Path.Combine(ImageCache.ImageCachePath, _deployedFileName);
		using Stream stream = _imageCache.GetImageAsStreamAsync(_imageUri).Result;
		long deployedFileSize = new FileInfo(Path.Combine(ImageCache.ImageCachePath, _deployedFileName)).Length;

		Assert.AreEqual(deployedFileSize, stream.Length);

		Assert.IsTrue(File.Exists(cachedFilePath));
	}


	[TestMethod("Throws ArgumentNullException if input is null.")]
	public void GetImageAsStreamAsync_InputIsNull_ThrowArgumentNullException() =>
		Assert.ThrowsExceptionAsync<ArgumentNullException>(() => _imageCache.GetImageAsStreamAsync(null!));
}