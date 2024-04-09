using Messenjoor.Shared.Models;
using System.ComponentModel;

namespace Messenjoor.Shared.Services
{
    public class AuthenticationState : INotifyPropertyChanged
    {
        public const string AuthStoreKey = "authkey";

        public event PropertyChangedEventHandler? PropertyChanged;

        public UserModel User { get; set; } = default!;
        public string? Token { get; set; }

        private bool _isAuthenticated;
        public bool IsAuthenticated
        {
            get => _isAuthenticated;
            set
            {
                if (_isAuthenticated != value)
                {
                    _isAuthenticated = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsAuthenticated)));
                }
            }
        }

        public void LoadState(AuthResponseModel authResponseDto)
        {
            User= authResponseDto.User;
            Token = authResponseDto.Token;
            IsAuthenticated = true;
        }
        public void UnLoadState()
        {
            User = default!;
            Token = null;
            IsAuthenticated = false;
        }
    }
}
