using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Ppdac.ImageCache;
public interface ICacheStore
{
	public static string _ImageCachePath = $"{FileSystem.Current.CacheDirectory}\\Images";
	public static string _CacheFile = "ppdac.imagecache.data";
	private static readonly HashAlgorithm s_sha256 = SHA256.Create();
	private static readonly Dictionary<string, byte[]> s_imageStore = [];
	
	
	public abstract static Task<string> Save();
	public abstract static Task<string> Restore();
	public abstract static Task<string> Purge();
}
