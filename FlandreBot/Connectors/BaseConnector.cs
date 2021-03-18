using System.Threading.Tasks;

namespace Connector {
    public abstract class BaseConnector {
        public abstract Task Initialize(Config config);
        public abstract Task SendMessage(IMessageTarget target, Message message);
    }
}
