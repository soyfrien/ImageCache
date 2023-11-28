using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ppdac.Cache.Tests;

[TestClass()]
public class ImageCache_Report_Should
{
	private readonly ImageCache _imageCache = new();
	// The following three items total to 124 KB (127,934 bytes)
	private readonly List<Uri> _imageUris = [
		new Uri("https://ppdac.ltd/wp-content/uploads/2020/04/logo1-dark@2x-1.png"),
		new Uri("https://ppdac.ltd/wp-content/uploads/2020/10/logo@2x-1.png"),
		new Uri("https://ppdac.ltd/wp-content/uploads/2021/09/cloud-storage-applications-internet-6621829-e1651375167197-1024x678.jpg")
	];

	public ImageCache_Report_Should()
	{
		_imageCache.ImageCachePath = @"..\Out\CachedItems\Report";
	}

	[TestMethod()]
	public void Report_ReturnTaskString()
	{
		foreach (Uri _imageUri in _imageUris)
			_imageCache.KeepAsync(_imageUri).Wait();

		Task<string> task = _imageCache.Report();
		Assert.AreEqual("3 items in cache (124 KiB).", task.Result);

		ImageCache.Clear();

		task = _imageCache.Report();
		Assert.AreEqual("3 items in cache (124 KiB).", task.Result);

		_imageCache.Purge();

		task = _imageCache.Report();
		Assert.AreEqual("0 items in cache (0 bytes).", task.Result);
	}
}