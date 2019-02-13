using System.Collections.Generic;
using Amazon.CDK;
using Amazon.CDK.AWS.DynamoDB;
using Amazon.CDK.AWS.Lambda;

namespace CdkWorkshop.Constructs
{
    public class HitCounterProps
    {
        public IIFunction Downstream { get; set; }
    }

    public class HitCounter : Construct
    {
        public IIFunction Handler { get; }

        public HitCounter(Construct scope, string id, HitCounterProps props) : base(scope, id)
        {
            var table = new Table(this, "Hits", new TableProps());
            table.AddPartitionKey(new Attribute
            {
                Name = "path",
                Type = AttributeType.String
            });

            Handler = new Function(this, "HitConterHandler", new FunctionProps
            {
                Runtime = Runtime.DotNetCore21,
                Timeout = 60,
                Code = Code.Asset("./HitCounterFunction/src/HitCounterFunction/bin/Debug/netcoreapp2.1"),
                Handler = "HitCounterFunction::HitCounterFunction.Function::FunctionHandler",
                Environment = new Dictionary<string, object>
                {
                    {"DOWNSTREAM_FUNCTION_NAME", props.Downstream.FunctionName},
                    {"HITS_TABLE_NAME", table.TableName}
                }
            });

            table.GrantReadWriteData(Handler.Role);
            props.Downstream.GrantInvoke(Handler.Role);
        }
    }
}