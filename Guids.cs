// Guids.cs
// MUST match guids.h
using System;
// ReSharper disable InconsistentNaming

namespace Lkytal.StatusInfo
{
	static class GuidList
	{
		public const string guidStatusInfoPkgString = "72581eb6-4dcd-4b8f-9add-c4257d4fb9d7";
		public const string guidStatusInfoCmdSetString = "fe879628-e88f-4564-abfe-babec6c33d3f";

		public static readonly Guid guidStatusInfoCmdSet = new Guid(guidStatusInfoCmdSetString);
	};
}