using BPEssentials.Abstractions;
using BPEssentials.ExtensionMethods;
using BrokeProtocol.Utility;
using BrokeProtocol.Entities;

namespace BPEssentials.Commands
{
    public class Arrest : Command
    {
        public void Invoke(ShPlayer player, ShPlayer target)
        {
            target.svPlayer.Restrain(target.manager.handcuffed);
            player.TS("arrested", target.username.CleanerMessage());
        }
    }
}
