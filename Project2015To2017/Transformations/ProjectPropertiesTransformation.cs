using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Project2015To2017.Definition;

namespace Project2015To2017.Transformations
{
	internal sealed class ProjectPropertiesTransformation : ITransformation
	{
		public Task TransformAsync(XDocument projectFile, DirectoryInfo projectFolder, Project definition, Settings settings)
		{
			XNamespace nsSys = "http://schemas.microsoft.com/developer/msbuild/2003";
			var propertyGroups = projectFile.Element(nsSys + "Project").Elements(nsSys + "PropertyGroup");

			var unconditionalPropertyGroups = propertyGroups.Where(x => x.Attribute("Condition") == null).ToList();
			if (unconditionalPropertyGroups == null)
			{
				throw new NotSupportedException("No unconditional property group found. Cannot determine important properties like target framework and others.");
			}
			
			// process unconditional property groups

			var targetFramework = unconditionalPropertyGroups.Elements(nsSys + "TargetFrameworkVersion").FirstOrDefault()?.Value;

			definition.Optimize = "true".Equals(unconditionalPropertyGroups.Elements(nsSys + "Optimize").FirstOrDefault()?.Value, StringComparison.OrdinalIgnoreCase);
			definition.TreatWarningsAsErrors = "true".Equals(unconditionalPropertyGroups.Elements(nsSys + "TreatWarningsAsErrors").FirstOrDefault()?.Value, StringComparison.OrdinalIgnoreCase);
			definition.AllowUnsafeBlocks = "true".Equals(unconditionalPropertyGroups.Elements(nsSys + "AllowUnsafeBlocks").FirstOrDefault()?.Value, StringComparison.OrdinalIgnoreCase);

			definition.RootNamespace = unconditionalPropertyGroups.Elements(nsSys + "RootNamespace").FirstOrDefault()?.Value;
			definition.AssemblyName = unconditionalPropertyGroups.Elements(nsSys + "AssemblyName").FirstOrDefault()?.Value;

			definition.SignAssembly = "true".Equals(unconditionalPropertyGroups.Elements(nsSys + "SignAssembly").FirstOrDefault()?.Value, StringComparison.OrdinalIgnoreCase);
			definition.AssemblyOriginatorKeyFile = unconditionalPropertyGroups.Elements(nsSys + "AssemblyOriginatorKeyFile").FirstOrDefault()?.Value;

			// Ref.: https://www.codeproject.com/Reference/720512/List-of-Visual-Studio-Project-Type-GUIDs
			if (unconditionalPropertyGroups.Elements(nsSys + "TestProjectType").Any() || 
				unconditionalPropertyGroups.Elements(nsSys + "ProjectTypeGuids").Any(e => e.Value.IndexOf("3AC096D0-A1C2-E12C-1390-A8335801FDAB", StringComparison.OrdinalIgnoreCase) > -1))
			{
				definition.Type = ApplicationType.TestProject;
			}
			else
			{
				definition.Type = ToApplicationType(unconditionalPropertyGroups.Elements(nsSys + "OutputType").FirstOrDefault()?.Value);
			}

			if (targetFramework != null)
			{
				definition.TargetFrameworks = new[] { ToTargetFramework(targetFramework) };
			}
			

			//process conditional property groups

			definition.ConditionalPropertyGroups = settings.IsMigrateToNetStandard 
				? ProcessConditionalPropertyGroups(propertyGroups.Where(x => x.Attribute("Condition") != null))
				: propertyGroups.Where(x => x.Attribute("Condition") != null).ToArray();
			
			if (definition.Type == ApplicationType.Unknown)
			{
				throw new NotSupportedException("Unable to parse output type.");
			}

			return Task.CompletedTask;
		}

		private IReadOnlyList<XElement> ProcessConditionalPropertyGroups(IEnumerable<XElement> elements)
		{
			List<XElement> ret = new List<XElement>();
			var buildConfigurationElements = elements.Where(IsBuildConfigurationElement).ToList();
			var otherElements = elements.Except(buildConfigurationElements);
			ret.AddRange(otherElements);
			ret.AddRange(ProcessBuildConfigurationElements(buildConfigurationElements));
			return ret;
		}

		private bool IsBuildConfigurationElement(XElement element)
		{
			/*<PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Debug|AnyCPU' ">
				<PlatformTarget>AnyCPU</PlatformTarget>
				<DebugSymbols>true</DebugSymbols>
				<DebugType>full</DebugType>
				<Optimize>false</Optimize>
				<OutputPath>bin\Debug\</OutputPath>
				<DefineConstants>DEBUG;TRACE</DefineConstants>
				<ErrorReport>prompt</ErrorReport>
				<WarningLevel>4</WarningLevel>
				<Prefer32Bit>false</Prefer32Bit>
			  </PropertyGroup>	 
			 */
			string[] conditionValues = new[]
			{
				"Debug",
				"Release",
				"AnuCPU"
			};

			string[] childrenNames = new[] {
				"PlatformTarget",
				"DebugSymbols",
				"DebugType",
				"Optimize",
				"OutputPath",
				"DefineConstants",
				"ErrorReport",
				"WarningLevel",
				"Prefer32Bit"
			};

			bool hasPlatformInCondition = conditionValues.Any(v => element.Attribute("Condition").Value.Contains(v));
			bool hasBuildConfigurationChildren = childrenNames.Any(n => element.Descendants().Any(d => d.Name == XName.Get(n, "http://schemas.microsoft.com/developer/msbuild/2003")));


			return hasPlatformInCondition && hasBuildConfigurationChildren;
		}

		private IEnumerable<XElement> ProcessBuildConfigurationElements(IEnumerable<XElement> elements)
		{
			foreach (var elt in elements)
			{
				elt.Add(new XElement(XName.Get("NoWarn", "http://schemas.microsoft.com/developer/msbuild/2003"), "1701;1702;1705;1591"));
			}
			return elements;
		}

		private string ToTargetFramework(string targetFramework)
		{
			if (targetFramework.StartsWith("v", StringComparison.OrdinalIgnoreCase))
			{
				return "net" + targetFramework.Substring(1).Replace(".", string.Empty);
			}

			throw new NotSupportedException($"Target framework {targetFramework} is not supported.");
		}

		private ApplicationType ToApplicationType(string outputType)
		{
			if (string.IsNullOrWhiteSpace(outputType))
			{
				return ApplicationType.Unknown;
			}

			switch (outputType.ToLowerInvariant())
			{
				case "exe": return ApplicationType.ConsoleApplication;
				case "library": return ApplicationType.ClassLibrary;
				case "winexe": return ApplicationType.WindowsApplication;
				default: throw new NotSupportedException($"OutputType {outputType} is not supported.");
			}
		}
	}
}
