import { getEmail, logout } from './authManager.js';
import { connection, start } from './api.js';
import { clearGrid, drawGrid } from './grid.js';

// Open connection to backend
// start();

// Constants
const rows = 30;
const cols = 50;
const cooldownIntervalSeconds = 30;
const countdownIntervalSeconds = 60;
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

// Start the round
setTimeout(() => startRound('cat', countdownIntervalSeconds), 3000);

drawGrid(rows, cols, 0.7, blockClickHandler);

document.documentElement.style.setProperty('--selected-color', colourPicker.value);

function blockClickHandler(event) {
  if (!inputAllowed) return;

  let block = event.target;
  let newEvent = {
    row: parseInt(block.dataset.row, 10),
    col: parseInt(block.dataset.col, 10),
    colour: colourPicker.value
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

function parseSecondsToTimeLeft(seconds) {
  let minutes = Math.floor(seconds / 60);
  let remainingSeconds = seconds % 60;
  return `${minutes < 10 ? '0' : ''}${minutes}:${remainingSeconds < 10 ? '0' : ''}${remainingSeconds}`;
}

function startRound(newTopic, secondsRemaining) {
  topicHeader.innerText = 'Draw the following:';
  topic.innerText = newTopic;
  countdownSecondsRemaining = secondsRemaining;
  mainTimer.innerText = parseSecondsToTimeLeft(countdownSecondsRemaining);
  grid.classList.remove('disabled');
  timerInterval = setInterval(countdown, 1000);
  cooldownTimerContainer.style.display = 'none';
  events = [];
  roundOver = false;

  // Mocked
  startReceiving();
}

function endRound() {
  roundOver = true;
  clearInterval(timerInterval);
  clearInterval(cooldownInterval);
  inputAllowed = false;
  grid.classList.add('disabled');
  playbackButton.disabled = false;
  playbackButton.classList.remove('disabled');
  colourPicker.disabled = true;
  colourPicker.classList.add('disabled');
}

function updateBlockColour(row, col, colour) {
  let block = document.getElementById(`block-${row}-${col}`);
  block.style.backgroundColor = colour;
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

// MOCK FUNCTIONS
function startReceiving() {
  setInterval(() => {
    if (roundOver) {
      clearInterval();
      setTimeout(() => startRound('dog', countdownIntervalSeconds), 10000);
      return;
    }
    const row = getRandomInt(rows) + 1;
    const col = getRandomInt(cols) + 1;
    const colour = getRandomColor();
    let event = { row, col, colour };
    receiveEvent(event);
  }, 100);
}

function sendEvent(event) {
  // Hit endpoint to send event
  receiveEvent(event);
}

function receiveEvent(event) {
  // Server will push the event to the frontend
  events.push(event);
  updateBlockColour(event.row, event.col, event.colour);
}

function getRandomInt(max) {
  return Math.floor(Math.random() * max);
}

function getRandomColor() {
  const letters = '0123456789ABCDEF';
  let color = '#';
  for (let i = 0; i < 6; i++) {
    color += letters[getRandomInt(16)];
  }
  return color;
}