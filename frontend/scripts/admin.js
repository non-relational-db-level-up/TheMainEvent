import { drawGrid } from './grid.js';
import { parseJwt } from './helpers/parseJwt.js';

window.addEventListener('load', () => {
  const idToken = sessionStorage.getItem('accessToken');
  if (idToken) {
    const decodedToken = parseJwt(idToken);
    const groups = decodedToken['cognito:groups'] || [];
    if (!groups.includes('Admin')) {
      window.location.href = '/index.html';
    }
  }
  else {
    console.error('No access token or ID token found');
    window.location.href = '/views/login.html';
  }
});

drawGrid(30, 50, 0.5, null);

populatePastTopics();


function populatePastTopics() {
  const accessToken = sessionStorage.getItem('accessToken');

  fetch('https://example.com/topics', {
    method: 'GET',
    headers: {
      'Authorization': `Bearer ${accessToken}`
    }
  })
    .then(response => response.json())
    .then(data => {
      let topics = data.topics;
      let select = document.getElementById('topic-selector');

      topics.forEach(topic => {
        let option = document.createElement('option');
        option.text = topic;
        select.add(option);
      });
    })
    .catch(error => {
      console.error('Error fetching topics:', error);
    });
}

