# ImageCache
This library allows one to save bandwidth or API calls by caching images locally in a cache folder.

For .NET MAUI apps, the location of this folder is decided by [FileSystem.CacheDirectory](https://learn.microsoft.com/en-us/dotnet/api/microsoft.maui.storage.filesystem.cachedirectory), 
and for .NET 6.0 Windows apps, it is decided by [Environment.SpecialFolder.ApplicationData](https://docs.microsoft.com/en-us/dotnet/api/system.environment.specialfolder?view=net-6.0#System_Environment_SpecialFolder_ApplicationData).

In place of the URI, use `GetAsImageSourceAsync(Uri uri)` instead, which will remember if this URI has been cached before, and if so, return the cached image source instead of downloading it again.

Because it is derived from the general-purpose `ImageCache` class, it can also be used for other classes not specific to .NET MAUI, such as Bitmaps, or other image types that can accept
byte arrays or streams.

For use with types that use an `ImageSource`, you can use the derived `ImageCache` class in [Ppdac.Cache.Maui](#) use `GetAsImageSourceAsync(Uri uri)` instead.

# Usage
Todo: learn the new GitHub usage thingy.

# Contibuting
You are actively encouraged to report bugs and contribute to this repository.

Contributions Are Appreciated and Welcome
--------------------------------------------
* If you want to improve this library:
   - Please make a [pull request](https://dev.azure.com/ppdac/Ppdac.Cache/_git/ImageCache.Maui/pullrequests).
* Contributions to this repository, any Fork, or copy remains with PPDAC LTD under the MIT license terms.
* You may add a note to any significant code contributions you have made, but may not copyright or patent your changes.
    * You may not modify licensing or copyright of any part or any derivative work (including a fork) of this software in any way without formal written consent.

Bugs and Issues
--------------------------------------------
* Check the issue log in case your issue is already documented.
* To make a Pull Request: https://dev.azure.com/ppdac/Ppdac.Cache/_git/ImageCache.Maui/pullrequests
* To report an issue: https://github.com/ppdac/Ppdac.ImageCache.Maui/issues

License Awareness
--------------------------------------------
You should be aware of the license of all required or optional Nuget dependencies including .NET libraries published on nuget.org or elsewhere including:
    * Microsoft.Maui.Controls under MIT licensing (like this is).
    * Microsoft.Maui.Storage under MIT licensing.