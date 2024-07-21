import { cognitoDomain, clientId, logoutRedirectUri } from './apiConfig.js';

async function getUserInfo() {
    const token = sessionStorage.getItem('accessToken');
    const response = await fetch(`https://${cognitoDomain}/oauth2/userInfo`, {
        headers: {
            Authorization: `Bearer ${token}`,
        },
    });

    const userInfo = await response.json();
    return userInfo;
}

async function getEmail() {
    return getUserInfo().then(userInfo => userInfo.email);
}

function checkToken() {
    getUserInfo()
        .then(userInfo => {
            if (!userInfo.email) {
                throw new Error('No email could be extracted from the token.');
            }
        })
        .catch(error => {
            console.error('Token check failed:', error);
            window.location.href = "/views/login.html";
        });
}

function logout() {
    const authUrl = `https://${cognitoDomain}/logout?client_id=${clientId}&logout_uri=${logoutRedirectUri}`;
    window.location.href = authUrl;
}

export { logout, getEmail, checkToken };