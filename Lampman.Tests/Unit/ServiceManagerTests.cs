using DotNetEnv;
using Lampman.Core;
using Lampman.Core.Services;
using Lampman.Tests.TestHelpers;

namespace Lampman.Tests.Unit;

[Trait("Category", "Unit"), Trait("Category", "ServiceManager"), TestCaseOrderer(typeof(PriorityOrderer))]
public class ServiceManagerTests
{
    private readonly ServiceManager _serviceManager;

    private readonly string[]? servicesToManage;

    public ServiceManagerTests()
    {
        Env.TraversePath().Load();

        _serviceManager = new ServiceManager();

        string? services = Environment.GetEnvironmentVariable("TESTING_SERVICES_TO_MANAGE");

        if (!string.IsNullOrEmpty(services))
        {
            servicesToManage = services.Split(';', StringSplitOptions.RemoveEmptyEntries);
        }
    }

    /** Test the 'lampman service install' commands via ServiceManager class **/
    [Fact, Trait("Category", "Manager_ServiceInstall"), TestPriority(200)]
    public async Task ServiceInstall_ShouldCreateServiceEntry()
    {
        if (null != servicesToManage && servicesToManage.Length > 0)
        {
            foreach (var service in servicesToManage)
            {
                await _serviceManager.InstallService(service);

                var (serviceName, version, meta) = ServiceResolver.Resolve(service);

                Assert.True(Directory.Exists(PathResolver.ServicesInstallDir));
                Assert.True(Directory.Exists(PathResolver.ServicePath(serviceName, version)));
            }
        }
    }

    /** Test the 'lampman service update' commands via ServiceManager class **/
    [Fact, Trait("Category", "Manager_ServiceUpdate"), TestPriority(201)]
    public async Task ServiceUpdate_ShouldUpdateServiceEntry()
    {
        if (null != servicesToManage && servicesToManage.Length > 0)
        {
            var firstService = servicesToManage.First();

            await _serviceManager.UpdateService(firstService);

            var (serviceName, version, meta) = ServiceResolver.Resolve(firstService);

            Assert.True(Directory.Exists(PathResolver.ServicesInstallDir));
            Assert.True(Directory.Exists(PathResolver.ServicePath(serviceName, version)));
        }
    }

    /** Test the 'lampman service remove' commands via ServiceManager class **/
    [Fact, Trait("Category", "Manager_ServiceRemove"), TestPriority(202)]
    public void ServiceRemove_ShouldDeleteServiceEntry()
    {
        if (null != servicesToManage && servicesToManage.Length > 0)
        {
            var lastService = servicesToManage.Last();

            _serviceManager.RemoveService(lastService);

            var (serviceName, version, meta) = ServiceResolver.Resolve(lastService);

            Assert.True(Directory.Exists(PathResolver.ServicesInstallDir));
            Assert.False(Directory.Exists(PathResolver.ServicePath(serviceName, version)));
        }
    }
}
