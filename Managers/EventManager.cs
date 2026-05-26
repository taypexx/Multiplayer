using Multiplayer.Data;
using Multiplayer.Static;
using Multiplayer.UI.Extensions;
using System.Net.Http.Json;
using System.Text.Json;

namespace Multiplayer.Managers
{
    internal static class EventManager
    {
        private static void DoAnnouncement(Event e)
        {
            SideNotification.Popup(
                e.Data["Message"].GetString(),
                new(
                    Localization.Get("FriendNotificationSent", "Ok").ToString(),
                    new(0f, 0.82f, 0.28f, 1f),
                    new(0.536f, 1f, 0.05f, 1f),
                    new(() =>
                    {
                        SoundManager.PlayClick();
                        SideNotification.Close();
                    })
                )
            );
        }

        private static async Task DoInvite(Event e)
        {
            var friendUid = e.Data["FriendUid"].GetString();
            var friend = await PlayerManager.GetPlayer(friendUid);

            var lobbyId = e.Data["LobbyId"].GetInt32();
            var lobby = await LobbyManager.GetLobby(lobbyId);
            if (lobby == null) return;

            SideNotification.Popup(
                String.Format(
                    Localization.Get("Invite", "Message").ToString(),
                    Constants.Yellow,
                    friend.MultiplayerStats.Name,
                    Constants.Pink,
                    lobby.Name
                ),
                new(
                    Localization.Get("Invite", "Join").ToString(),
                    new(0f, 0.82f, 0.28f, 1f),
                    new(0.536f, 1f, 0.05f, 1f),
                    new(() =>
                    {
                        SoundManager.PlayClick();
                        SideNotification.Close();
                        _ = UIManager.OpenLobbyWindow(lobby);
                    })
                ),
                new(
                    Localization.Get("Invite", "Ignore").ToString(),
                    new(0.625f, 0f, 0.453f, 1f),
                    new(1f, 0.1f, 0.506f, 1f),
                    new(() =>
                    {
                        SoundManager.PlayClick();
                        SideNotification.Close();
                    })
                )
            );
        }

        private static async Task DoFriendRequest(Event e)
        {
            var type = e.Data["Type"].GetString();

            var friendUid = e.Data["FriendUid"].GetString();
            var friend = await PlayerManager.GetPlayer(friendUid);

            if (type == "Accept")
            {
                SideNotification.Popup(
                    String.Format(
                        Localization.Get("FriendNotificationAccept", "Message").ToString(),
                        Constants.Yellow,
                        friend.MultiplayerStats.Name
                    ),
                    new(
                        Localization.Get("FriendNotificationSent", "Ok").ToString(),
                        new(0f, 0.82f, 0.28f, 1f),
                        new(0.536f, 1f, 0.05f, 1f),
                        new(() =>
                        {
                            SoundManager.PlayClick();
                            SideNotification.Close();
                        })
                    )
                );
            }
            else if (type == "Sent")
            {
                SideNotification.Popup(
                    String.Format(
                        Localization.Get("FriendNotificationSent", "Message").ToString(),
                        Constants.Yellow,
                        friend.MultiplayerStats.Name
                    ),
                    new(
                        Localization.Get("FriendNotificationSent", "Accept").ToString(),
                        new(0f, 0.82f, 0.28f, 1f),
                        new(0.536f, 1f, 0.05f, 1f),
                        new(() =>
                        {
                            SoundManager.PlayClick();
                            SideNotification.Close();
                            _ = UIManager.OpenProfileWindow(friend);
                        })
                    ),
                    new(
                        Localization.Get("FriendNotificationSent", "Ignore").ToString(),
                        new(0.625f, 0f, 0.453f, 1f),
                        new(1f, 0.1f, 0.506f, 1f),
                        new(() =>
                        {
                            SoundManager.PlayClick();
                            SideNotification.Close();
                        })
                    )
                );
            }
        }

        internal static async Task StartEventPolling()
        {
            while (Client.Connected)
            {
                var response = await Client.PostAsync("getEvents", new
                {
                    PlayerManager.LocalPlayer.Uid
                });
                if (response == null) return;

                var body = await response.Content.ReadFromJsonAsync<HashSet<Dictionary<string, JsonElement>>>();
                if (body == null) return;

                foreach (var e_ in body)
                {
                    var type = (EventType)e_["Type"].GetByte();
                    var data = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(e_["Data"]);
                    var time = DateTimeOffset.FromUnixTimeSeconds(e_["Timestamp"].GetInt64()).Date;
                    if (DateTime.Now - time > Constants.EventExpirationTime) continue;

                    var e = new Event(type, data, time);

                    switch (e.Type)
                    {
                        case EventType.Announcement:
                            DoAnnouncement(e);
                            break;
                        case EventType.Invite:
                            await DoInvite(e);
                            break;
                        case EventType.FriendRequest:
                            await DoFriendRequest(e);
                            break;
                    }
                }

                await Task.Delay(Constants.EventDelayMS);
            }
        }
    }
}
