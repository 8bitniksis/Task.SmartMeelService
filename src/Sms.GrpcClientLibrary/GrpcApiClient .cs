using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using Sms.Models; // Импортируем Sms.Models
// Не импортируем Protos, чтобы избежать конфликта имен в глобальной области
// using Sms.GrpcClientLibrary.Protos;

namespace Sms.GrpcClientLibrary
{
    /// <summary>
    /// Provides methods to interact with the server via gRPC calls.
    /// </summary>
    public class GrpcApiClient : IDisposable
    {
        private readonly GrpcChannel _channel;
        private readonly Sms.GrpcClientLibrary.Protos.SmsTestService.SmsTestServiceClient _client; // Уточняем полный путь к клиенту

        /// <summary>
        /// Initializes a new instance of the <see cref="GrpcApiClient"/> class.
        /// </summary>
        /// <param name="grpcEndpoint">The gRPC endpoint address (e.g., https://localhost:5001).</param>
        public GrpcApiClient(string grpcEndpoint)
        {
            // NOTE: In production, configure channel options securely (e.g., TLS, max message size).
            _channel = GrpcChannel.ForAddress(grpcEndpoint);
            _client = new Sms.GrpcClientLibrary.Protos.SmsTestService.SmsTestServiceClient(_channel); // Уточняем полный путь к клиенту
        }

        /// <summary>
        /// Sends a request to retrieve the menu from the server via gRPC.
        /// </summary>
        /// <param name="withPrice">Whether to include prices in the response (passed as BoolValue argument).</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>A list of <see cref="Dish"/> objects if successful, otherwise throws an exception.</returns>
        /// <exception cref="RpcException">Thrown when the gRPC call fails or the server returns an error.</exception>
        public async Task<List<Dish>> GetMenuAsync(bool withPrice = true, CancellationToken cancellationToken = default)
        {
            var request = new BoolValue { Value = withPrice };

            try
            {
                var response = await _client.GetMenuAsync(request, cancellationToken: cancellationToken);

                if (!response.Success)
                {
                    var errorMessage = !string.IsNullOrEmpty(response.ErrorMessage) ? response.ErrorMessage : "Unknown error occurred while fetching menu via gRPC.";
                    throw new RpcException(new Grpc.Core.Status(StatusCode.Unknown, errorMessage));
                }

                var dishes = new List<Dish>();
                foreach (var protoItem in response.MenuItems)
                {
                    var dish = new Dish // Создаем экземпляр Dish
                    {
                        Id = protoItem.Id,
                        Article = protoItem.Article,
                        Name = protoItem.Name,
                        Price = protoItem.Price,
                        IsWeighted = protoItem.IsWeighted,
                        FullPath = protoItem.FullPath,
                        // Barcodes = { protoItem.Barcodes } // СТАРОЕ: НЕПРАВИЛЬНО
                    };
                    dish.Barcodes.AddRange(protoItem.Barcodes); // НОВОЕ: ПРАВИЛЬНО - копируем элементы
                    dishes.Add(dish); // Не забываем добавить созданный блюдо в список
                }
                return dishes;
            }
            catch (RpcException ex)
            {
                throw new RpcException(ex.Status, $"gRPC error during GetMenu: {ex.Status.Detail}");
            }
        }


        /// <summary>
        /// Sends an order to the server via gRPC.
        /// </summary>
        /// <param name="order">The <see cref="Sms.Models.Order"/> object to send.</param> <!-- Уточняем тип в XML-документации -->
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>True if the server indicates success, otherwise throws an exception.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the order is null.</exception>
        /// <exception cref="RpcException">Thrown when the gRPC call fails or the server returns an error.</exception>
        public async Task<bool> SendOrderAsync(Sms.Models.Order order, CancellationToken cancellationToken = default) // Уточняем тип параметра
        {
            if (order == null)
            {
                throw new ArgumentNullException(nameof(order));
            }

            // Преобразуем Sms.Models.Order в Sms.GrpcClientLibrary.Protos.Order
            var protoOrder = new Sms.GrpcClientLibrary.Protos.Order { Id = order.OrderId }; // Явно указываем протобафферный Order
            foreach (var item in order.Items)
            {
                protoOrder.OrderItems.Add(new Sms.GrpcClientLibrary.Protos.OrderItem { Id = item.Id, Quantity = item.Quantity }); // Явно указываем протобафферный OrderItem
            }

            try
            {
                var response = await _client.SendOrderAsync(protoOrder, cancellationToken: cancellationToken); // Передаем протобафферный Order

                if (!response.Success)
                {
                    var errorMessage = !string.IsNullOrEmpty(response.ErrorMessage) ? response.ErrorMessage : "Unknown error occurred while sending order via gRPC.";
                    throw new RpcException(new Grpc.Core.Status(StatusCode.Unknown, errorMessage));
                }

                return true; // Success field was true
            }
            catch (RpcException ex)
            {
                throw new RpcException(ex.Status, $"gRPC error during SendOrder: {ex.Status.Detail}");
            }
        }

        public void Dispose()
        {
            _channel?.ShutdownAsync().Wait(); // Graceful shutdown
        }
    }
}