# Configuration Spotify pour AeroHear

## Étapes de configuration

1. **Créer une application Spotify** :
   - Allez sur [Spotify Developer Dashboard](https://developer.spotify.com/dashboard)
   - Connectez-vous avec votre compte Spotify
   - Cliquez sur "Create an App"
   - Remplissez le nom et la description de votre application
   - Acceptez les termes et conditions

2. **Obtenir les identifiants** :
   - Dans votre application, notez le `Client ID` et `Client Secret`
   - Cliquez sur "Edit Settings"
   - Ajoutez `http://localhost:5543/callback` dans "Redirect URIs"
   - Sauvegardez les modifications

3. **Configurer AeroHear** :
   - Ouvrez le fichier `appsettings.json` dans le répertoire de l'application
   - Remplacez `your_spotify_client_id_here` par votre Client ID
   - Remplacez `your_spotify_client_secret_here` par votre Client Secret
   - Sauvegardez le fichier

## Exemple de configuration

```json
{
  "Spotify": {
    "ClientId": "votre_client_id_ici",
    "ClientSecret": "votre_client_secret_ici",
    "RedirectPort": 5543
  }
}
```

## Utilisation

1. Lancez AeroHear
2. Cliquez sur "Se connecter" dans la section Spotify
3. Votre navigateur s'ouvrira pour l'authentification Spotify
4. Autorisez l'application AeroHear
5. Recherchez et sélectionnez des pistes
6. Les aperçus de 30 secondes seront disponibles pour la lecture multi-périphérique

## Limitations

- Seuls les aperçus de 30 secondes sont disponibles sans abonnement Spotify Premium
- Les pistes complètes nécessitent un abonnement Premium et des APIs supplémentaires
- La qualité des aperçus est limitée par l'API Spotify