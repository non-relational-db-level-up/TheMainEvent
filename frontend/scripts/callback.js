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
    const idToken = sessionStorage.getItem('accessToken');

    if (idToken) {
      const decodedToken = parseJwt(idToken);
      console.log(decodedToken);
      const groups = decodedToken['cognito:groups'] || [];

      if (groups.includes('Admin')) {
        window.location.href = '/views/admin.html';
      } else {
        window.location.href = '/index.html';
      }
    }
  } else {
    console.error('No access token or ID token found');
    window.location.href = '/views/login.html';
  }
});

function parseJwt(token) {
  var base64Url = token.split('.')[1];
  var base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
  var jsonPayload = decodeURIComponent(window.atob(base64).split('').map(function (c) {
    return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
  }).join(''));

  return JSON.parse(jsonPayload);
}

window.addEventListener('load', () => {

});