using ClosedXML.Excel;
using FileCreateWorkerService.Models;
using FileCreateWorkerService.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared;
using System.Data;
using System.Text;
using System.Text.Json;

namespace FileCreateWorkerService;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IServiceProvider serviceProvider;
    private readonly RabbitMQClientService rabbitMQClientService;
    private IModel channel;

    public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider, RabbitMQClientService rabbitMQClientService)
    {
        _logger = logger;
        this.serviceProvider = serviceProvider;
        this.rabbitMQClientService = rabbitMQClientService;
    }
    public override Task StartAsync(CancellationToken cancellationToken)
    {
        channel = rabbitMQClientService.Connect();
        channel.BasicQos(0, 1, false);
        return base.StartAsync(cancellationToken);
    }

    protected override  Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer=new AsyncEventingBasicConsumer(channel);
        channel.BasicConsume(RabbitMQClientService.QueueName,false,consumer);
        consumer.Received += Consumer_Received;
        return Task.CompletedTask;
    }

    private async Task Consumer_Received(object sender, BasicDeliverEventArgs @event)
    {
       await Task.Delay(5000);

        var excelMessage = JsonSerializer.Deserialize<CreateExcelMessage>(Encoding.UTF8.GetString(@event.Body.ToArray()));

        using var ms = new MemoryStream();
        
            var wb = new XLWorkbook();
            var ds = new DataSet();
            ds.Tables.Add(GetTable("Products"));
            wb.Worksheets.Add(ds);
            wb.SaveAs(ms);
        
        MultipartFormDataContent multipartFormDataContent = new MultipartFormDataContent();
        multipartFormDataContent.Add(new ByteArrayContent(ms.ToArray()),"file",Guid.NewGuid().ToString()+"xlsx");
        var baseUrl = "http://localhost:40232/api/files";
        using (var httpclient= new HttpClient())
        {
            var response= await httpclient.PostAsync($"{baseUrl}?fileId={excelMessage.FileId}", multipartFormDataContent);
            if (response.IsSuccessStatusCode)
            {
                channel.BasicAck(@event.DeliveryTag,false);
            }
        }


    }

    private DataTable GetTable(string tableName)
    {
        List<FileCreateWorkerService.Models.Product> products;
        using (var scope = serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AdventureWorks2019Context>();
            products = context.Products.ToList();
        }

        DataTable table = new DataTable() { TableName = tableName };
        table.Columns.Add("ProductId", typeof(int));
        table.Columns.Add("Name", typeof(string));
        table.Columns.Add("ProductNumber", typeof(string));
        table.Columns.Add("Color", typeof(string));
        foreach (var item in products)
        {
            table.Rows.Add(item.ProductId,item.Name,item.ProductNumber,item.Color);
        }
        return table;

    }
}