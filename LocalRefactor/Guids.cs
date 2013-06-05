// Guids.cs
// MUST match guids.h
using System;

namespace HotspotDevelopments.LocalRefactor
{
    static class GuidList
    {
        public const string guidLocalRefactorPkgString = "3d4c35f0-b1d7-4e4e-9377-5b7aa736c711";
        public const string guidLocalRefactorCmdSetString = "65fcf894-86b7-4a19-bf64-63f2d333fd2c";

        public static readonly Guid guidLocalRefactorCmdSet = new Guid(guidLocalRefactorCmdSetString);
    };
}