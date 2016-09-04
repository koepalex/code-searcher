using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml;
using System.Xml.Serialization;

namespace CodeSearcher.WebServer
{
	public class ConfigManager
	{
		private static ConfigManager _self;
		private WebServerConfig _config;
		private Uri _uri;
		private bool _isConsistent = false;

		private ConfigManager()
		{
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public static ConfigManager Get()
		{
			if (_self == null)
			{
				_self = new ConfigManager();
			}

			return _self;
		}

		public bool TryLoadConfig()
		{
			if (_isConsistent && _config != null)
			{
				return true;
			}

			var path = GetConfigPath();

			if (File.Exists(path))
			{
				if (DeserializeConfigFile(path))
				{
					if (CheckConsistency())
					{
						Log.Get.Info("[Info] Config successfully loaded");
						return true;
					}
				}
				else
				{
					Log.Get.Error("could not deserialize CodeSearcher.WebServer config file");
				}
			}
			else
			{
				Log.Get.Error("could not find CodeSearcher.WebServer config file");
			}
			return false;
		}

		private string GetConfigPath()
		{
			return Path.Combine(
				Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
				"Config",
				"CodeSearcher.WebServer.xml");
		}

		private bool DeserializeConfigFile(string path)
		{
			var reader = new XmlSerializer(typeof(WebServerConfig));
			using (var file = XmlReader.Create(path))
			{
				if (reader.CanDeserialize(file))
				{
					_config = (WebServerConfig)reader.Deserialize(file);
				}
			}

			return _config != null;
		}

		private bool CheckConsistency()
		{
			_isConsistent = true;
			if (_config.Port <= 0)
			{
				Log.Get.Error("Config: port number is invalid");
				_isConsistent = false;
			}


			if (!Uri.TryCreate(_config.Uri + ":" + _config.Port, UriKind.Absolute, out _uri))
			{
				Log.Get.Error("Config: uri is not consistent");
				_isConsistent = false;
			}

			if (string.IsNullOrWhiteSpace(_config.IndexPath)
				|| !Directory.Exists(_config.IndexPath))
			{
				Log.Get.Error("Config: index path doesn't exist");
				_isConsistent = false;
			}

			return _isConsistent;
		}

		public Uri Uri
		{
			get
			{
				return _uri;
			}
		}

		public string IndexPath
		{
			get
			{
				return _config.IndexPath;
			}
		}
	}
}

