using Orleans;
using Orleans.CodeGeneration;
using Orleans.Providers;
using Orleans.Runtime;
using Serilog;
using Serilog.Events;
using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Net;

namespace SBTech.OrleansSerilogUtils
{
    public class SerilogBootstrap : IBootstrapProvider
    {
        readonly ILogger _logger = Log.ForContext<SerilogBootstrap>();
        string _siloId = string.Empty;

        public string Name { get; private set; }

        public Task Close() => TaskDone.Done;

        public Task Init(string name, IProviderRuntime providerRuntime,
                         IProviderConfiguration config)
        {
            Name = name;
            _siloId = providerRuntime.SiloIdentity;

            // added orleans client log consumer
            LogManager.LogConsumers.Add(new OrleansLogClient());

            providerRuntime.SetInvokeInterceptor(async (method, request, grain, invoker) =>
            {
                try
                {
                    if (_logger.IsEnabled(LogEventLevel.Debug))
                        LogBeginInvoke(method, request, grain, invoker);

                    var response = await invoker.Invoke(grain, request);

                    if (_logger.IsEnabled(LogEventLevel.Debug))
                        LogEndInvoke(method, request, grain, invoker, response);

                    return response;
                }
                catch (Exception ex)
                {
                    LogError(method, request, grain, invoker, ex);
                    throw;
                }
            });
            return TaskDone.Done;
        }

        void LogBeginInvoke(MethodInfo method, InvokeMethodRequest request,
                            IGrain grain, IGrainMethodInvoker invoker)
        {
            if (request.Arguments == null || request.Arguments?.Length == 0)
            {
                _logger.Debug("BeginInvoke {SiloId} {Grain} {Method} {GrainId}",
                    _siloId, grain.GetType().Name, method.Name, grain.GetPrimaryKeyString());
            }
            else if (request.Arguments.Length == 1)
            {
                _logger.Debug("BeginInvoke {SiloId} {Grain} {Method} {GrainId} {@Argument}",
                    _siloId, grain.GetType().Name, method.Name, grain.GetPrimaryKeyString(), request.Arguments[0]);
            }
            else if (request.Arguments.Length == 2)
            {
                _logger.Debug("BeginInvoke {SiloId} {Grain} {Method} {GrainId} {@Argument_1} {@Argument_2}",
                    _siloId, grain.GetType().Name, method.Name, grain.GetPrimaryKeyString(), request.Arguments[0], request.Arguments[1]);
            }
            else if (request.Arguments.Length == 3)
            {
                _logger.Debug("BeginInvoke {SiloId} {Grain} {Method} {GrainId} {@Argument_1} {@Argument_2} {@Argument_3}",
                    _siloId, grain.GetType().Name, method.Name, grain.GetPrimaryKeyString(), request.Arguments[0], request.Arguments[1], request.Arguments[2]);
            }
            else
            {
                _logger.Debug("BeginInvoke {SiloId} {Grain} {Method} {GrainId} {@Argument_1} {@Argument_2} {@Argument_3} {@Argument_4}",
                    _siloId, grain.GetType().Name, method.Name, grain.GetPrimaryKeyString(), request.Arguments[0], request.Arguments[1], request.Arguments[2], request.Arguments[3]);
            }
        }

        void LogEndInvoke(MethodInfo method, InvokeMethodRequest request,
                          IGrain grain, IGrainMethodInvoker invoker, object response)
        {
            _logger.Debug("EndInvoke {SiloId} {Grain} {Method} {GrainId} {@Response}",
                _siloId, grain.GetType().Name, method.Name, grain.GetPrimaryKeyString(), response);
        }

        void LogError(MethodInfo method, InvokeMethodRequest request,
                      IGrain grain, IGrainMethodInvoker invoker, Exception error)
        {
            if (request.Arguments == null || request.Arguments?.Length == 0)
            {
                _logger.Error(error, "Error {SiloId} {Grain} {Method} {GrainId}",
                    _siloId, grain.GetType().Name, method.Name, grain.GetPrimaryKeyString());
            }
            else if (request.Arguments.Length == 1)
            {
                _logger.Error(error, "Error {SiloId} {Grain} {Method} {GrainId} {@Argument}",
                    _siloId, grain.GetType().Name, method.Name, grain.GetPrimaryKeyString(), request.Arguments[0]);
            }
            else if (request.Arguments.Length == 2)
            {
                _logger.Error(error, "Error {SiloId} {Grain} {Method} {GrainId} {@Argument_1} {@Argument_2}",
                    _siloId, grain.GetType().Name, method.Name, grain.GetPrimaryKeyString(), request.Arguments[0], request.Arguments[1]);
            }
            else if (request.Arguments.Length == 3)
            {
                _logger.Error(error, "Error {SiloId} {Grain} {Method} {GrainId} {@Argument_1} {@Argument_2} {@Argument_3}",
                    _siloId, grain.GetType().Name, method.Name, grain.GetPrimaryKeyString(), request.Arguments[0], request.Arguments[1], request.Arguments[2]);
            }
            else
            {
                _logger.Error(error, "Error {SiloId} {Grain} {Method} {GrainId} {@Argument_1} {@Argument_2} {@Argument_3} {@Argument_4}",
                    _siloId, grain.GetType().Name, method.Name, grain.GetPrimaryKeyString(), request.Arguments[0], request.Arguments[1], request.Arguments[2], request.Arguments[3]);
            }
        }

        class OrleansLogClient : ILogConsumer
        {
            readonly ILogger _logger = Serilog.Log.ForContext<OrleansLogClient>();            

            public void Log(Severity severity, LoggerType loggerType, string caller, string message, IPEndPoint myIPEndPoint, Exception exception, int eventCode = 0)
            {
                switch (severity)
                {
                    case Severity.Error:
                        _logger.Error(exception, "{Caller} {Message}", caller, message);
                        break;
                    case Severity.Warning:
                        _logger.Warning(exception, "{Caller} {Message}", caller, message);
                        break;
                    case Severity.Info:
                        _logger.Information(exception, "{Caller} {Message}", caller, message);
                        break;
                    case Severity.Verbose:
                        _logger.Debug(exception, "{Caller} {Message}", caller, message);
                        break;
                    case Severity.Verbose2:
                    case Severity.Verbose3:
                        _logger.Verbose(exception, "{Caller} {Message}", caller, message);
                        break;
                }
            }
        }
    }
}
