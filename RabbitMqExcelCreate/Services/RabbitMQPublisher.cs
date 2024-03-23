using Shared;
using System.Text;
using System.Text.Json;

namespace RabbitMqExcelCreate.Services;

public class RabbitMQPublisher
{
    private readonly RabbitMQClientService rabbitMQClientService;

    public RabbitMQPublisher(RabbitMQClientService rabbitMQClientService)
    {
        this.rabbitMQClientService = rabbitMQClientService;
    }
    public void Publish(CreateExcelMessage excelMessage)
    {
        var channel = rabbitMQClientService.Connect();
        var bodyString=JsonSerializer.Serialize(excelMessage);
        var bodyBytes=Encoding.UTF8.GetBytes(bodyString);
        var properties = channel.CreateBasicProperties();
        properties.Persistent = true;
        channel.BasicPublish(exchange:RabbitMQClientService.ExchangeName,routingKey:RabbitMQClientService.RoutingExcel,true,basicProperties:properties,body:bodyBytes);

    }
}
