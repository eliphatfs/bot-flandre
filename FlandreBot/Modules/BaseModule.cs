using System.Threading.Tasks;

using Connector;

namespace Module {
    public abstract class BaseModule {
        public abstract Task Initialize(Config config, BaseConnector[] connectors);
        public abstract Task HandleMessage(Message message);
    }
}
