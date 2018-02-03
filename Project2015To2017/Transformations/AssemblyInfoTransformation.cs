using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Project2015To2017.Definition;
using Project2015To2017.Utilities;

namespace Project2015To2017.Transformations
{
	internal sealed class AssemblyInfoTransformation : ITransformation
	{
		private readonly Dictionary<string, Type > _attributeTypesByNames = new Dictionary<string, Type>(){
			["AssemblyDescription"] = typeof(AssemblyDescriptionAttribute),
			["AssemblyTitle"] = typeof(AssemblyTitleAttribute),
			["AssemblyConfiguration"] = typeof(AssemblyConfigurationAttribute),
			["AssemblyCompany"] = typeof(AssemblyCompanyAttribute),
			["AssemblyProduct"] = typeof(AssemblyProductAttribute),
			["AssemblyCopyright"] = typeof(AssemblyCopyrightAttribute),
			["AssemblyTrademark"] = typeof(AssemblyTrademarkAttribute),
			["AssemblyCulture"] = typeof(AssemblyCultureAttribute),
			["AssemblyVersion"] = typeof(AssemblyVersionAttribute),
			["AssemblyFileVersion"] = typeof(AssemblyFileVersionAttribute),
			["InternalsVisibleTo"] = typeof(InternalsVisibleToAttribute),
			["AssemblyInformationalVersion"] = typeof(AssemblyInformationalVersionAttribute)		
		};
		
		public async Task TransformAsync(XDocument projectFile, DirectoryInfo projectFolder, Project definition, Settings settings)
		{
			var assemblyInfoFiles = projectFolder
				.EnumerateFiles("AssemblyInfo.cs", SearchOption.AllDirectories)
				.ToArray();

			if (assemblyInfoFiles.Length == 1)
			{
				Console.WriteLine($"Reading assembly info from {assemblyInfoFiles[0].FullName}.");

				string assemblyInfoFileText;
				using (var filestream = File.Open(assemblyInfoFiles[0].FullName, FileMode.Open, FileAccess.Read))
				{
					using (var streamReader = new StreamReader(filestream))
					{
						assemblyInfoFileText = await streamReader.ReadToEndAsync().ConfigureAwait(false);
					}
				}

				Dictionary<Type, IEnumerable<string>> attributes = GetAttributesWithValues(assemblyInfoFileText, _attributeTypesByNames);

				definition.AssemblyAttributes = new AssemblyAttributes
				{
					AssemblyName = definition.AssemblyName ?? projectFolder.Name,
					Description = GetAttributeValue<AssemblyDescriptionAttribute>(attributes),
					Title = GetAttributeValue<AssemblyTitleAttribute>(attributes),
					Company = GetAttributeValue<AssemblyCompanyAttribute>(attributes),
					Product = GetAttributeValue<AssemblyProductAttribute>(attributes),
					Copyright = GetAttributeValue<AssemblyCopyrightAttribute>(attributes),
					InformationalVersion = GetAttributeValue<AssemblyInformationalVersionAttribute>(attributes),
					Version = GetAttributeValue<AssemblyVersionAttribute>(attributes),
					FileVersion = GetAttributeValue<AssemblyFileVersionAttribute>(attributes),
					Configuration = GetAttributeValue<AssemblyConfigurationAttribute>(attributes)
				};
			}
			else
			{
				Console.WriteLine(
					$@"Could not read from assemblyinfo, multiple assemblyinfo files found: {
							string.Join(Environment.NewLine, assemblyInfoFiles.Select(x => x.FullName))
					}.");
			}
		}

		private Dictionary<Type, IEnumerable<string>> GetAttributesWithValues(string fileText, Dictionary<string, Type> attributeTypesByNames)
		{
			CSharpSyntaxTree tree = (CSharpSyntaxTree)CSharpSyntaxTree.ParseText(fileText);
			var attributes = tree.GetRoot().DescendantNodes().OfType<AttributeSyntax>();
			
			return attributes
				.Where(attr => attributeTypesByNames.ContainsKey(attr.Name.ToString()))
				.ToDictionary(
					attr => attributeTypesByNames.GetValue(attr.Name.ToString()),
					attr => attr.ArgumentList.Arguments.Select(arg => arg.ToString())
				);
		}

		private string GetAttributeValue<T>(Dictionary<Type, IEnumerable<string>> allAttributes) where T : Attribute
		{
			var t = typeof(T);
			var value = allAttributes.GetValue(t)?.FirstOrDefault()?.Trim('"');
			return value ?? string.Empty;
		}
	}
}