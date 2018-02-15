using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project2015To2017
{
	public class Settings
	{
		public bool IsMigrateToNetStandard { set; get; }
		public bool IsUseAssemblyInfoFile { set; get; }
		public bool IsUseVsPsccFileToConfigureVersionControl { set; get; }
		public bool IsDeleteOldFilesExceptProject { set; get; }
		public bool IsDeleteTfsSettingsFile { set; get; }
		public bool IsEnableDefaultCompileItems { set; get; }

		/// <summary>
		/// Gets or sets a value indicating whether <c>Version</c> and <c>FileVersion</c> Property nodes should be generated during poject write.
		/// </summary>
		/// <value>
		///   <c>true</c> if <c>Version</c> and <c>FileVersion</c> Property nodes should be generated during poject write; otherwise, <c>false</c>.
		/// </value>
		public bool IsGenerateVersionsElements { set; get; }
	}
}
