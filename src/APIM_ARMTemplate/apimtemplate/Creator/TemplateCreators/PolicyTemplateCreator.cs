﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Apim.Arm.Creator.Creator.TemplateCreators;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Create
{
    public class PolicyTemplateCreator: TemplateCreator,ITemplateCreator
    {
        private FileReader fileReader;

        public PolicyTemplateCreator(FileReader fileReader)
        {
            this.fileReader = fileReader;
        }

        public Template Create(CreatorConfig creatorConfig)
        {
            // create empty template
            Template policyTemplate = CreateEmptyTemplate();

            // add parameters
            policyTemplate.parameters = new Dictionary<string, TemplateParameterProperties>
            {
                { "ApimServiceName", new TemplateParameterProperties(){ type = "string" } }
            };

            List<TemplateResource> resources = new List<TemplateResource>();

            // create global service policy resource with properties
            string globalServicePolicy = creatorConfig.policy;
            Uri uriResult;
            bool isUrl = Uri.TryCreate(globalServicePolicy, UriKind.Absolute, out uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
            // create policy resource with properties
            PolicyTemplateResource policyTemplateResource = new PolicyTemplateResource()
            {
                name = $"[concat(parameters('ApimServiceName'), '/policy')]",
                type = ResourceType.GlobalServicePolicy,
                apiVersion = GlobalConstants.APIVersion,
                properties = new PolicyTemplateProperties()
                {
                    // if policy is a url inline the url, if it is a local file inline the file contents
                    format = isUrl ? "rawxml-link" : "rawxml",
                    value = isUrl ? globalServicePolicy : this.fileReader.RetrieveLocalFileContents(globalServicePolicy)
                },
                dependsOn = new string[] { }
            };
            resources.Add(policyTemplateResource);

            policyTemplate.resources = resources.ToArray();
            return policyTemplate;
        }

        public PolicyTemplateResource CreateAPIPolicyTemplateResource(ApiConfiguration api, string[] dependsOn)
        {
            Uri uriResult;
            bool isUrl = Uri.TryCreate(api.policy, UriKind.Absolute, out uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
            // create policy resource with properties
            PolicyTemplateResource policyTemplateResource = new PolicyTemplateResource()
            {
                name = $"[concat(parameters('ApimServiceName'), '/{api.name}/policy')]",
                type = ResourceType.ApiPolicy,
                apiVersion = GlobalConstants.APIVersion,
                properties = new PolicyTemplateProperties()
                {
                    // if policy is a url inline the url, if it is a local file inline the file contents
                    format = isUrl ? "rawxml-link" : "rawxml",
                    value = isUrl ? api.policy : this.fileReader.RetrieveLocalFileContents(api.policy)
                },
                dependsOn = dependsOn
            };
            return policyTemplateResource;
        }

        public PolicyTemplateResource CreateProductPolicyTemplateResource(ProductConfig product, string[] dependsOn)
        {
            Uri uriResult;
            bool isUrl = Uri.TryCreate(product.policy, UriKind.Absolute, out uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
            // create policy resource with properties
            PolicyTemplateResource policyTemplateResource = new PolicyTemplateResource()
            {
                name = $"[concat(parameters('ApimServiceName'), '/{product.displayName}/policy')]",
                type = ResourceType.ProductPolicy,
                apiVersion = GlobalConstants.APIVersion,
                properties = new PolicyTemplateProperties()
                {
                    // if policy is a url inline the url, if it is a local file inline the file contents
                    format = isUrl ? "rawxml-link" : "rawxml",
                    value = isUrl ? product.policy : this.fileReader.RetrieveLocalFileContents(product.policy)
                },
                dependsOn = dependsOn
            };
            return policyTemplateResource;
        }

        public PolicyTemplateResource CreateOperationPolicyTemplateResource(KeyValuePair<string, OperationsConfig> policyPair, string apiName, string[] dependsOn)
        {
            Uri uriResult;
            bool isUrl = Uri.TryCreate(policyPair.Value.policy, UriKind.Absolute, out uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
            // create policy resource with properties
            PolicyTemplateResource policyTemplateResource = new PolicyTemplateResource()
            {
                name = $"[concat(parameters('ApimServiceName'), '/{apiName}/{policyPair.Key}/policy')]",
                type = ResourceType.ApiOperationPolicy,
                apiVersion = GlobalConstants.APIVersion,
                properties = new PolicyTemplateProperties()
                {
                    // if policy is a url inline the url, if it is a local file inline the file contents
                    format = isUrl ? "rawxml-link" : "rawxml",
                    value = isUrl ? policyPair.Value.policy : this.fileReader.RetrieveLocalFileContents(policyPair.Value.policy)
                },
                dependsOn = dependsOn
            };
            return policyTemplateResource;
        }

        public List<PolicyTemplateResource> CreateOperationPolicyTemplateResources(ApiConfiguration api, string[] dependsOn)
        {
            // create a policy resource for each policy listed in the config file and its associated provided xml file
            List<PolicyTemplateResource> policyTemplateResources = new List<PolicyTemplateResource>();
            foreach (KeyValuePair<string, OperationsConfig> pair in api.operations)
            {
                policyTemplateResources.Add(this.CreateOperationPolicyTemplateResource(pair, api.name, dependsOn));
            }
            return policyTemplateResources;
        }
    }
}
