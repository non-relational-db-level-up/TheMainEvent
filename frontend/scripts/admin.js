import { backendUrl } from './apiConfig.js';
import { logout } from './authManager.js';
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

const rows = 30;
const cols = 50;
const playbackDuration = 10;

drawGrid(rows, cols, 0.5, null);


populatePastTopics();
document.getElementById('logout-button').addEventListener('click', logout);
document.getElementById('playback-button').addEventListener('click', getEvents);
document.getElementById('start-session-button').addEventListener('click', startSession);


function populatePastTopics() {
  const accessToken = sessionStorage.getItem('accessToken');

  fetch(`${backendUrl}/board/topics`, {
    method: 'GET',
    headers: {
      'Authorization': `Bearer ${accessToken}`
    }
  })
    .then(response => response.json())
    .then(data => {
      console.log('Topics:', data);
      let topics = data;
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

function getEvents(){
  const accessToken = sessionStorage.getItem('accessToken');
  const topic = document.getElementById('topic-selector').value;

  fetch(`${backendUrl}/board/events`, {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${accessToken}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({ TopicName: topic })
  })
    .then(response => response.json())
    .then(data => {
      console.log('Events:', data);
      playback(data);
    })
    .catch(error => {
      console.error('Error fetching events:', error);
    });
}


function startSession() {
  const accessToken = sessionStorage.getItem('accessToken');
  const topic = document.getElementById('topic-input').value;

  fetch(`${backendUrl}/admin`, {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${accessToken}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({ TopicName: topic})
  })
    .then(response => {
      if (response.ok) {
        console.log('Session started');     }
      else {
        console.error('Error starting session:', response);
      }
    })
    .catch(error => {
      console.error('Error starting session:', error);
    });
}

function updateBlockColour(row, col, colour) {
  let block = document.getElementById(`block-${row}-${col}`);
  block.style.backgroundColor = colour;
}


function clearGrid() {
  for (let i = 1; i <= rows; i++) {
    for (let j = 1; j <= cols; j++) {
      let block = document.getElementById(`block-${i}-${j}`);
      block.style.backgroundColor = 'white';
    }
  }
}

function playback(events) {
  clearGrid();
  const delay = (1000 * playbackDuration) / events.length;
  for (let i = 0; i < events.length; i++) {
    setTimeout(() => {
      console.log(events[i]);
      updateBlockColour((events[i]).row, (events[i]).column, (events[i]).hexColour);
    }, delay * i); // Stagger the updates
  }
}
