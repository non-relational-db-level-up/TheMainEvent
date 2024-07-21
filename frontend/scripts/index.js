import {getEmail, logout} from './authManager.js';

document.getElementById('logout-button').addEventListener('click', logout);

const userEmail = await getEmail();
console.log(`User email: ${userEmail}`);

document.getElementById('welcome').innerText = `Welcome ${userEmail}!`;