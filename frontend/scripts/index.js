import { getEmail, logout } from './authManager.js';
import { connection, start } from './api.js';
import { drawGrid } from './grid.js';


// Open connection to backend
start();

// Constants
const rows = 30;
const cols = 50;
const cooldownIntervalSeconds = 30;
const countdownIntervalSeconds = 60;
const playbackDuration = 10;

// Elements
// let root = document.documentElement;
let colourPicker = document.getElementById('colour-input');
let mainTimer = document.getElementById('time-left');
let cooldownTimer = document.getElementById('cooldown-timer');
let cooldownTimerContainer = document.getElementById('cooldown-timer-container');
let grid = document.getElementById('grid');
let playbackButton = document.getElementById('playback-button');

// Variables
const userEmail = await getEmail();
let inputAllowed = true;
let roundOver = false;
let events = [];

// Fetch from the server
const topic = "Cat";
let countdownSecondsRemaining = countdownIntervalSeconds;
let cooldownSecondsRemaining = 0;

if (cooldownSecondsRemaining === 0) {
  cooldownTimerContainer.style.display = 'none';
}

// Start the countdown loop
mainTimer.innerText = parseSecondsToTimeLeft(countdownSecondsRemaining);
const timerInterval = setInterval(countdown, 1000);
let cooldownInterval;

drawGrid(rows, cols, 0.6, blockClickHandler);

document.getElementById('logout-button').addEventListener('click', connection);
document.getElementById('welcome').innerText = `Welcome, ${userEmail}`;
colourPicker.addEventListener('input', function () {
  const colour = this.value;
  document.documentElement.style.setProperty('--selected-color', colour);
});
document.getElementById('topic').innerText = topic;
playbackButton.addEventListener('click', playback);

document.documentElement.style.setProperty('--selected-color', colourPicker.value);

function blockClickHandler(event) {
  if (!inputAllowed) return;

  let block = event.target;
  let newEvent = {
    row: parseInt(block.dataset.row, 10),
    col: parseInt(block.dataset.col, 10),
    colour: colourPicker.value
  };
  sendEvenet(newEvent);
  inputAllowed = false;
  grid.classList.add('disabled');

  cooldownSecondsRemaining = cooldownIntervalSeconds;
  cooldownTimer.innerText = cooldownSecondsRemaining;
  cooldownTimerContainer.style.display = 'flex';
  cooldownInterval = setInterval(cooldown, 1000);
}

function cooldown() {
  if (cooldownSecondsRemaining === 0) {
    cooldownTimer.innerText = '';
    clearInterval(cooldownInterval);
    inputAllowed = true;
    grid.classList.remove('disabled');
    cooldownTimerContainer.style.display = 'none';
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

function endRound() {
  roundOver = true;
  clearInterval(timerInterval);
  clearInterval(cooldownInterval);
  inputAllowed = false;
  grid.classList.add('disabled');
  playbackButton.disabled = false;
  playbackButton.classList.remove('disabled');
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

function playback() {
  clearGrid();
  const delay = (1000 * playbackDuration) / events.length;
  for (let i = 0; i < events.length; i++) {
    setTimeout(() => {
      console.log(events[i]);
      updateBlockColour((events[i]).row, (events[i]).col, (events[i]).colour);
    }, delay * i); // Stagger the updates
  }
}

// MOCK FUNCTIONS
setInterval(() => {
  if (roundOver) {
    clearInterval();
    return;
  }
  const row = getRandomInt(rows) + 1;
  const col = getRandomInt(cols) + 1;
  const colour = getRandomColor();
  let event = { row, col, colour };
  receiveEvent(event);
}, 100);

function sendEvenet(event) {
  receiveEvent(event);
}

function receiveEvent(event) {
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