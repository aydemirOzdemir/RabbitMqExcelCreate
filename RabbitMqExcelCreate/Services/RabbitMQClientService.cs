using RabbitMQ.Client;

namespace RabbitMqExcelCreate.Services;

public class RabbitMQClientService : IDisposable
{
    private readonly ConnectionFactory connectionFactory;
    private readonly ILogger<RabbitMQClientService> logger;
    private IConnection connection;
    private IModel channel;
    public static string ExchangeName = "ExcelDirectExchange";
    public static string RoutingExcel = "excel-route-file";
    public static string QueueName = "queue-excel-file";

    public RabbitMQClientService(ConnectionFactory connectionFactory,ILogger<RabbitMQClientService> logger)
    {
        this.connectionFactory = connectionFactory;
        this.logger = logger;
    }
    public IModel Connect()
    { 
        connection = connectionFactory.CreateConnection();
        if (channel.IsOpen)
        {
            return channel;
        }
        channel= connection.CreateModel();
        channel.ExchangeDeclare(ExchangeName, ExchangeType.Direct, true, false);
        channel.QueueDeclare(QueueName,true,false,false,null);
        channel.QueueBind(exchange:ExchangeName,queue:QueueName,routingKey:RoutingExcel);
        logger.LogInformation("RabbitMq ile bağlantı kuruldu");
        return channel;
    }
    public void Dispose()
    {
        channel?.Close();
        channel?.Dispose();
        connection?.Close();
        connection?.Dispose();
        logger.LogInformation("RabbitMq ile bağlantı koptu");
    }
}
