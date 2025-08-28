using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using AeroHear.Audio;
using SpotifyAPI.Web;

namespace AeroHear.Forms
{
    public class SpotifyControl : UserControl
    {
        private readonly SpotifyService _spotifyService;
        private readonly SpotifyAudioHandler _audioHandler;
        
        private Button _connectButton;
        private TextBox _searchTextBox;
        private Button _searchButton;
        private ListBox _tracksListBox;
        private Button _playSelectedButton;
        private Label _statusLabel;
        private Label _connectionStatusLabel;

        private List<SimpleTrack> _currentTracks = new List<SimpleTrack>();

        public event EventHandler<string> TrackSelected;

        public SpotifyControl()
        {
            _spotifyService = new SpotifyService();
            _audioHandler = new SpotifyAudioHandler();
            InitializeControl();
        }

        private void InitializeControl()
        {
            BackColor = Color.FromArgb(240, 240, 240);
            BorderStyle = BorderStyle.FixedSingle;
            Size = new Size(640, 200);

            // Title
            var titleLabel = new Label
            {
                Text = "Intégration Spotify",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(10, 10),
                Size = new Size(200, 25)
            };
            Controls.Add(titleLabel);

            // Connection status
            _connectionStatusLabel = new Label
            {
                Text = "Non connecté",
                Location = new Point(220, 10),
                Size = new Size(200, 25),
                ForeColor = Color.Red
            };
            Controls.Add(_connectionStatusLabel);

            // Connect button
            _connectButton = new Button
            {
                Text = "Se connecter",
                Location = new Point(10, 40),
                Size = new Size(120, 30)
            };
            _connectButton.Click += async (s, e) => await ConnectToSpotify();
            Controls.Add(_connectButton);

            // Search controls
            _searchTextBox = new TextBox
            {
                PlaceholderText = "Rechercher une chanson...",
                Location = new Point(140, 40),
                Size = new Size(300, 30),
                Enabled = false
            };
            Controls.Add(_searchTextBox);

            _searchButton = new Button
            {
                Text = "Rechercher",
                Location = new Point(450, 40),
                Size = new Size(100, 30),
                Enabled = false
            };
            _searchButton.Click += async (s, e) => await SearchTracks();
            Controls.Add(_searchButton);

            // Tracks list
            _tracksListBox = new ListBox
            {
                Location = new Point(10, 80),
                Size = new Size(540, 80),
                Enabled = false
            };
            _tracksListBox.DoubleClick += async (s, e) => await PlaySelectedTrack();
            Controls.Add(_tracksListBox);

            // Play button
            _playSelectedButton = new Button
            {
                Text = "Jouer la sélection",
                Location = new Point(10, 170),
                Size = new Size(150, 25),
                Enabled = false
            };
            _playSelectedButton.Click += async (s, e) => await PlaySelectedTrack();
            Controls.Add(_playSelectedButton);

            // Status label
            _statusLabel = new Label
            {
                Text = "Prêt",
                Location = new Point(170, 170),
                Size = new Size(400, 25),
                ForeColor = Color.Blue
            };
            Controls.Add(_statusLabel);
        }

        private async Task ConnectToSpotify()
        {
            _connectButton.Enabled = false;
            _statusLabel.Text = "Connexion à Spotify...";
            _statusLabel.ForeColor = Color.Orange;

            try
            {
                var success = await _spotifyService.AuthenticateAsync();
                
                if (success)
                {
                    // Wait a bit for authentication to complete
                    await Task.Delay(3000);
                    
                    if (_spotifyService.IsAuthenticated)
                    {
                        _connectionStatusLabel.Text = "Connecté";
                        _connectionStatusLabel.ForeColor = Color.Green;
                        _connectButton.Text = "Déconnecter";
                        _searchTextBox.Enabled = true;
                        _searchButton.Enabled = true;
                        _tracksListBox.Enabled = true;
                        _statusLabel.Text = "Connecté à Spotify avec succès!";
                        _statusLabel.ForeColor = Color.Green;
                    }
                    else
                    {
                        _statusLabel.Text = "Échec de l'authentification";
                        _statusLabel.ForeColor = Color.Red;
                    }
                }
                else
                {
                    _statusLabel.Text = "Impossible de se connecter à Spotify";
                    _statusLabel.ForeColor = Color.Red;
                }
            }
            catch (Exception ex)
            {
                _statusLabel.Text = $"Erreur: {ex.Message}";
                _statusLabel.ForeColor = Color.Red;
            }
            finally
            {
                _connectButton.Enabled = true;
            }
        }

        private async Task SearchTracks()
        {
            if (string.IsNullOrWhiteSpace(_searchTextBox.Text))
                return;

            _searchButton.Enabled = false;
            _statusLabel.Text = "Recherche en cours...";
            _statusLabel.ForeColor = Color.Orange;

            try
            {
                _currentTracks = await _spotifyService.SearchTracksAsync(_searchTextBox.Text);
                
                _tracksListBox.Items.Clear();
                
                foreach (var track in _currentTracks)
                {
                    var displayName = $"{string.Join(", ", track.Artists.Select(a => a.Name))} - {track.Name}";
                    _tracksListBox.Items.Add(displayName);
                }

                _statusLabel.Text = $"{_currentTracks.Count} pistes trouvées";
                _statusLabel.ForeColor = Color.Blue;
                
                if (_currentTracks.Any())
                {
                    _playSelectedButton.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                _statusLabel.Text = $"Erreur de recherche: {ex.Message}";
                _statusLabel.ForeColor = Color.Red;
            }
            finally
            {
                _searchButton.Enabled = true;
            }
        }

        private async Task PlaySelectedTrack()
        {
            if (_tracksListBox.SelectedIndex < 0 || _tracksListBox.SelectedIndex >= _currentTracks.Count)
            {
                MessageBox.Show("Veuillez sélectionner une piste.");
                return;
            }

            _playSelectedButton.Enabled = false;
            var selectedTrack = _currentTracks[_tracksListBox.SelectedIndex];
            
            _statusLabel.Text = "Préparation de la piste...";
            _statusLabel.ForeColor = Color.Orange;

            try
            {
                // Get preview URL
                var previewUrl = await _spotifyService.GetTrackPreviewUrlAsync(selectedTrack.Id);
                
                if (string.IsNullOrEmpty(previewUrl))
                {
                    _statusLabel.Text = "Pas d'aperçu disponible pour cette piste";
                    _statusLabel.ForeColor = Color.Red;
                    return;
                }

                // Download preview
                var trackName = $"{string.Join(", ", selectedTrack.Artists.Select(a => a.Name))} - {selectedTrack.Name}";
                var tempFilePath = await _audioHandler.DownloadPreviewTrackAsync(previewUrl, trackName);
                
                if (string.IsNullOrEmpty(tempFilePath))
                {
                    _statusLabel.Text = "Impossible de télécharger la piste";
                    _statusLabel.ForeColor = Color.Red;
                    return;
                }

                // Notify parent form that a track is ready to play
                TrackSelected?.Invoke(this, tempFilePath);
                
                _statusLabel.Text = "Piste prête à jouer!";
                _statusLabel.ForeColor = Color.Green;
            }
            catch (Exception ex)
            {
                _statusLabel.Text = $"Erreur: {ex.Message}";
                _statusLabel.ForeColor = Color.Red;
            }
            finally
            {
                _playSelectedButton.Enabled = true;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _spotifyService?.Disconnect();
                _audioHandler?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}