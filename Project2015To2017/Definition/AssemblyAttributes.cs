using System.Collections.Generic;

namespace Project2015To2017.Definition
{
	internal sealed class AssemblyAttributes
	{
		public string Title { get; set; }
		public string Company { get; set; }
		public string Product { get; set; }
		public string Copyright { get; set; }
		public string InformationalVersion { get; set; }
		public string Version { get; set; }
		public string AssemblyName { get; set; }
		public string Description { get; set; }
		public string Configuration { get; internal set; }
		public string FileVersion { get; internal set; }
		
		public IDictionary<string, string> GetAttributes(bool includeVersionNodes)
		{
			Dictionary<string,string> ret = new Dictionary<string, string>(){
				["Title"] = Title,
				["Company"] = Company,
				["Product"] = Product,
				["Copyright"] = Copyright,
				["InformationalVersion"] = InformationalVersion,
				["Version"] = Version,
				["FileVersion"] = FileVersion,
				["AssemblyName"] = AssemblyName,
				["Description"] = Description,
				["Configuration"] = Configuration,
				["Author"] = Company
			};
			if (!includeVersionNodes)
			{
				ret.Remove("Version");
				ret.Remove("FileVersion");
			}
			return ret;
		}
	}
}
