﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Project2015To2017.Writing;
using Project2015To2017Tests.Helpers;

namespace Project2015To2017Tests
{
	[TestClass]
	public class ProjectWriterTest
	{
		[TestMethod]
		public void GenerateAssemblyInfoOnNothingSpecifiedTest()
		{
			var writer = new ProjectWriter(SettingsFactory.Create());
			var xmlNode = writer.CreateXml(new Project2015To2017.Definition.Project
			{
				AssemblyAttributes = new Project2015To2017.Definition.AssemblyAttributes()
			}, new System.IO.FileInfo("test.cs"));

			var generateAssemblyInfo = xmlNode.Element("PropertyGroup").Element("GenerateAssemblyInfo");
			Assert.IsNotNull(generateAssemblyInfo);
			Assert.AreEqual("false", generateAssemblyInfo.Value);
		}

		[TestMethod]
		public void GeneratesAssemblyInfoNodesWhenSpecifiedTest()
		{
			var writer = new ProjectWriter(SettingsFactory.Create());
			var xmlNode = writer.CreateXml(new Project2015To2017.Definition.Project
			{
				AssemblyAttributes = new Project2015To2017.Definition.AssemblyAttributes
				{
					Company = "Company"
				}
			}, new System.IO.FileInfo("test.cs"));

			var generateAssemblyInfo = xmlNode.Element("PropertyGroup").Element("GenerateAssemblyInfo");
			Assert.IsNull(generateAssemblyInfo);

			var generateAssemblyCompany = xmlNode.Element("PropertyGroup").Element("GenerateAssemblyCompanyAttribute");
			Assert.IsNotNull(generateAssemblyCompany);
			Assert.AreEqual("false", generateAssemblyCompany.Value);
		}
	}
}
