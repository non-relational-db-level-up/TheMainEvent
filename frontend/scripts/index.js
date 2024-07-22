import {getEmail, logout} from './authManager.js';

// Constants
const rows = 30;
const cols = 50;
const cooldownIntervalSeconds = 30;


// Elements
let root = document.documentElement;
let colourPicker = document.getElementById('colour-input');
let mainTimer = document.getElementById('time-left');
let cooldownTimer = document.getElementById('cooldown-timer');
let cooldownTimerContainer = document.getElementById('cooldown-timer-container');
let grid = document.getElementById('grid');

const userEmail = await getEmail();
let inputAllowed = true;
let roundOver = false;

// Fetch from the server
const topic = "Cat";
let countdownSecondsRemaining = 300;
let cooldownSecondsRemaining = 0;

if (cooldownSecondsRemaining === 0) {
  cooldownTimerContainer.style.display = 'none';
}

// Start the countdown loop
mainTimer.innerText = parseSecondsToTimeLeft(countdownSecondsRemaining);
const timerInterval = setInterval(countdown, 1000);
let cooldownInterval;

drawGrid(rows, cols);

document.getElementById('logout-button').addEventListener('click', logout);
document.getElementById('welcome').innerText = `Welcome, ${userEmail}`;
colourPicker.addEventListener('input', function() {
  const colour = this.value;
  document.documentElement.style.setProperty('--selected-color', colour);
});
document.getElementById('topic').innerText = topic;

document.documentElement.style.setProperty('--selected-color', colourPicker.value);

function drawGrid(rows, cols) {
  let blockSize = 0.6 * (screen.width / cols);
  root.style.setProperty('--block-size', `${blockSize}px`);

  for (let i = 1; i <= rows; i++) {
    let row = document.createElement('section');
    row.className = 'row';
    for (let j = 1; j <= cols; j++) {
      let block = document.createElement('div');
      block.className = 'block';
      block.dataset.row = i;
      block.dataset.col = j;
      block.addEventListener('click', blockClickHandler);
      row.appendChild(block);
    }
    grid.appendChild(row);
  }
}

function blockClickHandler(event) {
  if (!inputAllowed) return;

  let block = event.target;
  block.style.backgroundColor = colourPicker.value;
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
    clearInterval(timerInterval);
    inputAllowed = false;
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