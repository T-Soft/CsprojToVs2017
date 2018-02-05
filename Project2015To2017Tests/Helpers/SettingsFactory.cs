using System;
using System.Collections.Generic;
using System.Text;
using Project2015To2017;

namespace Project2015To2017Tests.Helpers
{
	public static class SettingsFactory
	{
		public static Settings Create()
		{
			return new Settings(){IsMigrateToNetStandard = false, IsUseAssemblyInfoFile = false};
		}
	}
}
