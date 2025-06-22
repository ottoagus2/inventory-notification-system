namespace NotificationService.Models
{
    public class RabbitMQSettings
    {
        public string HostName { get; set; } = "localhost";
        public int Port { get; set; } = 5672;
        public string UserName { get; set; } = "guest";
        public string Password { get; set; } = "guest";
        public string ExchangeName { get; set; } = "inventory_exchange";
        public string QueueName { get; set; } = "inventory_queue";
        public bool Durable { get; set; } = true;
        public bool AutoDelete { get; set; } = false;
    }
}