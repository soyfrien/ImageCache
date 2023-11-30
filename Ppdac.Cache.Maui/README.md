# ImageCache
This library allows one to save bandwidth or API calls by caching images locally in a cache folder.

For .NET MAUI apps, the location of this folder is decided by [FileSystem.CacheDirectory](https://learn.microsoft.com/en-us/dotnet/api/microsoft.maui.storage.filesystem.cachedirectory), 
and for .NET 6.0 Windows apps, it is decided by [Environment.SpecialFolder.ApplicationData](https://docs.microsoft.com/en-us/dotnet/api/system.environment.specialfolder?view=net-6.0#System_Environment_SpecialFolder_ApplicationData).

This version for .NET MAUI apps adds a new method to the `ImageCache` class, `GetAsImageSourceAsync(Uri uri)`, which returns an `ImageSource` instead of a `Stream`, `byte[]` or `Func<Stream>`.
This is useful for binding to an `Image` control's `Source` property, which are often given a URI to a web resource.

In place of the URI, use `GetAsImageSourceAsync(Uri uri)` instead, which will remember if this URI has been cached before, and if so, return the cached image source instead of downloading it again.

Because it is derived from the general-purpose `ImageCache` class, it can also be used for other classes not specific to .NET MAUI, such as Bitmaps, or other image types that can accept
byte arrays or streams.

# Usage
Let's say you have an Image control that you currently pass a URL string into the `Source` property, instead of passing the string do something like:
```...
Source = await ImageCache.GetAsImageSourceAsync("https://www.example.com/image.png");
// or ideally:
Source = await ImageCache.GetAsImageSourceAsync(new Uri("https://www.example.com/image.png"));

// Or use the ImageSource type directly:
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

## Advanced Usage
You may only want to use it on certain pages. There are several ways to do this, including with dependcy injection, as well as changing the default cache folder to whatever you like: 
```
// Page A
_imageCache.ImageCachePath = nameof(MyPage);

// Page B
_imageCacheB.ImageCachePath = nameof(MyOtherPage);
```

Though, DI is probably the best way to do this, injecting the class into the desired page.


# Contibuting
You are actively encouraged to report bugs and contribute to this repository.

Contributions Are Appreciated and Welcome
--------------------------------------------
* If you want to improve this library please make a pull request to: https://github.com/soyfrien/ImageCache/pulls

Bugs and Issues
--------------------------------------------
* Please report any issues you find, old or new: https://github.com/soyfrien/ImageCache/issues

License Awareness
--------------------------------------------
You should be aware of the license of all required or optional Nuget dependencies including .NET libraries published on nuget.org or elsewhere including:
    * Microsoft.Maui.Controls under MIT licensing (like this is).
    * Microsoft.Maui.Storage under MIT licensing.