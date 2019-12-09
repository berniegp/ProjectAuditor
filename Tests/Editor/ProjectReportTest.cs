﻿using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.IO;
using Unity.ProjectAuditor.Editor;

namespace UnityEditor.ProjectAuditor.EditorTests
{
	class ProjectReportTest {

		[Test]
		public void UninitializedTestPasses()
		{
			var uninitialised = new ProjectReport();
			Assert.Zero( uninitialised.NumTotalIssues);
			Assert.Zero( uninitialised.GetNumIssues(IssueCategory.ApiCalls));
			Assert.Zero( uninitialised.GetNumIssues(IssueCategory.ProjectSettings));
		}

		[Test]
		public void AddIssueTestPasses()
		{
			var projectReport = new ProjectReport();
			
			projectReport.AddIssue(new ProjectIssue
			{
				category = IssueCategory.ApiCalls								
			});
			
			Assert.AreEqual(1, projectReport.NumTotalIssues);
			Assert.AreEqual(1, projectReport.GetNumIssues(IssueCategory.ApiCalls));
			Assert.AreEqual(0, projectReport.GetNumIssues(IssueCategory.ProjectSettings));
		}

		[Test]
		public void CanExportReport()
		{
			var projectReport = new ProjectReport();
			
			projectReport.AddIssue(new ProjectIssue
			{
				category = IssueCategory.ApiCalls,
				descriptor = new ProblemDescriptor
				{
					area = "CPU",
					type = "SomeType",
					method = "SomeMethod",
					problem = "",
					solution = ""
				}
			});
			projectReport.AddIssue(new ProjectIssue
			{
				category = IssueCategory.ProjectSettings,
				descriptor = new ProblemDescriptor
				{
					area = "CPU",
					type = "SomeType",
					method = "SomeMethod",
					problem = "",
					solution = ""
				}
			});			
			projectReport.Export();
			Assert.True(File.Exists("ProjectAuditor_Report_" + IssueCategory.ApiCalls.ToString() + ".csv"));			
			Assert.True(File.Exists("ProjectAuditor_Report_" + IssueCategory.ProjectSettings.ToString() + ".csv"));
		}
	}	
}

