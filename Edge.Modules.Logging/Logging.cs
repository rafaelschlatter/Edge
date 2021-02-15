using Autofac;
using Serilog;

namespace RaaLabs.Edge.Modules.Logging
{
    public class Logging : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(_ => CreateLogger()).As<ILogger>();
        }

        private Serilog.Core.Logger CreateLogger()
        {
            var log = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();

            return log;
        }
    }
}
