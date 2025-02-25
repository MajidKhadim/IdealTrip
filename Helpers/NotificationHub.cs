using Microsoft.AspNetCore.SignalR;

namespace IdealTrip.Helpers
{
	public class NotificationHub : Hub
	{
		public async Task SendNotification(string messege)
		{
			await Clients.All.SendAsync("Recieve Notification", messege);

		}
	}
}
