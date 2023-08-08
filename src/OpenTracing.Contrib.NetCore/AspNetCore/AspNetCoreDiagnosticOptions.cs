using OpenTracing.Contrib.NetCore.AspNetCore;

namespace OpenTracing.Contrib.NetCore.Configuration
{
    public class AspNetCoreDiagnosticOptions
    {
        public HostingOptions Hosting { get; } = new HostingOptions();
        public MvcOptions Mvc { get; } = new MvcOptions();
    }
}
