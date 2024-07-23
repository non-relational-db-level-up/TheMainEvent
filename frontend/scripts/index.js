import { getEmail, logout } from './authManager.js';

const rows = 30;
const cols = 50;

// Elements
let colourPicker = document.getElementById('colour-input');
let timer = document.getElementById('time-left');

const userEmail = await getEmail();
let inputAllowed = true;

// Fetch from the server
const topic = "Cat";
let minutes = 5;
let seconds = 0;

// Start the countdown loop
timer.innerText = `${minutes}:${seconds < 10 ? '0' : ''}${seconds}`;
const timerInterval = setInterval(countdown, 1000);

document.getElementById('logout-button').addEventListener('click', logout);
document.getElementById('welcome').innerText = `Welcome, ${userEmail}`;
colourPicker.addEventListener('input', function () {
  const colour = this.value;
  document.documentElement.style.setProperty('--selected-color', colour);
});
document.getElementById('topic').innerText = topic;

document.documentElement.style.setProperty('--selected-color', colourPicker.value);

drawGrid(rows, cols);

export function drawGrid(rows, cols, interactable = true) {
  let root = document.documentElement;
  let grid = document.getElementById('grid');
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
      if (interactable) {
        block.addEventListener('click', blockClickHandler);
      }
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

  setTimeout(() => {
    inputAllowed = true;
  }, 30000);
}

function countdown() {
  if (seconds === 0) {
    if (minutes === 0) {
      alert('Time is up!');
      clearInterval(timerInterval);
      inputAllowed = false;

      return;
    }
    minutes--;
    seconds = 60;
  }
  seconds--;
  timer.innerText = `${minutes}:${seconds < 10 ? '0' : ''}${seconds}`;
}