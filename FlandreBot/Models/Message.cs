using System.Threading.Tasks;
using System.Collections.Generic;
using Connector;

public class Message: List<IMessageSub> {
    public BaseConnector source;  // Leave null for outbound messages
    public IMessageTarget sender;  // Leave null for outbound messages
    public async Task Reply(Message message) {
        await source.SendMessage(sender, message);
    }
}
