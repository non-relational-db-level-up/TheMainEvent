import { getEmail, logout } from './authManager.js';
import { connection, start } from './api.js';
import { clearGrid, drawGrid } from './grid.js';
import { backendUrl } from './apiConfig.js';
import { parseSecondsToTimeLeft } from './helpers/time.js';

// Open connection to backend
start();

// Constants
const rows = 30;
const cols = 50;
const cooldownIntervalSeconds = 5;
const playbackDuration = 10;

// Elements
let colourPicker = document.getElementById('colour-input');
let mainTimer = document.getElementById('time-left');
let cooldownTimer = document.getElementById('cooldown-timer');
let cooldownTimerContainer = document.getElementById('cooldown-timer-container');
let playbackButton = document.getElementById('playback-button');
let grid = document.getElementById('grid');
let topic = document.getElementById('topic');
let topicHeader = document.getElementById('topic-header');

// Variables
const userEmail = await getEmail();
let inputAllowed = true;
let roundOver = false;
let countdownSecondsRemaining;
let cooldownSecondsRemaining = 0;
let cooldownInterval;
let timerInterval;
let events = [];

if (cooldownSecondsRemaining === 0) {
  cooldownTimerContainer.style.display = 'none';
}

// Set initial element attributes
document.getElementById('logout-button').addEventListener('click', logout);
document.getElementById('welcome').innerText = `Welcome, ${userEmail}`;
colourPicker.addEventListener('input', function () {
  const colour = this.value;
  document.documentElement.style.setProperty('--selected-color', colour);
});
playbackButton.addEventListener('click', () => playback(events, clearGrid));

drawGrid(rows, cols, 0.7, blockClickHandler);

document.documentElement.style.setProperty('--selected-color', colourPicker.value);

// Socket events
connection.on("ReceiveMessage", (message) => {
  if (roundOver) {
    return;
  }
  let data = message;
  let event = {
    row: data.row,
    col: data.column,
    colour: data.hexColour || "#000000"
  };
  receiveEvent(event);
});

connection.on("StartMessage", (message) => {
  let data = message;
  if (!data.topic){
    return;
  }
  let topic = data.topic;
  let secondsRemaining = data.endTime;

  startRound(topic, secondsRemaining);
});

function sendEvent(event) {
  const token = sessionStorage.getItem('accessToken');
  
  fetch(`${backendUrl}/board`, {
    method: 'POST',
    headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`,
      },
    body: JSON.stringify(event)
    })
    .then(response => response.json())
    .catch(error => console.error('Error:', error));
}

function receiveEvent(event) {
  events.push(event);
  updateBlockColour(event.row, event.col, event.colour);
}

// Block handlers
function blockClickHandler(event) {
  if (!inputAllowed) return;

  let block = event.target;
  let newEvent = {
    row: parseInt(block.dataset.row, 10),
    column: parseInt(block.dataset.col, 10),
    hexColour: colourPicker.value
  };
  sendEvent(newEvent);
  inputAllowed = false;
  grid.classList.add('disabled');

  cooldownSecondsRemaining = cooldownIntervalSeconds;
  cooldownTimer.innerText = cooldownSecondsRemaining;
  cooldownTimerContainer.style.display = 'flex';
  colourPicker.disabled = true;
  cooldownInterval = setInterval(cooldown, 1000);
}

function updateBlockColour(row, col, colour) {
  let block = document.getElementById(`block-${row}-${col}`);
  block.style.backgroundColor = colour;
}

// Timer intervals
function cooldown() {
  if (cooldownSecondsRemaining === 0) {
    cooldownTimer.innerText = '';
    clearInterval(cooldownInterval);
    inputAllowed = true;
    grid.classList.remove('disabled');
    cooldownTimerContainer.style.display = 'none';
    colourPicker.disabled = false;
    return;
  }
  cooldownSecondsRemaining--;
  cooldownTimer.innerText = cooldownSecondsRemaining;
}

function countdown() {
  if (countdownSecondsRemaining === 0) {
    alert('Time is up!');
    endRound();
    return;
  }
  countdownSecondsRemaining--;
  mainTimer.innerText = parseSecondsToTimeLeft(countdownSecondsRemaining);
}

// Round logic
function startRound(newTopic, secondsRemaining) {
  endRound();
  clearGrid(rows, cols);
  inputAllowed = true;
  topicHeader.innerText = 'Draw the following:';
  topic.innerText = newTopic;
  countdownSecondsRemaining = secondsRemaining;
  mainTimer.innerText = parseSecondsToTimeLeft(countdownSecondsRemaining);
  grid.classList.remove('disabled');
  timerInterval = setInterval(countdown, 1000);
  cooldownTimerContainer.style.display = 'none';
  colourPicker.disabled = false;
  colourPicker.classList.remove('disabled');
  events = [];
  roundOver = false;
}

function endRound() {
  roundOver = true;
  clearInterval(timerInterval);
  clearInterval(cooldownInterval);
  inputAllowed = false;
  grid.classList.add('disabled');
  playbackButton.disabled = false;
  playbackButton.classList.remove('disabled');
  cooldownTimerContainer.style.display = 'none';
  colourPicker.disabled = true;
  colourPicker.classList.add('disabled');
}

function playback(events, clearGridFunction) {
  clearGridFunction(rows, cols);
  // clearGrid(rows, cols);
  const delay = (1000 * playbackDuration) / events.length;
  for (let i = 0; i < events.length; i++) {
    setTimeout(() => {
      updateBlockColour((events[i]).row, (events[i]).col, (events[i]).colour);
    }, delay * i); // Stagger the updates
  }
}