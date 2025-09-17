using DotNetEnv;

using Lampman.Core.Services;
using Lampman.Tests.Fixtures;
using Lampman.Tests.TestHelpers;

namespace Lampman.Tests.Unit;

[Trait("Category", "Unit"), Trait("Category", "StackManager"), TestCaseOrderer(typeof(PriorityOrderer))]
public class StackManagerTests : IClassFixture<MockRegistryFixture>
{
    private readonly StackManager _stackManager;

    private readonly string[]? _servicesToManage;

    private readonly MockRegistryFixture _fixture;

    public StackManagerTests(MockRegistryFixture fixture)
    {
        Env.TraversePath().Load();

        _fixture = fixture;

        _stackManager = new StackManager();

        string? services = Environment.GetEnvironmentVariable("TESTING_SERVICES_TO_MANAGE");

        if (!string.IsNullOrEmpty(services))
        {
            _servicesToManage = services.Split(';', StringSplitOptions.RemoveEmptyEntries);
        }
    }

    /** Test the 'lampman list' commands via StackManager class **/
    [Fact, Trait("Category", "Manager_StackList"), TestPriority(200)]
    public void StackList_ShouldDisplayServicesListing()
    {
        _stackManager.ListServices();
    }

    /** Test the 'lampman start [service-key]' commands via StackManager class **/
    [Fact, Trait("Category", "Manager_StackStart"), TestPriority(201)]
    public void StackStart_ShouldRunServiceInstalled()
    {
        if (null != _servicesToManage && _servicesToManage.Length > 0)
        {
            foreach (string service in _servicesToManage)
            {
                _stackManager.StartServices([service]);
            }
        }
    }

    /** Test the 'lampman restart [service-key]' commands via StackManager class **/
    [Fact, Trait("Category", "Manager_StackRestart"), TestPriority(202)]
    public void StackRestart_ShouldRestartServiceInstalled()
    {
        if (null != _servicesToManage && _servicesToManage.Length > 0)
        {
            foreach (string service in _servicesToManage)
            {
                _stackManager.RestartServices([service]);
            }
        }
    }

    /** Test the 'lampman stop [service-key]' commands via StackManager class **/
    [Fact, Trait("Category", "Manager_StackStop"), TestPriority(203)]
    public void StackStop_ShouldStopServiceInstalled()
    {
        if (null != _servicesToManage && _servicesToManage.Length > 0)
        {
            foreach (string service in _servicesToManage)
            {
                _stackManager.StartServices([service]);
            }
        }
    }
}