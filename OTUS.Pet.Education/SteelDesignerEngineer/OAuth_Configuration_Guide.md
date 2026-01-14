# OAuth Configuration Guide

## Problem Solved
This guide explains how to fix the **404/401 errors** that occur after OAuth authentication with Google and GitHub.

## Root Cause
The issue was caused by:
1. OAuth providers redirecting to an HTML file instead of an API endpoint
2. Session state being lost between redirects
3. CORS and cookie issues with direct HTML redirects

## Solution Architecture

### Flow:
```
1. User clicks "Login with Google/GitHub" ? Frontend calls /api/Auth/oauth/{provider}/url
2. Backend generates OAuth URL with CSRF state ? Returns to frontend
3. Frontend redirects to OAuth provider (Google/GitHub)
4. User authenticates ? OAuth provider redirects to /api/Auth/oauth/callback (GET)
5. Backend GET endpoint stores code/state in session ? Redirects to /oauth-callback.html
6. HTML page calls /api/Auth/oauth/callback (POST) ? Backend retrieves code/state from session
7. Backend exchanges code for token ? Gets user info ? Creates session
8. Frontend receives success ? Redirects to profile page
```

## Configuration Steps

### 1. Google OAuth Setup

1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Create a new project or select existing one
3. Enable **Google+ API**
4. Go to **Credentials** ? **Create Credentials** ? **OAuth 2.0 Client ID**
5. Application type: **Web application**
6. Add **Authorized redirect URIs**:
   ```
   https://steel-designer-engineer.ru/api/Auth/oauth/callback
   http://localhost:5000/api/Auth/oauth/callback (for local testing)
   ```
7. Copy **Client ID** and **Client Secret**
8. Update `appsettings.json`:
   ```json
   "OAuth": {
     "Google": {
       "ClientId": "YOUR_GOOGLE_CLIENT_ID_HERE",
       "ClientSecret": "YOUR_GOOGLE_CLIENT_SECRET_HERE",
       "RedirectUri": "https://steel-designer-engineer.ru/api/Auth/oauth/callback"
     }
   }
   ```

### 2. GitHub OAuth Setup

1. Go to [GitHub Developer Settings](https://github.com/settings/developers)
2. Click **New OAuth App**
3. Fill in:
   - **Application name**: Steel Designer Engineer
   - **Homepage URL**: `https://steel-designer-engineer.ru`
   - **Authorization callback URL**: `https://steel-designer-engineer.ru/api/Auth/oauth/callback`
4. Click **Register application**
5. Copy **Client ID** and generate **Client Secret**
6. Update `appsettings.json`:
   ```json
   "OAuth": {
     "GitHub": {
       "ClientId": "YOUR_GITHUB_CLIENT_ID_HERE",
       "ClientSecret": "YOUR_GITHUB_CLIENT_SECRET_HERE",
       "RedirectUri": "https://steel-designer-engineer.ru/api/Auth/oauth/callback"
     }
   }
   ```

### 3. Local Development Setup

For local testing, add localhost URLs to OAuth providers:

**Google**: Add `http://localhost:5000/api/Auth/oauth/callback`
**GitHub**: Add `http://localhost:5000/api/Auth/oauth/callback`

Update `appsettings.Development.json`:
```json
{
  "ASPNETCORE_URLS": "http://localhost:5000",
  "OAuth": {
    "Google": {
      "RedirectUri": "http://localhost:5000/api/Auth/oauth/callback"
    },
    "GitHub": {
      "RedirectUri": "http://localhost:5000/api/Auth/oauth/callback"
    }
  }
}
```

## Testing

1. Start the application
2. Navigate to `/login.html`
3. Click **Google** or **GitHub** button
4. You should be redirected to the OAuth provider
5. After authentication, you'll be redirected back to your app
6. The app will process the callback and redirect to `/profile.html`

## Troubleshooting

### 404 Error on Callback
- **Cause**: Redirect URI doesn't match the one configured in OAuth provider
- **Fix**: Make sure the redirect URI in `appsettings.json` exactly matches the one in Google/GitHub settings

### 401 Error
- **Cause**: Session not being maintained or CORS issues
- **Fix**: Check that `credentials: 'include'` is set in all fetch calls and CORS policy allows credentials

### "Invalid state parameter"
- **Cause**: CSRF state mismatch
- **Fix**: Make sure session middleware is enabled and cookies are working

### "OAuth not configured"
- **Cause**: Missing ClientId or ClientSecret in configuration
- **Fix**: Add credentials to `appsettings.json`

## Security Notes

1. **Never commit OAuth secrets** to source control
2. Use **environment variables** or **Azure Key Vault** for production secrets
3. Always validate the **state parameter** to prevent CSRF attacks
4. Use **HTTPS** in production (enforced by `CookieSecurePolicy.Always`)
5. Set **HttpOnly** and **Secure** flags on session cookies

## Additional Resources

- [Google OAuth 2.0 Documentation](https://developers.google.com/identity/protocols/oauth2)
- [GitHub OAuth Documentation](https://docs.github.com/en/developers/apps/building-oauth-apps)
