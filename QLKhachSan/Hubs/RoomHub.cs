using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace QLKhachSan.Hubs
{
    public class RoomHub : Hub
    {
        public async Task NotifyRoomStatusChange(string maPhong, string trangThaiMoi)
        {
            await Clients.All.SendAsync("ReceiveRoomStatusChange", maPhong, trangThaiMoi);
        }

        public async Task NotifyNewBooking(string maDatPhong)
        {
            await Clients.All.SendAsync("ReceiveNewBooking", maDatPhong);
        }
    }
}
