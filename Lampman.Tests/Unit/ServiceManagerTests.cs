using Lampman.Core;
using Lampman.Core.Services;
using Lampman.Tests.TestHelpers;

namespace Lampman.Tests.Unit;

[Trait("Category", "Unit"), TestCaseOrderer(typeof(PriorityOrderer))]
public class ServiceManagerTests
{
    private readonly string serviceInput = "php:8.2";
    private readonly ServiceManager _serviceManager;

    public ServiceManagerTests()
    {
        _serviceManager = new ServiceManager();
    }

    /** Test the 'lampman service install' commands via ServiceManager class **/
    [Fact, Trait("Category", "ServiceInstallManager"), TestPriority(1)]
    public async Task ServiceInstall_ShouldCreateServiceEntry()
    {
        await _serviceManager.InstallService(serviceInput);

        var (serviceName, version, meta) = ServiceResolver.Resolve(serviceInput);

        Assert.True(Directory.Exists(PathResolver.ServicesInstallDir));
        Assert.True(Directory.Exists(PathResolver.ServicePath(serviceName, version)));
    }

    /** Test the 'lampman service update' commands via ServiceManager class **/
    [Fact, Trait("Category", "ServiceUpdateManager"), TestPriority(2)]
    public async Task ServiceUpdate_ShouldUpdateServiceEntry()
    {
        await _serviceManager.UpdateService(serviceInput);

        var (serviceName, version, meta) = ServiceResolver.Resolve(serviceInput);

        Assert.True(Directory.Exists(PathResolver.ServicesInstallDir));
        Assert.True(Directory.Exists(PathResolver.ServicePath(serviceName, version)));
    }

    /** Test the 'lampman service remove' commands via ServiceManager class **/
    [Fact, Trait("Category", "ServiceRemoveManager"), TestPriority(3)]
    public void ServiceRemove_ShouldDeleteServiceEntry()
    {
        _serviceManager.RemoveService(serviceInput);

        var (serviceName, version, meta) = ServiceResolver.Resolve(serviceInput);

        Assert.True(Directory.Exists(PathResolver.ServicesInstallDir));
        Assert.False(Directory.Exists(PathResolver.ServicePath(serviceName, version)));
    }
}
