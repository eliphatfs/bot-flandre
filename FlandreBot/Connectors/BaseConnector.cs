
namespace Connector {
    public abstract class BaseConnector {
        public virtual void Initialize(Config config) { }
        public virtual void SendMessage(IMessageTarget target, Message message) { }
    }
}
