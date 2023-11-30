# ImageCache
-----------------------------------------
This library allows one to save bandwidth or API calls by caching images locally in a cache folder.

## Video Demonstration
![Demonstration video showing load times.](https://github.com/soyfrien/ImageCache/raw/main/ImageCacheDemo.mp4)

*Notice the instant load times for this CollectionView of web images.*


For .NET MAUI apps, the location of this folder is decided by [FileSystem.CacheDirectory](https://learn.microsoft.com/en-us/dotnet/api/microsoft.maui.storage.filesystem.cachedirectory), 
and for .NET 6.0 Windows apps, it is decided by [Environment.SpecialFolder.ApplicationData](https://docs.microsoft.com/en-us/dotnet/api/system.environment.specialfolder?view=net-6.0#System_Environment_SpecialFolder_ApplicationData).

This version for .NET MAUI apps adds a new method to the `ImageCache` class, `GetAsImageSourceAsync(Uri uri)`, which returns an `ImageSource` instead of a `Stream`, `byte[]` or `Func<Stream>`.
This is useful for binding to an `Image` control's `Source` property, which are often given a URI to a web resource.

In place of the URI, use `GetAsImageSourceAsync(Uri uri)` instead, which will remember if this URI has been cached before, and if so, return the cached image source instead of downloading it again.

Because it is derived from the general-purpose `ImageCache` class, it can also be used for other classes not specific to .NET MAUI, such as bitmaps, or other image types that can accept byte arrays or a `Stream`.

# Usage
-----------------------------------------
* You can always set the cache folder by its public property. For .NET MAUI apps, the default is the `FileSystem.CacheDirectory` folder, and for .NET 6.0 Windows apps, it is the `Environment.SpecialFolder.ApplicationData` folder.

* Use the **Purg()** method to clear the cache, the **GetAs...(Uri)** methods to retrieve images, and **GetByteCount(Uri)** if you need to check a remote images' filesize.
* Some methods will let you pass in a string URL for convenience, but using Uris is preferred.
* The **Save()** method does not need to be called. It will make sure the list of tracked Uris are saved to the cache. You may want to call if if you use **Clear()** on the list.
* The **Restore()** method is obsolete, and does not need to be called.

## Examples
### Dependency Injection
Use this class to cache images from the Internet. Its functions receive a URI and turn the resource into an `ImageSource`, byte array, `Stream`, or `Func‹Stream›`. Where ever you would give a control a URL, a stream, or byte[], do so as normal, but have ImageCache sit in the middle. For example, in .NET MAUI:
```
// MauiProgram.cs:
...
builder.Services.AddSingleton‹ImageCache›();
...
```
Then use Dependency Injection to gain access to the class of a view, viewmodel, page or control. Assume a viewmodel has `Collection‹Image› Images` that will be used in a template:
```
Using Ppdac.ImageCache.Maui;
ImageCache _imageCache;
...
Page(ViewModel viewModel, ImageCache imageCache)
{
	_imageCache = imageCache;
	foreach (Image image in viewModel.Images)
		Image.Source = _imageCache.GetImageAsImageSource(image.Url);
	
	Stream imageStream = await _imageCache.GetImageAsStreamAsync(image.Url);
	Bitmap bitmap = new(imageStream);
	
	byte[] imageBytes = await _imageCache.GetImageAsBytesAsync(image.Url);
	...
```
### Helper Helping Out However Examples
You have an [Microsoft.Maui.Controls.Image](https://learn.microsoft.com/en-us/dotnet/maui/user-interface/controls/image) that you currently pass a URL string into the `Source` property, instead of passing the string do something like:
```
Source = await ImageCache.GetAsImageSourceAsync("https://www.example.com/image.png");
// or ideally:
Source = await ImageCache.GetAsImageSourceAsync(new Uri("https://www.example.com/image.png"));
```

Or use the ImageSource type directly:
```
ImageSource imageSource = await ImageCache.GetAsImageSourceAsync(imageUri);
Microsoft.Maui.Controls.Image mmcImage = new Microsoft.Maui.Controls.Image
{
	Source = imageSource
};
```

It also works with controls that expect a byte array or stream, `System.Drawing.Image`, or the `Bitmap` class:
```
System.Drawing.Bitmap sdBitmap = new Bitmap(await _imageStore.GetAsStreamAsync(uri));

// If you prefer synchronous methods:
System.Drawing.Image sdImage = System.Drawing.Image.FromStream(_imageStore.GetAsStreamAsync(uri).Result);

// Or just set up your byte array, and you can use these anywhere!
byte[] imageBytes = await _imageStore.GetAsByteArrayAsync(uri);
```

As you can see, you are simply establishing a source for the image, but having a helper function sit right in the middle.

### Other Usage
You may only want to use it on certain pages. There are several ways to do this, including with dependcy injection, as well as changing the default cache folder to whatever you like: 
```
// Page A
_imageCache.ImageCachePath = nameof(MyPage);

// Page B
_imageCacheB.ImageCachePath = nameof(MyOtherPage);
```

Though, DI is probably the best way to do this, injecting the class into the desired page.


# Contibuting
-----------------------------------------
You are actively encouraged to report bugs and contribute to this repository
 
Contributions Are Appreciated and Welcome

* If you want to improve this library please make a pull request to: https://github.com/soyfrien/ImageCache/pulls

Bugs and Issues
--------------------------------------------
* Please report any issues you find, old or new: https://github.com/soyfrien/ImageCache/issues.