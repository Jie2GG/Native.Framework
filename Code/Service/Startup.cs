using Nancy;
using Nancy.Bootstrapper;
using Nancy.Conventions;
using Nancy.TinyIoc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Code.Service
{
    public class Startup
    {
        public class NancyBootstrapper : DefaultNancyBootstrapper
        {
            protected override void ConfigureApplicationContainer(TinyIoCContainer container)
            {
                base.ConfigureApplicationContainer(container);
            }

            protected override void ConfigureConventions(NancyConventions nancyConventions)
            {
                base.ConfigureConventions(nancyConventions);
                Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, "data", "web", "images"));
                nancyConventions.StaticContentsConventions.Add(StaticContentConventionBuilder.AddDirectory("images", Path.Combine(Environment.CurrentDirectory, "data", "web", "images")));
            }

            protected override void RequestStartup(TinyIoCContainer container, IPipelines pipelines, NancyContext context)
            {
                pipelines.AfterRequest.AddItemToEndOfPipeline((ctx) =>
                {
                    ctx.Response.WithHeader("Access-Control-Allow-Origin", "*")
                                    .WithHeader("Access-Control-Allow-Methods", "POST,GET")
                                    .WithHeader("Access-Control-Allow-Headers", "Accept, Origin, Content-type");
                });
            }
        }

        public class NancyWebHttpBinding : WebHttpBinding
        {

        }
    }
}
