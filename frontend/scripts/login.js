import { clientId, cognitoDomain, redirectUri } from './apiConfig.js';

document.getElementById('login-button').addEventListener('click', async () => {
  const authUrl = `https://${cognitoDomain}/oauth2/authorize?response_type=token&client_id=${clientId}&redirect_uri=${redirectUri}&scope=openid+email+profile+aws.cognito.signin.user.admin`;
  window.location.href = authUrl;
});