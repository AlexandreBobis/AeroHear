using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace AeroHear.Audio
{
    public class SpotifyService
    {
        private SpotifyApi _spotify;
        private EmbedIOAuthServer _server;
        private bool _isAuthenticated = false;

        private string _clientId;
        private string _clientSecret;
        private int _serverPort = 5543;
        private Uri _redirectUri;

        public bool IsAuthenticated => _isAuthenticated;

        public SpotifyService()
        {
            LoadConfiguration();
            _redirectUri = new Uri($"http://localhost:{_serverPort}/callback");
        }

        private void LoadConfiguration()
        {
            try
            {
                var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
                if (File.Exists(configPath))
                {
                    var json = File.ReadAllText(configPath);
                    var config = JsonSerializer.Deserialize<JsonElement>(json);
                    
                    if (config.TryGetProperty("Spotify", out var spotifyConfig))
                    {
                        _clientId = spotifyConfig.GetProperty("ClientId").GetString();
                        _clientSecret = spotifyConfig.GetProperty("ClientSecret").GetString();
                        _serverPort = spotifyConfig.GetProperty("RedirectPort").GetInt32();
                    }
                }
            }
            catch
            {
                // Use default values if config fails to load
                _clientId = "your_client_id_here";
                _clientSecret = "your_client_secret_here";
                _serverPort = 5543;
            }
        }

        public async Task<bool> AuthenticateAsync()
        {
            if (string.IsNullOrEmpty(_clientId) || _clientId == "your_client_id_here")
            {
                throw new InvalidOperationException("Spotify Client ID not configured. Please update appsettings.json with your Spotify application credentials.");
            }

            try
            {
                // Start embedded server for OAuth callback
                _server = new EmbedIOAuthServer(_redirectUri, _serverPort);
                await _server.Start();

                _server.AuthorizationCodeReceived += async (sender, response) =>
                {
                    await _server.Stop();
                    
                    var config = SpotifyClientConfig.CreateDefault();
                    var tokenResponse = await new OAuthClient(config).RequestToken(
                        new AuthorizationCodeTokenRequest(
                            _clientId, _clientSecret, response.Code, _redirectUri)
                    );

                    _spotify = new SpotifyApi(config.WithToken(tokenResponse.AccessToken));
                    _isAuthenticated = true;
                };

                var request = new LoginRequest(_redirectUri, _clientId, LoginRequest.ResponseType.Code)
                {
                    Scope = new List<string> { 
                        Scopes.UserReadCurrentlyPlaying, 
                        Scopes.UserModifyPlaybackState,
                        Scopes.UserReadPlaybackState,
                        Scopes.Streaming
                    }
                };

                var uri = request.ToUri();
                
                // Open browser for authentication
                Process.Start(new ProcessStartInfo
                {
                    FileName = uri.ToString(),
                    UseShellExecute = true
                });

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<List<SimpleTrack>> SearchTracksAsync(string query, int limit = 20)
        {
            if (!_isAuthenticated || _spotify == null)
                return new List<SimpleTrack>();

            try
            {
                var searchRequest = new SearchRequest(SearchRequest.Types.Track, query)
                {
                    Limit = limit
                };

                var searchResponse = await _spotify.Search.Item(searchRequest);
                return searchResponse.Tracks.Items ?? new List<SimpleTrack>();
            }
            catch
            {
                return new List<SimpleTrack>();
            }
        }

        public async Task<string> GetTrackPreviewUrlAsync(string trackId)
        {
            if (!_isAuthenticated || _spotify == null)
                return null;

            try
            {
                var track = await _spotify.Tracks.Get(trackId);
                return track.PreviewUrl;
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> StartPlaybackAsync(string trackUri)
        {
            if (!_isAuthenticated || _spotify == null)
                return false;

            try
            {
                var playbackRequest = new PlayerResumePlaybackRequest
                {
                    Uris = new List<string> { trackUri }
                };

                await _spotify.Player.ResumePlayback(playbackRequest);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> PausePlaybackAsync()
        {
            if (!_isAuthenticated || _spotify == null)
                return false;

            try
            {
                await _spotify.Player.PausePlayback();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void Disconnect()
        {
            _isAuthenticated = false;
            _spotify = null;
            _server?.Stop();
        }
    }
}