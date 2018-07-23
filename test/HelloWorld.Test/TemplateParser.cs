using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Xunit;
using YamlDotNet.RepresentationModel;

namespace HelloWorld.Test
{
    public class TemplateParser
    {
        private static IDictionary<YamlNode, YamlNode> LocateResourceProperties(string path, string resourceName)
        {
            var template = File.ReadAllText(path);

            var input = new StringReader(template);

            var yaml = new YamlStream();
            yaml.Load(input);

            var rootNode = (YamlMappingNode)yaml.Documents[0].RootNode;

            var resourcesNode = (YamlMappingNode)rootNode.Children["Resources"];
            var specificResourceNode = (YamlMappingNode)resourcesNode.Children[resourceName];
            var resourcePropertiesNode = (YamlMappingNode)specificResourceNode.Children["Properties"];
            
            return resourcePropertiesNode.Children;
        }

        public static CreateTableRequest GetDynamoDbTable(string templatePath, string tableName)
        {
            var props = LocateResourceProperties(templatePath, tableName);

            var result = new CreateTableRequest();
            result.KeySchema = GetKeySchema(props);
            result.AttributeDefinitions = GetAttributeDefinitions(props);
            result.ProvisionedThroughput = GetProvisionedThroughput(props);
            result.LocalSecondaryIndexes = GetLocalSecondaryIndexes(props);

            return result;
        }

        private static List<LocalSecondaryIndex> GetLocalSecondaryIndexes(IDictionary<YamlNode, YamlNode> props)
        {
            if (props.ContainsKey("LocalSecondaryIndexes"))
            {
                var localSecondaryIndexesNode = (YamlSequenceNode)props["LocalSecondaryIndexes"];
                var result = new List<LocalSecondaryIndex>();
                foreach (YamlMappingNode entry in localSecondaryIndexesNode.Children)
                {
                    var indexName = entry.Children["IndexName"].ToString();
                    var projectionNode = (YamlMappingNode)entry.Children["Projection"];
                    var projectionType = projectionNode.Children["ProjectionType"].ToString();
                    var KeySchema = GetKeySchema(entry.Children);
                   
                    var localSecondaryIndex = new LocalSecondaryIndex();
                    localSecondaryIndex.IndexName = indexName;
                    localSecondaryIndex.Projection = new Projection { ProjectionType = new ProjectionType(projectionType) };
                    localSecondaryIndex.KeySchema = KeySchema;
                    result.Add(localSecondaryIndex);
                }
                return result;
            }
            return null;
        }

        private static List<KeySchemaElement> GetKeySchema(IDictionary<YamlNode, YamlNode> props)
        {
            var result = new List<KeySchemaElement>();
            var keySchemaNode = (YamlSequenceNode)props["KeySchema"];
            foreach (YamlMappingNode entry in keySchemaNode.Children)
            {
                var attrName = entry.Children["AttributeName"].ToString();
                var keyType = entry.Children["KeyType"].ToString();
                var keySchemaElement = new KeySchemaElement(attrName, keyType);
                result.Add(keySchemaElement);
            }
            return result;
        }

        private static List<AttributeDefinition> GetAttributeDefinitions(IDictionary<YamlNode, YamlNode> props)
        {
            var attributeDefinitionsNode = (YamlSequenceNode)props["AttributeDefinitions"];
            var result = new List<AttributeDefinition>();
            foreach (YamlMappingNode entry in attributeDefinitionsNode.Children)
            {
                var attrName = entry.Children["AttributeName"].ToString();
                var attrType = entry.Children["AttributeType"].ToString();
                var attrDef = new AttributeDefinition(attrName, attrType);
                result.Add(attrDef);
            }
            return result;
        }

        private static ProvisionedThroughput GetProvisionedThroughput(IDictionary<YamlNode, YamlNode> props)
        {
            if (props.ContainsKey("ProvisionedThroughput"))
            {
                var provisionedThroughputNode = (YamlMappingNode)props["ProvisionedThroughput"];
                var rcu = Convert.ToInt64(provisionedThroughputNode.Children["ReadCapacityUnits"].ToString());
                var wcu = Convert.ToInt64(provisionedThroughputNode.Children["WriteCapacityUnits"].ToString());
                return new ProvisionedThroughput(rcu, wcu);
            }
            return null;
        }
    }
}