using Akka.Actor;
using Akka.Dispatch;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Samurai.Integration.Application.Services;
using Samurai.Integration.Domain.Enums.Tray;
using Samurai.Integration.Domain.Extensions;
using Samurai.Integration.Domain.Messages;
using Samurai.Integration.Domain.Messages.Tray;
using Samurai.Integration.Domain.Messages.Tray.ProductActor;
using Samurai.Integration.Domain.Queues;
using Serilog;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Samurai.Integration.Application.Actors.Tray
{
    public class TrayProductActor : BaseTrayTenantActor
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly CancellationToken _cancellationToken;
        private readonly IActorRef _apiActorGroup;

        private readonly TrayQueue.Queues _queues;

        public TrayProductActor(IServiceProvider serviceProvider, CancellationToken cancellationToken, TenantDataMessage tenantData, IActorRef apiActorGroup)
            : base("TrayProductActor")
        {
            _serviceProvider = serviceProvider;
            _cancellationToken = cancellationToken;
            _tenantData = tenantData;
            _apiActorGroup = apiActorGroup;

            using (var scope = _serviceProvider.CreateScope())
            {
                var tenantService = scope.ServiceProvider.GetService<TenantService>();

                _queues = new TrayQueue.Queues
                {
                    ProcessProductQueue = tenantService.GetQueueClient(_tenantData, TrayQueue.ProcessProductQueue, false),
                    //ProcessCategoriesProductQueue = tenantService.GetQueueClient(_tenantData, TrayQueue.ProcessCategoriesProductQueue),
                    //ProcessManufacturersProductQueue = tenantService.GetQueueClient(_tenantData, TrayQueue.ProcessManufacturersProductQueue),
                    //ProcessAttributesProductQueue = tenantService.GetQueueClient(_tenantData, TrayQueue.ProcessAttributesProductQueue),
                    //ProcessVariationOptionsProductQueue = tenantService.GetQueueClient(_tenantData, TrayQueue.ProcessVariationOptionsProductQueue),
                    TrayAppReturnMessage = tenantService.GetQueueClient(_tenantData, TrayQueue.TrayAppReturnMessage, false)
                };
            }

           //ReceiveAsync((Func<TrayProcessProductMessage, Task>)(async message =>
           // {
           //     try
           //     {
           //         //LogInfo($"TrayProcessProductMessage - StoreId = {message.StoreId}, ProductId = {message.Product.AppTrayProductId}, TrayProductId = {message.Product.Id}, Status = {message.Status}");

           //         using (var scope = _serviceProvider.CreateScope())
           //         {
           //             var trayService = scope.ServiceProvider.GetService<TrayService>();

           //             trayService.Init(_apiActorGroup, GetLog());

           //             var result = await trayService.ProcessProduct(message, _cancellationToken);

           //             Sender.Tell(result);
           //         }
           //     }
           //     catch (Exception ex)
           //     {
           //         LogError(ex, $"Error in TrayProcessProductMessage - StoreId = {message.StoreId}, ProductId = {message.Product.AppTrayProductId}, TrayProductId = {message.Product.Id}, Status = {message.Status}");

           //         Sender.Tell(new ReturnMessage
           //         {
           //             Result = Result.Error,
           //             Error = ex
           //         });
           //     }
           // })); 

            //ReceiveAsync((Func<TrayProcessVariationMessage, Task>)(async message =>Ne
            //{
            //    try
            //    {
            //        using (var scope = _serviceProvider.CreateScope())
            //        {
            //            var trayService = scope.ServiceProvider.GetService<TrayService>();

            //            trayService.Init(_apiActorGroup, GetLog());

            //            var trayAppReturnMessage = await trayService.ProcessVariationOptions(message, _cancellationToken);
            //        }

            //        Sender.Tell(new ReturnMessage() { Result = Result.OK });
            //    }
            //    catch (Exception ex)
            //    {
            //        Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
            //    }
            //}));

            //ReceiveAsync((Func<TrayProcessAttributesProductMessage, Task>)(async message =>
            //{
            //    try
            //    {
            //        LogDebug($"Starting TrayProcessAttributesProductMessage");

            //        ReturnMessage result;

            //        using (var scope = _serviceProvider.CreateScope())
            //        {
            //            var trayService = scope.ServiceProvider.GetService<TrayService>();
            //            trayService.Init(_apiActorGroup, GetLog());

            //            result = await trayService.ProcessAttributesProduct(message, _cancellationToken);
            //        }

            //        LogDebug("Ending TrayProcessAttributesProductMessage");

            //        Sender.Tell(result);
            //    }
            //    catch (Exception ex)
            //    {
            //        LogError(ex, "Error in TrayProcessAttributesProductMessage");
            //        Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
            //    }
            //}));

            //ReceiveAsync((Func<TrayProcessCategoyMessage, Task>)(async message =>
            //{
            //    try
            //    {
            //        ReturnMessage result;

            //        using (var scope = _serviceProvider.CreateScope())
            //        {
            //            var trayService = scope.ServiceProvider.GetService<TrayService>();

            //            trayService.Init(_apiActorGroup, GetLog());

            //            result = await trayService.ProcessProcessCategoy(message, _cancellationToken);
            //        }

            //        Sender.Tell(result);
            //    }
            //    catch (Exception ex)
            //    {
            //        Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
            //    }
            //}));

            //ReceiveAsync((Func<TrayProcessManufactureMessage, Task>)(async message =>
            //{
            //    try
            //    {
            //        ReturnMessage result;

            //        using (var scope = _serviceProvider.CreateScope())
            //        {
            //            var trayService = scope.ServiceProvider.GetService<TrayService>();

            //            trayService.Init(_apiActorGroup, GetLog());

            //            result = await trayService.ProcessManufacturers(message, _cancellationToken);
            //        }

            //        Sender.Tell(result);
            //    }
            //    catch (Exception ex)
            //    {
            //        Sender.Tell(new ReturnMessage { Result = Result.Error, Error = ex });
            //    }
            //}));
        }

        protected override void PostStop()
        {
            base.PostStop();

            ActorTaskScheduler.RunTask(async () =>
            {
                await _queues.ProcessProductQueue.CloseAsyncSafe();
                //await _queues.ProcessCategoriesProductQueue.CloseAsyncSafe();
                //await _queues.ProcessManufacturersProductQueue.CloseAsyncSafe();
                //await _queues.ProcessAttributesProductQueue.CloseAsyncSafe();
                //await _queues.ProcessVariationOptionsProductQueue.CloseAsyncSafe();
                await _queues.TrayAppReturnMessage.CloseAsyncSafe();
            });
        }

        public static Props Props(IServiceProvider serviceProvider, CancellationToken cancellationToken, TenantDataMessage tenantData, IActorRef apiActorGroup)
        {
            return Akka.Actor.Props.Create(() => new TrayProductActor(serviceProvider, cancellationToken, tenantData, apiActorGroup));
        }

    }
}
