using System.ComponentModel.DataAnnotations.Schema;

using Newtonsoft.Json;

namespace ECommercePlatform.Data.Models
{
    public class OutboxMessage
    {
        private static readonly JsonSerializerSettings SerializerSettings =
            new() { NullValueHandling = NullValueHandling.Ignore };

        private string serializedData = default!;

        public OutboxMessage(object data)
        {
            this.Id = Guid.NewGuid();
            this.CreatedAt = DateTime.UtcNow;
            this.Data = data;
        }

        public OutboxMessage() { }

        public Guid Id { get; private set; }

        public Type Type { get; private set; } = default!;

        public bool Published { get; private set; }

        public DateTime CreatedAt { get; private set; }

        public DateTime? PublishedAt { get; private set; }

        public int RetryCount { get; private set; }

        public string? Error { get; private set; }

        public void MarkAsPublished()
        {
            this.Published = true;
            this.PublishedAt = DateTime.UtcNow;
            this.Error = null;
        }

        public void RecordFailure(string error)
        {
            this.RetryCount++;
            this.Error = error;
        }

        [NotMapped]
        public object Data
        {
            get => JsonConvert.DeserializeObject(this.serializedData, this.Type,
                SerializerSettings) ?? new();

            set
            {
                this.Type = value.GetType();

                this.serializedData = JsonConvert.SerializeObject(value,
                    SerializerSettings);
            }
        }
    }
}
