﻿using Capl.Authorization;
using Capl.Issuance;
using System;
using System.Management.Automation;

namespace Capl.Module
{
    [Cmdlet(VerbsCommon.New, "CiplPolicy")]
    public class CiplPolicy : Cmdlet
    {
        [Parameter(HelpMessage = "Uniquely identifies the policy as a URI", Mandatory = true)]
        public string PolicyID;

        [Parameter(HelpMessage = "Issuance mode", Mandatory = true)]
        public IssueModeType IssueMode;

        [Parameter(HelpMessage = "Transforms used to issue claims", Mandatory = true)]
        public Transform[] Transforms;

        protected override void ProcessRecord()
        {
            IssuePolicy policy = new IssuePolicy() { PolicyId = this.PolicyID };
            if(this.IssueMode == IssueModeType.Aggregate)
            {
                policy.Mode = Issuance.IssueMode.Aggregate;

            }
            else if (this.IssueMode == IssueModeType.Unique)
            {
                policy.Mode = Issuance.IssueMode.Unique;
            }
            else
            {
                throw new ArgumentOutOfRangeException("IssueMode");
            }
            
            if (this.Transforms != null && this.Transforms.Length > 0)
            {
                foreach (Transform transform in this.Transforms)
                {
                    policy.Transforms.Add(transform);
                }
            }

            WriteObject(policy);
        }
    }
}