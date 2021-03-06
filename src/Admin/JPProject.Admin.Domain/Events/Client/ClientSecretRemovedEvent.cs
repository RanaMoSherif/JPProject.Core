using JPProject.Domain.Core.Events;

namespace JPProject.Admin.Domain.Events.Client
{
    public class ClientSecretRemovedEvent : Event
    {
        public string SecretType { get; set; }

        public ClientSecretRemovedEvent(string type, string clientId)
            : base(EventTypes.Success)
        {
            SecretType = type;
            AggregateId = clientId;
        }

    }
}