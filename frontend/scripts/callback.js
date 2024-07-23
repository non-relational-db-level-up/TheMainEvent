function getHashParams() {
  const hash = window.location.hash.substring(1);
  const params = {};

  hash.split('&').forEach(param => {
    const [key, value] = param.split('=');
    params[key] = decodeURIComponent(value);
  });

  return params;
}

window.addEventListener('load', () => {
  const params = getHashParams();

  if (params.access_token && params.id_token) {
    sessionStorage.setItem('accessToken', params.access_token);
    sessionStorage.setItem('idToken', params.id_token);
    window.location.href = "/index.html";
  } else {
    console.error('No access token or ID token found');
    window.location.href = '/views/login.html';
  }
});