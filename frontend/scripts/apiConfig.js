const hostname = window.location.hostname;
const clientId = '3r3pj65risff80d98rpk6b22n7';
const cognitoDomain = 'the-main-event.auth.eu-west-1.amazoncognito.com';
let backendUrl = 'https://socket.themainevent.projects.bbdgrad.com';
let redirectUri = 'https://themainevent.projects.bbdgrad.com/views/callback.html';
let logoutRedirectUri = 'https://themainevent.projects.bbdgrad.com/views/logout.html';
if (hostname.includes("localhost") || hostname.includes("127.0.0.1")) {
  backendUrl = 'http://localhost:5000';
  redirectUri = 'http://localhost:5500/views/callback.html';
  logoutRedirectUri = 'http://localhost:5500/views/logout.html';
}

export { backendUrl, redirectUri, logoutRedirectUri, clientId, cognitoDomain };