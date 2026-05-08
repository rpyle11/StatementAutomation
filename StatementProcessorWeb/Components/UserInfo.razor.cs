using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using StatementProcessorWeb.Services;

namespace StatementProcessorWeb.Components
{
    public partial class UserInfo
    {
        [CascadingParameter]
        private Task<AuthenticationState> AuthenticationStateTask { get; set; } = null!;

        private string? FullName { get; set; } = "Unknown";

        protected override async Task OnInitializedAsync()
        {
            var authState = await AuthenticationStateTask;
            SetUserFullName(authState.User.Identity?.Name?.Split('\\').Last());
        }

        private void SetUserFullName(string? user)
        {
            var ud = new AppUserData(user);
            FullName = ud.UserFullName;
        }
    }
}
